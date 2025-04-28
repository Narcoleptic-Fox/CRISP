using CRISP.Core.Resilience;
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
        RetryStrategy retryStrategy = new(_retryLoggerMock.Object, _maxRetryAttempts, _initialDelay, _backoffFactor);
        CircuitBreakerStrategy circuitBreakerStrategy = new(_circuitBreakerLoggerMock.Object, _failureThreshold, _breakDuration);
        TimeoutStrategy timeoutStrategy = new(_timeoutLoggerMock.Object, _timeout);

        CompositeResilienceStrategy compositeStrategy = new(
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
        result.ShouldBe(expectedResult);
        callCount.ShouldBe(1);
    }

    [Fact]
    public async Task Execute_WithRetryAndSuccessfulRetry()
    {
        // Arrange
        // Add retry predicate to handle InvalidOperationException
        Func<Exception, bool> retryPredicate = ex => ex is InvalidOperationException;
        RetryStrategy retryStrategy = new(_retryLoggerMock.Object, _maxRetryAttempts, _initialDelay, _backoffFactor, retryPredicate);
        TimeoutStrategy timeoutStrategy = new(_timeoutLoggerMock.Object, _timeout);

        // Note: Order matters - the retry strategy needs to be first in the array
        CompositeResilienceStrategy compositeStrategy = new(
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
        result.ShouldBe(expectedResult);
        callCount.ShouldBe(2); // Initial + 1 retry
    }

    [Fact]
    public async Task Execute_WithCircuitBreakerAndTimeout_TimeoutExceededException()
    {
        // Arrange
        CircuitBreakerStrategy circuitBreakerStrategy = new(_circuitBreakerLoggerMock.Object, _failureThreshold, _breakDuration);

        // Short timeout to trigger timeout exception
        TimeSpan shortTimeout = TimeSpan.FromMilliseconds(10);
        TimeoutStrategy timeoutStrategy = new(_timeoutLoggerMock.Object, shortTimeout);

        CompositeResilienceStrategy compositeStrategy = new(
            new IResilienceStrategy[] { timeoutStrategy, circuitBreakerStrategy });

        // Act
        Func<Task<int>> act = async () => await compositeStrategy.Execute<int>(async ct =>
        {
            // Don't use the cancellation token so the operation will time out
            await Task.Delay(1000);
            return 42;
        }, CancellationToken.None);

        // Assert
        await Should.ThrowAsync<TimeoutException>(act);
    }

    [Fact]
    public async Task Execute_WithCircuitBreakerThatOpens_ThrowsCircuitBreakerOpenException()
    {
        // Arrange
        int failureThreshold = 2; // Lower threshold for testing
        CircuitBreakerStrategy circuitBreakerStrategy = new(_circuitBreakerLoggerMock.Object, failureThreshold, _breakDuration);
        CompositeResilienceStrategy compositeStrategy = new(new IResilienceStrategy[] { circuitBreakerStrategy });

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
        await Should.ThrowAsync<CircuitBreakerOpenException>(act);
        callCount.ShouldBe(0); // Call shouldn't be made when circuit is open
    }

    [Fact]
    public async Task Execute_WithRetryAndCircuitBreaker_AllRetriesFailAndCircuitOpensPreventing_FurtherCalls()
    {
        // Arrange
        int failureThreshold = 3; // Will open after 3 failures
        TimeSpan breakDuration = TimeSpan.FromMilliseconds(200); // Longer break duration for test stability

        // Create strategies with a retry predicate to retry InvalidOperationException
        Func<Exception, bool> retryPredicate = ex => ex is InvalidOperationException;
        RetryStrategy retryStrategy = new(_retryLoggerMock.Object, _maxRetryAttempts, _initialDelay, _backoffFactor, retryPredicate);
        CircuitBreakerStrategy circuitBreaker = new(_circuitBreakerLoggerMock.Object, failureThreshold, breakDuration);

        // Create two separate strategy instances for the two operations:
        // 1. First to cause the failures and open the circuit
        // 2. Second to test the circuit is open
        CompositeResilienceStrategy compositeStrategy1 = new(circuitBreaker, retryStrategy);
        CompositeResilienceStrategy compositeStrategy2 = new(circuitBreaker); // Same circuit breaker instance

        List<DateTime> callCounts = [];

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
        callCounts.Count.ShouldBe(expectedCallCount);

        // Reset counter
        callCounts.Clear();

        // Verify the circuit is now open
        circuitBreaker.State.ShouldBe(CircuitState.Open);

        // Act - try again with circuit now open
        Func<Task<int>> act = async () => await compositeStrategy2.Execute<int>(ct =>
        {
            callCounts.Add(DateTime.Now);
            return new ValueTask<int>(42);
        }, CancellationToken.None);

        // Assert - should throw CircuitBreakerOpenException 
        await Should.ThrowAsync<CircuitBreakerOpenException>(act);

        // And operation should not have been called
        callCounts.Count.ShouldBe(0);
    }

    [Fact]
    public async Task Execute_WithMultipleStrategies_AppliesStrategiesInOrder()
    {
        // Arrange
        Mock<IResilienceStrategy> mockStrategy1 = new();
        Mock<IResilienceStrategy> mockStrategy2 = new();

        // Set up the first mock strategy to do something when Execute is called
        mockStrategy1.Setup(s => s.Execute<int>(It.IsAny<Func<CancellationToken, ValueTask<int>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task<int>>, CancellationToken>(
                async (operation, ct) => await operation(ct) * 2); // Multiply result by 2

        // Set up the second mock strategy to do something when Execute is called
        mockStrategy2.Setup(s => s.Execute<int>(It.IsAny<Func<CancellationToken, ValueTask<int>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task<int>>, CancellationToken>(
                async (operation, ct) => await operation(ct) + 1); // Add 1 to result

        // Create the composite strategy with our mocks
        CompositeResilienceStrategy strategy = new(new[] { mockStrategy1.Object, mockStrategy2.Object });

        // Act
        int result = await strategy.Execute<int>(ct => ValueTask.FromResult(10), CancellationToken.None);

        // Assert
        // If strategies are applied in the correct order, 10 should first be passed to mockStrategy1
        // which multiplies by 2 (20), and then the result should be passed to mockStrategy2 which adds 1 (21)
        result.ShouldBe(21);

        // Verify that both strategies were called
        mockStrategy1.Verify(s => s.Execute<int>(It.IsAny<Func<CancellationToken, ValueTask<int>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        mockStrategy2.Verify(s => s.Execute<int>(It.IsAny<Func<CancellationToken, ValueTask<int>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Execute_WithoutReturnValue_AppliesStrategiesInOrder()
    {
        // Arrange
        Mock<IResilienceStrategy> mockStrategy1 = new();
        Mock<IResilienceStrategy> mockStrategy2 = new();
        bool executed = false;

        // Set up the first mock strategy to delegate to the operation
        mockStrategy1.Setup(s => s.Execute(It.IsAny<Func<CancellationToken, ValueTask>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task>, CancellationToken>(
                async (operation, ct) => await operation(ct));

        // Set up the second mock strategy to delegate to the operation
        mockStrategy2.Setup(s => s.Execute(It.IsAny<Func<CancellationToken, ValueTask>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task>, CancellationToken>(
                async (operation, ct) => await operation(ct));

        // Create the composite strategy with our mocks
        CompositeResilienceStrategy strategy = new(new[] { mockStrategy1.Object, mockStrategy2.Object });

        // Act
        await strategy.Execute(ct =>
        {
            executed = true;
            return ValueTask.CompletedTask;
        }, CancellationToken.None);

        // Assert
        executed.ShouldBeTrue();

        // Verify that both strategies were called
        mockStrategy1.Verify(s => s.Execute(It.IsAny<Func<CancellationToken, ValueTask>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        mockStrategy2.Verify(s => s.Execute(It.IsAny<Func<CancellationToken, ValueTask>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_WithNoStrategies_ThrowsArgumentException()
    {
        // Act & Assert
        ArgumentException exception = Should.Throw<ArgumentException>(() => new CompositeResilienceStrategy(Array.Empty<IResilienceStrategy>()));
        exception.Message.ShouldContain("At least one resilience strategy must be provided");
    }

    [Fact]
    public void Constructor_WithNullStrategies_ThrowsArgumentNullException()
    {
        // Act & Assert
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() => new CompositeResilienceStrategy(null!));
        exception.ParamName.ShouldBe("strategies");
    }

    [Fact]
    public async Task Execute_RealisticComposition_AppliesAllStrategies()
    {
        // Arrange - Create real retry, circuit breaker and timeout strategies
        RetryStrategy retryStrategy = new(_retryLoggerMock.Object, 1, TimeSpan.FromMilliseconds(1));
        CircuitBreakerStrategy circuitBreaker = new(_circuitBreakerLoggerMock.Object, 3, TimeSpan.FromSeconds(1));
        TimeoutStrategy timeoutStrategy = new(_timeoutLoggerMock.Object, TimeSpan.FromSeconds(1));

        // Create a composite of all three strategies
        CompositeResilienceStrategy compositeStrategy = new(new IResilienceStrategy[]
        {
            timeoutStrategy, // Timeout should be the outermost strategy to ensure operations don't hang
            circuitBreaker, // Circuit breaker is next to prevent calls if circuit is open
            retryStrategy // Retry is innermost to retry the actual operation
        });

        // Act - Execute an operation that will complete quickly
        int result = await compositeStrategy.Execute<int>(_ => ValueTask.FromResult(42), CancellationToken.None);

        // Assert
        result.ShouldBe(42);
    }

    [Fact]
    public async Task Execute_WithExceptionInOperation_PropagatesException()
    {
        // Arrange
        RetryStrategy retryStrategy = new(_retryLoggerMock.Object, 1, TimeSpan.FromMilliseconds(1),
            retryPredicate:  // Retry only on TimeoutException 
                ex => ex is TimeoutException);

        CircuitBreakerStrategy circuitBreaker = new(_circuitBreakerLoggerMock.Object, 3, TimeSpan.FromSeconds(1));

        CompositeResilienceStrategy compositeStrategy = new(new IResilienceStrategy[]
        {
            circuitBreaker,
            retryStrategy
        });

        // Act - This should throw immediately since the retry predicate doesn't match
        Func<Task<int>> act = async () => await compositeStrategy.Execute<int>(_ =>
            throw new InvalidOperationException("Test exception"), CancellationToken.None);

        // Assert
        InvalidOperationException exception = await Should.ThrowAsync<InvalidOperationException>(act);
        exception.Message.ShouldBe("Test exception");
    }

    [Fact]
    public async Task Execute_WithTimeoutExceedingLimit_ThrowsTimeoutException()
    {
        // Arrange - Create a timeout strategy with a very short timeout
        TimeoutStrategy timeoutStrategy = new(_timeoutLoggerMock.Object, TimeSpan.FromMilliseconds(10));

        CompositeResilienceStrategy compositeStrategy = new(new[] { timeoutStrategy });

        // Act - Execute an operation that takes longer than the timeout
        Func<Task<int>> act = async () => await compositeStrategy.Execute<int>(async _ =>
        {
            await Task.Delay(100); // Much longer than the timeout
            return 42;
        }, CancellationToken.None);

        // Assert
        TimeoutException exception = await Should.ThrowAsync<TimeoutException>(act);
        exception.Message.ShouldContain("timed out after");
    }

    [Fact]
    public async Task Execute_WithCancellationToken_PropagatesCancellation()
    {
        // Arrange
        RetryStrategy retryStrategy = new(_retryLoggerMock.Object, 2, TimeSpan.FromMilliseconds(1));
        CompositeResilienceStrategy compositeStrategy = new(new[] { retryStrategy });

        // Create a cancellation token that's already canceled
        using CancellationTokenSource cts = new();
        cts.Cancel();

        // Act
        Func<Task<int>> act = async () => await compositeStrategy.Execute<int>(_ =>
            ValueTask.FromResult(42), cts.Token);

        // Assert
        await Should.ThrowAsync<OperationCanceledException>(act);
    }
}