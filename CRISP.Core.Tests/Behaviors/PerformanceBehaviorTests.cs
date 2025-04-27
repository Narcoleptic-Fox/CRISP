using CRISP.Core.Behaviors;
using CRISP.Core.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CRISP.Core.Tests.Behaviors;

public class PerformanceBehaviorTests
{
    private readonly Mock<ILogger<PerformanceBehavior<TestRequest, string>>> _loggerMock;

    public PerformanceBehaviorTests() => _loggerMock = new Mock<ILogger<PerformanceBehavior<TestRequest, string>>>();

    [Fact]
    public async Task Handle_FastRequest_LogsDebugMessage()
    {
        // Arrange
        PerformanceBehavior<TestRequest, string> behavior = new(_loggerMock.Object, 500);
        TestRequest request = new();
        string expectedResponse = "Test Response";

        // Fast executing delegate
        RequestHandlerDelegate<string> next = (CancellationToken cancellationToken) => new ValueTask<string>(expectedResponse);

        // Act
        string response = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        response.Should().Be(expectedResponse);

        // Verify that debug log was called with the correct message format
        VerifyLoggerCalled(LogLevel.Debug, "Request performance: TestRequest", 1);
    }

    [Fact]
    public async Task Handle_SlowRequest_LogsWarningMessage()
    {
        // Arrange
        const int threshold = 50;
        PerformanceBehavior<TestRequest, string> behavior = new(_loggerMock.Object, threshold);
        TestRequest request = new();
        string expectedResponse = "Test Response";

        // Slow executing delegate that exceeds the threshold
        RequestHandlerDelegate<string> next = async (CancellationToken cancellationToken) =>
        {
            await Task.Delay(threshold * 2);
            return expectedResponse;
        };

        // Act
        string response = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        response.Should().Be(expectedResponse);

        // Verify that warning log was called with the correct message format
        VerifyLoggerCalled(LogLevel.Warning, "Long running request: TestRequest", 1);
    }

    [Fact]
    public async Task Handle_WithException_PropagatesException()
    {
        // Arrange
        PerformanceBehavior<TestRequest, string> behavior = new(_loggerMock.Object);
        TestRequest request = new();
        InvalidOperationException expectedException = new("Test exception");
        RequestHandlerDelegate<string> next = (CancellationToken cancellationToken) => throw expectedException;

        // Act
        Func<Task> act = () => behavior.Handle(request, next, CancellationToken.None).AsTask();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Test exception");
    }

    [Fact]
    public async Task Handle_WithCustomThreshold_RespectsThreshold()
    {
        // Arrange
        const int customThreshold = 100;
        PerformanceBehavior<TestRequest, string> behavior = new(_loggerMock.Object, customThreshold);
        TestRequest request = new();
        string expectedResponse = "Test Response";

        // Fast executing delegate that should be under the custom threshold
        RequestHandlerDelegate<string> next = (CancellationToken cancellationToken) => new ValueTask<string>(expectedResponse);

        // Act
        string response = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        response.Should().Be(expectedResponse);

        // Verify that debug log was called (because it's under threshold)
        VerifyLoggerCalled(LogLevel.Debug, "Request performance: TestRequest", 1);
    }

    [Fact]
    public async Task Handle_WithCancellation_PropagatesCancellation()
    {
        // Arrange
        PerformanceBehavior<TestRequest, string> behavior = new(_loggerMock.Object);
        TestRequest request = new();

        // Create a cancellation token that is already canceled
        CancellationTokenSource cts = new();
        cts.Cancel();

        RequestHandlerDelegate<string> next = ValueTask.FromCanceled<string>;

        // Act & Assert
        await behavior.Invoking(b => b.Handle(request, next, cts.Token).AsTask())
            .Should().ThrowAsync<TaskCanceledException>();
    }

    private void VerifyLoggerCalled(LogLevel level, string messageContains, int times) => _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == level),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(messageContains)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Exactly(times));
}

// Test request class used for testing behaviors
public class TestRequest : IRequest<string>
{
}