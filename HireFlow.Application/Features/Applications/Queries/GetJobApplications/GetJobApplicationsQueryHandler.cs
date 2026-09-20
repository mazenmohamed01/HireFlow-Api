using HireFlow.Application.Common;
using HireFlow.Application.DTOs.Applications;
using HireFlow.Application.Interfaces;
using HireFlow.Domain.Shared;
using MediatR;

namespace HireFlow.Application.Features.Applications.Queries.GetJobApplications;

public sealed class GetJobApplicationsQueryHandler(
    IJobApplicationRepository applicationRepository,
    IJobRepository jobRepository,
    ICurrentUser currentUser) : IRequestHandler<GetJobApplicationsQuery, Result<PagedResult<RecruiterApplicationItemDto>>>
{
    public async Task<Result<PagedResult<RecruiterApplicationItemDto>>> Handle(GetJobApplicationsQuery query, CancellationToken cancellationToken)
    {
        if (currentUser.RecruiterId is null)
            return Result.Failure<PagedResult<RecruiterApplicationItemDto>>(new Error("Auth.Forbidden", "Only recruiters can access this.", ErrorType.Forbidden));

        var job = await jobRepository.GetByIdAsync(query.JobId, cancellationToken);
        if (job is null || job.RecruiterId != currentUser.RecruiterId.Value)
            return Result.Failure<PagedResult<RecruiterApplicationItemDto>>(new Error("Job.NotFound", "Job not found.", ErrorType.NotFound));

        var page = query.Pagination.Page > 0 ? query.Pagination.Page : 1;
        var pageSize = Math.Clamp(query.Pagination.PageSize, 1, 50);

        var (items, totalCount) = await applicationRepository.GetJobApplicationsAsync(
            query.JobId, query.Status, page, pageSize, cancellationToken);

        var dtos = items.Select(a => new RecruiterApplicationItemDto(
            a.Id, a.CandidateId, a.Candidate.User.FullName, a.Candidate.User.Email,
            a.CvUrl, a.CoverLetter, a.Status.ToString(), a.AppliedAt)).ToList();

        return Result.Success(new PagedResult<RecruiterApplicationItemDto>(dtos, page, pageSize, totalCount));
    }
}
