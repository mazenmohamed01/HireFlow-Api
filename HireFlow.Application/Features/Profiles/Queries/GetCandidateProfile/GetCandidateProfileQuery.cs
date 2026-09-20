using HireFlow.Application.Common.Messaging;
using HireFlow.Application.DTOs.Profiles;

namespace HireFlow.Application.Features.Profiles.Queries.GetCandidateProfile;

/// <summary>Query to return the current candidate's profile.</summary>
public sealed record GetCandidateProfileQuery : IQuery<CandidateProfileDto>;
