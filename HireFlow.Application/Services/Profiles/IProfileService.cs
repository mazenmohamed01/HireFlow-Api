using HireFlow.Application.DTOs.Profiles;
using HireFlow.Domain.Shared;

namespace HireFlow.Application.Services.Profiles;

public interface IProfileService
{
    Task<Result<CandidateProfileDto>> GetCandidateProfileAsync(CancellationToken cancellationToken = default);
    Task<Result<CandidateProfileDto>> UpdateCandidateProfileAsync(UpdateCandidateProfileRequest request, CancellationToken cancellationToken = default);

    Task<Result<RecruiterProfileDto>> GetRecruiterProfileAsync(CancellationToken cancellationToken = default);
    Task<Result<RecruiterProfileDto>> UpdateRecruiterProfileAsync(UpdateRecruiterProfileRequest request, CancellationToken cancellationToken = default);
}
