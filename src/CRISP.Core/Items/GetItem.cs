namespace CRISP.Core.Items;

/// <summary>
/// Represents a query for retrieving a single item by identifier.
/// </summary>
public sealed record GetItem : SingularQuery<Item>;
