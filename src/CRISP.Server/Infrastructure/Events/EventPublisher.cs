using CRISP.Server.Common;
using System.Threading.Channels;

namespace CRISP.Server.Infrastructure.Events;

/// <summary>
/// In-process <see cref="IEventPublisher"/> backed by a bounded channel. Owns
/// the single channel that <see cref="EventWorker"/> drains, so it must be
/// registered as a singleton.
/// </summary>
internal sealed class EventPublisher : IEventPublisher
{
    private const int Capacity = 500;

    private readonly Channel<BaseEvent> _channel = Channel.CreateBounded<BaseEvent>(
        new BoundedChannelOptions(Capacity)
        {
            // Block the publisher when the channel is full rather than dropping
            // events; the single worker is the only reader.
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });

    /// <summary>
    /// Queues an event for asynchronous dispatch. Completes once the event has
    /// been written to the channel, or waits if the channel is at capacity.
    /// </summary>
    /// <typeparam name="TEvent">The concrete event type.</typeparam>
    /// <param name="event">The event to publish.</param>
    /// <param name="cancellationToken">A token that cancels the write.</param>
    /// <returns>A task that completes when the event has been queued.</returns>
    public ValueTask Publish<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : BaseEvent =>
        _channel.Writer.WriteAsync(@event, cancellationToken);

    /// <summary>
    /// Reads queued events until the channel is completed or the token is
    /// cancelled. Consumed only by <see cref="EventWorker"/>.
    /// </summary>
    /// <param name="cancellationToken">A token that stops reading.</param>
    /// <returns>An async stream of published events.</returns>
    public IAsyncEnumerable<BaseEvent> ReadAllAsync(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAllAsync(cancellationToken);
}
