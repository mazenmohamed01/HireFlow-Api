using HireFlow.Application.Common;
using HireFlow.Application.Interfaces;
using HireFlow.Domain.Entities;
using HireFlow.Domain.Shared;
using MediatR;

namespace HireFlow.Application.Features.Applications.Commands.Apply;

public sealed class ApplyCommandHandler(
    IJobApplicationRepository applicationRepository,
    IJobRepository jobRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    TimeProvider timeProvider) : IRequestHandler<ApplyCommand, Result<int>>
{
    public async Task<Result<int>> Handle(ApplyCommand command, CancellationToken cancellationToken)
    {
        if (currentUser.CandidateId is null)
            return Result.Failure<int>(new Error("Auth.Forbidden", "Only candidates can apply for jobs.", ErrorType.Forbidden));

        var job = await jobRepository.GetByIdAsync(command.JobId, cancellationToken);
        if (job is null)
            return Result.Failure<int>(new Error("Job.NotFound", "Job not found.", ErrorType.NotFound));

        if (!job.IsOpen)
            return Result.Failure<int>(new Error("Job.Closed", "Cannot apply to a closed job.", ErrorType.Conflict));

        var candidateUser = await userRepository.GetByIdWithProfilesAsync(currentUser.UserId, cancellationToken);
        var profileCvUrl = candidateUser?.Candidate?.CvUrl;

        var applicationResult = JobApplication.Create(
            currentUser.CandidateId.Value,
            job,
            command.CvUrl,
            profileCvUrl,
            command.CoverLetter,
            timeProvider.GetUtcNow().UtcDateTime);

        if (applicationResult.IsFailure)
            return Result.Failure<int>(applicationResult.Error!);

        await applicationRepository.InsertAsync(applicationResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(applicationResult.Value.Id);
    }
}
