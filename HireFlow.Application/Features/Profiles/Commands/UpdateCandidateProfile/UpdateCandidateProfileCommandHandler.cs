using HireFlow.Application.Common;
using HireFlow.Application.DTOs.Profiles;
using HireFlow.Application.Interfaces;
using HireFlow.Domain.Enums;
using HireFlow.Domain.Shared;
using MediatR;

namespace HireFlow.Application.Features.Profiles.Commands.UpdateCandidateProfile;

public sealed class UpdateCandidateProfileCommandHandler(
    IUserRepository userRepository,
    ICandidateRepository candidateRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser) : IRequestHandler<UpdateCandidateProfileCommand, Result<CandidateProfileDto>>
{
    public async Task<Result<CandidateProfileDto>> Handle(UpdateCandidateProfileCommand command, CancellationToken cancellationToken)
    {
        if (currentUser.Role != UserRole.Candidate.ToString() || currentUser.CandidateId is null)
            return Result.Failure<CandidateProfileDto>(new Error("Auth.Forbidden", "Only candidates can update this profile.", ErrorType.Forbidden));

        var user = await userRepository.GetByIdWithProfilesAsync(currentUser.UserId, cancellationToken);
        if (user is null || user.Candidate is null)
            return Result.Failure<CandidateProfileDto>(new Error("Candidate.NotFound", "Candidate profile not found.", ErrorType.NotFound));

        user.UpdateFullName(command.FullName);
        user.Candidate.UpdateProfile(command.CvUrl, command.Phone);

        userRepository.Update(user);
        candidateRepository.Update(user.Candidate);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CandidateProfileDto(
            user.Candidate.Id, user.FullName, user.Email, user.Candidate.Phone, user.Candidate.CvUrl));
    }
}
