using HireFlow.Application.Common.Messaging;

namespace HireFlow.Application.Features.Jobs.Commands.ReopenJob;

/// <summary>Command to reopen a closed job. Recruiter only.</summary>
public sealed record ReopenJobCommand(int JobId) : ICommand;
