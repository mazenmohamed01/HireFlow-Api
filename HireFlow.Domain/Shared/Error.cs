namespace HireFlow.Domain.Shared;

/// <summary>
/// Represents a domain or application error with a machine-readable code.
/// Defined as a record so static catalogue members can be compared by value.
/// </summary>
public record Error(string Code, string Description, ErrorType Type)
{
    /// <summary>Sentinel value used as the error field of a successful <see cref="Result"/>.</summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);
}
