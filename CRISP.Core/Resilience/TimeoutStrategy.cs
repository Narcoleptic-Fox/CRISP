using Microsoft.Extensions.Logging;

namespace CRISP.Core.Resilience;

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
        TimeSpan? timeout = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _timeout = timeout ?? TimeSpan.FromSeconds(30);
    }

    /// <inheritdoc />
    public async ValueTask<T> Execute<T>(Func<CancellationToken, ValueTask<T>> operation, CancellationToken cancellationToken = default)
    {
        using var timeoutCts = new CancellationTokenSource(_timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken);
        
        try
        {
            _logger.LogDebug("Executing operation with timeout of {Timeout}ms", _timeout.TotalMilliseconds);
            return await operation(linkedCts.Token);
        }
        catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Operation timed out after {Timeout}ms", _timeout.TotalMilliseconds);
            throw new TimeoutException($"Operation timed out after {_timeout.TotalMilliseconds}ms");
        }
    }

    /// <inheritdoc />
    public async ValueTask Execute(Func<CancellationToken, ValueTask> operation, CancellationToken cancellationToken = default)
    {
        using var timeoutCts = new CancellationTokenSource(_timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken);
        
        try
        {
            _logger.LogDebug("Executing operation with timeout of {Timeout}ms", _timeout.TotalMilliseconds);
            await operation(linkedCts.Token);
        }
        catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Operation timed out after {Timeout}ms", _timeout.TotalMilliseconds);
            throw new TimeoutException($"Operation timed out after {_timeout.TotalMilliseconds}ms");
        }
    }
}