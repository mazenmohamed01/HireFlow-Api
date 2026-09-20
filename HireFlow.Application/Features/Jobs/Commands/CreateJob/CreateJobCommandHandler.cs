using HireFlow.Application.Common;
using HireFlow.Application.Interfaces;
using HireFlow.Domain.Entities;
using HireFlow.Domain.Enums;
using HireFlow.Domain.Shared;
using MediatR;

namespace HireFlow.Application.Features.Jobs.Commands.CreateJob;

public sealed class CreateJobCommandHandler(
    IJobRepository jobRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    TimeProvider timeProvider) : IRequestHandler<CreateJobCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateJobCommand command, CancellationToken cancellationToken)
    {
        if (currentUser.RecruiterId is null)
            return Result.Failure<int>(new Error("Auth.Forbidden", "Only recruiters can create jobs.", ErrorType.Forbidden));

        // JobType already validated by CreateJobCommandValidator.
        Enum.TryParse<JobType>(command.JobType, true, out var jobType);

        var jobResult = Job.Create(
            currentUser.RecruiterId.Value,
            command.Title,
            command.Description,
            command.Location,
            jobType,
            timeProvider.GetUtcNow().UtcDateTime);

        if (jobResult.IsFailure)
            return Result.Failure<int>(jobResult.Error!);

        await jobRepository.InsertAsync(jobResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(jobResult.Value.Id);
    }
}
