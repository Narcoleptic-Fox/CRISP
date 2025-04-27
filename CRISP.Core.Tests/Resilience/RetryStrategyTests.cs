using CRISP.Core.Options;
using CRISP.Core.Resilience;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CRISP.Core.Tests.Resilience;

public class RetryStrategyTests
{
    private readonly Mock<ILogger<RetryStrategy>> _loggerMock;
    private readonly int _maxRetryAttempts = 3;
    private readonly TimeSpan _initialDelay = TimeSpan.FromMilliseconds(50);
    private readonly double _backoffFactor = 2.0;

    public RetryStrategyTests()
    {
        _loggerMock = new Mock<ILogger<RetryStrategy>>();
    }

    [Fact]
    public async Task Execute_SucceedsOnFirstTry_ReturnsResult()
    {
        // Arrange
        RetryStrategy strategy = new(_loggerMock.Object, _maxRetryAttempts, _initialDelay, _backoffFactor);
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
        RetryStrategy strategy = new(_loggerMock.Object, _maxRetryAttempts, _initialDelay, _backoffFactor);
        int callCount = 0;
        int expectedResult = 42;

        // Act
        int result = await strategy.Execute(ct =>
        {
            callCount++;
            if (callCount < 3)
                throw new InvalidOperationException($"Attempt {callCount} failed");
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
        RetryStrategy strategy = new(_loggerMock.Object, _maxRetryAttempts, _initialDelay, _backoffFactor);
        int callCount = 0;
        var expectedException = new InvalidOperationException("All attempts failed");

        // Act
        Func<Task<int>> act = async () => await strategy.Execute<int>(ct =>
        {
            callCount++;
            throw expectedException;
        }, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<RetryFailedException>();
        callCount.Should().Be(_maxRetryAttempts + 1); // Initial attempt + retries
    }

    [Fact]
    public async Task Execute_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        RetryStrategy strategy = new(_loggerMock.Object, _maxRetryAttempts, _initialDelay, _backoffFactor);
        int callCount = 0;
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        Func<Task<int>> act = async () => await strategy.Execute<int>(ct =>
        {
            callCount++;
            return new ValueTask<int>(42);
        }, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
        callCount.Should().Be(0); // Shouldn't even try when already canceled
    }

    [Fact]
    public async Task Execute_WithCancellationDuringRetry_ThrowsOperationCanceledException()
    {
        // Arrange
        RetryStrategy strategy = new(_loggerMock.Object, _maxRetryAttempts, _initialDelay, _backoffFactor);
        int callCount = 0;
        var cts = new CancellationTokenSource();

        // Act
        Func<Task<int>> act = async () => await strategy.Execute<int>(ct =>
        {
            callCount++;
            if (callCount == 2) // On second attempt, cancel the token
            {
                cts.Cancel();
            }
            throw new InvalidOperationException($"Attempt {callCount} failed");
        }, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
        callCount.Should().Be(2); // Should stop after cancellation
    }

    [Fact]
    public async Task Execute_WithoutReturnValue_SucceedsAfterRetries()
    {
        // Arrange
        RetryStrategy strategy = new(_loggerMock.Object, _maxRetryAttempts, _initialDelay, _backoffFactor);
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
}