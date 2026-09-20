using HireFlow.Application.Common;
using HireFlow.Application.Common.Messaging;
using HireFlow.Application.DTOs.Applications;

namespace HireFlow.Application.Features.Applications.Queries.GetJobApplications;

/// <summary>Query to return paginated applications for a recruiter-owned job.</summary>
public sealed record GetJobApplicationsQuery(
    int JobId,
    string? Status,
    PaginationParams Pagination) : IQuery<PagedResult<RecruiterApplicationItemDto>>;
