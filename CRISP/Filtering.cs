using System.Reflection;

namespace CRISP;

/// <summary>
/// Base class for query filtering parameters.
/// </summary>
public abstract class FilterBase
{
    /// <summary>
    /// Gets or sets the page number (1-based).
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Gets or sets the page size. (default is int.MaxValue).
    /// </summary>
    public int PageSize { get; set; } = int.MaxValue;

    /// <summary>
    /// Gets or sets the sort field.
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to sort in descending order.
    /// </summary>
    public bool SortDescending { get; set; }

    public virtual string ToString()
    {
        PropertyInfo[] properties = GetType().GetProperties();
        string filterString = string.Join("&", properties.Select(p =>
        {
            object? value = p.GetValue(this);
            return value != null ? $"{p.Name}={value}" : null;
        }).Where(x => x != null));
        return filterString;
    }
}

/// <summary>
/// Represents a search filter.
/// </summary>
public class SearchFilter : FilterBase
{
    /// <summary>
    /// Gets or sets the search term.
    /// </summary>
    public string? Search { get; set; }
    public override string ToString()
    {
        string filterString = base.ToString();
        if (!string.IsNullOrEmpty(Search))
        {
            filterString += $"&Search={Search}";
        }
        return filterString;
    }
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

    public override string ToString()
    {
        string filterString = base.ToString();
        if (StartDate.HasValue)
        {
            filterString += $"&StartDate={StartDate.Value:yyyy-MM-dd}";
        }
        if (EndDate.HasValue)
        {
            filterString += $"&EndDate={EndDate.Value:yyyy-MM-dd}";
        }
        return filterString;
    }
}