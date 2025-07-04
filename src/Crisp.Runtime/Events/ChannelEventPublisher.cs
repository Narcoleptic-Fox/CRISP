using Crisp.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace Crisp.Runtime.Events;

/// <summary>
/// Channel-based event publisher that processes events asynchronously
/// </summary>
public sealed class ChannelEventPublisher : IEventPublisher, IDisposable
{
    private readonly Channel<IEvent> _channel;
    private readonly ChannelReader<IEvent> _reader;
    private readonly ChannelWriter<IEvent> _writer;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ChannelEventPublisher> _logger;
    private readonly CancellationTokenSource _stoppingCts = new();
    private readonly Task _processingTask;
    private bool _disposed;

    public ChannelEventPublisher(IServiceProvider serviceProvider, ILogger<ChannelEventPublisher> logger)
    {
        BoundedChannelOptions options = new(1000)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        };

        _channel = Channel.CreateBounded<IEvent>(options);
        _reader = _channel.Reader;
        _writer = _channel.Writer;
        _serviceProvider = serviceProvider;
        _logger = logger;

        _processingTask = ProcessEventsAsync(_stoppingCts.Token);
    }

    public async Task Publish<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));

        if (_disposed)
            throw new ObjectDisposedException(nameof(ChannelEventPublisher));

        await _writer.WriteAsync(@event, cancellationToken);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _writer.Complete();
        _stoppingCts.Cancel();

        try
        {
            _processingTask.Wait(TimeSpan.FromSeconds(5));
        }
        catch (AggregateException)
        {
            // Ignore cancellation exceptions during disposal
        }

        _stoppingCts.Dispose();
    }

    private async Task ProcessEventsAsync(CancellationToken cancellationToken)
    {
        await foreach (IEvent @event in _reader.ReadAllAsync(cancellationToken))
        {
            try
            {
                await ProcessEventAsync(@event, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing event {EventType}", @event.GetType().Name);
            }
        }
    }

    private async Task ProcessEventAsync(IEvent @event, CancellationToken cancellationToken)
    {
        Type eventType = @event.GetType();
        Type handlerType = typeof(IEventHandler<>).MakeGenericType(eventType);

        using IServiceScope scope = _serviceProvider.CreateScope();
        IEnumerable<object?> handlers = scope.ServiceProvider.GetServices(handlerType);

        List<Task> tasks = [];

        foreach (object? handler in handlers)
        {
            System.Reflection.MethodInfo handleMethod = handlerType.GetMethod(nameof(IEventHandler<IEvent>.Handle))!;
            Task task = (Task)handleMethod.Invoke(handler, [@event, cancellationToken])!;
            tasks.Add(task);
        }

        if (tasks.Count > 0)
        {
            await Task.WhenAll(tasks);
            _logger.LogDebug("Processed event {EventType} with {HandlerCount} handlers",
                eventType.Name, tasks.Count);
        }
    }
}