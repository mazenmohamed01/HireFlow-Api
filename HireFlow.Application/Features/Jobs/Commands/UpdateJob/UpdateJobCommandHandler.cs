using HireFlow.Application.Common;
using HireFlow.Application.Interfaces;
using HireFlow.Domain.Enums;
using HireFlow.Domain.Shared;
using MediatR;

namespace HireFlow.Application.Features.Jobs.Commands.UpdateJob;

public sealed class UpdateJobCommandHandler(
    IJobRepository jobRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    TimeProvider timeProvider) : IRequestHandler<UpdateJobCommand, Result>
{
    public async Task<Result> Handle(UpdateJobCommand command, CancellationToken cancellationToken)
    {
        if (currentUser.RecruiterId is null)
            return Result.Failure(new Error("Job.NotFound", "Job not found.", ErrorType.NotFound));

        var job = await jobRepository.GetByIdAsync(command.JobId, cancellationToken);
        if (job is null || job.RecruiterId != currentUser.RecruiterId.Value)
            return Result.Failure(new Error("Job.NotFound", "Job not found.", ErrorType.NotFound));

        // JobType already validated by UpdateJobCommandValidator.
        Enum.TryParse<JobType>(command.JobType, true, out var jobType);

        var updateResult = job.Update(command.Title, command.Description, command.Location, jobType, timeProvider.GetUtcNow().UtcDateTime);
        if (updateResult.IsFailure)
            return updateResult;

        jobRepository.Update(job);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
