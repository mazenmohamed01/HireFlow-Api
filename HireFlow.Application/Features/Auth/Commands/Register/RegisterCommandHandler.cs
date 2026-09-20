using HireFlow.Application.Common;
using HireFlow.Application.Interfaces;
using HireFlow.Domain.Entities;
using HireFlow.Domain.Enums;
using HireFlow.Domain.Shared;
using MediatR;

namespace HireFlow.Application.Features.Auth.Commands.Register;

public sealed class RegisterCommandHandler(
    IUserRepository userRepository,
    ICandidateRepository candidateRepository,
    IRecruiterRepository recruiterRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    TimeProvider timeProvider) : IRequestHandler<RegisterCommand, Result<int>>
{
    public async Task<Result<int>> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        // Role is already validated by RegisterCommandValidator — safe to parse directly.
        Enum.TryParse<UserRole>(command.Role, true, out var role);

        var existingUser = await userRepository.GetByEmailAsync(command.Email, cancellationToken);
        if (existingUser is not null)
            return Result.Failure<int>(new Error("Auth.EmailAlreadyExists", "Email is already registered.", ErrorType.Conflict));

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var hashedPassword = passwordHasher.Hash(command.Password);
        var user = User.Create(command.FullName, command.Email, hashedPassword, role, now);

        await userRepository.InsertAsync(user, cancellationToken);
        // First flush to get the generated User.Id.
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (role == UserRole.Candidate)
        {
            var candidate = Candidate.Create(user.Id, null, null);
            await candidateRepository.InsertAsync(candidate, cancellationToken);
        }
        else
        {
            var recruiter = Recruiter.Create(user.Id, command.CompanyName!);
            await recruiterRepository.InsertAsync(recruiter, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(user.Id);
    }
}
