namespace CRISP.Models
{
    /// <summary>
    /// Base abstract record for entity models that have a unique identifier.
    /// </summary>
    /// <typeparam name="TId">The type of the entity identifier.</typeparam>
    /// <remarks>
    /// This class extends the base model with identity capabilities, making it suitable
    /// for domain entities that need to be uniquely identified and tracked.
    /// Use this as the base class for your entity models to ensure they have consistent
    /// identification properties.
    /// </remarks>
    public abstract record BaseEntityModel<TId> : BaseModel
        where TId : IEqualityComparer<TId>
    {
        /// <summary>
        /// Gets or initializes the unique identifier for this entity.
        /// </summary>
        public TId Id { get; init; } = default!;
    }
}
