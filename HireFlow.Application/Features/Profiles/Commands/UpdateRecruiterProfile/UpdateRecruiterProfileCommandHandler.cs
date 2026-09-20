using HireFlow.Application.Common;
using HireFlow.Application.DTOs.Profiles;
using HireFlow.Application.Interfaces;
using HireFlow.Domain.Enums;
using HireFlow.Domain.Shared;
using MediatR;

namespace HireFlow.Application.Features.Profiles.Commands.UpdateRecruiterProfile;

public sealed class UpdateRecruiterProfileCommandHandler(
    IUserRepository userRepository,
    IRecruiterRepository recruiterRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser) : IRequestHandler<UpdateRecruiterProfileCommand, Result<RecruiterProfileDto>>
{
    public async Task<Result<RecruiterProfileDto>> Handle(UpdateRecruiterProfileCommand command, CancellationToken cancellationToken)
    {
        if (currentUser.Role != UserRole.Recruiter.ToString() || currentUser.RecruiterId is null)
            return Result.Failure<RecruiterProfileDto>(new Error("Auth.Forbidden", "Only recruiters can update this profile.", ErrorType.Forbidden));

        var user = await userRepository.GetByIdWithProfilesAsync(currentUser.UserId, cancellationToken);
        if (user is null || user.Recruiter is null)
            return Result.Failure<RecruiterProfileDto>(new Error("Recruiter.NotFound", "Recruiter profile not found.", ErrorType.NotFound));

        user.UpdateFullName(command.FullName);
        user.Recruiter.UpdateCompanyName(command.CompanyName);

        userRepository.Update(user);
        recruiterRepository.Update(user.Recruiter);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new RecruiterProfileDto(
            user.Recruiter.Id, user.FullName, user.Email, user.Recruiter.CompanyName));
    }
}
