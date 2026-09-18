namespace HireFlow.Application.Common;

/// <summary>
/// Classifies the nature of an error for HTTP status code mapping.
/// </summary>
public enum ErrorType
{
    /// <summary>Input validation failed. Maps to HTTP 400.</summary>
    Validation,

    /// <summary>Resource not found or not visible to the caller. Maps to HTTP 404.</summary>
    NotFound,

    /// <summary>Business rule conflict. Maps to HTTP 409.</summary>
    Conflict,

    /// <summary>Authentication failure. Maps to HTTP 401.</summary>
    Unauthorized,

    /// <summary>Authorization / ownership failure. Maps to HTTP 403.</summary>
    Forbidden,

    /// <summary>Unexpected server-side failure. Maps to HTTP 500.</summary>
    Failure
}
