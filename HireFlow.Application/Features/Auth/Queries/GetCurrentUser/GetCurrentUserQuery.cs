using HireFlow.Application.Common.Messaging;
using HireFlow.Application.DTOs.Auth;

namespace HireFlow.Application.Features.Auth.Queries.GetCurrentUser;

/// <summary>Query to return the currently authenticated user's profile.</summary>
public sealed record GetCurrentUserQuery : IQuery<UserDto>;
