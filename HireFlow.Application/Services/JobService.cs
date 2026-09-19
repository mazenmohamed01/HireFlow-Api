using HireFlow.Application.Common;
using HireFlow.Application.Interfaces;
using HireFlow.Domain.Shared;
using HireFlow.Application.DTOs.Jobs;
using HireFlow.Application.Services.Jobs;
using HireFlow.Domain.Entities;

namespace HireFlow.Application.Services.Jobs;

/// <summary>
/// Stub implementation of <see cref="IJobService"/>. Methods are completed phase by phase.
/// Phase 1 only provides CreateAsync as a working demonstration of the Result pipeline.
/// </summary>
#pragma warning disable CS9113 // currentUser and timeProvider used from Phase 6 onward
public sealed class JobService(
    IJobRepository jobRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    TimeProvider timeProvider) : IJobService
#pragma warning restore CS9113
{
    public async Task<Result<int>> CreateAsync(CreateJobRequest request, CancellationToken cancellationToken = default)
    {
        if (currentUser.RecruiterId == null)
            return Result.Failure<int>(new Error("Auth.Forbidden", "Only recruiters can create jobs.", ErrorType.Forbidden));

        if (!Enum.TryParse<HireFlow.Domain.Enums.JobType>(request.JobType, true, out var jobType))
            return Result.Failure<int>(new ValidationError("Validation.Failed", "Validation failed", new Dictionary<string, string[]> { { "JobType", new[] { "Invalid job type." } } }));

        if (string.IsNullOrWhiteSpace(request.Title))
            return Result.Failure<int>(new ValidationError("Validation.Failed", "Validation failed", new Dictionary<string, string[]> { { "Title", new[] { "Title is required." } } }));

        var jobResult = Job.Create(
            currentUser.RecruiterId.Value,
            request.Title,
            request.Description,
            request.Location,
            jobType,
            timeProvider.GetUtcNow().UtcDateTime);

        if (jobResult.IsFailure)
        {
            return Result.Failure<int>(jobResult.Error!);
        }

        var job = jobResult.Value;

        await jobRepository.InsertAsync(job, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(job.Id);
    }

    public async Task<Result<PagedResult<JobListItemDto>>> GetOpenJobsAsync(JobsFilter filter, CancellationToken cancellationToken = default)
    {
        var page = filter.Pagination.Page > 0 ? filter.Pagination.Page : 1;
        var pageSize = filter.Pagination.PageSize > 0 ? filter.Pagination.PageSize : 10;
        if (pageSize > 50) pageSize = 50;

        var (items, totalCount) = await jobRepository.GetPagedOpenJobsAsync(
            filter.Search, filter.JobType, filter.Location, page, pageSize, cancellationToken);

        var dtos = items.Select(j => new JobListItemDto(
            j.Id, j.RecruiterId, j.Recruiter.CompanyName, j.Title, j.Location, j.JobType.ToString(), j.CreatedAt)).ToList();

        return Result.Success(new PagedResult<JobListItemDto>(dtos, page, pageSize, totalCount));
    }

    public async Task<Result<JobDto>> GetByIdAsync(int jobId, CancellationToken cancellationToken = default)
    {
        var job = await jobRepository.GetByIdWithRecruiterAsync(jobId, cancellationToken);
        if (job == null)
            return Result.Failure<JobDto>(new Error("Job.NotFound", "Job not found.", ErrorType.NotFound));

        var dto = new JobDto(
            job.Id, job.RecruiterId, job.Recruiter.CompanyName, job.Title, job.Description,
            job.Location, job.JobType.ToString(), job.Status.ToString(), job.CreatedAt, job.UpdatedAt, job.ClosedAt);

        return Result.Success(dto);
    }

    public async Task<Result> UpdateAsync(int jobId, UpdateJobRequest request, CancellationToken cancellationToken = default)
    {
        if (currentUser.RecruiterId == null)
            return Result.Failure(new Error("Job.NotFound", "Job not found.", ErrorType.NotFound)); // Hide existence if not recruiter

        var job = await jobRepository.GetByIdAsync(jobId, cancellationToken);
        if (job == null || job.RecruiterId != currentUser.RecruiterId.Value)
            return Result.Failure(new Error("Job.NotFound", "Job not found.", ErrorType.NotFound));

        if (!Enum.TryParse<HireFlow.Domain.Enums.JobType>(request.JobType, true, out var jobType))
            return Result.Failure(new ValidationError("Validation.Failed", "Validation failed", new Dictionary<string, string[]> { { "JobType", new[] { "Invalid job type." } } }));

        var updateResult = job.Update(request.Title, request.Description, request.Location, jobType, timeProvider.GetUtcNow().UtcDateTime);
        if (updateResult.IsFailure)
            return updateResult;

        jobRepository.Update(job);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> CloseAsync(int jobId, CancellationToken cancellationToken = default)
    {
        if (currentUser.RecruiterId == null)
            return Result.Failure(new Error("Job.NotFound", "Job not found.", ErrorType.NotFound));

        var job = await jobRepository.GetByIdAsync(jobId, cancellationToken);
        if (job == null || job.RecruiterId != currentUser.RecruiterId.Value)
            return Result.Failure(new Error("Job.NotFound", "Job not found.", ErrorType.NotFound));

        var closeResult = job.Close(timeProvider.GetUtcNow().UtcDateTime);
        if (closeResult.IsFailure)
            return closeResult;

        jobRepository.Update(job);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> ReopenAsync(int jobId, CancellationToken cancellationToken = default)
    {
        if (currentUser.RecruiterId == null)
            return Result.Failure(new Error("Job.NotFound", "Job not found.", ErrorType.NotFound));

        var job = await jobRepository.GetByIdAsync(jobId, cancellationToken);
        if (job == null || job.RecruiterId != currentUser.RecruiterId.Value)
            return Result.Failure(new Error("Job.NotFound", "Job not found.", ErrorType.NotFound));

        var reopenResult = job.Reopen(timeProvider.GetUtcNow().UtcDateTime);
        if (reopenResult.IsFailure)
            return reopenResult;

        jobRepository.Update(job);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<PagedResult<RecruiterJobItemDto>>> GetMyJobsAsync(MyJobsFilter filter, CancellationToken cancellationToken = default)
    {
        if (currentUser.RecruiterId == null)
            return Result.Failure<PagedResult<RecruiterJobItemDto>>(new Error("Auth.Forbidden", "Only recruiters can access this.", ErrorType.Forbidden));

        var page = filter.Pagination.Page > 0 ? filter.Pagination.Page : 1;
        var pageSize = filter.Pagination.PageSize > 0 ? filter.Pagination.PageSize : 10;
        if (pageSize > 50) pageSize = 50;

        var (items, totalCount) = await jobRepository.GetPagedMyJobsAsync(
            currentUser.RecruiterId.Value, filter.Status, page, pageSize, cancellationToken);

        var dtos = items.Select(x => new RecruiterJobItemDto(
            x.Job.Id, x.Job.Title, x.Job.Location, x.Job.JobType.ToString(), x.Job.Status.ToString(),
            x.Job.CreatedAt, x.Job.ClosedAt, x.ApplicationsCount)).ToList();

        return Result.Success(new PagedResult<RecruiterJobItemDto>(dtos, page, pageSize, totalCount));
    }
}
