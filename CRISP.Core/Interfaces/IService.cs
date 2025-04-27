namespace CRISP.Core.Interfaces;

/// <summary>
/// Base interface for all services in the CRISP architecture.
/// </summary>
public interface IService
{
}

/// <summary>
/// Interface for services that create entities.
/// </summary>
/// <typeparam name="TRequest">The type of the create request.</typeparam>
/// <typeparam name="TResponse">The type of the response returned after creation.</typeparam>
public interface ICreateService<in TRequest, TResponse> : IService
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Creates a new entity based on the request.
    /// </summary>
    /// <param name="request">The create request.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A value task representing the operation with the created entity ID.</returns>
    ValueTask<TResponse> Create(TRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for services that retrieve entities.
/// </summary>
/// <typeparam name="TKey">The type of the entity key.</typeparam>
/// <typeparam name="TResponse">The type of the response returned when retrieving.</typeparam>
public interface IReadService<in TKey, TResponse> : IService
{
    /// <summary>
    /// Gets an entity by ID.
    /// </summary>
    /// <param name="id">The entity ID.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A value task representing the operation with the entity.</returns>
    ValueTask<TResponse> GetById(TKey id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for services that update entities.
/// </summary>
/// <typeparam name="TRequest">The type of the update request.</typeparam>
public interface IUpdateService<in TRequest> : IService
    where TRequest : IRequest
{
    /// <summary>
    /// Updates an entity based on the request.
    /// </summary>
    /// <param name="request">The update request.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A value task representing the operation.</returns>
    ValueTask Update(TRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for services that delete entities.
/// </summary>
/// <typeparam name="TKey">The type of the entity key.</typeparam>
public interface IDeleteService<in TKey> : IService
{
    /// <summary>
    /// Deletes an entity by ID.
    /// </summary>
    /// <param name="id">The entity ID.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A value task representing the operation.</returns>
    ValueTask Delete(TKey id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for services that list entities.
/// </summary>
/// <typeparam name="TFilter">The type of filter to apply to the list.</typeparam>
/// <typeparam name="TResponse">The type of entities returned.</typeparam>
public interface IListService<in TFilter, TResponse> : IService
{
    /// <summary>
    /// Lists entities based on a filter.
    /// </summary>
    /// <param name="filter">The filter to apply.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A value task representing the operation with the entities.</returns>
    ValueTask<IEnumerable<TResponse>> List(TFilter filter, CancellationToken cancellationToken = default);
}