using HireFlow.Application.Common.Messaging;

namespace HireFlow.Application.Features.Applications.Commands.ChangeApplicationStatus;

/// <summary>Command to change an application's status. Recruiter only.</summary>
public sealed record ChangeApplicationStatusCommand(
    int ApplicationId,
    string Status) : ICommand;
