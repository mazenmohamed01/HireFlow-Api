using HireFlow.API.Extensions;
using HireFlow.Application.Common;
using HireFlow.Application.DTOs.Applications;
using HireFlow.Application.DTOs.Jobs;
using HireFlow.Application.Features.Applications.Commands.Apply;
using HireFlow.Application.Features.Applications.Queries.GetJobApplications;
using HireFlow.Application.Features.Jobs.Commands.CloseJob;
using HireFlow.Application.Features.Jobs.Commands.CreateJob;
using HireFlow.Application.Features.Jobs.Commands.ReopenJob;
using HireFlow.Application.Features.Jobs.Commands.UpdateJob;
using HireFlow.Application.Features.Jobs.Queries.GetJobById;
using HireFlow.Application.Features.Jobs.Queries.GetOpenJobs;
using HireFlow.Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireFlow.API.Controllers;

/// <summary>
/// Job management endpoints.
/// </summary>
[Route("api/jobs")]
[ApiController]
[Tags("Jobs")]
public sealed class JobsController(IMediator mediator) : ControllerBase
{
    /// <summary>Creates a new job posting. Recruiter only.</summary>
    /// <response code="201">Job created. Returns the new job id.</response>
    /// <response code="400">Validation failure.</response>
    /// <response code="401">Unauthorized access.</response>
    [HttpPost]
    [Authorize(Roles = "Recruiter")]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateJobRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateJobCommand(request.Title, request.Description, request.Location, request.JobType),
            cancellationToken);

        if (result.IsFailure)
            return result.ToActionResult();

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    /// <summary>
    /// Gets a paginated list of open jobs. Available to anyone.
    /// </summary>
    /// <response code="200">Paginated list of open jobs.</response>
    /// <response code="400">Invalid filter parameters.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<JobListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetOpenJobs([FromQuery] JobsFilter filter, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetOpenJobsQuery(filter.Search, filter.JobType, filter.Location, filter.Pagination),
            cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Gets a specific job by id.
    /// </summary>
    /// <response code="200">Job details retrieved.</response>
    /// <response code="404">Job not found.</response>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(JobDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetJobByIdQuery(id), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Updates an existing job posting. Recruiter only.
    /// </summary>
    /// <response code="200">Job successfully updated.</response>
    /// <response code="400">Validation failure.</response>
    /// <response code="401">Unauthorized access.</response>
    /// <response code="404">Job not found or unauthorized to modify.</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "Recruiter")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateJobRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateJobCommand(id, request.Title, request.Description, request.Location, request.JobType),
            cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Closes an open job posting. Recruiter only.
    /// </summary>
    /// <response code="200">Job successfully closed.</response>
    /// <response code="401">Unauthorized access.</response>
    /// <response code="404">Job not found or unauthorized to modify.</response>
    /// <response code="409">Job already closed.</response>
    [HttpPost("{id}/close")]
    [Authorize(Roles = "Recruiter")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Close(int id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CloseJobCommand(id), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Reopens a closed job posting. Recruiter only.
    /// </summary>
    /// <response code="200">Job successfully reopened.</response>
    /// <response code="401">Unauthorized access.</response>
    /// <response code="404">Job not found or unauthorized to modify.</response>
    /// <response code="409">Job already open.</response>
    [HttpPost("{id}/reopen")]
    [Authorize(Roles = "Recruiter")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Reopen(int id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ReopenJobCommand(id), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Applies to an open job. Candidate only.
    /// </summary>
    /// <response code="201">Application submitted. Returns the application id.</response>
    /// <response code="400">Validation failure.</response>
    /// <response code="401">Unauthorized access.</response>
    /// <response code="404">Job not found or is closed.</response>
    /// <response code="409">Candidate already applied.</response>
    [HttpPost("{id}/applications")]
    [Authorize(Roles = "Candidate")]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Apply(int id, [FromBody] ApplyRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ApplyCommand(id, request.CvUrl, request.CoverLetter), cancellationToken);

        if (result.IsFailure)
            return result.ToActionResult();

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    /// <summary>
    /// Gets applications for a specific job. Recruiter only.
    /// </summary>
    /// <response code="200">Paginated list of applications.</response>
    /// <response code="401">Unauthorized access.</response>
    /// <response code="404">Job not found or unauthorized to view.</response>
    [HttpGet("{id}/applications")]
    [Authorize(Roles = "Recruiter")]
    [ProducesResponseType(typeof(PagedResult<RecruiterApplicationItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetJobApplications(int id, [FromQuery] ApplicationsFilter filter, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetJobApplicationsQuery(id, filter.Status, filter.Pagination),
            cancellationToken);
        return result.ToActionResult();
    }
}
