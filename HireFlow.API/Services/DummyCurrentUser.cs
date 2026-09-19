using HireFlow.Application.Common;
using HireFlow.Domain.Enums;

namespace HireFlow.API.Services;

/// <summary>
/// Dummy implementation of ICurrentUser for Phase 1-3.
/// Real implementation using HttpContextAccessor is added in Phase 4.
/// </summary>
public class DummyCurrentUser : ICurrentUser
{
    public bool IsAuthenticated => true;
    public int UserId => 1;
    public string Role => UserRole.Recruiter.ToString();
    public int? CandidateId => null;
    public int? RecruiterId => 1;
}
