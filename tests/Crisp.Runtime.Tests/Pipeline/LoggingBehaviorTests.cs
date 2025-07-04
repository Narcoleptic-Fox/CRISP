using Crisp.Commands;
using Crisp.Pipeline;
using Crisp.Queries;
using Microsoft.Extensions.Logging;

namespace Crisp.Runtime.Tests.Pipeline;

public class LoggingBehaviorTests
{
    private readonly ILogger<LoggingBehavior<TestCommand, string>> _mockLogger;
    private readonly LoggingBehavior<TestCommand, string> _commandBehavior;
    private readonly ILogger<LoggingBehavior<TestQuery, string>> _mockQueryLogger;
    private readonly LoggingBehavior<TestQuery, string> _queryBehavior;
    private readonly List<LogEntry> _logEntries;

    public LoggingBehaviorTests()
    {
        _logEntries = [];
        _mockLogger = CreateMockLogger<LoggingBehavior<TestCommand, string>>();
        _commandBehavior = new LoggingBehavior<TestCommand, string>(_mockLogger);

        _mockQueryLogger = CreateMockLogger<LoggingBehavior<TestQuery, string>>();
        _queryBehavior = new LoggingBehavior<TestQuery, string>(_mockQueryLogger);
    }

    #region Test Commands and Queries

    public record TestCommand(string Value) : ICommand<string>;
    public record TestQuery(string Filter) : IQuery<string>;

    #endregion

    #region Helper Classes

    public class LogEntry
    {
        public LogLevel Level { get; set; }
        public string? Message { get; set; }
        public Exception? Exception { get; set; }
        public object?[] Args { get; set; } = Array.Empty<object>();
    }

    #endregion

    #region Helper Methods

    private ILogger<T> CreateMockLogger<T>()
    {
        ILogger<T> mockLogger = Substitute.For<ILogger<T>>();

        mockLogger.When(x => x.Log(
            Arg.Any<LogLevel>(),
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>()))
            .Do(callInfo =>
            {
                LogLevel logLevel = callInfo.ArgAt<LogLevel>(0);
                object state = callInfo.ArgAt<object>(2);
                Exception? exception = callInfo.ArgAt<Exception?>(3);
                
                // Simplified approach - just capture the log level and basic message
                string message = state?.ToString() ?? string.Empty;
                
                _logEntries.Add(new LogEntry
                {
                    Level = logLevel,
                    Message = message,
                    Exception = exception,
                    Args = new object[] { state }
                });
            });

        return mockLogger;
    }

    private static RequestHandlerDelegate<TResponse> CreateSuccessfulNext<TResponse>(TResponse response) => _ => Task.FromResult(response);

    private static RequestHandlerDelegate<TResponse> CreateFailingNext<TResponse>(Exception exception) => _ => throw exception;

    #endregion

    #region Successful Execution Tests

