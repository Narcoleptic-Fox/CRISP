using CRISP.Models;
using CRISP.Requests;
using CRISP.Responses;

namespace CRISP
{
    /// <summary>
    /// Generic interface for a request service
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    public interface IRequestService<in TRequest> : IDisposable
        where TRequest : IRequest
    {
        /// <summary>
        /// Executes the request and returns a response
        /// </summary>
        /// <param name="request"><see cref="TRequest"/></param>
        /// <exception cref="DomainException"></exception>
        ValueTask Send(TRequest request);
    }

    /// <summary>
    /// Generic interface for a request service with a response
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public interface IRequestService<in TRequest, TResponse> : IDisposable
        where TRequest : IRequest<TResponse>
    {
        /// <summary>
        /// Executes the request and returns a response
        /// </summary>
        /// <param name="request"><see cref="TRequest"/></param>
        /// <returns><see cref="TResponse"/></returns>
        /// <exception cref="DomainException"></exception>
        ValueTask<TResponse> Send(TRequest request);
    }

    public interface IQueryService<in TQuery, TResponse> : IRequestService<TQuery, TResponse>
        where TQuery : Query<TResponse>
        where TResponse : BaseModel
    {
    }

    public interface IFilteredQueryService<in TQuery, TResponse> : IRequestService<TQuery, FilteredResponse<TResponse>>
        where TQuery : FilteredQuery<TResponse>
        where TResponse : BaseModel
    {

    }
}
