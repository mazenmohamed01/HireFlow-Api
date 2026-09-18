using HireFlow.Application.Common;
using HireFlow.Application.DTOs.Auth;

namespace HireFlow.Application.Services.Auth;

/// <summary>
/// Handles user registration, login, and identity queries.
/// </summary>
public interface IAuthService
{
    /// <summary>Registers a new Candidate or Recruiter user (BR-A1..A4).</summary>
    Task<Result<int>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    /// <summary>Authenticates a user and returns a signed JWT (BR-A5).</summary>
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>Returns the profile information of the currently authenticated user.</summary>
    Task<Result<UserDto>> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}