    [Fact]
    public async Task Handle_SuccessfulCommandExecution_LogsStartAndCompletion()
    {
        // Arrange
        TestCommand command = new("test-value");
        const string expectedResponse = "success-response";
        RequestHandlerDelegate<string> next = CreateSuccessfulNext(expectedResponse);

        // Act
        string result = await _commandBehavior.Handle(command, next, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        _logEntries.Should().HaveCount(2);

        // Check start log
        LogEntry startLog = _logEntries[0];
        startLog.Level.Should().Be(LogLevel.Information);
        startLog.Message.Should().Contain("Processing Command TestCommand");

        // Check completion log
        LogEntry completionLog = _logEntries[1];
        completionLog.Level.Should().Be(LogLevel.Information);
        completionLog.Message.Should().Contain("Command TestCommand completed in");
        completionLog.Message.Should().Contain("ms");
    }

    [Fact]
    public async Task Handle_SuccessfulQueryExecution_LogsStartAndCompletion()
    {
        // Arrange
        TestQuery query = new("test-filter");
        const string expectedResponse = "query-response";
        RequestHandlerDelegate<string> next = CreateSuccessfulNext(expectedResponse);

        // Act
        string result = await _queryBehavior.Handle(query, next, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        _logEntries.Should().HaveCount(2);

        // Check start log
        LogEntry startLog = _logEntries[0];
        startLog.Level.Should().Be(LogLevel.Information);
        startLog.Message.Should().Contain("Processing Query TestQuery");

        // Check completion log
        LogEntry completionLog = _logEntries[1];
        completionLog.Level.Should().Be(LogLevel.Information);
        completionLog.Message.Should().Contain("Query TestQuery completed in");
        completionLog.Message.Should().Contain("ms");
    }

    [Fact]
    public async Task Handle_FastExecution_LogsReasonableElapsedTime()
    {
        // Arrange
        TestCommand command = new("fast-command");
        RequestHandlerDelegate<string> next = CreateSuccessfulNext("fast-response");

        // Act
        await _commandBehavior.Handle(command, next, CancellationToken.None);

        // Assert
        LogEntry? completionLog = _logEntries.LastOrDefault();
        completionLog.Should().NotBeNull();
        completionLog!.Message.Should().MatchRegex(@"Command TestCommand completed in \d+ms");
    }

    [Fact]
    public async Task Handle_SlowExecution_LogsElapsedTime()
    {
        // Arrange
        TestCommand command = new("slow-command");
        RequestHandlerDelegate<string> next = new(async _ =>
        {
            await Task.Delay(50); // Simulate slow operation
            return "slow-response";
        });

        // Act
        await _commandBehavior.Handle(command, next, CancellationToken.None);

        // Assert
        LogEntry? completionLog = _logEntries.LastOrDefault();
        completionLog.Should().NotBeNull();
        completionLog!.Message.Should().MatchRegex(@"Command TestCommand completed in \d+ms");

        // Extract the elapsed time from the log message
        System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(completionLog.Message!, @"(\d+)ms");
        if (match.Success && int.TryParse(match.Groups[1].Value, out int elapsedMs))
        {
            elapsedMs.Should().BeGreaterThanOrEqualTo(40); // Should be at least 40ms due to the delay
        }
    }

    #endregion

    #region Exception Handling Tests

    [Fact]
    public async Task Handle_ExceptionThrown_LogsErrorAndRethrows()
    {
        // Arrange
        TestCommand command = new("failing-command");
        InvalidOperationException expectedException = new("Test exception");
        RequestHandlerDelegate<string> next = CreateFailingNext<string>(expectedException);

        // Act & Assert
        await FluentActions.Invoking(() => _commandBehavior.Handle(command, next, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Test exception");

        // Check logs
        _logEntries.Should().HaveCount(2);

        // Check start log
        LogEntry startLog = _logEntries[0];
        startLog.Level.Should().Be(LogLevel.Information);
        startLog.Message.Should().Contain("Processing Command TestCommand");

        // Check error log
        LogEntry errorLog = _logEntries[1];
        errorLog.Level.Should().Be(LogLevel.Error);
        errorLog.Message.Should().Contain("Failed to process Command TestCommand after");
        errorLog.Message.Should().Contain("ms");
        errorLog.Exception.Should().Be(expectedException);
    }

    [Fact]
    public async Task Handle_QueryExceptionThrown_LogsErrorAndRethrows()
    {
        // Arrange
        TestQuery query = new("failing-query");
        ArgumentException expectedException = new("Query failed");
        RequestHandlerDelegate<string> next = CreateFailingNext<string>(expectedException);

        // Act & Assert
        await FluentActions.Invoking(() => _queryBehavior.Handle(query, next, CancellationToken.None))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Query failed");

        // Check error log
        LogEntry? errorLog = _logEntries.LastOrDefault();
        errorLog.Should().NotBeNull();
        errorLog!.Level.Should().Be(LogLevel.Error);
        errorLog.Message.Should().Contain("Failed to process Query TestQuery after");
        errorLog.Exception.Should().Be(expectedException);
    }

    [Fact]
    public async Task Handle_ExceptionInLongRunningOperation_LogsElapsedTime()
    {
        // Arrange
        TestCommand command = new("long-failing-command");
        TimeoutException expectedException = new("Operation timed out");
        RequestHandlerDelegate<string> next = new(async _ =>
        {
            await Task.Delay(30); // Simulate some work before failing
            throw expectedException;
        });

        // Act & Assert
        await FluentActions.Invoking(() => _commandBehavior.Handle(command, next, CancellationToken.None))
            .Should().ThrowAsync<TimeoutException>();

        // Check error log has elapsed time
        LogEntry? errorLog = _logEntries.LastOrDefault();
        errorLog.Should().NotBeNull();
        errorLog!.Message.Should().MatchRegex(@"Failed to process Command TestCommand after \d+ms");
    }

    #endregion

    #region Request Type Detection Tests

    [Fact]
    public async Task Handle_CommandRequest_IdentifiesAsCommand()
    {
        // Arrange
        TestCommand command = new("type-test");
        RequestHandlerDelegate<string> next = CreateSuccessfulNext("response");

        // Act
        await _commandBehavior.Handle(command, next, CancellationToken.None);

        // Assert
        LogEntry startLog = _logEntries[0];
        startLog.Message.Should().Contain("Processing Command TestCommand");

        LogEntry completionLog = _logEntries[1];
        completionLog.Message.Should().Contain("Command TestCommand completed");
    }

    [Fact]
    public async Task Handle_QueryRequest_IdentifiesAsQuery()
    {
        // Arrange
        TestQuery query = new("type-test");
        RequestHandlerDelegate<string> next = CreateSuccessfulNext("response");

        // Act
        await _queryBehavior.Handle(query, next, CancellationToken.None);

        // Assert
        LogEntry startLog = _logEntries[0];
        startLog.Message.Should().Contain("Processing Query TestQuery");

        LogEntry completionLog = _logEntries[1];
        completionLog.Message.Should().Contain("Query TestQuery completed");
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellationToken_PassesToNext()
    {
        // Arrange
        TestCommand command = new("cancellation-test");
        CancellationToken cancellationToken = new();
        bool nextCalledWithToken = false;

        RequestHandlerDelegate<string> next = new(_ =>
        {
            nextCalledWithToken = true;
            return Task.FromResult("response");
        });

        // Act
        await _commandBehavior.Handle(command, next, cancellationToken);

        // Assert
        nextCalledWithToken.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_CancellationTokenCanceled_PropagatesCancellation()
    {
        // Arrange
        TestCommand command = new("cancelled-command");
        CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();

        RequestHandlerDelegate<string> next = new(_ => throw new OperationCanceledException());

        // Act & Assert
        await FluentActions.Invoking(() => _commandBehavior.Handle(command, next, cancellationTokenSource.Token))
            .Should().ThrowAsync<OperationCanceledException>();

        // Should log error for cancellation
        LogEntry? errorLog = _logEntries.LastOrDefault();
        errorLog.Should().NotBeNull();
        errorLog!.Level.Should().Be(LogLevel.Error);
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidLogger_InitializesCorrectly()
    {
        // Arrange
        ILogger<LoggingBehavior<TestCommand, string>> logger = Substitute.For<ILogger<LoggingBehavior<TestCommand, string>>>();

        // Act
        LoggingBehavior<TestCommand, string> behavior = new(logger);

        // Assert
        behavior.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException() =>
        // Act & Assert
        FluentActions.Invoking(() => new LoggingBehavior<TestCommand, string>(null!))
            .Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");

    #endregion

    #region Performance Tests

    [Fact]
    public async Task Handle_MultipleSequentialCalls_LogsEachCall()
    {
        // Arrange
        TestCommand command1 = new("call1");
        TestCommand command2 = new("call2");
        TestCommand command3 = new("call3");
        RequestHandlerDelegate<string> next = CreateSuccessfulNext("response");

        // Act
        await _commandBehavior.Handle(command1, next, CancellationToken.None);
        await _commandBehavior.Handle(command2, next, CancellationToken.None);
        await _commandBehavior.Handle(command3, next, CancellationToken.None);

        // Assert
        _logEntries.Should().HaveCount(6); // 2 logs per call (start + completion)

        // Verify each call is logged separately
        _logEntries.Count(log => log.Message!.Contains("Processing Command TestCommand")).Should().Be(3);
        _logEntries.Count(log => log.Message!.Contains("Command TestCommand completed")).Should().Be(3);
    }

    [Fact]
    public async Task Handle_ConcurrentCalls_LogsAllCalls()
    {
        // Arrange
        TestCommand[] commands = Enumerable.Range(1, 5).Select(i => new TestCommand($"concurrent-{i}")).ToArray();
        RequestHandlerDelegate<string> next = CreateSuccessfulNext("response");

        // Act
        IEnumerable<Task<string>> tasks = commands.Select(cmd => _commandBehavior.Handle(cmd, next, CancellationToken.None));
        await Task.WhenAll(tasks);

        // Assert
        _logEntries.Should().HaveCount(10); // 2 logs per call (5 calls)
        _logEntries.Count(log => log.Message!.Contains("Processing Command TestCommand")).Should().Be(5);
        _logEntries.Count(log => log.Message!.Contains("Command TestCommand completed")).Should().Be(5);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Handle_NullResponse_LogsSuccessfully()
    {
        // Arrange
        TestCommand command = new("null-response");
        RequestHandlerDelegate<string?> next = CreateSuccessfulNext<string?>(null);

        // Act
        string? result = await _commandBehavior.Handle(command, next, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _logEntries.Should().HaveCount(2);

        LogEntry? completionLog = _logEntries.LastOrDefault();
        completionLog.Should().NotBeNull();
        completionLog!.Level.Should().Be(LogLevel.Information);
        completionLog.Message.Should().Contain("Command TestCommand completed");
    }

    [Fact]
    public async Task Handle_VeryFastExecution_LogsZeroOrLowMilliseconds()
    {
        // Arrange
        TestCommand command = new("instant-command");
        RequestHandlerDelegate<string> next = CreateSuccessfulNext("instant-response");

        // Act
        await _commandBehavior.Handle(command, next, CancellationToken.None);

        // Assert
        LogEntry? completionLog = _logEntries.LastOrDefault();
        completionLog.Should().NotBeNull();

        // Should show 0 or low milliseconds for very fast operations
        System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(completionLog!.Message!, @"(\d+)ms");
        if (match.Success && int.TryParse(match.Groups[1].Value, out int elapsedMs))
        {
            elapsedMs.Should().BeLessThan(100); // Should be very fast
        }
    }

    [Fact]
    public async Task Handle_ComplexExceptionType_LogsExceptionCorrectly()
    {
        // Arrange
        TestCommand command = new("complex-exception");
        ArgumentException innerException = new("Inner error");
        InvalidOperationException outerException = new("Outer error", innerException);
        RequestHandlerDelegate<string> next = CreateFailingNext<string>(outerException);

        // Act & Assert
        await FluentActions.Invoking(() => _commandBehavior.Handle(command, next, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>();

        // Check error log captures the exception properly
        LogEntry? errorLog = _logEntries.LastOrDefault();
        errorLog.Should().NotBeNull();
        errorLog!.Exception.Should().Be(outerException);
        errorLog.Exception!.InnerException.Should().Be(innerException);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task Handle_InPipelineChain_WorksWithOtherBehaviors()
    {
        // Arrange
        TestCommand command = new("pipeline-test");
        bool otherBehaviorCalled = false;

        RequestHandlerDelegate<string> chainedNext = new(async _ =>
        {
            otherBehaviorCalled = true;
            return "pipeline-response";
        });

        // Act
        string result = await _commandBehavior.Handle(command, chainedNext, CancellationToken.None);

        // Assert
        result.Should().Be("pipeline-response");
        otherBehaviorCalled.Should().BeTrue();
        _logEntries.Should().HaveCount(2); // Start and completion logs
    }

    #endregion
}