namespace CRISP.Events
{
    /// <summary>
    /// Defines a handler for processing specific event types.
    /// </summary>
    /// <typeparam name="TEvent">The type of event this handler can process.</typeparam>
    /// <remarks>
    /// Event handlers implement the observer pattern in the CRISP architecture.
    /// They react to events that have already occurred in the system and perform
    /// follow-up actions such as triggering workflows, sending notifications,
    /// or updating projections for read models.
    /// </remarks>
    public interface IEventHandler<TEvent> where TEvent : IEvent
    {
        /// <summary>
        /// Handles the specified event asynchronously.
        /// </summary>
        /// <param name="event">The event to handle.</param>
        /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
        ValueTask Handle(TEvent @event);
    }
}
