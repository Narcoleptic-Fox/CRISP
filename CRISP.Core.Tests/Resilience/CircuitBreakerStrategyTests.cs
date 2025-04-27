using CRISP.Core.Options;
using CRISP.Core.Resilience;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CRISP.Core.Tests.Resilience;

public class CircuitBreakerStrategyTests
{
    private readonly Mock<ILogger<CircuitBreakerStrategy>> _loggerMock;
    private readonly int _failureThreshold = 3;
    private readonly TimeSpan _durationOfBreak = TimeSpan.FromMilliseconds(500);

    public CircuitBreakerStrategyTests()
    {
        _loggerMock = new Mock<ILogger<CircuitBreakerStrategy>>();
    }

    [Fact]
    public async Task Execute_InClosedState_Success_ReturnsResult()
    {
        // Arrange
        CircuitBreakerStrategy strategy = new(_loggerMock.Object, _failureThreshold, _durationOfBreak);
        int expectedResult = 42;

        // Act
        int result = await strategy.Execute(ct => new ValueTask<int>(expectedResult), CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        strategy.State.Should().Be(CircuitState.Closed);
    }

    [Fact]
    public async Task Execute_FailuresBelowThreshold_CircuitRemainsInClosedState()
    {
        // Arrange
        CircuitBreakerStrategy strategy = new(_loggerMock.Object, _failureThreshold, _durationOfBreak);
        int callCount = 0;
        int failureCount = _failureThreshold - 1;

        // Act & Assert
        for (int i = 0; i < failureCount; i++)
        {
            Func<Task<int>> act = async () => await strategy.Execute<int>(ct =>
            {
                callCount++;
                throw new InvalidOperationException($"Failure {callCount}");
            }, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        // Circuit should still be closed
        strategy.State.Should().Be(CircuitState.Closed);

        // Circuit should still allow new calls
        callCount = 0;
        int result = await strategy.Execute(ct =>
        {
            callCount++;
            return new ValueTask<int>(42);
        }, CancellationToken.None);

        // Assert
        result.Should().Be(42);
        callCount.Should().Be(1);
    }

    [Fact]
    public async Task Execute_FailuresReachThreshold_CircuitOpens()
    {
        // Arrange
        CircuitBreakerStrategy strategy = new(_loggerMock.Object, _failureThreshold, _durationOfBreak);
        int callCount = 0;
        int failureCount = _failureThreshold;

        // Act & Assert - First cause enough failures to open circuit
        for (int i = 0; i < failureCount; i++)
        {
            Func<Task<int>> act = async () => await strategy.Execute<int>(ct =>
            {
                callCount++;
                throw new InvalidOperationException($"Failure {callCount}");
            }, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        // Circuit should now be open
        strategy.State.Should().Be(CircuitState.Open);

        // Now try another call - should throw CircuitBreakerOpenException
        callCount = 0;
        Func<Task<int>> actAfterOpen = async () => await strategy.Execute<int>(ct =>
        {
            callCount++;
            return new ValueTask<int>(42);
        }, CancellationToken.None);

        // Assert
        await actAfterOpen.Should().ThrowAsync<CircuitBreakerOpenException>();
        callCount.Should().Be(0); // Function should not be called when circuit is open
    }

    [Fact]
    public async Task Execute_CircuitOpens_ThenHalfOpen_ThenCloses()
    {
        // Arrange
        int failureThreshold = 2;
        TimeSpan breakDuration = TimeSpan.FromMilliseconds(50); // Short timeout for testing
        
        CircuitBreakerStrategy strategy = new(_loggerMock.Object, failureThreshold, breakDuration);
        int callCount = 0;

        // Act & Assert - First cause failures to open circuit
        for (int i = 0; i < failureThreshold; i++)
        {
            Func<Task<int>> act = async () => await strategy.Execute<int>(ct =>
            {
                callCount++;
                throw new InvalidOperationException($"Failure {callCount}");
            }, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        // Circuit should now be open
        strategy.State.Should().Be(CircuitState.Open);

        // Wait for break duration to expire
        await Task.Delay(breakDuration + TimeSpan.FromMilliseconds(10));

        // Circuit should transition to half-open on next operation
        callCount = 0;
        int result1 = await strategy.Execute(ct =>
        {
            callCount++;
            return new ValueTask<int>(100);
        }, CancellationToken.None);

        // Should be closed after success in half-open state
        result1.Should().Be(100);
        callCount.Should().Be(1);
        strategy.State.Should().Be(CircuitState.Closed);

        // Another successful operation in closed state
        callCount = 0;
        int result2 = await strategy.Execute(ct =>
        {
            callCount++;
            return new ValueTask<int>(200);
        }, CancellationToken.None);

        result2.Should().Be(200);
        callCount.Should().Be(1);
        strategy.State.Should().Be(CircuitState.Closed);
    }

    [Fact]
    public async Task Execute_CircuitHalfOpen_FailureOpensCircuit()
    {
        // Arrange
        int failureThreshold = 2;
        TimeSpan breakDuration = TimeSpan.FromMilliseconds(50); // Short timeout for testing
        
        CircuitBreakerStrategy strategy = new(_loggerMock.Object, failureThreshold, breakDuration);
        int callCount = 0;

        // Act & Assert - First cause failures to open circuit
        for (int i = 0; i < failureThreshold; i++)
        {
            Func<Task<int>> act = async () => await strategy.Execute<int>(ct =>
            {
                callCount++;
                throw new InvalidOperationException($"Failure {callCount}");
            }, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        // Circuit should now be open
        strategy.State.Should().Be(CircuitState.Open);

        // Wait for break duration to expire
        await Task.Delay(breakDuration + TimeSpan.FromMilliseconds(10));

        // A failure in half-open state should open the circuit again
        callCount = 0;
        Func<Task<int>> actHalfOpen = async () => await strategy.Execute<int>(ct =>
        {
            callCount++;
            throw new InvalidOperationException("Half-open state failure");
        }, CancellationToken.None);

        await actHalfOpen.Should().ThrowAsync<InvalidOperationException>();
        callCount.Should().Be(1);
        strategy.State.Should().Be(CircuitState.Open);

        // Circuit should be open again - verify next call throws CircuitBreakerOpenException
        callCount = 0;
        Func<Task<int>> actAfterReopened = async () => await strategy.Execute<int>(ct =>
        {
            callCount++;
            return new ValueTask<int>(42);
        }, CancellationToken.None);

        await actAfterReopened.Should().ThrowAsync<CircuitBreakerOpenException>();
        callCount.Should().Be(0);
    }
}