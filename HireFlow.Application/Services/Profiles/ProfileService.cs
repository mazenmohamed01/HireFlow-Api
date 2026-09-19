using HireFlow.Application.Common;
using HireFlow.Application.DTOs.Profiles;
using HireFlow.Application.Interfaces;
using HireFlow.Domain.Enums;
using HireFlow.Domain.Shared;

namespace HireFlow.Application.Services.Profiles;

public sealed class ProfileService(
    IUserRepository userRepository,
    ICandidateRepository candidateRepository,
    IRecruiterRepository recruiterRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser) : IProfileService
{
    public async Task<Result<CandidateProfileDto>> GetCandidateProfileAsync(CancellationToken cancellationToken = default)
    {
        if (currentUser.Role != UserRole.Candidate.ToString() || currentUser.CandidateId == null)
            return Result.Failure<CandidateProfileDto>(new Error("Auth.Forbidden", "Only candidates can access this profile.", ErrorType.Forbidden));

        var user = await userRepository.GetByIdWithProfilesAsync(currentUser.UserId, cancellationToken);
        if (user == null || user.Candidate == null)
            return Result.Failure<CandidateProfileDto>(new Error("Candidate.NotFound", "Candidate profile not found.", ErrorType.NotFound));

        return Result.Success(new CandidateProfileDto(
            user.Candidate.Id,
            user.FullName,
            user.Email,
            user.Candidate.Phone,
            user.Candidate.CvUrl));
    }

    public async Task<Result<CandidateProfileDto>> UpdateCandidateProfileAsync(UpdateCandidateProfileRequest request, CancellationToken cancellationToken = default)
    {
        if (currentUser.Role != UserRole.Candidate.ToString() || currentUser.CandidateId == null)
            return Result.Failure<CandidateProfileDto>(new Error("Auth.Forbidden", "Only candidates can update this profile.", ErrorType.Forbidden));

        if (string.IsNullOrWhiteSpace(request.FullName))
            return Result.Failure<CandidateProfileDto>(new ValidationError("Validation.Failed", "Validation failed", new Dictionary<string, string[]> { { "FullName", new[] { "FullName is required." } } }));

        var user = await userRepository.GetByIdWithProfilesAsync(currentUser.UserId, cancellationToken);
        if (user == null || user.Candidate == null)
            return Result.Failure<CandidateProfileDto>(new Error("Candidate.NotFound", "Candidate profile not found.", ErrorType.NotFound));

        user.UpdateFullName(request.FullName);
        user.Candidate.UpdateProfile(request.CvUrl, request.Phone);

        userRepository.Update(user);
        candidateRepository.Update(user.Candidate);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CandidateProfileDto(
            user.Candidate.Id,
            user.FullName,
            user.Email,
            user.Candidate.Phone,
            user.Candidate.CvUrl));
    }

    public async Task<Result<RecruiterProfileDto>> GetRecruiterProfileAsync(CancellationToken cancellationToken = default)
    {
        if (currentUser.Role != UserRole.Recruiter.ToString() || currentUser.RecruiterId == null)
            return Result.Failure<RecruiterProfileDto>(new Error("Auth.Forbidden", "Only recruiters can access this profile.", ErrorType.Forbidden));

        var user = await userRepository.GetByIdWithProfilesAsync(currentUser.UserId, cancellationToken);
        if (user == null || user.Recruiter == null)
            return Result.Failure<RecruiterProfileDto>(new Error("Recruiter.NotFound", "Recruiter profile not found.", ErrorType.NotFound));

        return Result.Success(new RecruiterProfileDto(
            user.Recruiter.Id,
            user.FullName,
            user.Email,
            user.Recruiter.CompanyName));
    }

    public async Task<Result<RecruiterProfileDto>> UpdateRecruiterProfileAsync(UpdateRecruiterProfileRequest request, CancellationToken cancellationToken = default)
    {
        if (currentUser.Role != UserRole.Recruiter.ToString() || currentUser.RecruiterId == null)
            return Result.Failure<RecruiterProfileDto>(new Error("Auth.Forbidden", "Only recruiters can update this profile.", ErrorType.Forbidden));

        if (string.IsNullOrWhiteSpace(request.FullName))
            return Result.Failure<RecruiterProfileDto>(new ValidationError("Validation.Failed", "Validation failed", new Dictionary<string, string[]> { { "FullName", new[] { "FullName is required." } } }));

        if (string.IsNullOrWhiteSpace(request.CompanyName))
            return Result.Failure<RecruiterProfileDto>(new ValidationError("Validation.Failed", "Validation failed", new Dictionary<string, string[]> { { "CompanyName", new[] { "CompanyName is required." } } }));

        var user = await userRepository.GetByIdWithProfilesAsync(currentUser.UserId, cancellationToken);
        if (user == null || user.Recruiter == null)
            return Result.Failure<RecruiterProfileDto>(new Error("Recruiter.NotFound", "Recruiter profile not found.", ErrorType.NotFound));

        user.UpdateFullName(request.FullName);
        user.Recruiter.UpdateCompanyName(request.CompanyName);

        userRepository.Update(user);
        recruiterRepository.Update(user.Recruiter);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new RecruiterProfileDto(
            user.Recruiter.Id,
            user.FullName,
            user.Email,
            user.Recruiter.CompanyName));
    }
}
