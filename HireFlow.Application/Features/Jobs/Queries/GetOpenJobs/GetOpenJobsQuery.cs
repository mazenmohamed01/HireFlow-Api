using HireFlow.Application.Common;
using HireFlow.Application.Common.Messaging;
using HireFlow.Application.DTOs.Jobs;

namespace HireFlow.Application.Features.Jobs.Queries.GetOpenJobs;

/// <summary>Query to return a paginated list of Open jobs matching the filter.</summary>
public sealed record GetOpenJobsQuery(
    string? Search,
    string? JobType,
    string? Location,
    PaginationParams Pagination) : IQuery<PagedResult<JobListItemDto>>;
