namespace HireFlow.Application.Common;

/// <summary>
/// Query parameters for paginated list endpoints.
/// <see cref="PageSize"/> is clamped to [1, 50] per the plan API contract.
/// </summary>
public sealed record PaginationParams
{
    private const int MinPageSize = 1;
    private const int MaxPageSize = 50;
    private const int DefaultPageSize = 10;

    private int _page = 1;
    private int _pageSize = DefaultPageSize;

    /// <summary>1-based page number (minimum 1).</summary>
    public int Page
    {
        get => _page;
        init => _page = value < 1 ? 1 : value;
    }

    /// <summary>Items per page; clamped to [1, 50].</summary>
    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = Math.Clamp(value, MinPageSize, MaxPageSize);
    }

    /// <summary>Number of records to skip for the current page.</summary>
    public int Skip => (Page - 1) * PageSize;
}
