using HireFlow.Application.Common.Messaging;
using HireFlow.Application.DTOs.Auth;

namespace HireFlow.Application.Features.Auth.Queries.Login;

/// <summary>Query to authenticate a user and return a signed JWT.</summary>
public sealed record LoginQuery(string Email, string Password) : IQuery<AuthResponse>;
