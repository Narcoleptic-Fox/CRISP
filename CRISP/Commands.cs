namespace CRISP;

/// <summary>  
/// Base class for commands that do not return data.  
/// </summary>  
public abstract record Command : IRequest<Response>
{
}

/// <summary>  
/// Base class for commands that return data of type <typeparamref name="TResult"/>.  
/// </summary>  
/// <typeparam name="TResult">The type of data returned by the command.</typeparam>  
public abstract record Command<TResult> : IRequest<Response<TResult>>
{
}

/// <summary>  
/// Base class for commands that create an entity that return an Id of type <typeparamref name="TId"/>.  
/// </summary>  
/// <typeparam name="TId">The type of the identifier of the entity to create.</typeparam>  
public abstract record CreateCommand<TId> : Command<TId>
   where TId : IComparable<TId>;

/// <summary>  
/// Base class for commands that update an entity with an Id of type <typeparamref name="TId"/>/>.  
/// </summary>  
/// <typeparam name="TId">The type of the identifier of the entity to update.</typeparam>  
public abstract record UpdateCommand<TId> : Command
   where TId : IComparable<TId>
{
    /// <summary>  
    /// The identifier of the entity to update.  
    /// </summary>  
    public TId Id { get; init; } = default!;
}

/// <summary>  
/// Base class for commands that delete an entity with an Id of type <typeparamref name="TId"/>/>.  
/// </summary>  
/// <typeparam name="TId">The type of the identifier of the entity to update.</typeparam>  
public abstract record DeleteCommand<TId> : UpdateCommand<TId>
   where TId : IComparable<TId>;

/// <summary>  
/// Base class for commands that archive an entity with an Id of type <typeparamref name="TId"/>/>.  
/// </summary>  
/// <typeparam name="TId">The type of the identifier of the entity to update.</typeparam>  
public abstract record ArchiveCommand<TId> : UpdateCommand<TId>
   where TId : IComparable<TId>;
