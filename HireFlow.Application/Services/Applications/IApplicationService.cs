using HireFlow.Application.Common;
using HireFlow.Application.DTOs.Applications;

namespace HireFlow.Application.Services.Applications;

/// <summary>
/// Application lifecycle management for candidates and recruiters.
/// </summary>
public interface IApplicationService
{
    // ── Candidate operations ──────────────────────────────────────────────

    /// <summary>
    /// Submits an application to a job (BR-P1..P5).
    /// Reference implementation described in plan section 7.3.
    /// </summary>
    Task<Result<int>> ApplyAsync(int jobId, ApplyRequest request, CancellationToken cancellationToken = default);

    /// <summary>Returns the current candidate's applications with job context (BR-P11).</summary>
    Task<Result<PagedResult<CandidateApplicationItemDto>>> GetMyApplicationsAsync(ApplicationsFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns application details. Visible to the owning candidate
    /// or the recruiter who owns the job (BR-P11).
    /// </summary>
    Task<Result<ApplicationDto>> GetByIdAsync(int applicationId, CancellationToken cancellationToken = default);

    /// <summary>Candidate cancels their own application (BR-P6).</summary>
    Task<Result> CancelAsync(int applicationId, CancellationToken cancellationToken = default);

    // ── Recruiter operations ──────────────────────────────────────────────

    /// <summary>
    /// Returns applications for a recruiter-owned job (BR-P8, BR-P11).
    /// Works for both Open and Closed jobs.
    /// </summary>
    Task<Result<PagedResult<RecruiterApplicationItemDto>>> GetJobApplicationsAsync(int jobId, ApplicationsFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes the status of an application on a recruiter-owned job (BR-P7..P10).
    /// </summary>
    Task<Result> ChangeStatusAsync(int applicationId, ChangeStatusRequest request, CancellationToken cancellationToken = default);
}
