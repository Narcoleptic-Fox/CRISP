namespace CRISP.Events;

/// <summary>
/// Defines a dispatcher for domain events.
/// </summary>
public interface IEventDispatcher
{
    /// <summary>
    /// Dispatches a domain event to all registered handlers.
    /// </summary>
    /// <param name="event">The domain event to dispatch.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A value task representing the asynchronous operation.</returns>
    ValueTask Dispatch(IEvent @event, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dispatches a collection of domain events to all registered handlers.
    /// </summary>
    /// <param name="events">The domain events to dispatch.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A value task representing the asynchronous operation.</returns>
    ValueTask DispatchAll(IEnumerable<IEvent> events, CancellationToken cancellationToken = default);
}