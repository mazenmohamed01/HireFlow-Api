using HireFlow.Application.Common;
using HireFlow.Application.Common.Messaging;
using HireFlow.Application.DTOs.Jobs;

namespace HireFlow.Application.Features.Jobs.Queries.GetMyJobs;

/// <summary>Query to return paged owned jobs for the current recruiter, with application counts.</summary>
public sealed record GetMyJobsQuery(
    string? Status,
    PaginationParams Pagination) : IQuery<PagedResult<RecruiterJobItemDto>>;
