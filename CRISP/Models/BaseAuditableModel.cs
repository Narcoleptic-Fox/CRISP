namespace CRISP.Models
{
    /// <summary>
    /// Base abstract record for entity models that include auditing information.
    /// </summary>
    /// <typeparam name="TId">The type of the entity identifier.</typeparam>
    /// <remarks>
    /// This class extends the base entity model with auditing capabilities, tracking
    /// creation, modification, and archival/deletion status. Use this as the base class
    /// for entities that require a complete audit trail.
    /// </remarks>
    public abstract record BaseAuditableModel<TId> : BaseEntityModel<TId>
        where TId : IEqualityComparer<TId>
    {
        /// <summary>
        /// Gets or initializes the date and time when the entity was created.
        /// </summary>
        public DateTimeOffset Created { get; init; } = DateTimeOffset.UtcNow;
        
        /// <summary>
        /// Gets or initializes the identifier of the user who created the entity.
        /// </summary>
        public string CreatedBy { get; init; } = default!;
        
        /// <summary>
        /// Gets or initializes the date and time when the entity was last modified.
        /// </summary>
        public DateTimeOffset LastModified { get; init; } = DateTimeOffset.UtcNow;
        
        /// <summary>
        /// Gets or initializes the identifier of the user who last modified the entity.
        /// </summary>
        public string LastModifiedBy { get; init; } = default!;
        
        /// <summary>
        /// Gets or initializes a value indicating whether the entity has been archived.
        /// </summary>
        /// <remarks>
        /// Archived entities are typically excluded from standard queries but retained in the system.
        /// </remarks>
        public bool IsArchived { get; init; }
        
        /// <summary>
        /// Gets or initializes a value indicating whether the entity has been marked as deleted.
        /// </summary>
        /// <remarks>
        /// This property enables soft deletion, where entities are marked as deleted instead of
        /// being physically removed from the database.
        /// </remarks>
        public bool IsDeleted { get; init; }
    }
}
