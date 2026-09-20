using HireFlow.Application.Common.Messaging;

namespace HireFlow.Application.Features.Auth.Commands.Register;

/// <summary>Command to register a new Candidate or Recruiter user.</summary>
public sealed record RegisterCommand(
    string FullName,
    string Email,
    string Password,
    string Role,
    string? CompanyName) : ICommand<int>;
