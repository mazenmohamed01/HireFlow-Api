using FluentValidation;
using HireFlow.Domain.Enums;

namespace HireFlow.Application.Features.Applications.Commands.ChangeApplicationStatus;

public sealed class ChangeApplicationStatusCommandValidator : AbstractValidator<ChangeApplicationStatusCommand>
{
    public ChangeApplicationStatusCommandValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(s => Enum.TryParse<ApplicationStatus>(s, true, out _))
            .WithMessage("Invalid application status. Valid values: Accepted, Rejected.");
    }
}
