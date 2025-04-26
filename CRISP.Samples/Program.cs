using CRISP.Events;
using CRISP.Requests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CRISP.Samples;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Setup dependency injection
        ServiceProvider serviceProvider = new ServiceCollection()
            .AddLogging(configure => configure.AddConsole())
            // Use auto-generated consolidated registration method from the source generator
            .AddCrispServices()
            // Register closed generic types manually
            .AddTransient<IRequestService<SampleRequest, SampleResponse>, SampleGenericRequestService<SampleRequest, SampleResponse>>()
            .BuildServiceProvider();

        // Demonstrate event handling through IEventService
        Console.WriteLine("\n=== Testing Event Service ===");
        IEventService eventService = serviceProvider.GetRequiredService<IEventService>();
        await eventService.Publish(new SampleEvent { Message = "Event published through IEventService" });

        // Demonstrate request handling through IRequestService
        Console.WriteLine("\n=== Testing Request Service ===");
        IRequestService<SampleRequest, SampleResponse> requestService = serviceProvider
            .GetRequiredService<IRequestService<SampleRequest, SampleResponse>>();
        SampleResponse response = await requestService.Send(new SampleRequest { Query = "Query processed through IRequestService" });
        Console.WriteLine($"Response: {response.Result}, Success: {response.IsSuccess}");

        // Demonstrate open generic event handler
        Console.WriteLine("\n=== Testing Generic Event Handler ===");
        IEventHandler<SampleEvent> genericEventHandler = serviceProvider.GetRequiredService<IEventHandler<SampleEvent>>();
        await genericEventHandler.Handle(new SampleEvent { Message = "Event handled by generic handler" });

        // Create a new event type to demonstrate the power of open generics
        Console.WriteLine("\n=== Testing Generic Event Handler with Dynamic Event ===");
        IEventHandler<DynamicEvent> dynamicHandler = serviceProvider.GetRequiredService<IEventHandler<DynamicEvent>>();
        await dynamicHandler.Handle(new DynamicEvent { Id = 123, Timestamp = DateTime.Now });
    }
}

// Dynamic event class to demonstrate open generics
public class DynamicEvent : IEvent
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
}

// Sample services
public interface ISampleEventService
{
    Task ProcessSampleEvent(string message);
}

public class SampleEventService : ISampleEventService
{
    private readonly ILogger<SampleEventService> _logger;
    private readonly IEventHandler<SampleEvent> _eventHandler;

    public SampleEventService(
        ILogger<SampleEventService> logger,
        IEventHandler<SampleEvent> eventHandler)
    {
        _logger = logger;
        _eventHandler = eventHandler;
    }

    public async Task ProcessSampleEvent(string message)
    {
        _logger.LogInformation("Processing sample event with message: {Message}", message);
        await _eventHandler.Handle(new SampleEvent { Message = message });
    }
}

public interface ISampleRequestService
{
    Task<SampleResponse> ProcessSampleRequest(string query);
}

public class SampleRequestService : ISampleRequestService
{
    private readonly ILogger<SampleRequestService> _logger;
    private readonly IRequestHandler<SampleRequest, SampleResponse> _requestHandler;

    public SampleRequestService(
        ILogger<SampleRequestService> logger,
        IRequestHandler<SampleRequest, SampleResponse> requestHandler)
    {
        _logger = logger;
        _requestHandler = requestHandler;
    }

    public async Task<SampleResponse> ProcessSampleRequest(string query)
    {
        _logger.LogInformation("Processing sample request with query: {Query}", query);
        return await _requestHandler.Handle(new SampleRequest { Query = query });
    }
}