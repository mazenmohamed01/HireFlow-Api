using HireFlow.Application.Common;
using HireFlow.Application.Interfaces;
using HireFlow.Domain.Shared;
using MediatR;

namespace HireFlow.Application.Features.Jobs.Commands.ReopenJob;

public sealed class ReopenJobCommandHandler(
    IJobRepository jobRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    TimeProvider timeProvider) : IRequestHandler<ReopenJobCommand, Result>
{
    public async Task<Result> Handle(ReopenJobCommand command, CancellationToken cancellationToken)
    {
        if (currentUser.RecruiterId is null)
            return Result.Failure(new Error("Job.NotFound", "Job not found.", ErrorType.NotFound));

        var job = await jobRepository.GetByIdAsync(command.JobId, cancellationToken);
        if (job is null || job.RecruiterId != currentUser.RecruiterId.Value)
            return Result.Failure(new Error("Job.NotFound", "Job not found.", ErrorType.NotFound));

        var reopenResult = job.Reopen(timeProvider.GetUtcNow().UtcDateTime);
        if (reopenResult.IsFailure)
            return reopenResult;

        jobRepository.Update(job);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
