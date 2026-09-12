using CRISP.Core.Items;
using CRISP.Server.Data.ValueObjects;

namespace CRISP.Server.Data.Entities;

/// <summary>
/// Represents the persisted form of an <see cref="Item"/>, including audit state,
/// soft-delete support, and attached file metadata.
/// </summary>
public class ItemEntity : BaseAuditableEntity, IHasStoredFiles
{
    /// <summary>
    /// Gets or sets the item display name.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the optional free-text description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the free-text owner name.
    /// </summary>
    public string? Owner { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the item is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the files attached to the item.
    /// </summary>
    public List<StoredFileObject> Files { get; set; } = [];

    /// <summary>
    /// Converts the persisted entity into the Core item contract, including attached files.
    /// </summary>
    /// <returns>The Core item model.</returns>
    public Item ToItem() => new()
    {
        Id = Id,
        Name = Name,
        Description = Description,
        Owner = Owner,
        IsActive = IsActive,
        CreatedOn = CreatedOn,
        CreatedBy = CreatedBy,
        UpdatedOn = UpdatedOn,
        UpdatedBy = UpdatedBy,
        Files = [.. Files.Select(f => f.ToStoredFile())]
    };
}

/// <summary>
/// Provides projection helpers for item queries.
/// </summary>
public static class ItemExtensions
{
    extension(IQueryable<ItemEntity> entities)
    {
        /// <summary>
        /// Projects item entities to Core item models in a queryable expression.
        /// </summary>
        /// <remarks>
        /// Attached files are intentionally omitted from the list projection; load a single
        /// item via <see cref="ItemEntity.ToItem"/> when the file collection is needed.
        /// </remarks>
        /// <returns>A queryable projection to <see cref="Item"/>.</returns>
        public IQueryable<Item> ProjectToItem() =>
            entities.Select(e => new Item
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                Owner = e.Owner,
                IsActive = e.IsActive,
                CreatedOn = e.CreatedOn,
                CreatedBy = e.CreatedBy,
                UpdatedOn = e.UpdatedOn,
                UpdatedBy = e.UpdatedBy
            });
    }
}
