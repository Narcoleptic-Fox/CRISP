using CRISP.Core.Resilience;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CRISP.Core.Tests.Resilience;

public class RetryStrategyTests
{
    private readonly Mock<ILogger<RetryStrategy>> _loggerMock;
    private readonly int _maxRetryAttempts = 3;
    private readonly TimeSpan _initialDelay = TimeSpan.FromMilliseconds(1); // Using minimal delay for faster tests
    private readonly double _backoffFactor = 2.0;
    // Add retry predicate to retry on InvalidOperationException for tests
    private readonly Func<Exception, bool> _retryPredicate = ex => ex is InvalidOperationException || RetryStrategy.IsTransientException(ex);

    public RetryStrategyTests() => _loggerMock = new Mock<ILogger<RetryStrategy>>();

    [Fact]
    public async Task Execute_SucceedsOnFirstTry_ReturnsResult()
    {
        // Arrange
        RetryStrategy strategy = new(_loggerMock.Object, _maxRetryAttempts, _initialDelay, _backoffFactor, _retryPredicate);
        int callCount = 0;
        int expectedResult = 42;

        // Act
        int result = await strategy.Execute(ct =>
        {
            callCount++;
            return new ValueTask<int>(expectedResult);
        }, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        callCount.Should().Be(1);
    }

    [Fact]
    public async Task Execute_SucceedsAfterRetries_ReturnsResult()
    {
        // Arrange
        RetryStrategy strategy = new(_loggerMock.Object, _maxRetryAttempts, _initialDelay, _backoffFactor, _retryPredicate);
        int callCount = 0;
        int expectedResult = 42;

        // Act
        int result = await strategy.Execute<int>(ct =>
        {
            callCount++;
            if (callCount < 3)
                throw new InvalidOperationException($"Attempt {callCount} failed"); // Use an exception RetryStrategy will retry
            return new ValueTask<int>(expectedResult);
        }, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        callCount.Should().Be(3);
    }

    [Fact]
    public async Task Execute_FailsAllRetries_ThrowsException()
    {
        // Arrange
        // Create a retry strategy that specifically retries on InvalidOperationException
        RetryStrategy strategy = new(_loggerMock.Object, _maxRetryAttempts, _initialDelay, _backoffFactor, 
            retryPredicate: ex => ex is InvalidOperationException);
            
        int callCount = 0;

        // Act
        Func<Task<int>> act = async () => await strategy.Execute<int>(ct =>
        {
            callCount++;
            throw new InvalidOperationException("All attempts failed");
        }, CancellationToken.None).AsTask();

        // Assert
        var exeption = await act.Should().ThrowAsync<RetryFailedException>();
        exeption.WithInnerException<InvalidOperationException>();
        callCount.Should().Be(_maxRetryAttempts + 1); // Initial attempt + retries
    }

    [Fact]
    public async Task Execute_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        RetryStrategy strategy = new(_loggerMock.Object, _maxRetryAttempts, _initialDelay, _backoffFactor, _retryPredicate);

        // Create token that's already canceled
        using CancellationTokenSource cts = new();
        cts.Cancel();

        // Act
        Func<Task<int>> act = async () => await strategy.Execute<int>(ct =>
        {
            // This should not be called due to canceled token
            throw new InvalidOperationException("Should not be called");
        }, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Execute_WithCancellationDuringRetry_ThrowsOperationCanceledException()
    {
        // Arrange
        RetryStrategy strategy = new(_loggerMock.Object, _maxRetryAttempts, _initialDelay, _backoffFactor);
        int callCount = 0;

        // Create cancellation token source
        using CancellationTokenSource cts = new();

        // Act
        Func<Task<int>> act = async () => await strategy.Execute<int>(ct =>
        {
            callCount++;

            // Cancel on first attempt
            if (callCount == 1)
            {
                cts.Cancel();
                ct.ThrowIfCancellationRequested();
            }

            return new ValueTask<int>(42);
        }, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
        callCount.Should().Be(1);
    }

    [Fact]
    public async Task Execute_WithoutReturnValue_SucceedsAfterRetries()
    {
        // Arrange
        // Create a retry strategy that specifically retries on InvalidOperationException
        RetryStrategy strategy = new(_loggerMock.Object, _maxRetryAttempts, _initialDelay, _backoffFactor, 
            retryPredicate: ex => ex is InvalidOperationException);
            
        int callCount = 0;

        // Act
        await strategy.Execute(ct =>
        {
            callCount++;
            if (callCount < 3)
                throw new InvalidOperationException($"Attempt {callCount} failed");
            return new ValueTask();
        }, CancellationToken.None);

        // Assert
        callCount.Should().Be(3);
    }

    [Fact]
    public void IsTransientException_TimeoutException_ReturnsTrue()
    {
        // Arrange
        var exception = new TimeoutException("Operation timed out");
        
        // Act
        bool isTransient = RetryStrategy.IsTransientException(exception);
        
        // Assert
        isTransient.Should().BeTrue();
    }
    
    [Fact]
    public void IsTransientException_HttpRequestException_ReturnsTrue()
    {
        // Arrange
        var exception = new System.Net.Http.HttpRequestException("Network error");
        
        // Act
        bool isTransient = RetryStrategy.IsTransientException(exception);
        
        // Assert
        isTransient.Should().BeTrue();
    }
    
    [Fact]
    public void IsTransientException_IOException_ReturnsTrue()
    {
        // Arrange
        var exception = new System.IO.IOException("I/O error");
        
        // Act
        bool isTransient = RetryStrategy.IsTransientException(exception);
        
        // Assert
        isTransient.Should().BeTrue();
    }
    
    [Fact]
    public void IsTransientException_ContainsTimeoutInMessage_ReturnsTrue()
    {
        // Arrange
        var exception = new Exception("The operation has timed out after 30 seconds");
        
        // Act
        bool isTransient = RetryStrategy.IsTransientException(exception);
        
        // Assert
        isTransient.Should().BeTrue();
    }
    
    [Fact]
    public void IsTransientException_ContainsTemporarilyUnavailableInMessage_ReturnsTrue()
    {
        // Arrange
        var exception = new Exception("The service is temporarily unavailable, try again later");
        
        // Act
        bool isTransient = RetryStrategy.IsTransientException(exception);
        
        // Assert
        isTransient.Should().BeTrue();
    }
    
    [Fact]
    public void IsTransientException_RegularException_ReturnsFalse()
    {
        // Arrange
        var exception = new Exception("Regular non-transient error");
        
        // Act
        bool isTransient = RetryStrategy.IsTransientException(exception);
        
        // Assert
        isTransient.Should().BeFalse();
    }
    
    [Fact]
    public void Constructor_InvalidParameters_ThrowsArgumentExceptions()
    {
        // Arrange
        var logger = _loggerMock.Object;
        
        // Act & Assert - Invalid maxRetryAttempts
        Action act1 = () => new RetryStrategy(logger, maxRetryAttempts: 0);
        act1.Should().Throw<ArgumentOutOfRangeException>()
            .And.ParamName.Should().Be("maxRetryAttempts");
            
        // Act & Assert - Invalid backoffFactor
        Action act2 = () => new RetryStrategy(logger, backoffFactor: 0.5);
        act2.Should().Throw<ArgumentOutOfRangeException>()
            .And.ParamName.Should().Be("backoffFactor");
            
        // Act & Assert - Null logger
        Action act3 = () => new RetryStrategy(null!);
        act3.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("logger");
    }
    
    [Fact]
    public async Task Execute_NonRetryableException_DoesNotRetry()
    {
        // Arrange
        // Create a retry strategy that only retries TimeoutExceptions
        RetryStrategy strategy = new(_loggerMock.Object, _maxRetryAttempts, _initialDelay, _backoffFactor, 
            retryPredicate: ex => ex is TimeoutException);
            
        int callCount = 0;

        // Act
        Func<Task<int>> act = async () => await strategy.Execute<int>(ct =>
        {
            callCount++;
            throw new InvalidOperationException("This exception should not be retried");
        }, CancellationToken.None).AsTask();

        // Assert
        await act.Should().ThrowAsync<RetryFailedException>();
        callCount.Should().Be(1); // Should not retry
    }
    
    [Fact]
    public async Task Execute_CancellationDuringDelay_ThrowsOperationCanceledException()
    {
        // Arrange
        var strategy = new RetryStrategy(_loggerMock.Object, _maxRetryAttempts, TimeSpan.FromMilliseconds(50), _backoffFactor);
        int callCount = 0;
        
        var cts = new CancellationTokenSource();

        // Act
        Func<Task<int>> action= () => strategy.Execute<int>(async ct =>
        {
            callCount++;
            
            // On first attempt, fail but set cancellation to trigger during the delay
            if (callCount == 1)
            {
                // Set cancellation to trigger during delay
                cts.CancelAfter(10); 
                throw new TimeoutException("First attempt fails");
            }
            
            return 42;
        }, cts.Token).AsTask();
        
        // Assert
        await action.Should().ThrowAsync<OperationCanceledException>();
        callCount.Should().Be(1);
    }
    
    [Fact]
    public async Task Execute_MaxRetryAttemptsOne_ThrowsOnFirstFailure()
    {
        // Arrange - Use 1 instead of 0 since the constructor validates maxRetryAttempts > 0
        var strategy = new RetryStrategy(_loggerMock.Object, 1, _initialDelay, _backoffFactor);
        int callCount = 0;
        
        // Act
        Func<Task<int>> act = async () => await strategy.Execute<int>(ct =>
        {
            callCount++;
            throw new TimeoutException("Operation timed out");
        }, CancellationToken.None).AsTask();
        
        // Assert
        await act.Should().ThrowAsync<RetryFailedException>();
        callCount.Should().Be(2); // Initial attempt + 1 retry
    }
    
    [Fact]
    public async Task Execute_VoidMethod_WithTimeoutException_Retries()
    {
        // Arrange
        RetryStrategy strategy = new(_loggerMock.Object, _maxRetryAttempts, _initialDelay, _backoffFactor);
        int callCount = 0;
        
        // Act - Operation will eventually succeed after retries
        await strategy.Execute(ct =>
        {
            callCount++;
            
            if (callCount <= _maxRetryAttempts - 1) 
            {
                throw new TimeoutException($"Attempt {callCount} timed out");
            }
            
            return new ValueTask();
        }, CancellationToken.None);
        
        // Assert - Should have retried and then succeeded
        callCount.Should().Be(_maxRetryAttempts); // Initial + retries that eventually succeed
    }
}