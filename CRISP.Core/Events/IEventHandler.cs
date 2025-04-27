namespace CRISP.Core.Events;

/// <summary>
/// Defines a handler for domain events.
/// </summary>
/// <typeparam name="TEvent">The type of event to handle.</typeparam>
public interface IEventHandler<in TEvent> where TEvent : IDomainEvent
{
    /// <summary>
    /// Handles the specified domain event.
    /// </summary>
    /// <param name="event">The event to handle.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A value task representing the asynchronous operation.</returns>
    ValueTask Handle(TEvent @event, CancellationToken cancellationToken = default);
}