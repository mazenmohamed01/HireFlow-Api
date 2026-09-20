using HireFlow.Application.Common.Messaging;

namespace HireFlow.Application.Features.Jobs.Commands.UpdateJob;

/// <summary>Command to update an existing owned job. Recruiter only.</summary>
public sealed record UpdateJobCommand(
    int JobId,
    string Title,
    string Description,
    string? Location,
    string JobType) : ICommand;
