using HireFlow.Application.Common;
using HireFlow.Application.DTOs.Jobs;
using HireFlow.Application.Interfaces;
using HireFlow.Domain.Shared;
using MediatR;

namespace HireFlow.Application.Features.Jobs.Queries.GetOpenJobs;

public sealed class GetOpenJobsQueryHandler(
    IJobRepository jobRepository) : IRequestHandler<GetOpenJobsQuery, Result<PagedResult<JobListItemDto>>>
{
    public async Task<Result<PagedResult<JobListItemDto>>> Handle(GetOpenJobsQuery query, CancellationToken cancellationToken)
    {
        var page = query.Pagination.Page > 0 ? query.Pagination.Page : 1;
        var pageSize = Math.Clamp(query.Pagination.PageSize, 1, 50);

        var (items, totalCount) = await jobRepository.GetPagedOpenJobsAsync(
            query.Search, query.JobType, query.Location, page, pageSize, cancellationToken);

        var dtos = items.Select(j => new JobListItemDto(
            j.Id, j.RecruiterId, j.Recruiter.CompanyName, j.Title, j.Location, j.JobType.ToString(), j.CreatedAt)).ToList();

        return Result.Success(new PagedResult<JobListItemDto>(dtos, page, pageSize, totalCount));
    }
}
