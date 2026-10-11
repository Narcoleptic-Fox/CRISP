namespace CRISP.Server.Data.Entities;

/// <summary>
/// Defines soft-delete state for entities that should be hidden rather than physically removed.
/// </summary>
public interface ISoftDelete
{
    /// <summary>
    /// Gets or sets a value indicating whether the entity has been soft deleted.
    /// </summary>
    bool IsDeleted { get; set; }
}
