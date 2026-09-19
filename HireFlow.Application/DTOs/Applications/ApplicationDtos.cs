using HireFlow.Application.Common;
using HireFlow.Domain.Shared;

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

/// <summary>
/// Payload for submitting a new job application.
/// </summary>
/// <param name="CvUrl">A valid URL pointing to the candidate's CV document.</param>
/// <param name="CoverLetter">Optional cover letter or pitch.</param>
public sealed record ApplyRequest(
    string? CvUrl,
    string? CoverLetter);

/// <summary>
/// Payload for changing an application's status.
/// </summary>
/// <param name="Status">Accepted or Rejected.</param>
public sealed record ChangeStatusRequest(string Status);

public sealed record ApplicationsFilter(
    string? Status,
    PaginationParams Pagination);
