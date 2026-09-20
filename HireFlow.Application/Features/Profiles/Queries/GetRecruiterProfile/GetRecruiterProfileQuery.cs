using HireFlow.Application.Common.Messaging;
using HireFlow.Application.DTOs.Profiles;

namespace HireFlow.Application.Features.Profiles.Queries.GetRecruiterProfile;

/// <summary>Query to return the current recruiter's profile.</summary>
public sealed record GetRecruiterProfileQuery : IQuery<RecruiterProfileDto>;
