namespace CRISP.Requests
{
    /// <summary>
    /// Base abstract record for command operations that return a specific response type.
    /// </summary>
    /// <typeparam name="TResponse">The type of response expected from this command.</typeparam>
    /// <remarks>
    /// Commands represent write operations that modify system state and return data,
    /// such as creation operations that return identifiers. This is part of the CQRS pattern implementation.
    /// </remarks>
    public abstract record Command<TResponse> : IRequest<TResponse>;
    
    /// <summary>
    /// Base abstract record for command operations that don't return a response.
    /// </summary>
    /// <remarks>
    /// Commands represent write operations that modify system state without returning data,
    /// such as updates or deletions. This is part of the CQRS pattern implementation.
    /// </remarks>
    public abstract record Command : IRequest;
    
    /// <summary>
    /// Represents a command that creates a new entity and returns its identifier.
    /// </summary>
    /// <typeparam name="TId">The type of identifier returned after creation.</typeparam>
    /// <remarks>
    /// Use this for operations that add new entities to the system and need to return
    /// the newly created entity's identifier.
    /// </remarks>
    public abstract record CreateCommand<TId> : Command<TId> where TId : IEqualityComparer<TId>;
    
    /// <summary>
    /// Base abstract record for commands that modify existing entities.
    /// </summary>
    /// <typeparam name="TId">The type of identifier of the entity to modify.</typeparam>
    /// <remarks>
    /// This is the base for commands that update existing entities in the system.
    /// </remarks>
    public abstract record ModifyCommand<TId> : Command where TId : IEqualityComparer<TId>
    {
        /// <summary>
        /// Gets or initializes the identifier of the entity to modify.
        /// </summary>
        public TId Id { get; init; } = default!;
    }
    
    /// <summary>
    /// Represents a command that permanently removes an entity from the system.
    /// </summary>
    /// <typeparam name="TId">The type of identifier of the entity to delete.</typeparam>
    /// <remarks>
    /// Use this for operations that permanently delete data from the system.
    /// Consider using <see cref="ArchiveCommand{TId}"/> instead for soft-delete operations.
    /// </remarks>
    public abstract record DeleteCommand<TId> : ModifyCommand<TId> where TId : IEqualityComparer<TId>;
    
    /// <summary>
    /// Represents a command that marks an entity as archived (soft-delete).
    /// </summary>
    /// <typeparam name="TId">The type of identifier of the entity to archive.</typeparam>
    /// <remarks>
    /// Use this for operations that logically remove entities from active use
    /// without permanently deleting their data from the system.
    /// </remarks>
    public abstract record ArchiveCommand<TId> : ModifyCommand<TId> where TId : IEqualityComparer<TId>;
}
