using HireFlow.Application.Common.Messaging;

namespace HireFlow.Application.Features.Jobs.Commands.CreateJob;

/// <summary>Command to create a new job posting. Recruiter only.</summary>
public sealed record CreateJobCommand(
    string Title,
    string Description,
    string? Location,
    string JobType) : ICommand<int>;
