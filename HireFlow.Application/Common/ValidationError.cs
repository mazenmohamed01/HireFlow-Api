namespace HireFlow.Application.Common;

/// <summary>
/// Specialised error for FluentValidation failures. Carries a per-field error dictionary
/// that the API serialises into the <c>errors</c> extension of ProblemDetails.
/// </summary>
public sealed record ValidationError(
    string Code,
    string Description,
    IReadOnlyDictionary<string, string[]> Errors)
    : Error(Code, Description, ErrorType.Validation);
