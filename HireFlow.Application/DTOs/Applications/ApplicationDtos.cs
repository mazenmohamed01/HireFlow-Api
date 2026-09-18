using HireFlow.Application.Common;

namespace HireFlow.Application.DTOs.Applications;

public sealed record ApplicationDto(
    int Id,
    int CandidateId,
    int JobId,
    string JobTitle,
    string CompanyName,
    string JobStatus,
    string Status,
    string CvUrl,
    string? CoverLetter,
    DateTime AppliedAt,
    DateTime? UpdatedAt,
    DateTime? CancelledAt);

/// <summary>Item returned in candidate's "my applications" list.</summary>
public sealed record CandidateApplicationItemDto(
    int Id,
    int JobId,
    string JobTitle,
    string CompanyName,
    string JobStatus,
    string Status,
    DateTime AppliedAt);

/// <summary>Item returned in recruiter's applications-per-job list.</summary>
public sealed record RecruiterApplicationItemDto(
    int Id,
    int CandidateId,
    string CandidateName,
    string CandidateEmail,
    string CvUrl,
    string? CoverLetter,
    string Status,
    DateTime AppliedAt);

public sealed record ApplyRequest(
    string? CvUrl,
    string? CoverLetter);

public sealed record ChangeStatusRequest(string Status);

public sealed record ApplicationsFilter(
    string? Status,
    PaginationParams Pagination);
