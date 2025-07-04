using Crisp.Commands;
using Crisp.Dispatchers;
using Crisp.Pipeline;
using NSubstitute.ExceptionExtensions;

namespace Crisp.Runtime.Tests.Dispatchers;

public class PreCompiledCommandDispatcherTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<Type, ICompiledPipeline> _pipelines;
    private readonly PreCompiledCommandDispatcher _dispatcher;

    public PreCompiledCommandDispatcherTests()
    {
        _serviceProvider = Substitute.For<IServiceProvider>();
        _pipelines = [];
        _dispatcher = new PreCompiledCommandDispatcher(_pipelines, _serviceProvider);
    }

    #region Test Commands and Responses

    public record TestCommand(string Value) : ICommand<string>;
    public record TestVoidCommand(string Value) : ICommand;
    public record TestResponse(string Result);

    #endregion

    #region Generic Send Method Tests

    [Fact]
    public async Task Send_WithValidCommand_ReturnsCorrectResponse()
    {
        // Arrange
        const string expectedResult = "test-result";
        TestCommand command = new("test-value");

        ICompiledPipeline<string> mockPipeline = Substitute.For<ICompiledPipeline<string>>();
        mockPipeline.ExecuteAsync(command, _serviceProvider, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        _pipelines[typeof(TestCommand)] = mockPipeline;

        // Act
        string result = await _dispatcher.Send<string>(command);

        // Assert
        result.Should().Be(expectedResult);
        await mockPipeline.Received(1).ExecuteAsync(command, _serviceProvider, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Send_WithCancellationToken_PassesTokenToPipeline()
    {
        // Arrange
        CancellationToken cancellationToken = new();
        TestCommand command = new("test-value");

        ICompiledPipeline<string> mockPipeline = Substitute.For<ICompiledPipeline<string>>();
        mockPipeline.ExecuteAsync(command, _serviceProvider, cancellationToken)
            .Returns("result");

        _pipelines[typeof(TestCommand)] = mockPipeline;

        // Act
        await _dispatcher.Send<string>(command, cancellationToken);

        // Assert
        await mockPipeline.Received(1).ExecuteAsync(command, _serviceProvider, cancellationToken);
    }

    [Fact]
    public async Task Send_WithNullCommand_ThrowsArgumentNullException() =>
        // Act & Assert
        await FluentActions.Invoking(() => _dispatcher.Send<string>(null!))
            .Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("command");

    [Fact]
    public async Task Send_WithUnregisteredCommand_ThrowsInvalidOperationException()
    {
        // Arrange
        TestCommand command = new("test-value");

        // Act & Assert
        await FluentActions.Invoking(() => _dispatcher.Send<string>(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No handler registered for command 'TestCommand'. Make sure the handler is registered and the assembly is scanned during startup.");
    }

    [Fact]
    public async Task Send_WithVoidPipelineButGenericCall_ThrowsInvalidOperationException()
    {
        // Arrange
        TestCommand command = new("test-value");
        ICompiledVoidPipeline voidPipeline = Substitute.For<ICompiledVoidPipeline>();
        _pipelines[typeof(TestCommand)] = voidPipeline;

        // Act & Assert
        await FluentActions.Invoking(() => _dispatcher.Send<string>(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Command 'TestCommand' is registered as a void command but was called with a response type 'String'. Use the void Send method instead.");
    }

    [Fact]
    public async Task Send_PipelineThrowsException_PropagatesException()
    {
        // Arrange
        TestCommand command = new("test-value");
        ICompiledPipeline<string> mockPipeline = Substitute.For<ICompiledPipeline<string>>();
        InvalidOperationException expectedException = new("Pipeline error");

        mockPipeline.ExecuteAsync(command, _serviceProvider, Arg.Any<CancellationToken>())
            .ThrowsAsync(expectedException);

        _pipelines[typeof(TestCommand)] = mockPipeline;

        // Act & Assert
        await FluentActions.Invoking(() => _dispatcher.Send<string>(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Pipeline error");
    }

    #endregion

    #region Void Send Method Tests

    [Fact]
    public async Task Send_VoidCommand_CompletesSuccessfully()
    {
        // Arrange
        TestVoidCommand command = new("test-value");
        ICompiledVoidPipeline mockPipeline = Substitute.For<ICompiledVoidPipeline>();
        mockPipeline.ExecuteAsync(command, _serviceProvider, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _pipelines[typeof(TestVoidCommand)] = mockPipeline;

        // Act
        await _dispatcher.Send(command);

        // Assert
        await mockPipeline.Received(1).ExecuteAsync(command, _serviceProvider, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Send_VoidCommandWithCancellationToken_PassesTokenToPipeline()
    {
        // Arrange
        CancellationToken cancellationToken = new();
        TestVoidCommand command = new("test-value");
        ICompiledVoidPipeline mockPipeline = Substitute.For<ICompiledVoidPipeline>();
        mockPipeline.ExecuteAsync(command, _serviceProvider, cancellationToken)
            .Returns(Task.CompletedTask);

        _pipelines[typeof(TestVoidCommand)] = mockPipeline;

        // Act
        await _dispatcher.Send(command, cancellationToken);

        // Assert
        await mockPipeline.Received(1).ExecuteAsync(command, _serviceProvider, cancellationToken);
    }

    [Fact]
    public async Task Send_VoidCommandWithNullCommand_ThrowsArgumentNullException() =>
        // Act & Assert
        await FluentActions.Invoking(() => _dispatcher.Send(null!))
            .Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("command");

    [Fact]
    public async Task Send_VoidCommandWithUnregisteredCommand_ThrowsInvalidOperationException()
    {
        // Arrange
        TestVoidCommand command = new("test-value");

        // Act & Assert
        await FluentActions.Invoking(() => _dispatcher.Send(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No handler registered for command 'TestVoidCommand'. Make sure the handler is registered and the assembly is scanned during startup.");
    }

    [Fact]
    public async Task Send_TypedPipelineButVoidCall_ThrowsInvalidOperationException()
    {
        // Arrange
        TestVoidCommand command = new("test-value");
        ICompiledPipeline<string> typedPipeline = Substitute.For<ICompiledPipeline<string>>();
        _pipelines[typeof(TestVoidCommand)] = typedPipeline;

        // Act & Assert
        await FluentActions.Invoking(() => _dispatcher.Send(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Command 'TestVoidCommand' is registered as a typed command but was called as void. Use the generic Send<TResponse> method instead.");
    }

    [Fact]
    public async Task Send_VoidPipelineThrowsException_PropagatesException()
    {
        // Arrange
        TestVoidCommand command = new("test-value");
        ICompiledVoidPipeline mockPipeline = Substitute.For<ICompiledVoidPipeline>();
        InvalidOperationException expectedException = new("Void pipeline error");

        mockPipeline.ExecuteAsync(command, _serviceProvider, Arg.Any<CancellationToken>())
            .ThrowsAsync(expectedException);

        _pipelines[typeof(TestVoidCommand)] = mockPipeline;

        // Act & Assert
        await FluentActions.Invoking(() => _dispatcher.Send(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Void pipeline error");
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_InitializesCorrectly()
    {
        // Arrange
        Dictionary<Type, ICompiledPipeline> pipelines = new();
        IServiceProvider serviceProvider = Substitute.For<IServiceProvider>();

        // Act
        PreCompiledCommandDispatcher dispatcher = new(pipelines, serviceProvider);

        // Assert
        dispatcher.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullPipelines_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceProvider serviceProvider = Substitute.For<IServiceProvider>();

        // Act & Assert
        FluentActions.Invoking(() => new PreCompiledCommandDispatcher(null!, serviceProvider))
            .Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullServiceProvider_ThrowsArgumentNullException()
    {
        // Arrange
        Dictionary<Type, ICompiledPipeline> pipelines = new();

        // Act & Assert
        FluentActions.Invoking(() => new PreCompiledCommandDispatcher(pipelines, null!))
            .Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Concurrent Access Tests

    [Fact]
    public async Task Send_ConcurrentAccess_HandlesCorrectly()
    {
        // Arrange
        TestCommand command1 = new("value1");
        TestCommand command2 = new("value2");

        ICompiledPipeline<string> mockPipeline = Substitute.For<ICompiledPipeline<string>>();
        mockPipeline.ExecuteAsync(Arg.Any<TestCommand>(), _serviceProvider, Arg.Any<CancellationToken>())
            .Returns(callInfo => $"result-{((TestCommand)callInfo[0]).Value}");

        _pipelines[typeof(TestCommand)] = mockPipeline;

        // Act
        Task<string> task1 = _dispatcher.Send<string>(command1);
        Task<string> task2 = _dispatcher.Send<string>(command2);

        string[] results = await Task.WhenAll(task1, task2);

        // Assert
        results.Should().Contain("result-value1");
        results.Should().Contain("result-value2");
        await mockPipeline.Received(2).ExecuteAsync(Arg.Any<TestCommand>(), _serviceProvider, Arg.Any<CancellationToken>());
    }

    #endregion

    #region Integration with Real Pipelines

    [Fact]
    public async Task Send_WithRealCompiledPipeline_ExecutesCorrectly()
    {
        // Arrange
        TestCommand command = new("integration-test");
        CompiledPipeline<string> realPipeline = new()
        {
            RequestType = typeof(TestCommand),
            ResponseType = typeof(string),
            HandlerType = typeof(object), // Mock handler type
            Executor = (request, serviceProvider, cancellationToken) =>
                Task.FromResult($"processed-{((TestCommand)request).Value}")
        };

        _pipelines[typeof(TestCommand)] = realPipeline;

        // Act
        string result = await _dispatcher.Send<string>(command);

        // Assert
        result.Should().Be("processed-integration-test");
    }

    [Fact]
    public async Task Send_WithRealVoidPipeline_ExecutesCorrectly()
    {
        // Arrange
        TestVoidCommand command = new("void-integration-test");
        bool wasExecuted = false;

        CompiledVoidPipeline realVoidPipeline = new()
        {
            RequestType = typeof(TestVoidCommand),
            HandlerType = typeof(object), // Mock handler type
            Executor = (request, serviceProvider, cancellationToken) =>
            {
                wasExecuted = true;
                return Task.CompletedTask;
            }
        };

        _pipelines[typeof(TestVoidCommand)] = realVoidPipeline;

        // Act
        await _dispatcher.Send(command);

        // Assert
        wasExecuted.Should().BeTrue();
    }

    #endregion
}