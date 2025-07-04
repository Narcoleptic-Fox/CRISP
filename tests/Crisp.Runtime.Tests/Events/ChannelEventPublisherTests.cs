using Crisp.Events;
using Crisp.Runtime.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Crisp.Runtime.Tests.Events;

public class ChannelEventPublisherTests : IDisposable
{
    private readonly ServiceCollection _services;
    private readonly ServiceProvider _serviceProvider;
    private readonly ILogger<ChannelEventPublisher> _logger;
    private readonly ChannelEventPublisher _publisher;

    public ChannelEventPublisherTests()
    {
        _services = new ServiceCollection();
        _services.AddLogging();
        _services.AddScoped<TestEventHandler>();
        _serviceProvider = _services.BuildServiceProvider();
        _logger = _serviceProvider.GetRequiredService<ILogger<ChannelEventPublisher>>();
        _publisher = new ChannelEventPublisher(_serviceProvider, _logger);
    }

    [Fact]
    public async Task Publish_WithValidEvent_ShouldProcessEvent()
    {
        // Arrange
        var testEvent = new TestEvent { Message = "Test" };

        // Act & Assert - Should not throw
        await _publisher.Publish(testEvent);
        
        // Give some time for background processing
        await Task.Delay(100);
    }

    [Fact]
    public async Task Publish_WithNullEvent_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _publisher.Publish<TestEvent>(null!));
    }

    [Fact]
    public void Dispose_ShouldCompleteGracefully()
    {
        // Act & Assert - Should not throw
        _publisher.Dispose();
    }

    [Fact]
    public async Task Publish_AfterDispose_ShouldThrowObjectDisposedException()
    {
        // Arrange
        var testEvent = new TestEvent { Message = "Test" };
        _publisher.Dispose();

        // Act & Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(() => _publisher.Publish(testEvent));
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_ShouldNotThrow()
    {
        // Act & Assert - Should not throw
        _publisher.Dispose();
        _publisher.Dispose();
        _publisher.Dispose();
    }

    public void Dispose()
    {
        _publisher?.Dispose();
        _serviceProvider?.Dispose();
    }

    private class TestEvent : IEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
        public string Message { get; set; } = string.Empty;
    }

    private class TestEventHandler : IEventHandler<TestEvent>
    {
        public Task Handle(TestEvent @event, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}