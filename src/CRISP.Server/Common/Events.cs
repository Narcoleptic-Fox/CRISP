namespace CRISP.Server.Common;

public abstract record BaseEvent
{
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}

public interface IEventHandler<in TEvent> where TEvent : BaseEvent
{
    ValueTask HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}

public interface IEventPublisher
{
    ValueTask Publish<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : BaseEvent;
}