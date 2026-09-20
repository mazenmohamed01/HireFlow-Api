using HireFlow.Application.Common;
using HireFlow.Application.DTOs.Auth;
using HireFlow.Application.Interfaces;
using HireFlow.Domain.Enums;
using HireFlow.Domain.Shared;
using MediatR;

namespace HireFlow.Application.Features.Auth.Queries.Login;

public sealed class LoginQueryHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService) : IRequestHandler<LoginQuery, Result<AuthResponse>>
{
    public async Task<Result<AuthResponse>> Handle(LoginQuery query, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(query.Email, cancellationToken);

        if (user is null || !passwordHasher.Verify(query.Password, user.PasswordHash))
            return Result.Failure<AuthResponse>(new Error("Auth.InvalidCredentials", "Invalid email or password.", ErrorType.Unauthorized));

        var profileId = user.Role == UserRole.Candidate
            ? user.Candidate?.Id ?? 0
            : user.Role == UserRole.Recruiter
                ? user.Recruiter?.Id ?? 0
                : 0;

        var tokenData = jwtTokenService.Generate(user, profileId);
        var userDto = new UserDto(user.Id, user.FullName, user.Email, user.Role.ToString(), profileId);

        return Result.Success(new AuthResponse(tokenData.AccessToken, tokenData.ExpiresAtUtc, userDto));
    }
}
