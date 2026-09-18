using HireFlow.Application.Common;
using Shouldly;
using Xunit;


namespace HireFlow.Application.Tests.Common;

/// <summary>
/// Unit tests for <see cref="PaginationParams"/> clamping behaviour.
/// Acceptance criteria: Phase 1 — pagination clamping covered.
/// </summary>
public sealed class PaginationParamsTests
{
    [Theory]
    [InlineData(0,  1)]   // below minimum → clamped to 1
    [InlineData(-5, 1)]   // negative → clamped to 1
    [InlineData(1,  1)]   // exactly minimum → unchanged
    [InlineData(25, 25)]  // mid-range → unchanged
    [InlineData(50, 50)]  // exactly maximum → unchanged
    [InlineData(51, 50)]  // above maximum → clamped to 50
    [InlineData(100, 50)] // well above maximum → clamped to 50
    public void PageSize_is_clamped_to_1_to_50(int input, int expected)
    {
        var p = new PaginationParams { PageSize = input };

        p.PageSize.ShouldBe(expected);
    }

    [Theory]
    [InlineData(0,  1)]  // zero page → clamped to 1
    [InlineData(-1, 1)]  // negative page → clamped to 1
    [InlineData(1,  1)]  // page 1 → unchanged
    [InlineData(5,  5)]  // positive → unchanged
    public void Page_below_1_is_clamped_to_1(int input, int expected)
    {
        var p = new PaginationParams { Page = input };

        p.Page.ShouldBe(expected);
    }

    [Theory]
    [InlineData(1, 10,  0)]  // page 1, skip 0
    [InlineData(2, 10, 10)]  // page 2, skip 10
    [InlineData(3, 25, 50)]  // page 3 of 25, skip 50
    public void Skip_is_correct_for_given_page_and_pageSize(int page, int pageSize, int expectedSkip)
    {
        var p = new PaginationParams { Page = page, PageSize = pageSize };

        p.Skip.ShouldBe(expectedSkip);
    }

    [Fact]
    public void Default_page_is_1_and_default_pageSize_is_10()
    {
        var p = new PaginationParams();

        p.Page.ShouldBe(1);
        p.PageSize.ShouldBe(10);
    }
}
