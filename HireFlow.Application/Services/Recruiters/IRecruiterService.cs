using HireFlow.Application.Common;
using HireFlow.Application.DTOs.Recruiters;

namespace HireFlow.Application.Services.Recruiters;

/// <summary>
/// Recruiter profile management.
/// </summary>
public interface IRecruiterService
{
    /// <summary>Returns the profile for the currently authenticated recruiter.</summary>
    Task<Result<RecruiterProfileDto>> GetProfileAsync(CancellationToken cancellationToken = default);

    /// <summary>Updates the currently authenticated recruiter's profile fields.</summary>
    Task<Result> UpdateProfileAsync(UpdateRecruiterProfileRequest request, CancellationToken cancellationToken = default);
}
