using Microsoft.Extensions.Logging;

namespace CRISP.Resilience;

/// <summary>
/// A resilience strategy that applies a timeout to operations.
/// </summary>
public class TimeoutStrategy : IResilienceStrategy
{
    private readonly ILogger<TimeoutStrategy> _logger;
    private readonly TimeSpan _timeout;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeoutStrategy"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="timeout">The timeout duration.</param>
    public TimeoutStrategy(
        ILogger<TimeoutStrategy> logger,
        TimeSpan timeout)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        if (timeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be greater than zero.");
        _timeout = timeout;
    }

    /// <inheritdoc />
    public async ValueTask<T> Execute<T>(Func<CancellationToken, ValueTask<T>> operation, CancellationToken cancellationToken = default)
    {
        using CancellationTokenSource timeoutCts = new(_timeout);
        using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken);

        try
        {
            _logger.LogDebug("Executing operation with timeout of {Timeout}ms", _timeout.TotalMilliseconds);

            // Create task for the operation
            ValueTask<T> operationTask = operation(linkedCts.Token);

            // Create a task that completes after the timeout
            Task timeoutTask = Task.Delay(Timeout.InfiniteTimeSpan, timeoutCts.Token);

            // Wait for either operation completion or timeout
            Task completedTask = await Task.WhenAny(operationTask.AsTask(), timeoutTask);

            // If the timeout task completed first, throw timeout exception
            if (completedTask == timeoutTask)
            {
                _logger.LogWarning("Operation timed out after {Timeout}ms", _timeout.TotalMilliseconds);
                throw new TimeoutException($"Operation timed out after {_timeout.TotalMilliseconds}ms");
            }

            // Operation completed successfully, return result
            return await operationTask;
        }
        catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Operation timed out after {Timeout}ms", _timeout.TotalMilliseconds);
            throw new TimeoutException($"Operation timed out after {_timeout.TotalMilliseconds}ms");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // If operation was canceled due to the provided token, propagate the exception
            throw;
        }
    }

    /// <inheritdoc />
    public async ValueTask Execute(Func<CancellationToken, ValueTask> operation, CancellationToken cancellationToken = default)
    {
        using CancellationTokenSource timeoutCts = new(_timeout);
        using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken);

        try
        {
            _logger.LogDebug("Executing operation with timeout of {Timeout}ms", _timeout.TotalMilliseconds);

            // Create task for the operation
            Task operationTask = operation(linkedCts.Token).AsTask();

            // Create a task that completes after the timeout
            Task timeoutTask = Task.Delay(Timeout.InfiniteTimeSpan, timeoutCts.Token);

            // Wait for either operation completion or timeout
            Task completedTask = await Task.WhenAny(operationTask, timeoutTask);

            // If the timeout task completed first, throw timeout exception
            if (completedTask == timeoutTask)
            {
                _logger.LogWarning("Operation timed out after {Timeout}ms", _timeout.TotalMilliseconds);
                throw new TimeoutException($"Operation timed out after {_timeout.TotalMilliseconds}ms");
            }

            // Operation completed successfully
            await operationTask;
        }
        catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Operation timed out after {Timeout}ms", _timeout.TotalMilliseconds);
            throw new TimeoutException($"Operation timed out after {_timeout.TotalMilliseconds}ms");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // If operation was canceled due to the provided token, propagate the exception
            throw;
        }
    }
}