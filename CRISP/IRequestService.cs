using CRISP.Models;
using CRISP.Requests;
using CRISP.Responses;

namespace CRISP
{
    /// <summary>
    /// Generic interface for a service that handles requests without returning data.
    /// </summary>
    /// <typeparam name="TRequest">The type of request to handle.</typeparam>
    /// <remarks>
    /// This interface is typically used for command operations in the CQRS pattern
    /// that modify state but don't return data.
    /// </remarks>
    public interface IRequestService<in TRequest> : IDisposable
        where TRequest : IRequest
    {
        /// <summary>
        /// Executes the request asynchronously.
        /// </summary>
        /// <param name="request">The request to execute.</param>
        /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
        /// <exception cref="DomainException">Thrown when the request processing fails due to a domain rule violation.</exception>
        ValueTask Send(TRequest request);
    }

    /// <summary>
    /// Generic interface for a service that handles requests and returns a response.
    /// </summary>
    /// <typeparam name="TRequest">The type of request to handle.</typeparam>
    /// <typeparam name="TResponse">The type of response to return.</typeparam>
    /// <remarks>
    /// This interface is used in the CQRS pattern for commands that need to return data
    /// or queries that retrieve data from the system.
    /// </remarks>
    public interface IRequestService<in TRequest, TResponse> : IDisposable
        where TRequest : IRequest<TResponse>
    {
        /// <summary>
        /// Executes the request and returns a response asynchronously.
        /// </summary>
        /// <param name="request">The request to execute.</param>
        /// <returns>A <see cref="ValueTask{TResponse}"/> containing the response data.</returns>
        /// <exception cref="DomainException">Thrown when the request processing fails due to a domain rule violation.</exception>
        ValueTask<TResponse> Send(TRequest request);
    }

    /// <summary>
    /// Specialized service interface for handling queries that return a single model.
    /// </summary>
    /// <typeparam name="TQuery">The type of query to handle.</typeparam>
    /// <typeparam name="TResponse">The type of model to return.</typeparam>
    /// <remarks>
    /// This interface specializes the request service for query operations
    /// that retrieve a single model from the system.
    /// </remarks>
    public interface IQueryService<in TQuery, TResponse> : IRequestService<TQuery, TResponse>
        where TQuery : Query<TResponse>
        where TResponse : BaseModel
    {
    }

    /// <summary>
    /// Specialized service interface for handling filtered queries that return paginated collections of models.
    /// </summary>
    /// <typeparam name="TQuery">The type of filtered query to handle.</typeparam>
    /// <typeparam name="TResponse">The type of model contained in the response.</typeparam>
    /// <remarks>
    /// This interface is used for query operations that retrieve collections of data
    /// with filtering, sorting, and pagination capabilities.
    /// </remarks>
    public interface IFilteredQueryService<in TQuery, TResponse> : IRequestService<TQuery, FilteredResponse<TResponse>>
        where TQuery : FilteredQuery<TResponse>
        where TResponse : BaseModel
    {
    }
}
