using HireFlow.Application.Common;
using HireFlow.Application.DTOs.Profiles;
using HireFlow.Application.Interfaces;
using HireFlow.Domain.Enums;
using HireFlow.Domain.Shared;
using MediatR;

namespace HireFlow.Application.Features.Profiles.Queries.GetCandidateProfile;

public sealed class GetCandidateProfileQueryHandler(
    IUserRepository userRepository,
    ICurrentUser currentUser) : IRequestHandler<GetCandidateProfileQuery, Result<CandidateProfileDto>>
{
    public async Task<Result<CandidateProfileDto>> Handle(GetCandidateProfileQuery query, CancellationToken cancellationToken)
    {
        if (currentUser.Role != UserRole.Candidate.ToString() || currentUser.CandidateId is null)
            return Result.Failure<CandidateProfileDto>(new Error("Auth.Forbidden", "Only candidates can access this profile.", ErrorType.Forbidden));

        var user = await userRepository.GetByIdWithProfilesAsync(currentUser.UserId, cancellationToken);
        if (user is null || user.Candidate is null)
            return Result.Failure<CandidateProfileDto>(new Error("Candidate.NotFound", "Candidate profile not found.", ErrorType.NotFound));

        return Result.Success(new CandidateProfileDto(
            user.Candidate.Id, user.FullName, user.Email, user.Candidate.Phone, user.Candidate.CvUrl));
    }
}
