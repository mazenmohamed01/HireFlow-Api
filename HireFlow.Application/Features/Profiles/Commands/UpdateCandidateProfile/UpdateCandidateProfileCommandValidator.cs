using FluentValidation;

namespace HireFlow.Application.Features.Profiles.Commands.UpdateCandidateProfile;

public sealed class UpdateCandidateProfileCommandValidator : AbstractValidator<UpdateCandidateProfileCommand>
{
    public UpdateCandidateProfileCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("FullName is required.")
            .MaximumLength(100).WithMessage("FullName must be at most 100 characters.");
    }
}
