namespace CRISP.Core.Items;

/// <summary>
/// Represents a paged item search query with optional item-specific filters.
/// </summary>
[ExportTsInterface]
public sealed record GetItems : PagedQuery<Item>
{
    /// <summary>
    /// Gets the item name filter.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets the owner filter.
    /// </summary>
    public string? Owner { get; init; }

    /// <summary>
    /// Gets a filter indicating whether active items should be matched.
    /// </summary>
    public bool? IsActive { get; init; }
}
