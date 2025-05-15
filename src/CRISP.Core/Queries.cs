namespace CRISP;

/// <summary>
/// Base class for queries that return data of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TResult">The type of data returned by the query.</typeparam>
public abstract record Query<TResult> : IRequest<Response<TResult>>;

/// <summary>
/// A query to retrieve an entity by its identifier.
/// </summary>
/// <typeparam name="TId">The type of the identifier.</typeparam>
/// <typeparam name="TResult">The type of data returned by the query.</typeparam>
public abstract record ByIdQuery<TId, TResult> : Query<TResult>
    where TId : IComparable<TId>
{
    /// <summary>
    /// The identifier of the entity to retrieve.
    /// </summary>
    public TId Id { get; init; } = default!;
}

/// <summary>
/// A query that applies a filter to retrieve paginated results of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TFilter">The type of the filter applied to the query.</typeparam>
/// <typeparam name="TResult">The type of data returned by the query.</typeparam>
public abstract record FilteredQuery<TFilter, TResult> : Query<PagedResult<TResult>>
    where TFilter : FilterBase
{
    /// <summary>
    /// The filter to apply to the query.
    /// </summary>
    public TFilter Filter { get; init; } = default!;
}