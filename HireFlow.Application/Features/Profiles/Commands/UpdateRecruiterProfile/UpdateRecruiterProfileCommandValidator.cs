using FluentValidation;

namespace HireFlow.Application.Features.Profiles.Commands.UpdateRecruiterProfile;

public sealed class UpdateRecruiterProfileCommandValidator : AbstractValidator<UpdateRecruiterProfileCommand>
{
    public UpdateRecruiterProfileCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("FullName is required.")
            .MaximumLength(100).WithMessage("FullName must be at most 100 characters.");

        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("CompanyName is required.")
            .MaximumLength(200).WithMessage("CompanyName must be at most 200 characters.");
    }
}
