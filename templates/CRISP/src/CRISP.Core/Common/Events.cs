namespace CRISP.Core.Common
{
    /// <summary>
    /// Represents a domain event that has occurred in the system.
    /// </summary>
    /// <remarks>
    /// Events are used to communicate that something important has happened in the domain.
    /// They provide a way to decouple different parts of the system and enable event-driven architectures.
    /// </remarks>
    public interface IEvent
    {
        /// <summary>
        /// Gets the date and time when the event occurred.
        /// </summary>
        /// <value>
        /// A <see cref="DateTime"/> value in UTC representing when the event was created.
        /// </value>
        DateTime OccurredOn { get; }
    }

    /// <summary>
    /// Provides a base implementation for domain events.
    /// </summary>
    /// <remarks>
    /// This abstract record serves as a foundation for concrete event implementations,
    /// automatically setting the occurrence time to the current UTC time when the event is created.
    /// Using a record type ensures immutability and provides value-based equality semantics.
    /// </remarks>
    public abstract record BaseEvent : IEvent
    {
        /// <summary>
        /// Gets the date and time when the event occurred, automatically set to the current UTC time.
        /// </summary>
        /// <value>
        /// A <see cref="DateTime"/> value in UTC representing when the event was created.
        /// This value is automatically initialized to <see cref="DateTime.UtcNow"/> when the event is instantiated.
        /// </value>
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Defines a contract for dispatching events to the event bus.
    /// </summary>
    /// <remarks>
    /// The event dispatcher provides a way to publish events to interested subscribers
    /// in a decoupled manner. This enables event-driven architectures and helps maintain
    /// separation of concerns between different parts of the system.
    /// </remarks>
    public interface IEventDispatcher
    {
        /// <summary>
        /// Publishes an event to the event bus for processing by interested subscribers.
        /// </summary>
        /// <typeparam name="TEvent">The type of event to publish. Must implement <see cref="IEvent"/>.</typeparam>
        /// <param name="event">The event instance to publish.</param>
        /// <param name="cancellationToken">
        /// A cancellation token to observe while waiting for the task to complete.
        /// The default value is <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask"/> that represents the asynchronous publish operation.
        /// The task completes when the event has been successfully dispatched to all subscribers.
        /// </returns>
        /// <remarks>
        /// This method is asynchronous and should be awaited. The event will be delivered
        /// to all registered event handlers that can process the specified event type.
        /// If no handlers are registered for the event type, the operation completes successfully without error.
        /// </remarks>
        ValueTask Publish<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
            where TEvent : IEvent;
    }

    /// <summary>
    /// Defines a contract for handling specific types of domain events.
    /// </summary>
    /// <typeparam name="TEvent">The type of event this handler can process. Must implement <see cref="IEvent"/>.</typeparam>
    /// <remarks>
    /// Event handlers implement the subscriber side of the event-driven architecture pattern.
    /// They contain the business logic for responding to specific domain events and are automatically
    /// invoked by the event dispatcher when matching events are published. Multiple handlers can
    /// be registered for the same event type, allowing for multiple reactions to a single event.
    /// 
    /// Implementations should be stateless and idempotent when possible, as events may be replayed
    /// or processed multiple times in distributed scenarios. Handlers should focus on a single
    /// responsibility and avoid complex business logic that spans multiple domains.
    /// </remarks>
    public interface IEventHandler<TEvent>
        where TEvent : IEvent
    {
        /// <summary>
        /// Handles the specified domain event by executing the appropriate business logic.
        /// </summary>
        /// <param name="event">The event instance containing the data to be processed.</param>
        /// <param name="cancellationToken">
        /// A cancellation token to observe while waiting for the task to complete.
        /// The default value is <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask"/> that represents the asynchronous event handling operation.
        /// The task completes when the event has been successfully processed by this handler.
        /// </returns>
        /// <remarks>
        /// This method should contain the specific business logic for responding to the event.
        /// Implementations should be designed to handle failures gracefully and should not throw
        /// exceptions unless the event processing cannot continue. Consider using compensation
        /// patterns for operations that might need to be rolled back.
        /// 
        /// The method should be idempotent where possible, as the same event might be delivered
        /// multiple times in distributed scenarios. Any side effects should be carefully considered
        /// and designed to handle duplicate processing safely.
        /// </remarks>
        ValueTask Handle(TEvent @event, CancellationToken cancellationToken = default);
    }
}
