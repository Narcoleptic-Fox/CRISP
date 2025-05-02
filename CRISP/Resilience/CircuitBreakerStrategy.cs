using Microsoft.Extensions.Logging;

namespace CRISP.Resilience;

/// <summary>
/// Circuit breaker states.
/// </summary>
public enum CircuitState
{
    /// <summary>
    /// The circuit is closed and operations are allowed to execute normally.
    /// </summary>
    Closed,

    /// <summary>
    /// The circuit is open and operations will immediately fail.
    /// </summary>
    Open,

    /// <summary>
    /// The circuit is half-open and allows a limited number of operations to test if the system has recovered.
    /// </summary>
    HalfOpen
}

/// <summary>
/// A resilience strategy that implements the circuit breaker pattern to prevent repeated calls to failing systems.
/// </summary>
public class CircuitBreakerStrategy : IResilienceStrategy
{
    private readonly ILogger<CircuitBreakerStrategy> _logger;
    private readonly int _failureThreshold;
    private readonly TimeSpan _durationOfBreak;
    private readonly object _lock = new();

    private CircuitState _state = CircuitState.Closed;
    private int _failureCount;
    private DateTime _lastFailureTime;
    private DateTime _openUntil;

    /// <summary>
    /// Initializes a new instance of the <see cref="CircuitBreakerStrategy"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="failureThreshold">The number of consecutive failures required to open the circuit.</param>
    /// <param name="resetTimeout">The duration that the circuit will stay open before transitioning to half-open.</param>
    public CircuitBreakerStrategy(
        ILogger<CircuitBreakerStrategy> logger,
        TimeSpan resetTimeout,
        int failureThreshold = 5)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _failureThreshold = failureThreshold > 0 ? failureThreshold : throw new ArgumentOutOfRangeException(nameof(failureThreshold));
        _durationOfBreak = resetTimeout > TimeSpan.Zero ? resetTimeout : throw new ArgumentOutOfRangeException(nameof(resetTimeout));
    }

    /// <inheritdoc />
    public async ValueTask<T> Execute<T>(Func<CancellationToken, ValueTask<T>> operation, CancellationToken cancellationToken = default)
    {
        if (operation is null) throw new ArgumentNullException(nameof(operation));
        EnsureCircuitAllowsOperation();

        try
        {
            T? result = await operation(cancellationToken);
            OnOperationSuccess();
            return result;
        }
        catch (Exception ex)
        {
            OnOperationFailure(ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async ValueTask Execute(Func<CancellationToken, ValueTask> operation, CancellationToken cancellationToken = default)
    {
        if (operation is null) throw new ArgumentNullException(nameof(operation));
        EnsureCircuitAllowsOperation();

        try
        {
            await operation(cancellationToken);
            OnOperationSuccess();
        }
        catch (Exception ex)
        {
            OnOperationFailure(ex);
            throw;
        }
    }

    private void EnsureCircuitAllowsOperation()
    {
        lock (_lock)
        {
            switch (_state)
            {
                case CircuitState.Open:
                    // If the break duration has passed, transition to half-open
                    if (DateTime.UtcNow >= _openUntil)
                    {
                        _logger.LogInformation("Circuit transitioning from Open to Half-Open");
                        _state = CircuitState.HalfOpen;
                    }
                    else
                    {
                        _logger.LogWarning("Circuit is Open, operation rejected. Circuit will remain open until {OpenUntil}", _openUntil);
                        throw new CircuitBreakerOpenException($"Circuit is open and is not allowing calls. Circuit will remain open until {_openUntil}");
                    }
                    break;

                case CircuitState.HalfOpen:
                    _logger.LogDebug("Circuit is Half-Open, allowing test operation");
                    break;

                case CircuitState.Closed:
                    _logger.LogDebug("Circuit is Closed, allowing operation");
                    break;
            }
        }
    }

    private void OnOperationSuccess()
    {
        lock (_lock)
        {
            if (_state == CircuitState.HalfOpen)
            {
                _logger.LogInformation("Test operation succeeded, circuit transitioning from Half-Open to Closed");

                // Reset the circuit
                _state = CircuitState.Closed;
                _failureCount = 0;
            }
            else if (_state == CircuitState.Closed)
            {
                // Reset any tracked failures
                _failureCount = 0;
            }
        }
    }

    private void OnOperationFailure(Exception exception)
    {
        lock (_lock)
        {
            _lastFailureTime = DateTime.UtcNow;

            switch (_state)
            {
                case CircuitState.HalfOpen:
                    // If a test operation fails, open the circuit again
                    _openUntil = _lastFailureTime + _durationOfBreak;
                    _state = CircuitState.Open;
                    _logger.LogWarning(exception, "Test operation failed, circuit transitioning from Half-Open to Open until {OpenUntil}", _openUntil);
                    break;

                case CircuitState.Closed:
                    _failureCount++;

                    if (_failureCount >= _failureThreshold)
                    {
                        _openUntil = _lastFailureTime + _durationOfBreak;
                        _state = CircuitState.Open;
                        _logger.LogWarning(exception, "Failure threshold reached, circuit transitioning from Closed to Open until {OpenUntil}", _openUntil);
                    }
                    else
                    {
                        _logger.LogWarning(exception, "Operation failed, failure count: {FailureCount}/{FailureThreshold}", _failureCount, _failureThreshold);
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Gets the current state of the circuit.
    /// </summary>
    public CircuitState State
    {
        get
        {
            lock (_lock)
            {
                if (_state == CircuitState.Open && DateTime.UtcNow >= _openUntil)
                    _state = CircuitState.HalfOpen;

                return _state;
            }
        }
    }
}

/// <summary>
/// Exception thrown when the circuit breaker is open.
/// </summary>
public class CircuitBreakerOpenException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CircuitBreakerOpenException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public CircuitBreakerOpenException(string message)
        : base(message)
    {
    }
}