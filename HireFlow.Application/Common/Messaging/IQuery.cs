using MediatR;
using HireFlow.Domain.Shared;

namespace HireFlow.Application.Common.Messaging;

/// <summary>A query that reads state only and returns a typed result. Must not mutate state.</summary>
public interface IQuery<TResponse> : IRequest<Result<TResponse>> { }
