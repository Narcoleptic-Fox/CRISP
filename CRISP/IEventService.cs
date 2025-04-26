using CRISP.Events;

namespace CRISP
{
    /// <summary>
    /// Defines a service for publishing events within a CRISP application.
    /// </summary>
    /// <remarks>
    /// The event service is responsible for distributing events to all registered
    /// event handlers in the application. It serves as a central point for event
    /// publication in the CQRS pattern.
    /// </remarks>
    public interface IEventService
    {
        /// <summary>
        /// Publishes an event to all registered handlers.
        /// </summary>
        /// <param name="event">The event to be published.</param>
        /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
        ValueTask Publish(IEvent @event);
    }
}
