using HireFlow.API.Extensions;
using HireFlow.Application.DTOs.Jobs;
using HireFlow.Application.Services.Jobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireFlow.API.Controllers;

/// <summary>
/// Job management endpoints.
/// Phase 1: only POST /api/jobs is wired end-to-end as the Result pipeline demo.
/// Full endpoint set is completed in Phase 6.
/// </summary>
[Route("api/jobs")]
[ApiController]
public sealed class JobsController(IJobService jobService) : ControllerBase
{
    /// <summary>Creates a new job posting. Recruiter only (enforced in Phase 4).</summary>
    /// <response code="201">Job created. Returns the new job id.</response>
    /// <response code="400">Validation failure.</response>
    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateJobRequest request,
        CancellationToken cancellationToken)
    {
        var result = await jobService.CreateAsync(request, cancellationToken);

        if (result.IsFailure)
            return result.ToActionResult();

        // Returns 201 with the new id; Location header added in Phase 6.
        return StatusCode(StatusCodes.Status201Created, result.Value);
    }
}
