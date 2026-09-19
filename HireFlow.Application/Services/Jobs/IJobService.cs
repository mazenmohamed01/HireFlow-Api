using HireFlow.Application.Common;
using HireFlow.Domain.Shared;
using HireFlow.Application.DTOs.Jobs;

namespace HireFlow.Application.Services.Jobs;

/// <summary>
/// Job management for both public browsing (candidates) and recruiter-owned operations.
/// </summary>
public interface IJobService
{
    // ── Public (anonymous / any authenticated user) ───────────────────────

    /// <summary>Returns a paged list of Open jobs matching the filter (BR-J6).</summary>
    Task<Result<PagedResult<JobListItemDto>>> GetOpenJobsAsync(JobsFilter filter, CancellationToken cancellationToken = default);

    /// <summary>Returns details of any existing job (Open or Closed) by id (BR-J6).</summary>
    Task<Result<JobDto>> GetByIdAsync(int jobId, CancellationToken cancellationToken = default);

    // ── Recruiter operations ──────────────────────────────────────────────

    /// <summary>Creates a new Open job owned by the current recruiter (BR-J1).</summary>
    Task<Result<int>> CreateAsync(CreateJobRequest request, CancellationToken cancellationToken = default);

    /// <summary>Updates an owned job (only while Open — BR-J2, BR-J3).</summary>
    Task<Result> UpdateAsync(int jobId, UpdateJobRequest request, CancellationToken cancellationToken = default);

    /// <summary>Closes an owned job (BR-J4). Does not modify existing applications (BR-J7).</summary>
    Task<Result> CloseAsync(int jobId, CancellationToken cancellationToken = default);

    /// <summary>Reopens an owned closed job (BR-J5). Does not modify existing applications (BR-J7).</summary>
    Task<Result> ReopenAsync(int jobId, CancellationToken cancellationToken = default);

    /// <summary>Returns paged owned jobs for the current recruiter, with application counts.</summary>
    Task<Result<PagedResult<RecruiterJobItemDto>>> GetMyJobsAsync(MyJobsFilter filter, CancellationToken cancellationToken = default);
}
