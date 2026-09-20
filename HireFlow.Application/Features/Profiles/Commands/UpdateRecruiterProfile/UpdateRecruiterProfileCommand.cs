using HireFlow.Application.Common.Messaging;
using HireFlow.Application.DTOs.Profiles;

namespace HireFlow.Application.Features.Profiles.Commands.UpdateRecruiterProfile;

/// <summary>Command to update the current recruiter's profile.</summary>
public sealed record UpdateRecruiterProfileCommand(
    string FullName,
    string CompanyName) : ICommand<RecruiterProfileDto>;
