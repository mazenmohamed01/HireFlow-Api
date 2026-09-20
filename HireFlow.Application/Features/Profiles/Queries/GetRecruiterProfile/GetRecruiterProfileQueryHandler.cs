using HireFlow.Application.Common;
using HireFlow.Application.DTOs.Profiles;
using HireFlow.Application.Interfaces;
using HireFlow.Domain.Enums;
using HireFlow.Domain.Shared;
using MediatR;

namespace HireFlow.Application.Features.Profiles.Queries.GetRecruiterProfile;

public sealed class GetRecruiterProfileQueryHandler(
    IUserRepository userRepository,
    ICurrentUser currentUser) : IRequestHandler<GetRecruiterProfileQuery, Result<RecruiterProfileDto>>
{
    public async Task<Result<RecruiterProfileDto>> Handle(GetRecruiterProfileQuery query, CancellationToken cancellationToken)
    {
        if (currentUser.Role != UserRole.Recruiter.ToString() || currentUser.RecruiterId is null)
            return Result.Failure<RecruiterProfileDto>(new Error("Auth.Forbidden", "Only recruiters can access this profile.", ErrorType.Forbidden));

        var user = await userRepository.GetByIdWithProfilesAsync(currentUser.UserId, cancellationToken);
        if (user is null || user.Recruiter is null)
            return Result.Failure<RecruiterProfileDto>(new Error("Recruiter.NotFound", "Recruiter profile not found.", ErrorType.NotFound));

        return Result.Success(new RecruiterProfileDto(
            user.Recruiter.Id, user.FullName, user.Email, user.Recruiter.CompanyName));
    }
}
