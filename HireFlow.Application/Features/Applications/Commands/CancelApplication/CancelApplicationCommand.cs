using HireFlow.Application.Common.Messaging;

namespace HireFlow.Application.Features.Applications.Commands.CancelApplication;

/// <summary>Command to cancel a candidate's own application.</summary>
public sealed record CancelApplicationCommand(int ApplicationId) : ICommand;
