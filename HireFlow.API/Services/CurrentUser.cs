using HireFlow.Application.Common;
using HireFlow.Domain.Enums;
using System.Security.Claims;

namespace HireFlow.API.Services;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public int UserId => int.TryParse(User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

    public string Role => User?.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

    public int? CandidateId
    {
        get
        {
            if (int.TryParse(User?.FindFirst("CandidateId")?.Value, out var id))
                return id;
            return null;
        }
    }

    public int? RecruiterId
    {
        get
        {
            if (int.TryParse(User?.FindFirst("RecruiterId")?.Value, out var id))
                return id;
            return null;
        }
    }
}
