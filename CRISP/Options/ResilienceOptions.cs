namespace CRISP.Options;

/// <summary>
/// Options for configuring the resilience strategies.
/// </summary>
public class ResilienceOptions
{
    /// <summary>
    /// Gets or sets the options for the retry strategy.
    /// </summary>
    public RetryOptions Retry { get; set; } = new RetryOptions();

    /// <summary>
    /// Gets or sets the options for the circuit breaker strategy.
    /// </summary>
    public CircuitBreakerOptions CircuitBreaker { get; set; } = new CircuitBreakerOptions();

    /// <summary>
    /// Gets or sets the options for the timeout strategy.
    /// </summary>
    public TimeoutOptions Timeout { get; set; } = new TimeoutOptions();
}

/// <summary>
/// Options for configuring the retry strategy.
/// </summary>
public class RetryOptions
{
    /// <summary>
    /// Gets or sets the maximum number of retry attempts.
    /// Default is 3.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the initial delay between retries in seconds.
    /// Default is 1 second.
    /// </summary>
    public double InitialDelaySeconds { get; set; } = 1.0;

    /// <summary>
    /// Gets or sets the factor by which to increase the delay after each retry.
    /// Default is 2.0 (exponential backoff).
    /// </summary>
    public double BackoffFactor { get; set; } = 2.0;
}

/// <summary>
/// Options for configuring the circuit breaker strategy.
/// </summary>
public class CircuitBreakerOptions
{
    /// <summary>
    /// Gets or sets the number of consecutive failures required to open the circuit.
    /// Default is 5.
    /// </summary>
    public int FailureThreshold { get; set; } = 5;

    /// <summary>
    /// Gets or sets the duration in seconds that the circuit will stay open before transitioning to half-open.
    /// Default is 30 seconds.
    /// </summary>
    public int DurationOfBreakSeconds { get; set; } = 30;
}

/// <summary>
/// Options for configuring the timeout strategy.
/// </summary>
public class TimeoutOptions
{
    /// <summary>
    /// Gets or sets the timeout duration in seconds.
    /// Default is 30 seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}