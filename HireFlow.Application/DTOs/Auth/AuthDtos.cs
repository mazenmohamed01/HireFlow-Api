namespace HireFlow.Application.DTOs.Auth;

public sealed record RegisterRequest(
    string FullName,
    string Email,
    string Password,
    string Role,
    string? CompanyName);

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
