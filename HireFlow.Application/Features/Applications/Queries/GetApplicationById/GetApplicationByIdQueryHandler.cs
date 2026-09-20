using HireFlow.Application.Common;
using HireFlow.Application.DTOs.Applications;
using HireFlow.Application.Interfaces;
using HireFlow.Domain.Shared;
using MediatR;

namespace HireFlow.Application.Features.Applications.Queries.GetApplicationById;

public sealed class GetApplicationByIdQueryHandler(
    IJobApplicationRepository applicationRepository,
    ICurrentUser currentUser) : IRequestHandler<GetApplicationByIdQuery, Result<ApplicationDto>>
{
    public async Task<Result<ApplicationDto>> Handle(GetApplicationByIdQuery query, CancellationToken cancellationToken)
    {
        var application = await applicationRepository.GetByIdWithJobAsync(query.ApplicationId, cancellationToken);
        if (application is null)
            return Result.Failure<ApplicationDto>(new Error("Application.NotFound", "Application not found.", ErrorType.NotFound));

        // BR-P11: Visible to owning candidate or the recruiter who owns the job.
        if (currentUser.CandidateId != application.CandidateId && currentUser.RecruiterId != application.Job.RecruiterId)
            return Result.Failure<ApplicationDto>(new Error("Application.NotFound", "Application not found.", ErrorType.NotFound));

        var dto = new ApplicationDto(
            application.Id, application.CandidateId, application.JobId,
            application.Job.Title, application.Job.Recruiter.CompanyName,
            application.Job.Status.ToString(), application.Status.ToString(),
            application.CvUrl, application.CoverLetter,
            application.AppliedAt, application.UpdatedAt, application.CancelledAt);

        return Result.Success(dto);
    }
}
