using HireFlow.API.Extensions;
using HireFlow.Application.DTOs.Auth;
using HireFlow.Application.Features.Auth.Commands.Register;
using HireFlow.Application.Features.Auth.Queries.GetCurrentUser;
using HireFlow.Application.Features.Auth.Queries.Login;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HireFlow.API.Controllers;

/// <summary>
/// Authentication and identity endpoints.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("AuthPolicy")]
[Tags("Authentication")]
public class AuthController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <response code="201">User successfully registered.</response>
    /// <response code="400">Validation failure.</response>
    /// <response code="409">Email already in use.</response>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new RegisterCommand(request.FullName, request.Email, request.Password, request.Role, request.CompanyName),
            cancellationToken);
        return result.ToCreatedActionResult("GetMe", new { }, this);
    }

    /// <summary>
    /// Authenticates a user and issues a JWT token.
    /// </summary>
    /// <response code="200">Successfully authenticated.</response>
    /// <response code="400">Validation failure.</response>
    /// <response code="401">Invalid credentials.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new LoginQuery(request.Email, request.Password), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Gets the current authenticated user's identity details.
    /// </summary>
    /// <response code="200">User details retrieved.</response>
    /// <response code="401">Unauthorized access.</response>
    [Authorize]
    [HttpGet("me", Name = "GetMe")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCurrentUserQuery(), cancellationToken);
        return result.ToActionResult();
    }
}
