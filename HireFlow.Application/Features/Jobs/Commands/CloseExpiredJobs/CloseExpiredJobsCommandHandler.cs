using HireFlow.Application.Common;
using HireFlow.Application.Common.Messaging;
using HireFlow.Application.Interfaces;
using HireFlow.Domain.Shared;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HireFlow.Application.Features.Jobs.Commands.CloseExpiredJobs;

public sealed class CloseExpiredJobsCommandHandler(
    IJobRepository jobRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<CloseExpiredJobsCommandHandler> logger)
    : IRequestHandler<CloseExpiredJobsCommand, Result>
{
    public async Task<Result> Handle(CloseExpiredJobsCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting to close expired jobs older than {DaysOld} days.", request.DaysOld);

        var expiredJobs = await jobRepository.GetExpiredOpenJobsAsync(request.DaysOld, cancellationToken);

        if (expiredJobs.Count == 0)
        {
            logger.LogInformation("No expired jobs found.");
            return Result.Success();
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;

        foreach (var job in expiredJobs)
        {
            job.Close(now);
            jobRepository.Update(job);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully closed {Count} expired jobs.", expiredJobs.Count);

        return Result.Success();
    }
}
