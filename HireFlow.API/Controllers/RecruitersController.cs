using HireFlow.API.Extensions;
using HireFlow.Application.Common;
using HireFlow.Application.DTOs.Jobs;
using HireFlow.Application.DTOs.Profiles;
using HireFlow.Application.Features.Jobs.Queries.GetMyJobs;
using HireFlow.Application.Features.Profiles.Commands.UpdateRecruiterProfile;
using HireFlow.Application.Features.Profiles.Queries.GetRecruiterProfile;
using HireFlow.Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireFlow.API.Controllers;

/// <summary>
/// Profile management and recruiter-specific actions.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Recruiter")]
[Tags("Recruiters")]
public class RecruitersController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Gets the current recruiter's profile.
    /// </summary>
    /// <response code="200">Profile retrieved.</response>
    /// <response code="401">Unauthorized access.</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(RecruiterProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetRecruiterProfileQuery(), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Updates the current recruiter's profile.
    /// </summary>
    /// <response code="200">Profile updated.</response>
    /// <response code="400">Validation failure.</response>
    /// <response code="401">Unauthorized access.</response>
    [HttpPut("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateMe(UpdateRecruiterProfileRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateRecruiterProfileCommand(request.FullName, request.CompanyName),
            cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Gets a paginated list of the current recruiter's created jobs.
    /// </summary>
    /// <response code="200">Paginated list of jobs retrieved.</response>
    /// <response code="400">Validation failure on pagination parameters.</response>
    /// <response code="401">Unauthorized access.</response>
    [HttpGet("me/jobs")]
    [ProducesResponseType(typeof(PagedResult<RecruiterJobItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyJobs([FromQuery] MyJobsFilter filter, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetMyJobsQuery(filter.Status, filter.Pagination), cancellationToken);
        return result.ToActionResult();
    }
}
