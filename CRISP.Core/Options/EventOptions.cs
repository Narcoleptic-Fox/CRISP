namespace CRISP.Core.Options;

/// <summary>
/// Options for configuring the domain events system.
/// </summary>
public class EventOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to throw exceptions when an event handler fails.
    /// Default is false, which means failures will be logged but won't interrupt event processing.
    /// </summary>
    public bool ThrowOnHandlerFailure { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to process events in parallel.
    /// Default is false, which means events are processed sequentially.
    /// </summary>
    public bool ProcessEventsInParallel { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum degree of parallelism when processing events in parallel.
    /// Default is -1, which means no limit. Only relevant if ProcessEventsInParallel is true.
    /// </summary>
    public int MaxDegreeOfParallelism { get; set; } = -1;

    /// <summary>
    /// Gets or sets the maximum number of events to dispatch in a batch.
    /// Default is 100. Used when dispatching a large number of events to prevent resource exhaustion.
    /// </summary>
    public int MaxBatchSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets a value indicating whether to log detailed event processing information.
    /// Default is true.
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to use channels for event processing.
    /// Default is false, which means events are processed immediately.
    /// </summary>
    public bool UseChannels { get; set; } = false;
}

/// <summary>
/// Options for configuring channel-based event processing.
/// </summary>
public class ChannelEventOptions
{
    /// <summary>
    /// Gets or sets the capacity of the event channel.
    /// Default is 1000. If set to 0 or negative, an unbounded channel is used.
    /// </summary>
    public int ChannelCapacity { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the number of consumers processing events from the channel.
    /// Default is Environment.ProcessorCount to match the number of available processors.
    /// </summary>
    public int ConsumerCount { get; set; } = Environment.ProcessorCount;

    /// <summary>
    /// Gets or sets the time in milliseconds to wait when the channel is full.
    /// Default is 1000 (1 second). If set to 0 or negative, it will wait indefinitely.
    /// </summary>
    public int FullChannelWaitTimeMs { get; set; } = 1000;

    /// <summary>
    /// Gets or sets a value indicating whether to wait for all events to be processed when shutting down.
    /// Default is true.
    /// </summary>
    public bool WaitForChannelDrainOnDispose { get; set; } = true;

    /// <summary>
    /// Gets or sets the timeout in milliseconds to wait for the channel to drain on shutdown.
    /// Default is 10000 (10 seconds). If set to 0 or negative, it will wait indefinitely.
    /// </summary>
    public int ChannelDrainTimeoutMs { get; set; } = 10000;
}