using HireFlow.Application.Common;
using HireFlow.Application.Interfaces;
using HireFlow.Domain.Shared;
using MediatR;

namespace HireFlow.Application.Features.Applications.Commands.CancelApplication;

public sealed class CancelApplicationCommandHandler(
    IJobApplicationRepository applicationRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    TimeProvider timeProvider) : IRequestHandler<CancelApplicationCommand, Result>
{
    public async Task<Result> Handle(CancelApplicationCommand command, CancellationToken cancellationToken)
    {
        if (currentUser.CandidateId is null)
            return Result.Failure(new Error("Auth.Forbidden", "Only candidates can cancel applications.", ErrorType.Forbidden));

        var application = await applicationRepository.GetByIdAsync(command.ApplicationId, cancellationToken);
        if (application is null || application.CandidateId != currentUser.CandidateId.Value)
            return Result.Failure(new Error("Application.NotFound", "Application not found.", ErrorType.NotFound));

        var cancelResult = application.Cancel(timeProvider.GetUtcNow().UtcDateTime);
        if (cancelResult.IsFailure)
            return cancelResult;

        applicationRepository.Update(application);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
