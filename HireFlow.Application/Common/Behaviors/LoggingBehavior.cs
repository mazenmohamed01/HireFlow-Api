using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace HireFlow.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that logs request start, completion, and duration.
/// Errors are re-thrown after logging so the GlobalExceptionHandler can catch them.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var sw = Stopwatch.StartNew();

        logger.LogInformation("Handling {RequestName}", requestName);

        try
        {
            var response = await next();
            sw.Stop();

            if (sw.ElapsedMilliseconds > 500)
                logger.LogWarning("Slow handler detected: {RequestName} took {ElapsedMs}ms", requestName, sw.ElapsedMilliseconds);
            else
                logger.LogInformation("Handled {RequestName} in {ElapsedMs}ms", requestName, sw.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(ex, "Error handling {RequestName} after {ElapsedMs}ms", requestName, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
