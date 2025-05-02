using Microsoft.Extensions.Logging;

namespace CRISP.Resilience;

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
        // Check for cancellation first
        cancellationToken.ThrowIfCancellationRequested();

        int attempts = 0;
        TimeSpan delay = _initialDelay;
        Exception? lastException = null;
        bool shouldRetry = true;

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
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Propagate cancellation exceptions
                throw;
            }
            catch (Exception ex)
            {
                lastException = ex;
                attempts++;
                shouldRetry = attempts <= _maxRetryAttempts && ShouldRetry(ex);

                if (shouldRetry)
                {
                    _logger.LogWarning(ex, "Operation failed. Retrying in {Delay}ms. Attempt {AttemptNumber} of {MaxAttempts}",
                        delay.TotalMilliseconds, attempts, _maxRetryAttempts);

                    try
                    {
                        await Task.Delay(delay, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        // Cancellation during delay, propagate it
                        throw;
                    }

                    delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * _backoffFactor);
                }
                else
                {
                    // Max retries reached or exception shouldn't be retried
                    break;
                }
            }
        }

        // If we get here, all retry attempts failed or exception wasn't retryable
        throw new RetryFailedException($"Operation failed after {attempts - 1} retry attempts", lastException!);
    }

    /// <inheritdoc />
    public async ValueTask Execute(Func<CancellationToken, ValueTask> operation, CancellationToken cancellationToken = default)
    {
        // Check for cancellation first
        cancellationToken.ThrowIfCancellationRequested();

        int attempts = 0;
        TimeSpan delay = _initialDelay;
        Exception? lastException = null;
        bool shouldRetry = true;

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
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Propagate cancellation exceptions
                throw;
            }
            catch (Exception ex)
            {
                lastException = ex;
                attempts++;
                shouldRetry = attempts <= _maxRetryAttempts && ShouldRetry(ex);

                if (shouldRetry)
                {
                    _logger.LogWarning(ex, "Operation failed. Retrying in {Delay}ms. Attempt {AttemptNumber} of {MaxAttempts}",
                        delay.TotalMilliseconds, attempts, _maxRetryAttempts);

                    try
                    {
                        await Task.Delay(delay, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        // Cancellation during delay, propagate it
                        throw;
                    }

                    delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * _backoffFactor);
                }
                else
                {
                    // Max retries reached or exception shouldn't be retried
                    break;
                }
            }
        }

        // If we get here, all retry attempts failed or exception wasn't retryable
        throw new RetryFailedException($"Operation failed after {attempts - 1} retry attempts", lastException!);
    }

    private bool ShouldRetry(Exception exception) => _retryPredicate?.Invoke(exception) ?? IsTransientException(exception);

    /// <summary>
    /// Determines if an exception is considered transient and should be retried.
    /// </summary>
    /// <param name="exception">The exception to check.</param>
    /// <returns>True if the exception is considered transient; otherwise, false.</returns>
    public static bool IsTransientException(Exception exception)
    {
        if (exception == null)
            return false;

        // Common transient exceptions, add more as needed
        return exception is TimeoutException ||
               exception is HttpRequestException ||
               exception is IOException ||
               exception.Message != null && (
                   exception.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
                   exception.Message.Contains("timed out", StringComparison.OrdinalIgnoreCase) ||
                   exception.Message.Contains("temporarily unavailable", StringComparison.OrdinalIgnoreCase)
               );
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