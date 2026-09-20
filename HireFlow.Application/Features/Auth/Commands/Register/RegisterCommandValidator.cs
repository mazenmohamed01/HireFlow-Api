using FluentValidation;

namespace HireFlow.Application.Features.Auth.Commands.Register;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    private static readonly string[] ValidRoles = ["Candidate", "Recruiter"];

    public RegisterCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is not valid.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .Must(r => ValidRoles.Contains(r, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Role must be Candidate or Recruiter.");

        When(x => string.Equals(x.Role, "Recruiter", StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.CompanyName)
                .NotEmpty().WithMessage("CompanyName is required for Recruiters.");
        });
    }
}
