using HireFlow.Application.Common.Messaging;

namespace HireFlow.Application.Features.Applications.Commands.Apply;

/// <summary>Command to submit a new job application. Candidate only.</summary>
public sealed record ApplyCommand(
    int JobId,
    string? CvUrl,
    string? CoverLetter) : ICommand<int>;
