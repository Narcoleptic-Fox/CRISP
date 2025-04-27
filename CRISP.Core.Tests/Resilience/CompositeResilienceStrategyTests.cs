using CRISP.Core.Options;
using CRISP.Core.Resilience;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CRISP.Core.Tests.Resilience;

public class CompositeResilienceStrategyTests
{
    private readonly Mock<ILogger<RetryStrategy>> _retryLoggerMock;
    private readonly Mock<ILogger<CircuitBreakerStrategy>> _circuitBreakerLoggerMock;
    private readonly Mock<ILogger<TimeoutStrategy>> _timeoutLoggerMock;
    private readonly int _maxRetryAttempts = 2;
    private readonly TimeSpan _initialDelay = TimeSpan.FromMilliseconds(50);
    private readonly double _backoffFactor = 2.0;
    private readonly int _failureThreshold = 3;
    private readonly TimeSpan _breakDuration = TimeSpan.FromMilliseconds(500);
    private readonly TimeSpan _timeout = TimeSpan.FromMilliseconds(200);

    public CompositeResilienceStrategyTests()
    {
        _retryLoggerMock = new Mock<ILogger<RetryStrategy>>();
        _circuitBreakerLoggerMock = new Mock<ILogger<CircuitBreakerStrategy>>();
        _timeoutLoggerMock = new Mock<ILogger<TimeoutStrategy>>();
    }

    [Fact]
    public async Task Execute_AllStrategies_SuccessfulExecution()
    {
        // Arrange
        var retryStrategy = new RetryStrategy(_retryLoggerMock.Object, _maxRetryAttempts, _initialDelay, _backoffFactor);
        var circuitBreakerStrategy = new CircuitBreakerStrategy(_circuitBreakerLoggerMock.Object, _failureThreshold, _breakDuration);
        var timeoutStrategy = new TimeoutStrategy(_timeoutLoggerMock.Object, _timeout);

        var compositeStrategy = new CompositeResilienceStrategy(
            new IResilienceStrategy[] { retryStrategy, circuitBreakerStrategy, timeoutStrategy });

        int callCount = 0;
        int expectedResult = 42;

        // Act
        int result = await compositeStrategy.Execute(ct =>
        {
            callCount++;
            return new ValueTask<int>(expectedResult);
        }, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        callCount.Should().Be(1);
    }

    [Fact]
    public async Task Execute_WithRetryAndSuccessfulRetry()
    {
        // Arrange
        var retryStrategy = new RetryStrategy(_retryLoggerMock.Object, _maxRetryAttempts, _initialDelay, _backoffFactor);
        var timeoutStrategy = new TimeoutStrategy(_timeoutLoggerMock.Object, _timeout);

        var compositeStrategy = new CompositeResilienceStrategy(
            new IResilienceStrategy[] { retryStrategy, timeoutStrategy });

        int callCount = 0;
        int expectedResult = 42;

        // Act
        int result = await compositeStrategy.Execute(ct =>
        {
            callCount++;
            if (callCount == 1)
            {
                throw new InvalidOperationException("First attempt fails");
            }
            return new ValueTask<int>(expectedResult);
        }, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        callCount.Should().Be(2); // Initial + 1 retry
    }

    [Fact]
    public async Task Execute_WithCircuitBreakerAndTimeout_TimeoutExceededException()
    {
        // Arrange
        var circuitBreakerStrategy = new CircuitBreakerStrategy(_circuitBreakerLoggerMock.Object, _failureThreshold, _breakDuration);
        
        // Short timeout to trigger timeout exception
        var shortTimeout = TimeSpan.FromMilliseconds(50);
        var timeoutStrategy = new TimeoutStrategy(_timeoutLoggerMock.Object, shortTimeout);

        var compositeStrategy = new CompositeResilienceStrategy(
            new IResilienceStrategy[] { circuitBreakerStrategy, timeoutStrategy });

        // Act
        Func<Task<int>> act = async () => await compositeStrategy.Execute<int>(async ct =>
        {
            // Operation takes longer than timeout
            await Task.Delay(200, ct);
            return 42;
        }, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<TimeoutException>();
    }

    [Fact]
    public async Task Execute_WithCircuitBreakerThatOpens_ThrowsCircuitBreakerOpenException()
    {
        // Arrange
        int failureThreshold = 2; // Lower threshold for testing
        var circuitBreakerStrategy = new CircuitBreakerStrategy(_circuitBreakerLoggerMock.Object, failureThreshold, _breakDuration);
        var compositeStrategy = new CompositeResilienceStrategy(new IResilienceStrategy[] { circuitBreakerStrategy });

        int callCount = 0;

        // First cause enough failures to open circuit
        for (int i = 0; i < failureThreshold; i++)
        {
            try
            {
                await compositeStrategy.Execute<int>(ct =>
                {
                    callCount++;
                    throw new InvalidOperationException($"Failure {callCount}");
                }, CancellationToken.None);
            }
            catch (InvalidOperationException)
            {
                // Expected exceptions
            }
        }

        // Act
        callCount = 0;
        Func<Task<int>> act = async () => await compositeStrategy.Execute<int>(ct =>
        {
            callCount++;
            return new ValueTask<int>(42);
        }, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<CircuitBreakerOpenException>();
        callCount.Should().Be(0); // Call shouldn't be made when circuit is open
    }

    [Fact]
    public async Task Execute_WithRetryAndCircuitBreaker_AllRetriesFailAndCircuitOpensPreventing_FurtherCalls()
    {
        // Arrange
        int maxRetryAttempts = 2;
        TimeSpan initialDelay = TimeSpan.FromMilliseconds(20);
        int failureThreshold = 3; // Will open after initial + 2 retries
        
        var retryStrategy = new RetryStrategy(_retryLoggerMock.Object, maxRetryAttempts, initialDelay, _backoffFactor);
        var circuitBreakerStrategy = new CircuitBreakerStrategy(_circuitBreakerLoggerMock.Object, failureThreshold, _breakDuration);

        var compositeStrategy = new CompositeResilienceStrategy(
            new IResilienceStrategy[] { retryStrategy, circuitBreakerStrategy });

        int callCount = 0;
        Exception expectedException = new InvalidOperationException("Expected failure");

        // Act - First operation that will use up all retries and open circuit
        Func<Task<int>> act1 = async () => await compositeStrategy.Execute<int>(ct =>
        {
            callCount++;
            throw expectedException;
        }, CancellationToken.None);

        // Assert - Should throw the original exception after all retries
        await act1.Should().ThrowAsync<RetryFailedException>();
        callCount.Should().Be(3); // Initial + 2 retries
        
        // Act again - Circuit should be open now
        callCount = 0;
        Func<Task<int>> act2 = async () => await compositeStrategy.Execute<int>(ct =>
        {
            callCount++;
            return new ValueTask<int>(42);
        }, CancellationToken.None);

        // Assert - Should throw CircuitBreakerOpenException and not call the function
        await act2.Should().ThrowAsync<CircuitBreakerOpenException>();
        callCount.Should().Be(0);
    }
}