using FluentValidation;

namespace HireFlow.Application.Common;

/// <summary>
/// Converts a FluentValidation result into a <see cref="Result"/>, without throwing exceptions.
/// Call this at the very start of each service method that accepts a request DTO.
/// </summary>
public static class ValidationHelper
{
    private const string DefaultCode = "Validation.Failed";
    private const string DefaultDescription = "One or more validation errors occurred.";

    /// <summary>
    /// Validates <paramref name="request"/> using <paramref name="validator"/> and returns
    /// a failed <see cref="Result"/> carrying a <see cref="ValidationError"/> on failure,
    /// or <see cref="Result.Success()"/> on success.
    /// </summary>
    public static Result Validate<TRequest>(
        IValidator<TRequest> validator,
        TRequest request)
    {
        var validationResult = validator.Validate(request);

        if (validationResult.IsValid)
            return Result.Success();

        var errors = validationResult.Errors
            .GroupBy(f => f.PropertyName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => g.Select(f => f.ErrorMessage).ToArray(),
                StringComparer.OrdinalIgnoreCase);

        return Result.Failure(new ValidationError(DefaultCode, DefaultDescription, errors));
    }
}
