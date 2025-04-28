using CRISP.Core.Behaviors;
using CRISP.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace CRISP.Core.Tests.Behaviors;

public class LoggingBehaviorTests
{
    private readonly Mock<ILogger<LoggingBehavior<TestRequest, string>>> _loggerMock;

    public LoggingBehaviorTests() => _loggerMock = new Mock<ILogger<LoggingBehavior<TestRequest, string>>>();

    [Fact]
    public async Task Handle_SuccessfulRequest_LogsStartAndEnd()
    {
        // Arrange
        LoggingBehavior<TestRequest, string> behavior = new(_loggerMock.Object);
        TestRequest request = new();
        string expectedResponse = "Test Response";
        RequestHandlerDelegate<string> next = (CancellationToken cancellationToken) => new ValueTask<string>(expectedResponse);

        // Act
        string response = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        response.ShouldBe(expectedResponse);

        // Verify that start log was called with the correct message
        VerifyLoggerCalled(LogLevel.Information, "[START] Request TestRequest", 1);

        // Verify that end log was called with the correct message
        VerifyLoggerCalled(LogLevel.Information, "[END] Request TestRequest", 1);
    }

    [Fact]
    public async Task Handle_FailedRequest_LogsStartAndError()
    {
        // Arrange
        LoggingBehavior<TestRequest, string> behavior = new(_loggerMock.Object);
        TestRequest request = new();
        InvalidOperationException expectedException = new("Test exception");
        RequestHandlerDelegate<string> next = (CancellationToken cancellationToken) => throw expectedException;

        // Act
        Func<Task> act = () => behavior.Handle(request, next, CancellationToken.None).AsTask();

        // Assert
        InvalidOperationException exception = await Should.ThrowAsync<InvalidOperationException>(act);
        exception.Message.ShouldBe("Test exception");

        // Verify that start log was called
        VerifyLoggerCalled(LogLevel.Information, "[START] Request TestRequest", 1);

        // Verify that error log was called
        VerifyLoggerCalled(LogLevel.Error, "[ERROR] Request TestRequest", 1);
    }

    [Fact]
    public async Task Handle_WithCancellation_PropagatesCancellation()
    {
        // Arrange
        LoggingBehavior<TestRequest, string> behavior = new(_loggerMock.Object);
        TestRequest request = new();

        // Create a cancellation token that is already canceled
        CancellationTokenSource cts = new();
        cts.Cancel();

        RequestHandlerDelegate<string> next = (CancellationToken cancellationToken) => new ValueTask<string>(Task.FromCanceled<string>(cts.Token));

        // Act
        Func<Task> act = () => behavior.Handle(request, next, cts.Token).AsTask();

        // Assert
        await Should.ThrowAsync<TaskCanceledException>(act);

        // Verify that start log was called
        VerifyLoggerCalled(LogLevel.Information, "[START] Request TestRequest", 1);

        // Verify that error log was called (for the TaskCanceledException)
        VerifyLoggerCalled(LogLevel.Error, "[ERROR] Request TestRequest", 1);
    }

    private void VerifyLoggerCalled(LogLevel level, string messageSubstring, int times) => _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == level),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(messageSubstring)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>(f => true)
            ),
            Times.Exactly(times)
        );

    public class TestRequest : IRequest<string> { }
}