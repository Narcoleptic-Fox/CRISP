using CRISP.Responses;

namespace CRISP.Requests
{
    /// <summary>
    /// Defines a handler for processing requests that don't return a response.
    /// </summary>
    /// <typeparam name="TRequest">The type of request to handle.</typeparam>
    /// <remarks>
    /// Request handlers implement the command pattern in the CRISP architecture.
    /// They process command requests that typically modify system state but don't
    /// return specific data. Examples include create, update, or delete operations.
    /// </remarks>
    public interface IRequestHandler<in TRequest> where TRequest : IRequest
    {
        /// <summary>
        /// Handles the specified request asynchronously.
        /// </summary>
        /// <param name="request">The request to handle.</param>
        /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
        ValueTask Handle(TRequest request);
    }
    
    /// <summary>
    /// Defines a handler for processing requests that return a specific response type.
    /// </summary>
    /// <typeparam name="TRequest">The type of request to handle.</typeparam>
    /// <typeparam name="TResponse">The type of response to return.</typeparam>
    /// <remarks>
    /// These handlers process requests that need to return data, such as queries
    /// retrieving information or commands that return identifiers or status information.
    /// They are a core component in implementing the CQRS pattern.
    /// </remarks>
    public interface IRequestHandler<in TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : IResponse
    {
        /// <summary>
        /// Handles the specified request and returns a response asynchronously.
        /// </summary>
        /// <param name="request">The request to handle.</param>
        /// <returns>A <see cref="ValueTask{TResponse}"/> containing the response data.</returns>
        ValueTask<TResponse> Handle(TRequest request);
    }
}
