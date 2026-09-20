using HireFlow.Application.Common;
using HireFlow.Application.Interfaces;
using HireFlow.Domain.Enums;
using HireFlow.Domain.Shared;
using MediatR;

namespace HireFlow.Application.Features.Applications.Commands.ChangeApplicationStatus;

public sealed class ChangeApplicationStatusCommandHandler(
    IJobApplicationRepository applicationRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    TimeProvider timeProvider) : IRequestHandler<ChangeApplicationStatusCommand, Result>
{
    public async Task<Result> Handle(ChangeApplicationStatusCommand command, CancellationToken cancellationToken)
    {
        if (currentUser.RecruiterId is null)
            return Result.Failure(new Error("Auth.Forbidden", "Only recruiters can change application status.", ErrorType.Forbidden));

        var application = await applicationRepository.GetByIdWithJobAsync(command.ApplicationId, cancellationToken);
        if (application is null || application.Job.RecruiterId != currentUser.RecruiterId.Value)
            return Result.Failure(new Error("Application.NotFound", "Application not found.", ErrorType.NotFound));

        // Status already validated by ChangeApplicationStatusCommandValidator.
        Enum.TryParse<ApplicationStatus>(command.Status, true, out var newStatus);

        var changeResult = application.ChangeStatusByRecruiter(newStatus, timeProvider.GetUtcNow().UtcDateTime);
        if (changeResult.IsFailure)
            return changeResult;

        applicationRepository.Update(application);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
