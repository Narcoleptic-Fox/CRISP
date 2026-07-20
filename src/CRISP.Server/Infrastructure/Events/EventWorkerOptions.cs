namespace CRISP.Server.Infrastructure.Events;

/// <summary>
/// Configures retry behaviour for the background event dispatch worker.
/// </summary>
public sealed class EventWorkerOptions
{
    /// <summary>
    /// The configuration section that binds to these options.
    /// </summary>
    public const string SectionName = "Events";

    /// <summary>
    /// Total number of attempts made to invoke a handler before the event is
    /// logged as failed and dropped. Must be at least 1.
    /// </summary>
    public int MaxAttempts { get; set; } = 3;

    /// <summary>
    /// Base delay applied between retry attempts. The delay is doubled on each
    /// subsequent attempt (exponential backoff).
    /// </summary>
    public TimeSpan RetryBaseDelay { get; set; } = TimeSpan.FromSeconds(2);
}
