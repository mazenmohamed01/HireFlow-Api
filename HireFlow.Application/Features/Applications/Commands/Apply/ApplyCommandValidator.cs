using FluentValidation;

namespace HireFlow.Application.Features.Applications.Commands.Apply;

public sealed class ApplyCommandValidator : AbstractValidator<ApplyCommand>
{
    public ApplyCommandValidator()
    {
        RuleFor(x => x.JobId)
            .GreaterThan(0).WithMessage("JobId must be a positive integer.");
    }
}
