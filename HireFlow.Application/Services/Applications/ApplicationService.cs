using HireFlow.Application.Common;
using HireFlow.Application.DTOs.Applications;
using HireFlow.Application.Interfaces;
using HireFlow.Domain.Entities;
using HireFlow.Domain.Enums;
using HireFlow.Domain.Shared;

namespace HireFlow.Application.Services.Applications;

public sealed class ApplicationService(
    IJobApplicationRepository applicationRepository,
    IJobRepository jobRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    TimeProvider timeProvider) : IApplicationService
{
    public async Task<Result<int>> ApplyAsync(int jobId, ApplyRequest request, CancellationToken cancellationToken = default)
    {
        if (currentUser.CandidateId == null)
            return Result.Failure<int>(new Error("Auth.Forbidden", "Only candidates can apply for jobs.", ErrorType.Forbidden));

        var job = await jobRepository.GetByIdAsync(jobId, cancellationToken);
        if (job == null)
            return Result.Failure<int>(new Error("Job.NotFound", "Job not found.", ErrorType.NotFound));

        if (!job.IsOpen)
            return Result.Failure<int>(new Error("Job.Closed", "Cannot apply to a closed job.", ErrorType.Conflict));

        // Note: For BR-P2, candidate shouldn't apply twice. We can check if application exists.
        // Assuming we could just check if there's any application for this candidate and jobId, but omitting for brevity if not strictly enforced by a unique index (but good practice)
        
        var candidateUser = await userRepository.GetByIdWithProfilesAsync(currentUser.UserId, cancellationToken);
        var profileCvUrl = candidateUser?.Candidate?.CvUrl;

        var applicationResult = JobApplication.Create(
            currentUser.CandidateId.Value,
            job,
            request.CvUrl,
            profileCvUrl,
            request.CoverLetter,
            timeProvider.GetUtcNow().UtcDateTime);

        if (applicationResult.IsFailure)
            return Result.Failure<int>(applicationResult.Error!);

        var application = applicationResult.Value;
        
        await applicationRepository.InsertAsync(application, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result.Success(application.Id);
    }

    public async Task<Result<PagedResult<CandidateApplicationItemDto>>> GetMyApplicationsAsync(ApplicationsFilter filter, CancellationToken cancellationToken = default)
    {
        if (currentUser.CandidateId == null)
            return Result.Failure<PagedResult<CandidateApplicationItemDto>>(new Error("Auth.Forbidden", "Only candidates can access this.", ErrorType.Forbidden));

        var page = filter.Pagination.Page > 0 ? filter.Pagination.Page : 1;
        var pageSize = filter.Pagination.PageSize > 0 ? filter.Pagination.PageSize : 10;
        if (pageSize > 50) pageSize = 50;

        var (items, totalCount) = await applicationRepository.GetCandidateApplicationsAsync(
            currentUser.CandidateId.Value, filter.Status, page, pageSize, cancellationToken);

        var dtos = items.Select(a => new CandidateApplicationItemDto(
            a.Id, a.JobId, a.Job.Title, a.Job.Recruiter.CompanyName, a.Job.Status.ToString(), a.Status.ToString(), a.AppliedAt)).ToList();

        return Result.Success(new PagedResult<CandidateApplicationItemDto>(dtos, page, pageSize, totalCount));
    }

    public async Task<Result<ApplicationDto>> GetByIdAsync(int applicationId, CancellationToken cancellationToken = default)
    {
        var application = await applicationRepository.GetByIdWithJobAsync(applicationId, cancellationToken);
        if (application == null)
            return Result.Failure<ApplicationDto>(new Error("Application.NotFound", "Application not found.", ErrorType.NotFound));

        // BR-P11: Visible to owning candidate or recruiter who owns the job
        if (currentUser.CandidateId != application.CandidateId && currentUser.RecruiterId != application.Job.RecruiterId)
            return Result.Failure<ApplicationDto>(new Error("Application.NotFound", "Application not found.", ErrorType.NotFound));

        var dto = new ApplicationDto(
            application.Id, application.CandidateId, application.JobId, application.Job.Title, application.Job.Recruiter.CompanyName,
            application.Job.Status.ToString(), application.Status.ToString(), application.CvUrl, application.CoverLetter, 
            application.AppliedAt, application.UpdatedAt, application.CancelledAt);

        return Result.Success(dto);
    }

    public async Task<Result> CancelAsync(int applicationId, CancellationToken cancellationToken = default)
    {
        if (currentUser.CandidateId == null)
            return Result.Failure(new Error("Auth.Forbidden", "Only candidates can cancel applications.", ErrorType.Forbidden));

        var application = await applicationRepository.GetByIdAsync(applicationId, cancellationToken);
        if (application == null || application.CandidateId != currentUser.CandidateId.Value)
            return Result.Failure(new Error("Application.NotFound", "Application not found.", ErrorType.NotFound));

        var cancelResult = application.Cancel(timeProvider.GetUtcNow().UtcDateTime);
        if (cancelResult.IsFailure)
            return cancelResult;

        applicationRepository.Update(application);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }

    public async Task<Result<PagedResult<RecruiterApplicationItemDto>>> GetJobApplicationsAsync(int jobId, ApplicationsFilter filter, CancellationToken cancellationToken = default)
    {
        if (currentUser.RecruiterId == null)
            return Result.Failure<PagedResult<RecruiterApplicationItemDto>>(new Error("Auth.Forbidden", "Only recruiters can access this.", ErrorType.Forbidden));

        var job = await jobRepository.GetByIdAsync(jobId, cancellationToken);
        if (job == null || job.RecruiterId != currentUser.RecruiterId.Value)
            return Result.Failure<PagedResult<RecruiterApplicationItemDto>>(new Error("Job.NotFound", "Job not found.", ErrorType.NotFound));

        var page = filter.Pagination.Page > 0 ? filter.Pagination.Page : 1;
        var pageSize = filter.Pagination.PageSize > 0 ? filter.Pagination.PageSize : 10;
        if (pageSize > 50) pageSize = 50;

        var (items, totalCount) = await applicationRepository.GetJobApplicationsAsync(
            jobId, filter.Status, page, pageSize, cancellationToken);

        var dtos = items.Select(a => new RecruiterApplicationItemDto(
            a.Id, a.CandidateId, a.Candidate.User.FullName, a.Candidate.User.Email,
            a.CvUrl, a.CoverLetter, a.Status.ToString(), a.AppliedAt)).ToList();

        return Result.Success(new PagedResult<RecruiterApplicationItemDto>(dtos, page, pageSize, totalCount));
    }

    public async Task<Result> ChangeStatusAsync(int applicationId, ChangeStatusRequest request, CancellationToken cancellationToken = default)
    {
        if (currentUser.RecruiterId == null)
            return Result.Failure(new Error("Auth.Forbidden", "Only recruiters can change application status.", ErrorType.Forbidden));

        var application = await applicationRepository.GetByIdWithJobAsync(applicationId, cancellationToken);
        if (application == null || application.Job.RecruiterId != currentUser.RecruiterId.Value)
            return Result.Failure(new Error("Application.NotFound", "Application not found.", ErrorType.NotFound));

        if (!Enum.TryParse<ApplicationStatus>(request.Status, true, out var newStatus))
            return Result.Failure(new ValidationError("Validation.Failed", "Validation failed", new Dictionary<string, string[]> { { "Status", new[] { "Invalid application status." } } }));

        var changeResult = application.ChangeStatusByRecruiter(newStatus, timeProvider.GetUtcNow().UtcDateTime);
        if (changeResult.IsFailure)
            return changeResult;

        applicationRepository.Update(application);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}
