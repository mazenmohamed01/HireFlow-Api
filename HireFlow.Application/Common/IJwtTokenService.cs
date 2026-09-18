using HireFlow.Domain.Entities;

namespace HireFlow.Application.Common;

/// <summary>
/// Application abstraction for JWT token generation.
/// Implemented in Infrastructure.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a signed JWT for the given user and their profile id.
    /// </summary>
    /// <param name="user">The authenticated user.</param>
    /// <param name="profileId">The candidate or recruiter profile id (for JWT claims per BR-A5).</param>
    /// <returns>A tuple of the access token string and its UTC expiry.</returns>
    (string AccessToken, DateTime ExpiresAtUtc) Generate(User user, int profileId);
}
