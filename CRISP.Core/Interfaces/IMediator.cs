namespace CRISP.Core.Interfaces;

/// <summary>
/// Defines a mediator for sending requests in the CRISP architecture.
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Sends a request to its corresponding handler.
    /// </summary>
    /// <typeparam name="TResponse">The type of response expected from the request.</typeparam>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>ValueTask containing the response from the request.</returns>
    ValueTask<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a request to its corresponding handler without expecting a response.
    /// </summary>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>ValueTask representing the operation.</returns>
    ValueTask Send(IRequest request, CancellationToken cancellationToken = default);
}