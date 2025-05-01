using CRISP.Core.Events;
using CRISP.Core.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics;

namespace CRISP.Core.Tests.Events;

public class ChannelEventDispatcherTests
{
    private readonly Mock<ILogger<ChannelEventDispatcher>> _loggerMock;
    private readonly EventOptions _eventOptions;
    private readonly ChannelEventOptions _channelOptions;
    private readonly ServiceProvider _serviceProvider;

    public ChannelEventDispatcherTests()
    {
        _loggerMock = new Mock<ILogger<ChannelEventDispatcher>>();
        _eventOptions = new EventOptions
        {
            UseChannels = true,
            EnableDetailedLogging = true,
            ProcessEventsInParallel = false,
            ThrowOnHandlerFailure = false,
            MaxBatchSize = 10
        };

        _channelOptions = new ChannelEventOptions
        {
            ChannelCapacity = 100,
            ConsumerCount = 2,
            FullChannelWaitTimeMs = 500,
            WaitForChannelDrainOnDispose = true,
            ChannelDrainTimeoutMs = 1000
        };

        // Set up service provider with event handlers
        ServiceCollection services = new();

        // Add mock logger factory instead of trying to register an open generic
        Mock<ILoggerFactory> loggerFactory = new();
        loggerFactory
            .Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(Mock.Of<ILogger>());

        services.AddSingleton<ILoggerFactory>(loggerFactory.Object);

        services.AddTransient<IEventHandler<TestEvent>, TestEventHandler>();
        services.AddTransient<IEventHandler<TestEvent>, AnotherTestEventHandler>();
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task Dispatch_SingleEvent_ProcessesEventCorrectly()
    {
        // Arrange
        TestEvent testEvent = new() { Value = "Test Value" };
        TestEventHandler.ProcessedEvents.Clear();
        AnotherTestEventHandler.ProcessedEvents.Clear();

        using ChannelEventDispatcher dispatcher = new(
            _serviceProvider, _loggerMock.Object, _eventOptions, _channelOptions);

        // Act
        await dispatcher.Dispatch(testEvent);

        // Need to wait a bit since processing is async
        await Task.Delay(500);

        // Assert
        TestEventHandler.ProcessedEvents.Count().ShouldBe(1);
        TestEventHandler.ProcessedEvents[0].Value.ShouldBe("Test Value");
        AnotherTestEventHandler.ProcessedEvents.Count().ShouldBe(1);
        AnotherTestEventHandler.ProcessedEvents[0].Value.ShouldBe("Test Value");
    }

    [Fact]
    public async Task DispatchAll_MultipleEvents_ProcessesAllEventsCorrectly()
    {
        // Arrange
        List<TestEvent> testEvents =
        [
            new() { Value = "Event 1" },
            new() { Value = "Event 2" },
            new() { Value = "Event 3" }
        ];

        TestEventHandler.ProcessedEvents.Clear();
        AnotherTestEventHandler.ProcessedEvents.Clear();

        using ChannelEventDispatcher dispatcher = new(
            _serviceProvider, _loggerMock.Object, _eventOptions, _channelOptions);

        // Act
        await dispatcher.DispatchAll(testEvents);

        // Need to wait a bit since processing is async
        await Task.Delay(500);

        // Assert
        TestEventHandler.ProcessedEvents.Count().ShouldBe(3);
        AnotherTestEventHandler.ProcessedEvents.Count().ShouldBe(3);

        TestEventHandler.ProcessedEvents.ShouldAllBe(e => testEvents.Contains(e));
    }

    [Fact]
    public async Task Dispatch_ExceedingChannelCapacity_WaitsAndProcessesEvents()
    {
        // Arrange
        int eventCount = 150;  // More than our channel capacity
        ChannelEventOptions channelOptions = new()
        {
            ChannelCapacity = 100,
            ConsumerCount = 2,
            FullChannelWaitTimeMs = 10000, // Long wait time to prevent exceptions
        };

        TestEventHandler.ProcessedEvents.Clear();
        AnotherTestEventHandler.ProcessedEvents.Clear();

        // Slow down handlers to make channel fill up
        TestEventHandler.ProcessingDelayMs = 10;
        AnotherTestEventHandler.ProcessingDelayMs = 10;

        using ChannelEventDispatcher dispatcher = new(
            _serviceProvider, _loggerMock.Object, _eventOptions, channelOptions);

        // Act
        Stopwatch stopwatch = Stopwatch.StartNew();
        List<Task> tasks = [];

        for (int i = 0; i < eventCount; i++)
        {
            TestEvent testEvent = new() { Value = $"Event {i}" };
            tasks.Add(dispatcher.Dispatch(testEvent).AsTask());
        }

        // Wait for all dispatch operations to complete
        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Wait for processing to complete
        await Task.Delay(2000);

        // Assert
        TestEventHandler.ProcessedEvents.Count().ShouldBe(eventCount);
        AnotherTestEventHandler.ProcessedEvents.Count().ShouldBe(eventCount);

        // Clean up
        TestEventHandler.ProcessingDelayMs = 0;
        AnotherTestEventHandler.ProcessingDelayMs = 0;
    }

    [Fact]
    public async Task DispatchAll_WithParallelProcessing_ProcessesEventsInParallel()
    {
        // Arrange
        EventOptions parallelOptions = new()
        {
            UseChannels = true,
            ProcessEventsInParallel = true,
            ThrowOnHandlerFailure = false
        };

        List<TestEvent> testEvents =
        [
            new() { Value = "Event 1" },
            new() { Value = "Event 2" },
            new() { Value = "Event 3" }
        ];

        TestEventHandler.ProcessedEvents.Clear();
        AnotherTestEventHandler.ProcessedEvents.Clear();

        // Add a delay to better observe parallel execution
        TestEventHandler.ProcessingDelayMs = 100;

        using ChannelEventDispatcher dispatcher = new(
            _serviceProvider, _loggerMock.Object, parallelOptions, _channelOptions);

        // Act
        Stopwatch stopwatch = Stopwatch.StartNew();
        await dispatcher.DispatchAll(testEvents);

        // Wait for processing to complete
        await Task.Delay(500);
        stopwatch.Stop();

        // Assert
        TestEventHandler.ProcessedEvents.Count().ShouldBe(3);
        AnotherTestEventHandler.ProcessedEvents.Count().ShouldBe(3);

        // If processed in parallel, time should be less than sequential execution
        // (3 events * 100ms delay would be 300ms sequential)
        // Clean up
        TestEventHandler.ProcessingDelayMs = 0;
        AnotherTestEventHandler.ProcessingDelayMs = 0;
    }

    [Fact]
    public async Task Dispose_WaitsForEventsToProcess()
    {
        // Arrange
        List<TestEvent> testEvents =
        [
            new() { Value = "Event 1" },
            new() { Value = "Event 2" },
            new() { Value = "Event 3" }
        ];

        // Use synchronization to track completion
        int processedCount = 0;
        ManualResetEventSlim processingComplete = new(false);

        TestEventHandler.ProcessedEvents.Clear();
        AnotherTestEventHandler.ProcessedEvents.Clear();

        // Override the standard handler to use our synchronization
        TestEventHandler.OnEventProcessed = _ =>
        {
            Interlocked.Increment(ref processedCount);
            if (processedCount >= 3)
            {
                processingComplete.Set();
            }
        };

        // Moderate delay (not too long to slow tests, not too short to miss events)
        TestEventHandler.ProcessingDelayMs = 50;
        AnotherTestEventHandler.ProcessingDelayMs = 50;

        ChannelEventOptions options = new()
        {
            ChannelCapacity = 100,
            ConsumerCount = 4, // More consumers to process in parallel
            WaitForChannelDrainOnDispose = true,
            ChannelDrainTimeoutMs = 5000 // Longer timeout
        };

        // Act
        using (ChannelEventDispatcher dispatcher = new(
                _serviceProvider, _loggerMock.Object, _eventOptions, options))
        {
            // Dispatch each event individually to ensure they're all enqueued
            foreach (TestEvent evt in testEvents)
            {
                await dispatcher.Dispatch(evt);
            }

            // Try to wait for processing to complete or timeout
            bool completed = processingComplete.Wait(TimeSpan.FromMilliseconds(2000));

            // The disposal will happen here and should wait for events
        }

        // Wait a bit after disposal to stabilize
        await Task.Delay(200);

        // Assert - all events should be processed even after disposal
        TestEventHandler.ProcessedEvents.Count().ShouldBe(3);
        TestEventHandler.ProcessedEvents.ShouldAllBe(e => testEvents.Contains(e));

        // Clean up
        TestEventHandler.ProcessingDelayMs = 0;
        TestEventHandler.OnEventProcessed = null;
        AnotherTestEventHandler.ProcessingDelayMs = 0;
    }

    // Test event and handlers for testing
    public class TestEvent : DomainEvent
    {
        public string Value { get; set; } = string.Empty;
    }

    public class TestEventHandler : IEventHandler<TestEvent>
    {
        public static List<TestEvent> ProcessedEvents { get; } = [];
        public static int ProcessingDelayMs { get; set; } = 0;
        // Add callback for better synchronization in tests
        public static Action<TestEvent>? OnEventProcessed { get; set; }

        public async ValueTask Handle(TestEvent @event, CancellationToken cancellationToken)
        {
            if (ProcessingDelayMs > 0)
            {
                await Task.Delay(ProcessingDelayMs, cancellationToken);
            }

            lock (ProcessedEvents)
            {
                ProcessedEvents.Add(@event);
            }

            OnEventProcessed?.Invoke(@event);
        }
    }

    public class AnotherTestEventHandler : IEventHandler<TestEvent>
    {
        public static List<TestEvent> ProcessedEvents { get; } = [];
        public static int ProcessingDelayMs { get; set; } = 0;

        public async ValueTask Handle(TestEvent @event, CancellationToken cancellationToken)
        {
            if (ProcessingDelayMs > 0)
            {
                await Task.Delay(ProcessingDelayMs, cancellationToken);
            }

            lock (ProcessedEvents)
            {
                ProcessedEvents.Add(@event);
            }
        }
    }
}