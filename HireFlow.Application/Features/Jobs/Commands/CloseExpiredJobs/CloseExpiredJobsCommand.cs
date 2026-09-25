using HireFlow.Application.Common.Messaging;
using HireFlow.Domain.Shared;

namespace HireFlow.Application.Features.Jobs.Commands.CloseExpiredJobs;

public record CloseExpiredJobsCommand(int DaysOld) : ICommand;
