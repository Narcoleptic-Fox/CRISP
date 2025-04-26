using CRISP.Events;
using Microsoft.Extensions.Logging;

namespace CRISP.Samples;

public class SampleEventPublisher : IEventService
{
    private readonly ILogger<SampleEventPublisher> _logger;
    private readonly IEnumerable<IEventHandler<SampleEvent>> _eventHandlers;

    public SampleEventPublisher(
        ILogger<SampleEventPublisher> logger,
        IEnumerable<IEventHandler<SampleEvent>> eventHandlers)
    {
        _logger = logger;
        _eventHandlers = eventHandlers;
    }

    public async ValueTask Publish(IEvent @event)
    {
        _logger.LogInformation("Publishing event of type: {EventType}", @event.GetType().Name);

        // Handle event based on its type
        if (@event is SampleEvent sampleEvent)
        {
            foreach (IEventHandler<SampleEvent> handler in _eventHandlers)
            {
                await handler.Handle(sampleEvent);
            }
        }
    }
}