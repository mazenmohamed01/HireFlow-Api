using HireFlow.Application.Common.Messaging;
using HireFlow.Application.DTOs.Profiles;

namespace HireFlow.Application.Features.Profiles.Commands.UpdateCandidateProfile;

/// <summary>Command to update the current candidate's profile.</summary>
public sealed record UpdateCandidateProfileCommand(
    string FullName,
    string? Phone,
    string? CvUrl) : ICommand<CandidateProfileDto>;
