using HireFlow.Application.Common;
using HireFlow.Domain.Shared;

namespace HireFlow.Application.DTOs.Jobs;

public sealed record JobDto(
    int Id,
    int RecruiterId,
    string CompanyName,
    string Title,
    string Description,
    string? Location,
    string JobType,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? ClosedAt);

public sealed record JobListItemDto(
    int Id,
    int RecruiterId,
    string CompanyName,
    string Title,
    string? Location,
    string JobType,
    DateTime CreatedAt);

public sealed record RecruiterJobItemDto(
    int Id,
    string Title,
    string? Location,
    string JobType,
    string Status,
    DateTime CreatedAt,
    DateTime? ClosedAt,
    int ApplicationsCount);

/// <summary>
/// Payload for creating a new job posting.
/// </summary>
/// <param name="Title">The job title. Example: Senior Software Engineer</param>
/// <param name="Description">Detailed job description.</param>
/// <param name="Location">Optional location. Example: Remote</param>
/// <param name="JobType">FullTime, PartTime, or Contract.</param>
public sealed record CreateJobRequest(
    string Title,
    string Description,
    string? Location,
    string JobType);

/// <summary>
/// Payload for updating an existing job posting.
/// </summary>
/// <param name="Title">The updated job title.</param>
/// <param name="Description">The updated detailed description.</param>
/// <param name="Location">The updated location.</param>
/// <param name="JobType">The updated job type.</param>
public sealed record UpdateJobRequest(
    string Title,
    string Description,
    string? Location,
    string JobType);

public sealed record JobsFilter(
    string? Search,
    string? JobType,
    string? Location,
    PaginationParams Pagination);

public sealed record MyJobsFilter(
    string? Status,
    PaginationParams Pagination);
