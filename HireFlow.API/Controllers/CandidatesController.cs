using HireFlow.API.Extensions;
using HireFlow.Application.Common;
using HireFlow.Application.DTOs.Applications;
using HireFlow.Application.DTOs.Profiles;
using HireFlow.Application.Features.Applications.Queries.GetMyApplications;
using HireFlow.Application.Features.Profiles.Commands.UpdateCandidateProfile;
using HireFlow.Application.Features.Profiles.Queries.GetCandidateProfile;
using HireFlow.Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireFlow.API.Controllers;

/// <summary>
/// Profile management and candidate-specific actions.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Candidate")]
[Tags("Candidates")]
public class CandidatesController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Gets the current candidate's profile.
    /// </summary>
    /// <response code="200">Profile retrieved.</response>
    /// <response code="401">Unauthorized access.</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(CandidateProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCandidateProfileQuery(), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Updates the current candidate's profile.
    /// </summary>
    /// <response code="200">Profile updated.</response>
    /// <response code="400">Validation failure.</response>
    /// <response code="401">Unauthorized access.</response>
    [HttpPut("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateMe(UpdateCandidateProfileRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateCandidateProfileCommand(request.FullName, request.Phone, request.CvUrl),
            cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Gets a paginated list of the current candidate's job applications.
    /// </summary>
    /// <response code="200">Paginated list of applications retrieved.</response>
    /// <response code="400">Validation failure on pagination parameters.</response>
    /// <response code="401">Unauthorized access.</response>
    [HttpGet("me/applications")]
    [ProducesResponseType(typeof(PagedResult<CandidateApplicationItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyApplications([FromQuery] ApplicationsFilter filter, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetMyApplicationsQuery(filter.Status, filter.Pagination),
            cancellationToken);
        return result.ToActionResult();
    }
}
