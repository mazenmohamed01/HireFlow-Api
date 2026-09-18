using HireFlow.Application.Common;
using HireFlow.Domain.Shared;
using HireFlow.Application.DTOs.Candidates;

namespace HireFlow.Application.Services.Candidates;

/// <summary>
/// Candidate profile management.
/// </summary>
public interface ICandidateService
{
    /// <summary>Returns the profile for the currently authenticated candidate.</summary>
    Task<Result<CandidateProfileDto>> GetProfileAsync(CancellationToken cancellationToken = default);

    /// <summary>Updates the currently authenticated candidate's profile fields.</summary>
    Task<Result> UpdateProfileAsync(UpdateCandidateProfileRequest request, CancellationToken cancellationToken = default);
}
