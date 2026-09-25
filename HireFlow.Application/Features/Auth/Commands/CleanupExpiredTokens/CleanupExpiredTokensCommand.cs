using HireFlow.Application.Common.Messaging;
using HireFlow.Domain.Shared;

namespace HireFlow.Application.Features.Auth.Commands.CleanupExpiredTokens;

public record CleanupExpiredTokensCommand() : ICommand;
