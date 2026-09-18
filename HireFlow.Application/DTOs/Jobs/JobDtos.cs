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

public sealed record CreateJobRequest(
    string Title,
    string Description,
    string? Location,
    string JobType);

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
