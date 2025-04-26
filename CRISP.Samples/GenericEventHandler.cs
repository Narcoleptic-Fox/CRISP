using CRISP.Events;
using Microsoft.Extensions.Logging;

namespace CRISP.Samples;

/// <summary>
/// A generic event handler that can handle any event type
/// </summary>
/// <typeparam name="TEvent">The type of event to handle</typeparam>
public class GenericEventHandler<TEvent> : IEventHandler<TEvent>
    where TEvent : IEvent
{
    private readonly ILogger<GenericEventHandler<TEvent>> _logger;

    public GenericEventHandler(ILogger<GenericEventHandler<TEvent>> logger) => _logger = logger;

    public ValueTask Handle(TEvent @event)
    {
        _logger.LogInformation("Handling event of type {EventType} with generic handler", typeof(TEvent).Name);
        Console.WriteLine($"Generic handler processed event of type {typeof(TEvent).Name}");

        return ValueTask.CompletedTask;
    }
}