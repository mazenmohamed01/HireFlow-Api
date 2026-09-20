using HireFlow.Application.Common;
using HireFlow.Application.Interfaces;
using HireFlow.Domain.Shared;
using MediatR;

namespace HireFlow.Application.Features.Jobs.Commands.CloseJob;

public sealed class CloseJobCommandHandler(
    IJobRepository jobRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    TimeProvider timeProvider) : IRequestHandler<CloseJobCommand, Result>
{
    public async Task<Result> Handle(CloseJobCommand command, CancellationToken cancellationToken)
    {
        if (currentUser.RecruiterId is null)
            return Result.Failure(new Error("Job.NotFound", "Job not found.", ErrorType.NotFound));

        var job = await jobRepository.GetByIdAsync(command.JobId, cancellationToken);
        if (job is null || job.RecruiterId != currentUser.RecruiterId.Value)
            return Result.Failure(new Error("Job.NotFound", "Job not found.", ErrorType.NotFound));

        var closeResult = job.Close(timeProvider.GetUtcNow().UtcDateTime);
        if (closeResult.IsFailure)
            return closeResult;

        jobRepository.Update(job);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
