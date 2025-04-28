using CRISP.Core.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace CRISP.Core.Events;

/// <summary>
/// Implementation of <see cref="IEventDispatcher"/> that uses System.Threading.Channels 
/// for high-throughput event processing.
/// </summary>
public class ChannelEventDispatcher : IEventDispatcher, IDisposable, IAsyncDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ChannelEventDispatcher> _logger;
    private readonly EventOptions _eventOptions;
    private readonly ChannelEventOptions _channelOptions;
    private readonly Channel<IDomainEvent> _channel;
    private readonly CancellationTokenSource _consumerCts;
    private readonly List<Task> _consumers;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _initialized;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChannelEventDispatcher"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="eventOptions">The event options.</param>
    /// <param name="channelOptions">The channel configuration options.</param>
    public ChannelEventDispatcher(
        IServiceProvider serviceProvider,
        ILogger<ChannelEventDispatcher> logger,
        EventOptions eventOptions,
        ChannelEventOptions channelOptions)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventOptions = eventOptions ?? throw new ArgumentNullException(nameof(eventOptions));
        _channelOptions = channelOptions ?? throw new ArgumentNullException(nameof(channelOptions));

        _channel = _channelOptions.ChannelCapacity > 0
            ? Channel.CreateBounded<IDomainEvent>(new BoundedChannelOptions(_channelOptions.ChannelCapacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleWriter = false,
                SingleReader = false,
                AllowSynchronousContinuations = true
            })
            : Channel.CreateUnbounded<IDomainEvent>(new UnboundedChannelOptions
            {
                SingleWriter = false,
                SingleReader = false,
                AllowSynchronousContinuations = true
            });

        _consumerCts = new CancellationTokenSource();
        _consumers = new List<Task>(_channelOptions.ConsumerCount);
        _initialized = false;
    }

    /// <summary>
    /// Initialize the event consumers.
    /// </summary>
    private async Task EnsureInitializedAsync()
    {
        if (_initialized) return;

        await _initLock.WaitAsync();
        try
        {
            if (_initialized) return;

            for (int i = 0; i < _channelOptions.ConsumerCount; i++)
            {
                int consumerId = i + 1;
                Task consumerTask = ConsumeEventsAsync(consumerId, _consumerCts.Token);
                _consumers.Add(consumerTask);
            }

            _initialized = true;

            if (_eventOptions.EnableDetailedLogging)
            {
                _logger.LogInformation(
                    "ChannelEventDispatcher initialized with {ConsumerCount} consumers and channel capacity of {ChannelCapacity}",
                    _channelOptions.ConsumerCount,
                    _channelOptions.ChannelCapacity > 0 ? _channelOptions.ChannelCapacity.ToString() : "unbounded");
            }
        }
        finally
        {
            _initLock.Release();
        }
    }

    /// <summary>
    /// Consumer task that processes events from the channel.
    /// </summary>
    /// <param name="consumerId">The ID of this consumer.</param>
    /// <param name="cancellationToken">Cancellation token to stop processing.</param>
    private async Task ConsumeEventsAsync(int consumerId, CancellationToken cancellationToken)
    {
        if (_eventOptions.EnableDetailedLogging)
        {
            _logger.LogDebug("Event consumer {ConsumerId} started", consumerId);
        }

        try
        {
            while (!cancellationToken.IsCancellationRequested && await _channel.Reader.WaitToReadAsync(cancellationToken))
            {
                while (_channel.Reader.TryRead(out IDomainEvent? @event))
                {
                    try
                    {
                        await ProcessEventAsync(@event, cancellationToken);
                    }
                    catch (Exception ex) when (!_eventOptions.ThrowOnHandlerFailure)
                    {
                        _logger.LogError(ex, "Error in event consumer {ConsumerId} processing event {EventType}",
                            consumerId, @event.GetType().Name);
                    }
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Normal shutdown
            if (_eventOptions.EnableDetailedLogging)
            {
                _logger.LogDebug("Event consumer {ConsumerId} shutdown", consumerId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in event consumer {ConsumerId}", consumerId);

            if (_eventOptions.ThrowOnHandlerFailure)
            {
                throw;
            }
        }
    }

    /// <summary>
    /// Process a single domain event by finding and invoking its handlers.
    /// </summary>
    /// <param name="event">The domain event to process.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    private async Task ProcessEventAsync(IDomainEvent @event, CancellationToken cancellationToken)
    {
        Type eventType = @event.GetType();

        if (_eventOptions.EnableDetailedLogging)
        {
            _logger.LogDebug("Processing domain event {EventType} - {EventId}", eventType.Name, @event.EventId);
        }

        // Create a scope to resolve handlers
        using IServiceScope scope = _serviceProvider.CreateScope();

        Type handlerType = typeof(IEventHandler<>).MakeGenericType(eventType);

        // Get all handlers for this event type
        List<object?> handlers = scope.ServiceProvider.GetServices(handlerType).Where(h => h != null).ToList();

        if (!handlers.Any())
        {
            if (_eventOptions.EnableDetailedLogging)
            {
                _logger.LogDebug("No handlers registered for {EventType}", eventType.Name);
            }
            return;
        }

        // Invoke all handlers
        if (_eventOptions.ProcessEventsInParallel)
        {
            IEnumerable<Task> tasks = handlers
                .Where(h => h != null)
                .Select(handler => InvokeHandlerAsync(handler!, @event, eventType, cancellationToken));

            await Task.WhenAll(tasks);
        }
        else
        {
            foreach (object? handler in handlers)
            {
                if (handler != null)
                {
                    await InvokeHandlerAsync(handler, @event, eventType, cancellationToken);
                }
            }
        }
    }

    /// <summary>
    /// Invoke a single event handler.
    /// </summary>
    private async Task InvokeHandlerAsync(object handler, IDomainEvent @event, Type eventType, CancellationToken cancellationToken)
    {
        try
        {
            // Get the Handle method
            System.Reflection.MethodInfo method = handler.GetType().GetInterfaceMap(
                typeof(IEventHandler<>).MakeGenericType(eventType)).TargetMethods.First();

            if (method == null)
            {
                throw new InvalidOperationException($"Handler for {eventType.Name} does not implement Handle method.");
            }

            // Invoke the Handle method
            object? result = method.Invoke(handler, new object[] { @event, cancellationToken });

            if (result == null)
            {
                throw new InvalidOperationException($"Handler for {eventType.Name} returned null.");
            }

            // Await the ValueTask
            await (ValueTask)result;

            if (_eventOptions.EnableDetailedLogging)
            {
                _logger.LogDebug("Handler {HandlerType} processed event {EventType}",
                    handler.GetType().Name, eventType.Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling domain event {EventType} by handler {HandlerType}",
                eventType.Name, handler.GetType().Name);

            // Throw exception if configured to do so
            if (_eventOptions.ThrowOnHandlerFailure)
            {
                throw;
            }
        }
    }

    /// <inheritdoc />
    public async ValueTask Dispatch(IDomainEvent @event, CancellationToken cancellationToken = default)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ChannelEventDispatcher));
        }

        if (@event == null)
        {
            throw new ArgumentNullException(nameof(@event));
        }

        await EnsureInitializedAsync();

        if (_eventOptions.EnableDetailedLogging)
        {
            _logger.LogDebug("Enqueuing domain event {EventType} - {EventId}", @event.GetType().Name, @event.EventId);
        }

        if (_channelOptions.FullChannelWaitTimeMs > 0)
        {
            // Use a timeout when the channel is full
            using CancellationTokenSource timeoutCts = new(TimeSpan.FromMilliseconds(_channelOptions.FullChannelWaitTimeMs));
            using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken);

            try
            {
                await _channel.Writer.WriteAsync(@event, linkedCts.Token);
            }
            catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Timeout waiting to enqueue event {EventType} - channel is full",
                    @event.GetType().Name);
                throw new TimeoutException($"Timeout waiting to enqueue event {@event.GetType().Name} - channel is full");
            }
        }
        else
        {
            // Wait indefinitely
            await _channel.Writer.WriteAsync(@event, cancellationToken);
        }
    }

    /// <inheritdoc />
    public async ValueTask DispatchAll(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ChannelEventDispatcher));
        }

        if (events == null) return;

        List<IDomainEvent> eventsList = events.ToList();
        if (!eventsList.Any()) return;

        await EnsureInitializedAsync();

        // Process events in batches if needed
        if (eventsList.Count > _eventOptions.MaxBatchSize)
        {
            for (int i = 0; i < eventsList.Count; i += _eventOptions.MaxBatchSize)
            {
                IEnumerable<IDomainEvent> batch = eventsList.Skip(i).Take(_eventOptions.MaxBatchSize);
                await DispatchBatchAsync(batch, cancellationToken);
            }
        }
        else
        {
            await DispatchBatchAsync(eventsList, cancellationToken);
        }
    }

    /// <summary>
    /// Dispatch a batch of events to the channel.
    /// </summary>
    private async ValueTask DispatchBatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken)
    {
        foreach (IDomainEvent @event in events)
        {
            await Dispatch(@event, cancellationToken);
        }
    }

    /// <summary>
    /// Dispose of resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose of resources asynchronously.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        Dispose(disposing: false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Protected implementation of Dispose pattern.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            ShutdownConsumers();
            _consumerCts.Dispose();
            _initLock.Dispose();
        }

        _disposed = true;
    }

    /// <summary>
    /// Protected implementation of async Dispose pattern.
    /// </summary>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_disposed) return;

        await ShutdownConsumersAsync();
        _consumerCts.Dispose();
        _initLock.Dispose();

        _disposed = true;
    }

    /// <summary>
    /// Shutdown event consumers.
    /// </summary>
    private void ShutdownConsumers()
    {
        if (!_initialized) return;

        // Signal cancellation
        _consumerCts.Cancel();

        // Complete the channel
        _channel.Writer.Complete();

        if (_channelOptions.WaitForChannelDrainOnDispose)
        {
            try
            {
                // Wait for all consumers to finish with timeout
                Task.WaitAll(_consumers.ToArray(), _channelOptions.ChannelDrainTimeoutMs);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error waiting for event consumers to shut down");
            }
        }
    }

    /// <summary>
    /// Shutdown event consumers asynchronously.
    /// </summary>
    private async Task ShutdownConsumersAsync()
    {
        if (!_initialized) return;

        // Signal cancellation
        _consumerCts.Cancel();

        // Complete the channel
        _channel.Writer.Complete();

        if (_channelOptions.WaitForChannelDrainOnDispose)
        {
            try
            {
                if (_channelOptions.ChannelDrainTimeoutMs > 0)
                {
                    // Wait for all consumers to finish with timeout
                    await Task.WhenAll(_consumers.Select(task =>
                        Task.WhenAny(task, Task.Delay(_channelOptions.ChannelDrainTimeoutMs))));
                }
                else
                {
                    // Wait indefinitely
                    await Task.WhenAll(_consumers);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error waiting for event consumers to shut down");
            }
        }
    }
}