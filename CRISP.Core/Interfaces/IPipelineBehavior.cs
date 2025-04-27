namespace CRISP.Core.Interfaces;

/// <summary>
/// Defines a pipeline behavior for processing requests.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Processes the request through the pipeline.
    /// </summary>
    /// <param name="request">The request to process.</param>
    /// <param name="next">The delegate to invoke the next behavior in the pipeline.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response from the pipeline.</returns>
    ValueTask<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken);
}

/// <summary>
/// Represents a delegate that handles a request and produces a response.
/// </summary>
/// <typeparam name="TResponse">The type of response.</typeparam>
/// <returns>A value task containing the response.</returns>
public delegate ValueTask<TResponse> RequestHandlerDelegate<TResponse>(CancellationToken cancellationToken);