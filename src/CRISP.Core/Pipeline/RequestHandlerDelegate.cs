namespace Crisp.Pipeline;

/// <summary>
/// Represents an asynchronous delegate that handles a request and returns a response of type TResponse.
/// </summary>
/// <typeparam name="TResponse">The type of response returned by the handler.</typeparam>
/// <returns>A task representing the response from the handler.</returns>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>(CancellationToken cancellationToken = default);

/// <summary>
/// Represents an asynchronous delegate that handles a request and returns a response of type TResponse.
/// </summary>
public delegate Task RequestHandlerDelegate(CancellationToken cancellationToken = default);
