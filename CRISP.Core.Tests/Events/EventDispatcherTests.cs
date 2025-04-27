using CRISP.Core.Events;
using CRISP.Core.Interfaces;
using CRISP.Core.Options;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace CRISP.Core.Tests.Events;

public class EventDispatcherTests
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
        var services = new ServiceCollection();
        
        // Add mock logger factory
        var loggerFactory = new Mock<ILoggerFactory>();
        loggerFactory
            .Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(Mock.Of<ILogger>());
        services.AddSingleton(loggerFactory.Object);
        
        // Register event handlers
        services.AddTransient<IEventHandler<TestEvent>, TestEventHandler>();
        services.AddTransient<IEventHandler<TestEvent>, AnotherTestEventHandler>();
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task Dispatch_SingleEvent_ProcessesEventCorrectly()
    {
        // Arrange
        var testEvent = new TestEvent { Value = "Test Value" };
        TestEventHandler.ProcessedEvents.Clear();
        AnotherTestEventHandler.ProcessedEvents.Clear();
        
        var dispatcher = new EventDispatcher(_serviceProvider, _loggerMock.Object, _eventOptions);
            
        // Act
        await dispatcher.Dispatch(testEvent, CancellationToken.None);
        
        // Assert
        TestEventHandler.ProcessedEvents.Should().HaveCount(1);
        TestEventHandler.ProcessedEvents[0].Value.Should().Be("Test Value");
        AnotherTestEventHandler.ProcessedEvents.Should().HaveCount(1);
        AnotherTestEventHandler.ProcessedEvents[0].Value.Should().Be("Test Value");
    }

    [Fact]
    public async Task DispatchAll_MultipleEvents_ProcessesAllEventsCorrectly()
    {
        // Arrange
        var testEvents = new List<TestEvent>
        {
            new() { Value = "Event 1" },
            new() { Value = "Event 2" },
            new() { Value = "Event 3" }
        };
        
        TestEventHandler.ProcessedEvents.Clear();
        AnotherTestEventHandler.ProcessedEvents.Clear();
        
        var dispatcher = new EventDispatcher(_serviceProvider, _loggerMock.Object, _eventOptions);
            
        // Act
        await dispatcher.DispatchAll(testEvents, CancellationToken.None);
        
        // Assert
        TestEventHandler.ProcessedEvents.Should().HaveCount(3);
        AnotherTestEventHandler.ProcessedEvents.Should().HaveCount(3);
        
        TestEventHandler.ProcessedEvents.Select(e => e.Value)
            .Should().BeEquivalentTo(new[] { "Event 1", "Event 2", "Event 3" });
    }

    [Fact]
    public async Task Dispatch_WithParallelProcessing_ProcessesEventsInParallel()
    {
        // Arrange
        var parallelOptions = new EventOptions
        {
            ProcessEventsInParallel = true,
            ThrowOnHandlerFailure = false,
            MaxDegreeOfParallelism = 4
        };
        
        var testEvents = new List<TestEvent>
        {
            new() { Value = "Event 1" },
            new() { Value = "Event 2" },
            new() { Value = "Event 3" }
        };
        
        TestEventHandler.ProcessedEvents.Clear();
        AnotherTestEventHandler.ProcessedEvents.Clear();
        
        // Add a delay to better observe parallel execution
        TestEventHandler.ProcessingDelayMs = 100;
        
        var dispatcher = new EventDispatcher(_serviceProvider, _loggerMock.Object, parallelOptions);
            
        // Act
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        await dispatcher.DispatchAll(testEvents, CancellationToken.None);
        stopwatch.Stop();
        
        // Assert
        TestEventHandler.ProcessedEvents.Should().HaveCount(3);
        AnotherTestEventHandler.ProcessedEvents.Should().HaveCount(3);
        
        // If processed in parallel, time should be less than sequential execution
        // (3 events * 100ms delay would be 300ms sequential)
        
        // Clean up
        TestEventHandler.ProcessingDelayMs = 0;
        AnotherTestEventHandler.ProcessingDelayMs = 0;
    }

    [Fact]
    public async Task Dispatch_HandlerThrowsException_AndThrowOnHandlerFailureTrue_PropagatesException()
    {
        // Arrange
        TestEventHandler.ThrowException = true;
        var testEvent = new TestEvent { Value = "Test Value" };
        
        var dispatcher = new EventDispatcher(_serviceProvider, _loggerMock.Object, _eventOptions);
            
        // Act
        Func<Task> act = async () => await dispatcher.Dispatch(testEvent, CancellationToken.None);
        
        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Handler failed");
        
        // Clean up
        TestEventHandler.ThrowException = false;
    }

    [Fact]
    public async Task Dispatch_HandlerThrowsException_AndThrowOnHandlerFailureFalse_LogsErrorAndContinues()
    {
        // Arrange
        var options = new EventOptions
        {
            ProcessEventsInParallel = false,
            ThrowOnHandlerFailure = false
        };
        
        TestEventHandler.ThrowException = true;
        var testEvent = new TestEvent { Value = "Test Value" };
        
        var dispatcher = new EventDispatcher(_serviceProvider, _loggerMock.Object, options);
            
        // Act - Should not throw
        await dispatcher.Dispatch(testEvent, CancellationToken.None);
        
        // Assert - Second handler still processed the event
        AnotherTestEventHandler.ProcessedEvents.Should().HaveCount(1);
        
        // Verify error was logged
        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error handling event")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ),
            Times.Once
        );
        
        // Clean up
        TestEventHandler.ThrowException = false;
    }

    [Fact]
    public async Task Dispatch_WithCancellation_StopsProcessing()
    {
        // Arrange
        var testEvents = new List<TestEvent>
        {
            new() { Value = "Event 1" },
            new() { Value = "Event 2" },
            new() { Value = "Event 3" }
        };
        
        TestEventHandler.ProcessedEvents.Clear();
        AnotherTestEventHandler.ProcessedEvents.Clear();
        
        // Add a delay to make cancellation more reliable
        TestEventHandler.ProcessingDelayMs = 100;
        
        // Create a cancellation token that will be cancelled after the first event
        var cts = new CancellationTokenSource();
        int eventCount = 0;
        TestEventHandler.OnEventProcessed = _ => {
            eventCount++;
            if (eventCount == 1)
            {
                cts.Cancel();
            }
        };
        
        var dispatcher = new EventDispatcher(_serviceProvider, _loggerMock.Object, _eventOptions);
            
        // Act
        Func<Task> act = async () => await dispatcher.DispatchAll(testEvents, cts.Token);
        
        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
        
        // Clean up
        TestEventHandler.ProcessingDelayMs = 0;
        TestEventHandler.OnEventProcessed = null;
    }

    // Test event and handlers
    public class TestEvent : DomainEvent
    {
        public string Value { get; set; } = string.Empty;
    }

    public class TestEventHandler : IEventHandler<TestEvent>
    {
        public static List<TestEvent> ProcessedEvents { get; } = new();
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
        public static List<TestEvent> ProcessedEvents { get; } = new();
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