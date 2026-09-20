using HireFlow.Application.Common;
using HireFlow.Application.DTOs.Auth;
using HireFlow.Application.Interfaces;
using HireFlow.Domain.Shared;
using MediatR;

namespace HireFlow.Application.Features.Auth.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler(
    IUserRepository userRepository,
    ICurrentUser currentUser) : IRequestHandler<GetCurrentUserQuery, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(GetCurrentUserQuery query, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated)
            return Result.Failure<UserDto>(new Error("Auth.Unauthorized", "User is not authenticated.", ErrorType.Unauthorized));

        var user = await userRepository.GetByIdWithProfilesAsync(currentUser.UserId, cancellationToken);
        if (user is null)
            return Result.Failure<UserDto>(new Error("User.NotFound", "User not found.", ErrorType.NotFound));

        var profileId = currentUser.CandidateId ?? currentUser.RecruiterId ?? 0;

        return Result.Success(new UserDto(user.Id, user.FullName, user.Email, user.Role.ToString(), profileId));
    }
}
