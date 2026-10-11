using Microsoft.Extensions.Options;
using CRISP.Server.Common;
using System.Collections.Concurrent;
using System.Reflection;

namespace CRISP.Server.Infrastructure.Events;

/// <summary>
/// Drains published events from the in-process channel and dispatches each one
/// to every registered <see cref="IEventHandler{TEvent}"/>. Handlers are
/// resolved per event from a fresh dependency injection scope, retried with
/// exponential backoff on failure, and isolated so one failing handler never
/// stops the worker or blocks the others.
/// </summary>
internal sealed class EventWorker(
    EventPublisher publisher,
    IServiceScopeFactory scopeFactory,
    TimeProvider timeProvider,
    IOptions<EventWorkerOptions> options,
    ILogger<EventWorker> logger) : BackgroundService
{
    private readonly EventWorkerOptions _options = options.Value;

    // The runtime type of an event is only known when it is read off the
    // channel, but IEventHandler<TEvent> is generic. Cache one closed
    // DispatchToHandlersAsync<TEvent> MethodInfo per concrete event type so the
    // reflection cost is paid once per type rather than once per event.
    private static readonly ConcurrentDictionary<Type, MethodInfo> _dispatchers = new();

    private static readonly MethodInfo _dispatchDefinition = typeof(EventWorker)
        .GetMethod(nameof(DispatchToHandlersAsync), BindingFlags.Instance | BindingFlags.NonPublic)!;

    /// <summary>
    /// Continuously reads events from the publisher channel until the host
    /// shuts down.
    /// </summary>
    /// <param name="stoppingToken">A token that stops the background service.</param>
    /// <returns>A task that completes when the background service stops.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var @event in publisher.ReadAllAsync(stoppingToken))
            {
                await DispatchAsync(@event, stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    private Task DispatchAsync(BaseEvent @event, CancellationToken cancellationToken)
    {
        var dispatcher = _dispatchers.GetOrAdd(
            @event.GetType(),
            eventType => _dispatchDefinition.MakeGenericMethod(eventType));

        return (Task)dispatcher.Invoke(this, [@event, cancellationToken])!;
    }

    private async Task DispatchToHandlersAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
        where TEvent : BaseEvent
    {
        using var scope = scopeFactory.CreateScope();
        var handlers = scope.ServiceProvider.GetServices<IEventHandler<TEvent>>();

        foreach (var handler in handlers)
        {
            await InvokeWithRetryAsync(handler, @event, cancellationToken);
        }
    }

    private async Task InvokeWithRetryAsync<TEvent>(
        IEventHandler<TEvent> handler,
        TEvent @event,
        CancellationToken cancellationToken)
        where TEvent : BaseEvent
    {
        var handlerName = handler.GetType().Name;
        var eventName = typeof(TEvent).Name;

        for (var attempt = 1; attempt <= _options.MaxAttempts; attempt++)
        {
            try
            {
                await handler.HandleAsync(@event, cancellationToken);
                return;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (attempt == _options.MaxAttempts)
                {
                    logger.LogError(
                        ex,
                        "Handler {Handler} failed for event {Event} after {Attempts} attempts; event dropped.",
                        handlerName,
                        eventName,
                        attempt);
                    return;
                }

                var delay = _options.RetryBaseDelay * Math.Pow(2, attempt - 1);
                logger.LogWarning(
                    ex,
                    "Handler {Handler} failed for event {Event} on attempt {Attempt}; retrying in {Delay}.",
                    handlerName,
                    eventName,
                    attempt,
                    delay);

                await Task.Delay(delay, timeProvider, cancellationToken);
            }
        }
    }
}
