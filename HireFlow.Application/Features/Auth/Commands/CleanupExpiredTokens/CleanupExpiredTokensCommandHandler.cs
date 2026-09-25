using HireFlow.Application.Common.Messaging;
using HireFlow.Domain.Shared;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HireFlow.Application.Features.Auth.Commands.CleanupExpiredTokens;

public sealed class CleanupExpiredTokensCommandHandler(
    ILogger<CleanupExpiredTokensCommandHandler> logger)
    : IRequestHandler<CleanupExpiredTokensCommand, Result>
{
    public Task<Result> Handle(CleanupExpiredTokensCommand request, CancellationToken cancellationToken)
    {
        // This is a structural placeholder for when Refresh Tokens are implemented in the DB.
        // It demonstrates how a recurring task triggers a CQRS command.
        
        logger.LogInformation("Simulating cleanup of expired JWT refresh tokens...");
        
        // TODO: jobRepository/tokenRepository.DeleteExpiredTokensAsync(DateTime.UtcNow);
        // await unitOfWork.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Cleanup of expired tokens finished.");

        return Task.FromResult(Result.Success());
    }
}
