using CRISP.Core.Options;
using CRISP.Core.Resilience;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CRISP.Core.Tests.Resilience;

public class TimeoutStrategyTests
{
    private readonly Mock<ILogger<TimeoutStrategy>> _loggerMock;
    private readonly TimeSpan _timeout = TimeSpan.FromMilliseconds(100);

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

        // Act
        Func<Task<int>> act = async () => await strategy.Execute<int>(async ct =>
        {
            // Delay longer than the timeout
            await Task.Delay((int)(_timeout.TotalMilliseconds * 2), ct);
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
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        Func<Task<int>> act = async () => await strategy.Execute<int>(ct =>
        {
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

        // Act
        Func<Task> act = async () => await strategy.Execute(async ct =>
        {
            // Delay longer than the timeout
            await Task.Delay((int)(_timeout.TotalMilliseconds * 2), ct);
        }, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<TimeoutException>()
            .WithMessage($"Operation timed out after {_timeout.TotalMilliseconds}ms");
    }

    [Fact]
    public async Task Execute_WithCancellationDuringExecution_ThrowsOperationCanceledException()
    {
        // Arrange
        TimeSpan longerTimeout = TimeSpan.FromMilliseconds(500); // Longer timeout for this test
        
        TimeoutStrategy strategy = new(_loggerMock.Object, longerTimeout);
        var cts = new CancellationTokenSource();

        // Act
        Func<Task<int>> act = async () => await strategy.Execute<int>(async ct =>
        {
            // Start a task that will be canceled externally
            var taskCompletionSource = new TaskCompletionSource<int>();
            
            // Register cancellation callback
            ct.Register(() => taskCompletionSource.TrySetCanceled());
            
            // Cancel after a short delay
            await Task.Delay(50);
            cts.Cancel();
            
            return await taskCompletionSource.Task;
        }, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}