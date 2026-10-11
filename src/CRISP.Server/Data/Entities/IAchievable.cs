namespace CRISP.Server.Data.Entities;

/// <summary>
/// Defines archival metadata for entities that can be removed from normal business views without deletion.
/// </summary>
public interface IAchievable
{
    /// <summary>
    /// Gets or sets a value indicating whether the entity is archived.
    /// </summary>
    bool IsArchived { get; set; }

    /// <summary>
    /// Gets or sets the business reason recorded when the entity was archived.
    /// </summary>
    string? ArchivingReason { get; set; }
}
