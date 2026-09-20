using MediatR;
using HireFlow.Domain.Shared;

namespace HireFlow.Application.Common.Messaging;

/// <summary>A command that mutates state and returns a typed result.</summary>
public interface ICommand<TResponse> : IRequest<Result<TResponse>> { }

/// <summary>A command that mutates state and returns a plain Result (no payload).</summary>
public interface ICommand : IRequest<Result> { }
