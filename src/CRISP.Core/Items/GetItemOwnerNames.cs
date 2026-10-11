namespace CRISP.Core.Items;

/// <summary>
/// Represents a query for the distinct item owner names, used to drive a free-text
/// autocomplete sourced from values that already exist in the data.
/// </summary>
[ExportTsInterface]
public sealed record GetItemOwnerNames : IQuery<IEnumerable<AutoComplete>>
{
    /// <summary>
    /// Gets the optional case-insensitive search term used to filter owner names.
    /// </summary>
    public string? SearchTerm { get; init; }
}
