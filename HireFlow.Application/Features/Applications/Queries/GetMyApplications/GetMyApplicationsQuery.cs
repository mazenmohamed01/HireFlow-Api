using HireFlow.Application.Common;
using HireFlow.Application.Common.Messaging;
using HireFlow.Application.DTOs.Applications;

namespace HireFlow.Application.Features.Applications.Queries.GetMyApplications;

/// <summary>Query to return the current candidate's applications with job context.</summary>
public sealed record GetMyApplicationsQuery(
    string? Status,
    PaginationParams Pagination) : IQuery<PagedResult<CandidateApplicationItemDto>>;
