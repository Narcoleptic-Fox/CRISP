using CRISP.Core.Resilience;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace CRISP.Core.Tests.Resilience;

public class TimeoutStrategyTests
{
    private readonly Mock<ILogger<TimeoutStrategy>> _loggerMock;

    public TimeoutStrategyTests() => _loggerMock = new Mock<ILogger<TimeoutStrategy>>();

    [Fact]
    public async Task Execute_WithinTimeout_CompletesSuccessfully()
    {
        // Arrange
        TimeoutStrategy strategy = new(_loggerMock.Object, TimeSpan.FromSeconds(1));
        int expectedResult = 42;

        // Act
        int result = await strategy.Execute((ct) =>
            ValueTask.FromResult(expectedResult), CancellationToken.None);

        // Assert
        result.ShouldBe(expectedResult);
    }

    [Fact]
    public async Task Execute_ExceedsTimeout_ThrowsTimeoutException()
    {
        // Arrange
        TimeoutStrategy strategy = new(_loggerMock.Object, TimeSpan.FromMilliseconds(50));

        // Act
        Func<Task<int>> act = async () => await strategy.Execute<int>(async (ct) =>
        {
            // Delay longer than the timeout
            await Task.Delay(500);
            return 42;
        }, CancellationToken.None);

        // Assert
        TimeoutException exception = await Should.ThrowAsync<TimeoutException>(act);
        exception.Message.ShouldContain("timed out after");
    }

    [Fact]
    public async Task Execute_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        TimeoutStrategy strategy = new(_loggerMock.Object, TimeSpan.FromSeconds(1));

        using CancellationTokenSource cts = new();
        cts.Cancel();

        // Act
        Func<Task<int>> act = async () => await strategy.Execute<int>((ct) =>
        {
            return ValueTask.FromResult(42);
        }, cts.Token);

        // Assert
        await Should.ThrowAsync<OperationCanceledException>(act);
    }

    [Fact]
    public async Task Execute_WithCancellationDuringExecution_ThrowsOperationCanceledException()
    {
        // Arrange
        TimeoutStrategy strategy = new(_loggerMock.Object, TimeSpan.FromMilliseconds(500)); // Longer timeout for this test
        using CancellationTokenSource cts = new();

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
        await Should.ThrowAsync<OperationCanceledException>(act);
    }

    [Fact]
    public async Task Execute_WithTaskDelay_TimesOutCorrectly()
    {
        // Arrange
        TimeoutStrategy strategy = new(_loggerMock.Object, TimeSpan.FromMilliseconds(50));

        // Act - Task.Delay that exceeds timeout
        Func<Task<int>> act = async () => await strategy.Execute<int>(async ct =>
        {
            // This delay should exceed the timeout
            await Task.Delay(200, ct);
            return 42;
        }, CancellationToken.None);

        // Assert
        TimeoutException exception = await Should.ThrowAsync<TimeoutException>(act);
        exception.Message.ShouldContain("timed out after");
    }

    [Fact]
    public async Task Execute_WithVoidTask_ReturnsSuccessfully()
    {
        // Arrange
        TimeoutStrategy strategy = new(_loggerMock.Object, TimeSpan.FromSeconds(1));
        bool executed = false;

        // Act
        await strategy.Execute(ct =>
        {
            executed = true;
            return ValueTask.CompletedTask;
        }, CancellationToken.None);

        // Assert
        executed.ShouldBeTrue();
    }

    [Fact]
    public async Task Execute_WithVoidTaskExceedingTimeout_ThrowsTimeoutException()
    {
        // Arrange
        TimeoutStrategy strategy = new(_loggerMock.Object, TimeSpan.FromMilliseconds(50));

        // Act
        Func<Task> act = async () => await strategy.Execute(async ct =>
        {
            // Delay exceeds timeout
            await Task.Delay(200, ct);
        }, CancellationToken.None);

        // Assert
        TimeoutException exception = await Should.ThrowAsync<TimeoutException>(act);
        exception.Message.ShouldContain("timed out after");
    }

    [Fact]
    public void Constructor_WithInvalidTimeout_ThrowsArgumentException()
    {
        // Act & Assert - Zero timeout
        ArgumentOutOfRangeException exception1 = Should.Throw<ArgumentOutOfRangeException>(() =>
            new TimeoutStrategy(_loggerMock.Object, TimeSpan.Zero));
        exception1.ParamName.ShouldBe("timeout");

        // Act & Assert - Negative timeout
        ArgumentOutOfRangeException exception2 = Should.Throw<ArgumentOutOfRangeException>(() =>
            new TimeoutStrategy(_loggerMock.Object, TimeSpan.FromSeconds(-1)));
        exception2.ParamName.ShouldBe("timeout");

        // Act & Assert - Null logger
        ArgumentNullException exception3 = Should.Throw<ArgumentNullException>(() =>
            new TimeoutStrategy(null!, TimeSpan.FromSeconds(1)));
        exception3.ParamName.ShouldBe("logger");
    }
}