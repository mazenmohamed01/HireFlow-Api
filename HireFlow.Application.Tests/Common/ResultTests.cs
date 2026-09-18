using HireFlow.Application.Common;
using HireFlow.Domain.Shared;
using Shouldly;
using Xunit;


namespace HireFlow.Application.Tests.Common;

/// <summary>
/// Unit tests for the Result pattern types.
/// Acceptance criteria: Phase 1 — Result pattern, covered.
/// </summary>
public sealed class ResultTests
{
    // ── Result (void) ────────────────────────────────────────────────────

    [Fact]
    public void Success_result_IsSuccess_is_true()
    {
        var result = Result.Success();

        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBeNull();
    }

    [Fact]
    public void Failure_result_IsFailure_is_true_and_Error_is_set()
    {
        var error = new Error("Test.Error", "Something failed.", ErrorType.Conflict);

        var result = Result.Failure(error);

        result.IsFailure.ShouldBeTrue();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(error);
    }

    // ── Result<T> ────────────────────────────────────────────────────────

    [Fact]
    public void Generic_success_result_has_value()
    {
        var result = Result.Success(42);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(42);
    }

    [Fact]
    public void Generic_failure_result_has_error()
    {
        var error = new Error("NotFound", "Not found.", ErrorType.NotFound);

        var result = Result.Failure<int>(error);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(error);
    }

    [Fact]
    public void Accessing_Value_on_failure_result_throws()
    {
        var result = Result.Failure<string>(new Error("E", "Err", ErrorType.Failure));

        Should.Throw<InvalidOperationException>(() => _ = result.Value);
    }

    [Fact]
    public void Implicit_conversion_from_value_creates_success_result()
    {
        Result<string> result = "hello";

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe("hello");
    }

    // ── Error record value equality ──────────────────────────────────────

    [Fact]
    public void Errors_with_same_values_are_equal()
    {
        var a = new Error("Code", "Desc", ErrorType.NotFound);
        var b = new Error("Code", "Desc", ErrorType.NotFound);

        a.ShouldBe(b);
    }

    // ── ValidationError ──────────────────────────────────────────────────

    [Fact]
    public void ValidationError_is_of_type_Validation()
    {
        var fieldErrors = new Dictionary<string, string[]>
        {
            ["Email"] = ["Email is required.", "Email must be valid."]
        };
        var error = new ValidationError("Validation.Failed", "Validation failed.", fieldErrors);

        error.Type.ShouldBe(ErrorType.Validation);
        error.Errors["Email"].Length.ShouldBe(2);
    }
}
