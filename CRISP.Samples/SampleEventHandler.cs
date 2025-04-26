using CRISP.Events;
using Microsoft.Extensions.Logging;

namespace CRISP.Samples;

public class SampleEventHandler : IEventHandler<SampleEvent>
{
    private readonly ILogger<SampleEventHandler> _logger;

    public SampleEventHandler(ILogger<SampleEventHandler> logger) => _logger = logger;

    public ValueTask Handle(SampleEvent @event)
    {
        _logger.LogInformation("Handling sample event with message: {Message}", @event.Message);
        Console.WriteLine($"Event handled: {@event.Message} at {DateTime.Now}");
        return ValueTask.CompletedTask;
    }
}