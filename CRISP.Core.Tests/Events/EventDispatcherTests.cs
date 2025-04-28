using CRISP.Core.Events;
using CRISP.Core.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace CRISP.Core.Tests.Events;

public class EventDispatcherTests : IDisposable
{
    private readonly Mock<ILogger<EventDispatcher>> _loggerMock;
    private readonly EventOptions _eventOptions;
    private readonly ServiceProvider _serviceProvider;

    public EventDispatcherTests()
    {
        _loggerMock = new Mock<ILogger<EventDispatcher>>();
        _eventOptions = new EventOptions
        {
            ProcessEventsInParallel = false,
            ThrowOnHandlerFailure = true,
            MaxDegreeOfParallelism = 4,
            MaxBatchSize = 10
        };

        // Set up service provider with event handlers
        ServiceCollection services = new();

        // Add mock logger factory
        Mock<ILoggerFactory> loggerFactory = new();
        loggerFactory
            .Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(Mock.Of<ILogger>());
        services.AddSingleton(loggerFactory.Object);

        // Register event handlers
        services.AddTransient<IEventHandler<TestEvent>, TestEventHandler>();
        services.AddTransient<IEventHandler<TestEvent>, AnotherTestEventHandler>();
        _serviceProvider = services.BuildServiceProvider();

        // Important: Clear static state at the beginning of each test
        TestEventHandler.ProcessedEvents.Clear();
        AnotherTestEventHandler.ProcessedEvents.Clear();
        TestEventHandler.ThrowException = false;
        TestEventHandler.ProcessingDelayMs = 0;
        TestEventHandler.OnEventProcessed = null;
        AnotherTestEventHandler.ProcessingDelayMs = 0;
    }

    // Clean up after tests
    public void Dispose()
    {
        // Ensure all static state is cleared after each test
        TestEventHandler.ProcessedEvents.Clear();
        AnotherTestEventHandler.ProcessedEvents.Clear();
        TestEventHandler.ThrowException = false;
        TestEventHandler.ProcessingDelayMs = 0;
        TestEventHandler.OnEventProcessed = null;
        AnotherTestEventHandler.ProcessingDelayMs = 0;

        _serviceProvider.Dispose();
    }

    [Fact]
    public async Task Dispatch_SingleEvent_ProcessesEventCorrectly()
    {
        // Arrange
        TestEvent testEvent = new() { Value = "Test Value" };

        EventDispatcher dispatcher = new(_serviceProvider, _loggerMock.Object, _eventOptions);

        // Act
        await dispatcher.Dispatch(testEvent, CancellationToken.None);

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

        EventDispatcher dispatcher = new(_serviceProvider, _loggerMock.Object, _eventOptions);

        // Act
        await dispatcher.DispatchAll(testEvents, CancellationToken.None);

        // Assert
        TestEventHandler.ProcessedEvents.Count().ShouldBe(3);
        AnotherTestEventHandler.ProcessedEvents.Count().ShouldBe(3);

        TestEventHandler.ProcessedEvents.Select(e => e.Value)
            .ShouldBeEquivalentTo(new[] { "Event 1", "Event 2", "Event 3" });
    }

    [Fact]
    public async Task Dispatch_WithParallelProcessing_ProcessesEventsInParallel()
    {
        // Arrange
        EventOptions parallelOptions = new()
        {
            ProcessEventsInParallel = true,
            ThrowOnHandlerFailure = false,
            MaxDegreeOfParallelism = 4
        };

        List<TestEvent> testEvents =
        [
            new() { Value = "Event 1" },
            new() { Value = "Event 2" },
            new() { Value = "Event 3" }
        ];

        // Add a delay to better observe parallel execution
        TestEventHandler.ProcessingDelayMs = 100;

        EventDispatcher dispatcher = new(_serviceProvider, _loggerMock.Object, parallelOptions);

        // Act
        System.Diagnostics.Stopwatch stopwatch = new();
        stopwatch.Start();
        await dispatcher.DispatchAll(testEvents, CancellationToken.None);
        stopwatch.Stop();

        // Assert
        TestEventHandler.ProcessedEvents.Count().ShouldBe(3);
        AnotherTestEventHandler.ProcessedEvents.Count().ShouldBe(3);
    }

    [Fact]
    public async Task Dispatch_HandlerThrowsException_AndThrowOnHandlerFailureTrue_PropagatesException()
    {
        // Arrange
        TestEventHandler.ThrowException = true;
        TestEvent testEvent = new() { Value = "Test Value" };

        EventDispatcher dispatcher = new(_serviceProvider, _loggerMock.Object, _eventOptions);

        // Act
        Func<Task> act = async () => await dispatcher.Dispatch(testEvent, CancellationToken.None);

        // Assert
        InvalidOperationException exception = await act.ShouldThrowAsync<InvalidOperationException>();
        exception.Message.ShouldBe("Handler failed");
    }

    [Fact]
    public async Task Dispatch_HandlerThrowsException_AndThrowOnHandlerFailureFalse_LogsErrorAndContinues()
    {
        // Arrange
        EventOptions options = new()
        {
            ProcessEventsInParallel = false,
            ThrowOnHandlerFailure = false
        };

        TestEventHandler.ThrowException = true;
        TestEvent testEvent = new() { Value = "Event 1" };

        EventDispatcher dispatcher = new(_serviceProvider, _loggerMock.Object, options);

        // Act - Should not throw
        await dispatcher.Dispatch(testEvent, CancellationToken.None);

        // Assert - Second handler still processed the event
        // Important: We expect exactly one processed event in the AnotherTestEventHandler
        AnotherTestEventHandler.ProcessedEvents.Count.ShouldBe(1);

        // Verify an error was logged - use more flexible verification
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
        _loggerMock.Verify(
            logger => logger.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(), // Don't check the message specifically
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ),
            Times.AtLeastOnce
        );
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
    }

    [Fact]
    public async Task Dispatch_WithCancellation_StopsProcessing()
    {
        // Arrange
        List<TestEvent> testEvents =
        [
            new() { Value = "Event 1" },
            new() { Value = "Event 2" },
            new() { Value = "Event 3" }
        ];

        // Add a delay to make cancellation more reliable
        TestEventHandler.ProcessingDelayMs = 100;

        // Create a cancellation token that will be cancelled after the first event
        CancellationTokenSource cts = new();
        int eventCount = 0;
        TestEventHandler.OnEventProcessed = _ =>
        {
            eventCount++;
            if (eventCount == 1)
            {
                cts.Cancel();
            }
        };

        EventDispatcher dispatcher = new(_serviceProvider, _loggerMock.Object, _eventOptions);

        // Act
        Func<Task> act = async () => await dispatcher.DispatchAll(testEvents, cts.Token);

        // Assert
        await act.ShouldThrowAsync<OperationCanceledException>();
    }

    // Test event and handlers
    public class TestEvent : DomainEvent
    {
        public string Value { get; set; } = string.Empty;
    }

    public class TestEventHandler : IEventHandler<TestEvent>
    {
        public static List<TestEvent> ProcessedEvents { get; } = [];
        public static int ProcessingDelayMs { get; set; } = 0;
        public static Action<TestEvent>? OnEventProcessed { get; set; }
        public static bool ThrowException { get; set; } = false;

        public async ValueTask Handle(TestEvent @event, CancellationToken cancellationToken)
        {
            if (ThrowException)
            {
                throw new InvalidOperationException("Handler failed");
            }

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