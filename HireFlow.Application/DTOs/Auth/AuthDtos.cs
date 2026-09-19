namespace HireFlow.Application.DTOs.Auth;

/// <summary>
/// Payload for registering a new user.
/// </summary>
/// <param name="FullName">The full name of the user. Example: John Doe</param>
/// <param name="Email">A valid email address. Example: john@example.com</param>
/// <param name="Password">Minimum 8 characters, 1 uppercase, 1 lowercase, 1 digit, 1 special character.</param>
/// <param name="Role">Candidate or Recruiter.</param>
/// <param name="CompanyName">Required if role is Recruiter.</param>
public sealed record RegisterRequest(
    string FullName,
    string Email,
    string Password,
    string Role,
    string? CompanyName);

/// <summary>
/// Payload for authenticating a user.
/// </summary>
/// <param name="Email">The registered email address. Example: john@example.com</param>
/// <param name="Password">The user's password.</param>
public sealed record LoginRequest(
    string Email,
    string Password);

public sealed record AuthResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    UserDto User);

public sealed record UserDto(
    int Id,
    string FullName,
    string Email,
    string Role,
    int ProfileId);
