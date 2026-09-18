namespace HireFlow.Application.Common;

/// <summary>
/// Exposes identity information extracted from the caller's JWT claims.
/// Implemented in the API layer via <c>IHttpContextAccessor</c>.
/// </summary>
public interface ICurrentUser
{
    /// <summary>The authenticated user's primary key.</summary>
    int UserId { get; }

    /// <summary>The user's role string (matches <c>UserRole</c> enum name).</summary>
    string Role { get; }

    /// <summary>The candidate profile id; present only when <see cref="Role"/> is Candidate.</summary>
    int? CandidateId { get; }

    /// <summary>The recruiter profile id; present only when <see cref="Role"/> is Recruiter.</summary>
    int? RecruiterId { get; }

    /// <summary>Whether the current HTTP request carries a valid, authenticated identity.</summary>
    bool IsAuthenticated { get; }
}
