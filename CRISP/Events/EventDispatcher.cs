using CRISP.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace CRISP.Events;

/// <summary>
/// Default implementation of <see cref="IEventDispatcher"/> that dispatches events to all registered handlers.
/// </summary>
public class EventDispatcher : IEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventDispatcher> _logger;
    private readonly EventOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventDispatcher"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="options">The event options.</param>
    public EventDispatcher(
        IServiceProvider serviceProvider,
        ILogger<EventDispatcher> logger,
        EventOptions options)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public async ValueTask Dispatch(IEvent @event, CancellationToken cancellationToken = default)
    {
        Type eventType = @event.GetType();

        if (_options.EnableDetailedLogging)
            _logger.LogDebug("Processing domain event {EventType}", eventType.Name);

        Type handlerType = typeof(IEventHandler<>).MakeGenericType(eventType);

        // Get all handlers for this event type
        List<object?> handlers = _serviceProvider.GetServices(handlerType).Where(h => h != null).ToList();

        if (!handlers.Any())
        {
            if (_options.EnableDetailedLogging)
                _logger.LogDebug("No handlers registered for {EventType}", eventType.Name);
            return;
        }

        // Invoke all handlers
        if (_options.ProcessEventsInParallel)
            await InvokeHandlersInParallel(handlers, @event, eventType, cancellationToken);
        else
        {
            await InvokeHandlersSequentially(handlers, @event, eventType, cancellationToken);
        }
    }

    private async ValueTask InvokeHandlersSequentially(
        IEnumerable<object?> handlers,
        IEvent @event,
        Type eventType,
        CancellationToken cancellationToken)
    {
        foreach (object? handler in handlers)
        {
            if (handler != null)
                await InvokeHandler(handler, @event, eventType, cancellationToken);
        }
    }

    private async ValueTask InvokeHandlersInParallel(
        IEnumerable<object?> handlers,
        IEvent @event,
        Type eventType,
        CancellationToken cancellationToken)
    {
        IEnumerable<Task> tasks = handlers
            .Where(h => h != null)
            .Select(handler =>
                InvokeHandler(handler!, @event, eventType, cancellationToken).AsTask());

        if (_options.MaxDegreeOfParallelism > 0)
        {
#if NET9_0
            OrderablePartitioner<Task> partitioner = Partitioner.Create(tasks);

            ParallelOptions parallelOptions = new()
            {
                MaxDegreeOfParallelism = _options.MaxDegreeOfParallelism,
                CancellationToken = cancellationToken
            };

            await Parallel.ForEachAsync(
                partitioner.GetPartitions(_options.MaxDegreeOfParallelism),
                parallelOptions,
                async (partition, ct) =>
                {
                    using (partition)
                    {
                        while (partition.MoveNext())
                        {
                            await partition.Current;
                        }
                    }
                });
#else
            // For .NET Standard 2.1, implement a custom limited parallel execution
            List<Task> taskList = tasks.ToList();
            SemaphoreSlim semaphore = new(_options.MaxDegreeOfParallelism);
            List<Task> runningTasks = [];

            foreach (Task? task in taskList)
            {
                // Wait until we have an available slot
                await semaphore.WaitAsync(cancellationToken);

                // Start a new task that will release the semaphore when done
                Task runningTask = task.ContinueWith(t =>
                {
                    semaphore.Release();
                    // Propagate any exceptions
                    if (t.IsFaulted && t.Exception != null)
                        throw t.Exception;
                }, cancellationToken);

                runningTasks.Add(runningTask);
            }

            // Wait for all tasks to complete
            await Task.WhenAll(runningTasks);
#endif
        }
        else
        {
            await Task.WhenAll(tasks);
        }
    }

    private async ValueTask InvokeHandler(
        object handler,
        IEvent @event,
        Type eventType,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get the Handle method
            System.Reflection.MethodInfo method = handler.GetType().GetInterfaceMap(
                typeof(IEventHandler<>).MakeGenericType(eventType)).TargetMethods.First();

            if (method == null)
                throw new InvalidOperationException($"Handler for {eventType.Name} does not implement Handle method.");

            // Invoke the Handle method
            object? result = method.Invoke(handler, [@event, cancellationToken]);

            if (result == null)
                throw new InvalidOperationException($"Handler for {eventType.Name} returned null.");

            // Await the ValueTask
            await (ValueTask)result;

            if (_options.EnableDetailedLogging)
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
            if (_options.ThrowOnHandlerFailure)
                throw;
        }
    }

    /// <inheritdoc />
    public async ValueTask DispatchAll(IEnumerable<IEvent> events, CancellationToken cancellationToken = default)
    {
        if (events == null) return;

        List<IEvent> eventsList = events.ToList();
        if (!eventsList.Any()) return;

        // Process events in batches if needed
        if (eventsList.Count > _options.MaxBatchSize)
        {
            for (int i = 0; i < eventsList.Count; i += _options.MaxBatchSize)
            {
                IEnumerable<IEvent> batch = eventsList.Skip(i).Take(_options.MaxBatchSize);
                await DispatchBatch(batch, cancellationToken);
            }
        }
        else
        {
            await DispatchBatch(eventsList, cancellationToken);
        }
    }

    private async ValueTask DispatchBatch(IEnumerable<IEvent> events, CancellationToken cancellationToken)
    {
        foreach (IEvent @event in events)
        {
            await Dispatch(@event, cancellationToken);
        }
    }
}