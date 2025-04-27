namespace CRISP.Core.Abstractions;

/// <summary>
/// Base class for query filtering parameters.
/// </summary>
public abstract class FilterBase
{
    /// <summary>
    /// Gets or sets the page number (1-based).
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Gets or sets the page size.
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Gets or sets the sort field.
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to sort in descending order.
    /// </summary>
    public bool SortDescending { get; set; }
}

/// <summary>
/// Represents a search filter.
/// </summary>
public class SearchFilter : FilterBase
{
    /// <summary>
    /// Gets or sets the search term.
    /// </summary>
    public string? SearchTerm { get; set; }
}

/// <summary>
/// Represents a date range filter.
/// </summary>
public class DateRangeFilter : FilterBase
{
    /// <summary>
    /// Gets or sets the start date.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Gets or sets the end date.
    /// </summary>
    public DateTime? EndDate { get; set; }
}