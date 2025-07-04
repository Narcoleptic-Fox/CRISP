namespace Crisp.Events;

/// <summary>
/// Interface for publishing events
/// </summary>
public interface IEventPublisher
{
    Task Publish<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent;
}