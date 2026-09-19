using HireFlow.API.Extensions;
using HireFlow.Application.DTOs.Applications;
using HireFlow.Application.Services.Applications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireFlow.API.Controllers;

/// <summary>
/// Application management endpoints for both candidates and recruiters.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Applications")]
public class ApplicationsController(IApplicationService applicationService) : ControllerBase
{
    /// <summary>
    /// Gets the details of a specific job application.
    /// </summary>
    /// <response code="200">Application details retrieved.</response>
    /// <response code="401">Unauthorized access.</response>
    /// <response code="404">Application not found or unauthorized to view.</response>
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ApplicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetApplication(int id, CancellationToken cancellationToken)
    {
        var result = await applicationService.GetByIdAsync(id, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Cancels a job application. Candidate only.
    /// </summary>
    /// <response code="200">Application successfully cancelled.</response>
    /// <response code="400">Application is in a terminal state and cannot be cancelled.</response>
    /// <response code="401">Unauthorized access.</response>
    /// <response code="404">Application not found or unauthorized to modify.</response>
    /// <response code="409">Concurrency conflict.</response>
    [HttpPost("{id}/cancel")]
    [Authorize(Roles = "Candidate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelApplication(int id, CancellationToken cancellationToken)
    {
        var result = await applicationService.CancelAsync(id, cancellationToken);
        
        if (result.IsFailure)
            return result.ToActionResult();
            
        return NoContent();
    }

    /// <summary>
    /// Changes the status of an application. Recruiter only.
    /// </summary>
    /// <response code="200">Status successfully changed.</response>
    /// <response code="400">Validation or business rule failure.</response>
    /// <response code="401">Unauthorized access.</response>
    /// <response code="404">Application not found or unauthorized to modify.</response>
    /// <response code="409">Concurrency conflict or invalid transition.</response>
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Recruiter")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await applicationService.ChangeStatusAsync(id, request, cancellationToken);
        return result.ToActionResult();
    }
}
