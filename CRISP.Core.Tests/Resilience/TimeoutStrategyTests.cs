using CRISP.Core.Options;
using CRISP.Core.Resilience;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CRISP.Core.Tests.Resilience;

public class TimeoutStrategyTests
{
    private readonly Mock<ILogger<TimeoutStrategy>> _loggerMock;
    private readonly TimeSpan _timeout = TimeSpan.FromMilliseconds(50); // Using a shorter timeout for faster tests

    public TimeoutStrategyTests()
    {
        _loggerMock = new Mock<ILogger<TimeoutStrategy>>();
    }

    [Fact]
    public async Task Execute_CompletesWithinTimeout_ReturnsResult()
    {
        // Arrange
        TimeoutStrategy strategy = new(_loggerMock.Object, _timeout);
        int expectedResult = 42;

        // Act
        int result = await strategy.Execute(ct =>
        {
            // Completes immediately
            return new ValueTask<int>(expectedResult);
        }, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task Execute_ExceedsTimeout_ThrowsTimeoutException()
    {
        // Arrange
        TimeoutStrategy strategy = new(_loggerMock.Object, _timeout);
        var longDelay = TimeSpan.FromMilliseconds(_timeout.TotalMilliseconds * 4); // Ensure it's much longer than the timeout

        // Act
        Func<Task<int>> act = async () => await strategy.Execute<int>(async ct =>
        {
            // Using Task.Delay with a much longer time than the timeout
            // And purposefully NOT passing the cancellation token so it won't be canceled
            await Task.Delay(longDelay);
            return 42;
        }, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<TimeoutException>()
            .WithMessage($"Operation timed out after {_timeout.TotalMilliseconds}ms");
    }

    [Fact]
    public async Task Execute_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        TimeoutStrategy strategy = new(_loggerMock.Object, _timeout);
        
        // Create a token source and immediately cancel it
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act - With pre-canceled token
        Func<Task<int>> act = async () => await strategy.Execute<int>(ct =>
        {
            // This shouldn't be called because token is canceled
            ct.ThrowIfCancellationRequested(); // Ensure we check the token immediately
            return new ValueTask<int>(42);
        }, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Execute_WithoutReturnValue_CompletesWithinTimeout()
    {
        // Arrange
        TimeoutStrategy strategy = new(_loggerMock.Object, _timeout);
        bool executed = false;

        // Act
        await strategy.Execute(ct =>
        {
            executed = true;
            return new ValueTask();
        }, CancellationToken.None);

        // Assert
        executed.Should().BeTrue();
    }

    [Fact]
    public async Task Execute_WithoutReturnValue_ExceedsTimeout_ThrowsTimeoutException()
    {
        // Arrange
        TimeoutStrategy strategy = new(_loggerMock.Object, _timeout);
        var longDelay = TimeSpan.FromMilliseconds(_timeout.TotalMilliseconds * 4); // Ensure it's much longer than the timeout

        // Act
        Func<Task> act = async () => await strategy.Execute(async ct =>
        {
            // Using Task.Delay with a much longer time than the timeout
            // And purposefully NOT passing the cancellation token so it won't be canceled
            await Task.Delay(longDelay); 
        }, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<TimeoutException>()
            .WithMessage($"Operation timed out after {_timeout.TotalMilliseconds}ms");
    }

    [Fact]
    public async Task Execute_WithCancellationDuringExecution_ThrowsOperationCanceledException()
    {
        // Arrange
        TimeoutStrategy strategy = new(_loggerMock.Object, TimeSpan.FromMilliseconds(500)); // Longer timeout for this test
        using var cts = new CancellationTokenSource();

        // Act - Cancel during execution
        Func<Task<int>> act = async () => await strategy.Execute<int>(async ct =>
        {
            // Cancel immediately after starting
            cts.Cancel();
            ct.ThrowIfCancellationRequested(); // This should throw now
            await Task.Yield(); // Add await to prevent CS1998 warning
            return 42;
        }, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}