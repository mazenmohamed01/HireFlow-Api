using HireFlow.Application.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HireFlow.API.Middleware;

/// <summary>
/// Global handler for unhandled exceptions. Maps every unexpected exception to a
/// <c>500 Internal Server Error</c> ProblemDetails response.
/// Stack traces are omitted outside the Development environment.
/// </summary>
internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(
            exception,
            "Unhandled exception for request {Method} {Path}",
            httpContext.Request.Method,
            httpContext.Request.Path);

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title  = "Unexpected",
            Detail = "An unexpected error occurred. Please try again later."
        };
        problem.Extensions["code"] = "Unexpected";

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true; // Mark as handled — prevents the default middleware from re-running.
    }
}
