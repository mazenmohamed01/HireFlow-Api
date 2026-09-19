namespace HireFlow.Application.Common;

/// <summary>
/// Application abstraction for password hashing.
/// Implemented in Infrastructure using <c>Microsoft.AspNetCore.Identity.PasswordHasher</c>
/// (from <c>Microsoft.Extensions.Identity.Core</c>), which uses PBKDF2-SHA512.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>Hashes a plain-text password.</summary>
    string Hash(string password);

    /// <summary>Verifies a plain-text password against a stored hash.</summary>
    bool Verify(string password, string hash);
}
