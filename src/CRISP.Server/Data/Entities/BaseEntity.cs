namespace CRISP.Server.Data.Entities
{
    /// <summary>
    /// Provides the base identifier for persisted entities.
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        public Guid Id { get; set; }
    }

    /// <summary>
    /// Provides audit, archival, and soft-delete fields for persisted entities.
    /// </summary>
    public abstract class BaseAuditableEntity : BaseEntity, IAchievable, ISoftDelete
    {
        /// <summary>
        /// Gets or sets the UTC timestamp when the entity was created.
        /// </summary>
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the user that created the entity.
        /// </summary>
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the UTC timestamp when the entity was last updated.
        /// </summary>
        public DateTime UpdatedOn { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the user that last updated the entity.
        /// </summary>
        public string UpdatedBy { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the entity has been archived.
        /// </summary>
        public bool IsArchived { get; set; }

        /// <summary>
        /// Gets or sets the business reason recorded when the entity was archived.
        /// </summary>
        public string? ArchivingReason { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity has been soft deleted.
        /// </summary>
        public bool IsDeleted { get; set; }
    }
}
