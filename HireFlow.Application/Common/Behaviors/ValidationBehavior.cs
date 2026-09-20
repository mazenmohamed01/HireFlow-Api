using FluentValidation;
using MediatR;
using HireFlow.Domain.Shared;

namespace HireFlow.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that runs all registered FluentValidation validators
/// for the incoming request before the handler is invoked.
/// On failure it returns a <see cref="Result"/> with a <see cref="ValidationError"/>
/// instead of throwing an exception.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
            return await next();

        var errors = failures
            .GroupBy(f => f.PropertyName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => g.Select(f => f.ErrorMessage).ToArray(),
                StringComparer.OrdinalIgnoreCase);

        var validationError = new ValidationError(
            "Validation.Failed",
            "One or more validation errors occurred.",
            errors);

        // Safely wrap the failure into whatever Result type TResponse is.
        // Works for both Result and Result<T> because both have an implicit operator.
        return (TResponse)(object)Result.Failure(validationError);
    }
}
