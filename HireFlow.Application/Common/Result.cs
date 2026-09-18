namespace HireFlow.Application.Common;

/// <summary>
/// Represents the outcome of an operation that may succeed or fail with a typed error.
/// Use <see cref="Result"/> for void operations and <see cref="Result{T}"/> for value-producing ones.
/// </summary>
public class Result
{
    // Protected so Result<T> can call the same constructor chain.
    protected Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>Whether the operation completed successfully.</summary>
    public bool IsSuccess { get; }

    /// <summary>Whether the operation failed.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>The error detail; <c>null</c> when <see cref="IsSuccess"/> is <c>true</c>.</summary>
    public Error? Error { get; }

    // ── Factory methods ──────────────────────────────────────────────────────

    /// <summary>Creates a successful void result.</summary>
    public static Result Success() => new(true, null);

    /// <summary>Creates a failure result with the supplied error.</summary>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>Creates a successful result carrying a value.</summary>
    public static Result<T> Success<T>(T value) => new(value, true, null);

    /// <summary>Creates a failed result of the generic form.</summary>
    public static Result<T> Failure<T>(Error error) => new(default!, false, error);
}

/// <summary>
/// Represents the outcome of an operation that produces a value of type <typeparamref name="T"/>
/// on success.
/// </summary>
/// <typeparam name="T">The value type returned on success.</typeparam>
public sealed class Result<T> : Result
{
    private readonly T _value;

    internal Result(T value, bool isSuccess, Error? error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// The result value. Only valid when <see cref="Result.IsSuccess"/> is <c>true</c>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessed on a failure result.</exception>
    public T Value => IsSuccess
        ? _value
        : throw new InvalidOperationException("Cannot access the value of a failure result.");

    /// <summary>
    /// Allows a value of type <typeparamref name="T"/> to be implicitly wrapped in a successful result.
    /// </summary>
    public static implicit operator Result<T>(T value) => Success(value);
}
