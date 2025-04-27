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
    private readonly TimeSpan _initialDelay = TimeSpan.FromMilliseconds(1); // Minimal delay for faster tests
    private readonly double _backoffFactor = 2.0;
    private readonly int _failureThreshold = 3;
    private readonly TimeSpan _breakDuration = TimeSpan.FromMilliseconds(50);
    private readonly TimeSpan _timeout = TimeSpan.FromMilliseconds(50);

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
        // Add retry predicate to handle InvalidOperationException
        Func<Exception, bool> retryPredicate = ex => ex is InvalidOperationException;
        var retryStrategy = new RetryStrategy(_retryLoggerMock.Object, _maxRetryAttempts, _initialDelay, _backoffFactor, retryPredicate);
        var timeoutStrategy = new TimeoutStrategy(_timeoutLoggerMock.Object, _timeout);

        // Note: Order matters - the retry strategy needs to be first in the array
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
                // InvalidOperationException should be retried thanks to our predicate
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
        var shortTimeout = TimeSpan.FromMilliseconds(10);
        var timeoutStrategy = new TimeoutStrategy(_timeoutLoggerMock.Object, shortTimeout);

        var compositeStrategy = new CompositeResilienceStrategy(
            new IResilienceStrategy[] { timeoutStrategy, circuitBreakerStrategy });

        // Act
        Func<Task<int>> act = async () => await compositeStrategy.Execute<int>(async ct =>
        {
            // Don't use the cancellation token so the operation will time out
            await Task.Delay(1000);
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
        int failureThreshold = 3; // Will open after 3 failures
        var breakDuration = TimeSpan.FromMilliseconds(200); // Longer break duration for test stability
        
        // Create strategies with a retry predicate to retry InvalidOperationException
        Func<Exception, bool> retryPredicate = ex => ex is InvalidOperationException;
        var retryStrategy = new RetryStrategy(_retryLoggerMock.Object, _maxRetryAttempts, _initialDelay, _backoffFactor, retryPredicate);
        var circuitBreaker = new CircuitBreakerStrategy(_circuitBreakerLoggerMock.Object, failureThreshold, breakDuration);

        // Create two separate strategy instances for the two operations:
        // 1. First to cause the failures and open the circuit
        // 2. Second to test the circuit is open
        var compositeStrategy1 = new CompositeResilienceStrategy(circuitBreaker, retryStrategy);
        var compositeStrategy2 = new CompositeResilienceStrategy(circuitBreaker); // Same circuit breaker instance
        
        var callCounts = new List<DateTime>();
        
        // Make initial calls that will fail to trigger circuit breaker opening
        for (int i = 0; i < failureThreshold; i++)
        {
            try
            {
                await compositeStrategy1.Execute<int>(ct =>
                {
                    callCounts.Add(DateTime.Now);
                    throw new InvalidOperationException($"Expected failure #{callCounts.Count}");
                }, CancellationToken.None);
                
                // Should not reach here
                Assert.Fail("Expected exception was not thrown");
            }
            catch (Exception)
            {
                // Expected exception
            }
        }

        // Circuit should now be open after enough failures
        // Note: Due to retries, the number of calls will be higher than just failureThreshold
        // Each failed call will be retried _maxRetryAttempts times
        int expectedCallCount = failureThreshold * (1 + _maxRetryAttempts);
        callCounts.Count.Should().Be(expectedCallCount);
        
        // Reset counter
        callCounts.Clear();
        
        // Verify the circuit is now open
        circuitBreaker.State.Should().Be(CircuitState.Open);

        // Act - try again with circuit now open
        Func<Task<int>> act = async () => await compositeStrategy2.Execute<int>(ct =>
        {
            callCounts.Add(DateTime.Now);
            return new ValueTask<int>(42);
        }, CancellationToken.None);

        // Assert - should throw CircuitBreakerOpenException 
        await act.Should().ThrowAsync<CircuitBreakerOpenException>();
        
        // And operation should not have been called
        callCounts.Count.Should().Be(0);
    }
}