using HireFlow.Application.Features.Auth.Commands.CleanupExpiredTokens;
using HireFlow.Application.Features.Jobs.Commands.CloseExpiredJobs;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HireFlow.API.Services;

/// <summary>
/// A thin wrapper class that Hangfire instantiates to dispatch MediatR commands.
/// This keeps the Hangfire dependency out of the Application layer.
/// </summary>
public class JobBackgroundService(IMediator mediator, ILogger<JobBackgroundService> logger)
{
    public async Task CloseExpiredJobsAsync()
    {
        logger.LogInformation("Hangfire triggered CloseExpiredJobsAsync.");
        // We consider jobs older than 30 days as expired.
        var command = new CloseExpiredJobsCommand(30);
        await mediator.Send(command);
    }

    public async Task CleanupExpiredTokensAsync()
    {
        logger.LogInformation("Hangfire triggered CleanupExpiredTokensAsync.");
        var command = new CleanupExpiredTokensCommand();
        await mediator.Send(command);
    }
}
