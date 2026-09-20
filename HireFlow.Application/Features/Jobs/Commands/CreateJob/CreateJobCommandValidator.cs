using FluentValidation;
using HireFlow.Domain.Enums;

namespace HireFlow.Application.Features.Jobs.Commands.CreateJob;

public sealed class CreateJobCommandValidator : AbstractValidator<CreateJobCommand>
{
    public CreateJobCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must be at most 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.");

        RuleFor(x => x.JobType)
            .NotEmpty().WithMessage("JobType is required.")
            .Must(jt => Enum.TryParse<JobType>(jt, true, out _))
            .WithMessage("JobType must be FullTime, PartTime, or Contract.");
    }
}
