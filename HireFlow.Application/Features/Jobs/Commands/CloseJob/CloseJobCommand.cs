using HireFlow.Application.Common.Messaging;

namespace HireFlow.Application.Features.Jobs.Commands.CloseJob;

/// <summary>Command to close an open job. Recruiter only.</summary>
public sealed record CloseJobCommand(int JobId) : ICommand;
