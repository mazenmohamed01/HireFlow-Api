using HireFlow.Application.Common;
using HireFlow.Application.DTOs.Auth;
using HireFlow.Application.Interfaces;
using HireFlow.Domain.Entities;
using HireFlow.Domain.Enums;
using HireFlow.Domain.Shared;

namespace HireFlow.Application.Services.Auth;

public sealed class AuthService(
    IUserRepository userRepository,
    ICandidateRepository candidateRepository,
    IRecruiterRepository recruiterRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    TimeProvider timeProvider,
    ICurrentUser currentUser) : IAuthService
{
    public async Task<Result<int>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return Result.Failure<int>(new ValidationError("Validation.Failed", "Validation failed", new Dictionary<string, string[]> { { "Email", new[] { "Email is required." } } }));

        if (!Enum.TryParse<UserRole>(request.Role, true, out var role) || (role != UserRole.Candidate && role != UserRole.Recruiter))
        {
            return Result.Failure<int>(new Error("Auth.InvalidRole", "Role must be Candidate or Recruiter.", ErrorType.Validation));
        }

        if (role == UserRole.Recruiter && string.IsNullOrWhiteSpace(request.CompanyName))
        {
            return Result.Failure<int>(new ValidationError("Validation.Failed", "Validation failed", new Dictionary<string, string[]> { { "CompanyName", new[] { "CompanyName is required for Recruiters." } } }));
        }

        var existingUser = await userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser != null)
        {
            return Result.Failure<int>(new Error("Auth.EmailAlreadyExists", "Email is already registered.", ErrorType.Conflict));
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        
        var hashedPassword = passwordHasher.Hash(request.Password);
        
        var user = User.Create(request.FullName, request.Email, hashedPassword, role, now);
        
        await userRepository.InsertAsync(user, cancellationToken);
        
        // We must flush to get the User ID
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        if (role == UserRole.Candidate)
        {
            var candidate = Candidate.Create(user.Id, null, null);
            await candidateRepository.InsertAsync(candidate, cancellationToken);
        }
        else if (role == UserRole.Recruiter)
        {
            var recruiter = Recruiter.Create(user.Id, request.CompanyName!);
            await recruiterRepository.InsertAsync(recruiter, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(user.Id);
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user == null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Result.Failure<AuthResponse>(new Error("Auth.InvalidCredentials", "Invalid email or password.", ErrorType.Unauthorized));
        }

        var profileId = user.Role == UserRole.Candidate ? user.Candidate?.Id ?? 0 
            : user.Role == UserRole.Recruiter ? user.Recruiter?.Id ?? 0 
            : 0;

        var tokenData = jwtTokenService.Generate(user, profileId);

        var userDto = new UserDto(user.Id, user.FullName, user.Email, user.Role.ToString(), profileId);

        return Result.Success(new AuthResponse(tokenData.AccessToken, tokenData.ExpiresAtUtc, userDto));
    }

    public async Task<Result<UserDto>> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated)
            return Result.Failure<UserDto>(new Error("Auth.Unauthorized", "User is not authenticated.", ErrorType.Unauthorized));

        var user = await userRepository.GetByIdWithProfilesAsync(currentUser.UserId, cancellationToken);

        if (user == null)
            return Result.Failure<UserDto>(new Error("User.NotFound", "User not found.", ErrorType.NotFound));

        var profileId = currentUser.CandidateId ?? currentUser.RecruiterId ?? 0;

        return Result.Success(new UserDto(user.Id, user.FullName, user.Email, user.Role.ToString(), profileId));
    }
}
