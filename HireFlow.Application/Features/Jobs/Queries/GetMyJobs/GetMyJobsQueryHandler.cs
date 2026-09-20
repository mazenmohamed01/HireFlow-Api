using HireFlow.Application.Common;
using HireFlow.Application.DTOs.Jobs;
using HireFlow.Application.Interfaces;
using HireFlow.Domain.Shared;
using MediatR;

namespace HireFlow.Application.Features.Jobs.Queries.GetMyJobs;

public sealed class GetMyJobsQueryHandler(
    IJobRepository jobRepository,
    ICurrentUser currentUser) : IRequestHandler<GetMyJobsQuery, Result<PagedResult<RecruiterJobItemDto>>>
{
    public async Task<Result<PagedResult<RecruiterJobItemDto>>> Handle(GetMyJobsQuery query, CancellationToken cancellationToken)
    {
        if (currentUser.RecruiterId is null)
            return Result.Failure<PagedResult<RecruiterJobItemDto>>(new Error("Auth.Forbidden", "Only recruiters can access this.", ErrorType.Forbidden));

        var page = query.Pagination.Page > 0 ? query.Pagination.Page : 1;
        var pageSize = Math.Clamp(query.Pagination.PageSize, 1, 50);

        var (items, totalCount) = await jobRepository.GetPagedMyJobsAsync(
            currentUser.RecruiterId.Value, query.Status, page, pageSize, cancellationToken);

        var dtos = items.Select(x => new RecruiterJobItemDto(
            x.Job.Id, x.Job.Title, x.Job.Location, x.Job.JobType.ToString(), x.Job.Status.ToString(),
            x.Job.CreatedAt, x.Job.ClosedAt, x.ApplicationsCount)).ToList();

        return Result.Success(new PagedResult<RecruiterJobItemDto>(dtos, page, pageSize, totalCount));
    }
}
