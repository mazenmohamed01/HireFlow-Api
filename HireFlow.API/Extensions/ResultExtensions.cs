using HireFlow.Application.Common;
using HireFlow.Domain.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HireFlow.API.Extensions;

/// <summary>
/// Extension methods that translate a <see cref="Result"/> or <see cref="Result{T}"/> into
/// an <see cref="IActionResult"/> with an appropriate HTTP status code and ProblemDetails body.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Maps a void <see cref="Result"/> to a <c>204 No Content</c> on success,
    /// or a ProblemDetails error response on failure.
    /// </summary>
    public static IActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
            return new NoContentResult();

        return result.Error!.ToProblemResult();
    }

    /// <summary>
    /// Maps a <see cref="Result{T}"/> to a <c>200 OK</c> carrying the value on success,
    /// or a ProblemDetails error response on failure.
    /// </summary>
    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return new OkObjectResult(result.Value);

        return result.Error!.ToProblemResult();
    }

    /// <summary>
    /// Maps a <see cref="Result{T}"/> to a <c>201 Created</c> on success.
    /// </summary>
    public static IActionResult ToCreatedActionResult<T>(
        this Result<T> result,
        string routeName,
        object? routeValues,
        ControllerBase controller)
    {
        if (result.IsSuccess)
            return controller.CreatedAtRoute(routeName, routeValues, result.Value);

        return result.Error!.ToProblemResult();
    }

    // ── Private helpers ──────────────────────────────────────────────────

    private static IActionResult ToProblemResult(this Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation   => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden    => StatusCodes.Status403Forbidden,
            ErrorType.NotFound     => StatusCodes.Status404NotFound,
            ErrorType.Conflict     => StatusCodes.Status409Conflict,
            ErrorType.Failure      => StatusCodes.Status500InternalServerError,
            _                      => StatusCodes.Status500InternalServerError
        };

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title  = error.Code,
            Detail = error.Description
        };

        // Always embed the machine-readable code in the extensions bag.
        problem.Extensions["code"] = error.Code;

        // For validation errors, add the per-field errors dictionary.
        if (error is ValidationError validationError)
            problem.Extensions["errors"] = validationError.Errors;

        return new ObjectResult(problem) { StatusCode = statusCode };
    }
}
