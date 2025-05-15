using CRISP.Validation;
using Microsoft.AspNetCore.Mvc;

namespace CRISP
{
    /// <summary>
    /// Interface for endpoints that handle queries and return results.
    /// Provides a standard pattern for query operations through HTTP endpoints.
    /// </summary>
    /// <typeparam name="TQuery">The type of query to handle.</typeparam>
    /// <typeparam name="TResult">The type of result returned by processing the query.</typeparam>
    public interface IQueryEndpoint<TQuery, TResult> : IEndpoint
        where TQuery : Query<TResult>
    {
        /// <summary>
        /// Handles the query request, validates it, and processes it through the appropriate service.
        /// Query parameters are extracted from the request query string.
        /// </summary>
        /// <param name="request">The query request from the query string.</param>
        /// <param name="service">The service responsible for processing the query.</param>
        /// <param name="validators">Collection of validators for the query.</param>
        /// <returns>A task that represents the asynchronous operation, containing the response with the result.</returns>
        static abstract Task<Response<TResult>> Handle([FromQuery] TQuery request, [FromServices] IQueryService<TQuery, TResult> service, [FromServices] IEnumerable<IValidator<TQuery>> validators);
    }

    /// <summary>
    /// Interface for endpoints that handle queries by entity ID.
    /// Supports operations where an entity is retrieved by its identifier from the route.
    /// </summary>
    /// <typeparam name="TQuery">The type of query to handle.</typeparam>
    /// <typeparam name="TId">The type of identifier for the entity being queried.</typeparam>
    /// <typeparam name="TResult">The type of result returned by processing the query.</typeparam>
    public interface IByIdQueryEndpoint<TQuery, TId, TResult> : IEndpoint
        where TQuery : ByIdQuery<TId, TResult>
        where TId : IComparable<TId>
    {
        /// <summary>
        /// Handles the query request for an entity identified by the provided ID.
        /// </summary>
        /// <param name="id">The identifier of the entity to query, extracted from the route.</param>
        /// <param name="queryParams">Additional query parameters as a string.</param>
        /// <param name="service">The service responsible for processing the query.</param>
        /// <param name="validators">Collection of validators for the query.</param>
        /// <returns>A task that represents the asynchronous operation, containing the response with the result.</returns>
        static abstract Task<Response<TResult>> Handle([FromRoute] TId id, [FromQuery] string queryParams, [FromServices] IQueryService<TQuery, TResult> service, [FromServices] IEnumerable<IValidator<TQuery>> validators);
    }

    /// <summary>
    /// Interface for endpoints that handle filtered queries returning paged results.
    /// Supports operations that return collections of items with filtering and pagination.
    /// </summary>
    /// <typeparam name="TQuery">The type of filtered query to handle.</typeparam>
    /// <typeparam name="TFilter">The type of filter criteria used in the query.</typeparam>
    /// <typeparam name="TResult">The type of items in the paged result.</typeparam>
    public interface IFilteredQueryEndpoint<TQuery, TFilter, TResult> : IQueryEndpoint<TQuery, PagedResult<TResult>>
        where TQuery : FilteredQuery<TFilter, TResult>
        where TFilter : FilterBase
    {
    }
}