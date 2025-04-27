using Microsoft.Extensions.Logging;

namespace CRISP.Core.Resilience;

/// <summary>
/// A resilience strategy that retries operations a specified number of times with exponential backoff.
/// </summary>
public class RetryStrategy : IResilienceStrategy
{
    private readonly ILogger<RetryStrategy> _logger;
    private readonly int _maxRetryAttempts;
    private readonly TimeSpan _initialDelay;
    private readonly double _backoffFactor;
    private readonly Func<Exception, bool>? _retryPredicate;

    /// <summary>
    /// Initializes a new instance of the <see cref="RetryStrategy"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="maxRetryAttempts">The maximum number of retry attempts.</param>
    /// <param name="initialDelay">The initial delay between retries.</param>
    /// <param name="backoffFactor">The factor by which to increase the delay after each retry.</param>
    /// <param name="retryPredicate">Optional predicate to determine if the exception is retryable.</param>
    public RetryStrategy(
        ILogger<RetryStrategy> logger,
        int maxRetryAttempts = 3,
        TimeSpan? initialDelay = null,
        double backoffFactor = 2.0,
        Func<Exception, bool>? retryPredicate = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _maxRetryAttempts = maxRetryAttempts > 0 ? maxRetryAttempts : throw new ArgumentOutOfRangeException(nameof(maxRetryAttempts));
        _initialDelay = initialDelay ?? TimeSpan.FromSeconds(1);
        _backoffFactor = backoffFactor > 1.0 ? backoffFactor : throw new ArgumentOutOfRangeException(nameof(backoffFactor));
        _retryPredicate = retryPredicate;
    }

    /// <inheritdoc />
    public async ValueTask<T> Execute<T>(Func<CancellationToken, ValueTask<T>> operation, CancellationToken cancellationToken = default)
    {
        int attempts = 0;
        TimeSpan delay = _initialDelay;
        Exception? lastException = null;

        while (attempts <= _maxRetryAttempts)
        {
            try
            {
                if (attempts > 0)
                {
                    _logger.LogDebug("Retry attempt {AttemptNumber} of {MaxAttempts}", 
                        attempts, _maxRetryAttempts);
                }

                return await operation(cancellationToken);
            }
            catch (Exception ex) when (ShouldRetry(ex) && attempts < _maxRetryAttempts)
            {
                lastException = ex;
                attempts++;

                _logger.LogWarning(ex, "Operation failed. Retrying in {Delay}ms. Attempt {AttemptNumber} of {MaxAttempts}",
                    delay.TotalMilliseconds, attempts, _maxRetryAttempts);

                await Task.Delay(delay, cancellationToken);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * _backoffFactor);
            }
        }

        // If we get here, all retry attempts failed
        throw new RetryFailedException($"Operation failed after {_maxRetryAttempts} retry attempts", lastException!);
    }

    /// <inheritdoc />
    public async ValueTask Execute(Func<CancellationToken, ValueTask> operation, CancellationToken cancellationToken = default)
    {
        int attempts = 0;
        TimeSpan delay = _initialDelay;
        Exception? lastException = null;

        while (attempts <= _maxRetryAttempts)
        {
            try
            {
                if (attempts > 0)
                {
                    _logger.LogDebug("Retry attempt {AttemptNumber} of {MaxAttempts}", 
                        attempts, _maxRetryAttempts);
                }

                await operation(cancellationToken);
                return;
            }
            catch (Exception ex) when (ShouldRetry(ex) && attempts < _maxRetryAttempts)
            {
                lastException = ex;
                attempts++;

                _logger.LogWarning(ex, "Operation failed. Retrying in {Delay}ms. Attempt {AttemptNumber} of {MaxAttempts}",
                    delay.TotalMilliseconds, attempts, _maxRetryAttempts);

                await Task.Delay(delay, cancellationToken);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * _backoffFactor);
            }
        }

        // If we get here, all retry attempts failed
        throw new RetryFailedException($"Operation failed after {_maxRetryAttempts} retry attempts", lastException!);
    }

    private bool ShouldRetry(Exception exception)
    {
        return _retryPredicate?.Invoke(exception) ?? IsTransientException(exception);
    }

    private static bool IsTransientException(Exception exception)
    {
        // Common transient exceptions, add more as needed
        return exception is TimeoutException ||
               exception is System.Net.Http.HttpRequestException ||
               exception is System.IO.IOException ||
               exception.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
               exception.Message.Contains("temporarily unavailable", StringComparison.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Exception thrown when all retry attempts have failed.
/// </summary>
public class RetryFailedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RetryFailedException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public RetryFailedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}