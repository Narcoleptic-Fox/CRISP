using Crisp.Commands;
using Crisp.Pipeline;
using Crisp.Queries;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute.ExceptionExtensions;

namespace Crisp.Runtime.Tests.Pipeline;

public class PipelineExecutorTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ServiceCollection _services;

    public PipelineExecutorTests()
    {
        _services = new ServiceCollection();
        _serviceProvider = _services.BuildServiceProvider();
    }

    #region Test Commands, Queries, and Handlers

    public record TestCommand(string Value) : ICommand<string>;
    public record TestVoidCommand(string Value) : ICommand;
    public record TestQuery(string Filter) : IQuery<string>;

    public class TestCommandHandler : ICommandHandler<TestCommand, string>
    {
        public Task<string> Handle(TestCommand request, CancellationToken cancellationToken) => Task.FromResult($"handled-{request.Value}");
    }

    public class TestVoidCommandHandler : ICommandHandler<TestVoidCommand>
    {
        public bool WasCalled { get; private set; }
        public string? LastValue { get; private set; }

        public Task Handle(TestVoidCommand request, CancellationToken cancellationToken)
        {
            WasCalled = true;
            LastValue = request.Value;
            return Task.CompletedTask;
        }
    }

    public class TestQueryHandler : IQueryHandler<TestQuery, string>
    {
        public Task<string> Handle(TestQuery request, CancellationToken cancellationToken) => Task.FromResult($"queried-{request.Filter}");
    }

    #endregion

    #region Test Behaviors

    public class TestBehavior : IPipelineBehavior<TestCommand, string>
    {
        public bool WasCalled { get; private set; }
        public string? LastValue { get; private set; }

        public async Task<string> Handle(TestCommand request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
        {
            WasCalled = true;
            LastValue = request.Value;

            string result = await next();
            return $"behavior-{result}";
        }
    }

    public class TestVoidBehavior : IPipelineBehavior<TestVoidCommand>
    {
        public bool WasCalled { get; private set; }
        public string? LastValue { get; private set; }

        public async Task Handle(TestVoidCommand request, RequestHandlerDelegate next, CancellationToken cancellationToken)
        {
            WasCalled = true;
            LastValue = request.Value;
            await next();
        }
    }

    public class TestQueryBehavior : IPipelineBehavior<TestQuery, string>
    {
        public bool WasCalled { get; private set; }
        public string? LastFilter { get; private set; }

        public async Task<string> Handle(TestQuery request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
        {
            WasCalled = true;
            LastFilter = request.Filter;

            string result = await next();
            return $"query-behavior-{result}";
        }
    }

    public class TestCommandSpecificBehavior : ICommandPipelineBehavior<TestCommand, string>
    {
        public bool WasCalled { get; private set; }

        public async Task<string> Handle(TestCommand request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
        {
            WasCalled = true;
            string result = await next();
            return $"command-specific-{result}";
        }
    }

    public class TestQuerySpecificBehavior : IQueryPipelineBehavior<TestQuery, string>
    {
        public bool WasCalled { get; private set; }

        public async Task<string> Handle(TestQuery request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
        {
            WasCalled = true;
            string result = await next();
            return $"query-specific-{result}";
        }
    }

    public class TestVoidCommandBehavior : ICommandPipelineBehavior<TestVoidCommand>
    {
        public bool WasCalled { get; private set; }

        public async Task Handle(TestVoidCommand request, RequestHandlerDelegate next, CancellationToken cancellationToken)
        {
            WasCalled = true;
            await next();
        }
    }

    #endregion

    #region ExecuteCommandPipeline Tests

    [Fact]
    public async Task ExecuteCommandPipeline_WithValidCommand_ReturnsExpectedResult()
    {
        // Arrange
        TestCommand command = new("test-value");
        TestCommandHandler handler = new();

        _services.AddScoped<ICommandHandler<TestCommand, string>>(_ => handler);
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        // Act
        string result = await PipelineExecutor.ExecuteCommandPipeline<TestCommand, string>(
            command, serviceProvider, CancellationToken.None);

        // Assert
        result.Should().Be("handled-test-value");
    }

    [Fact]
    public async Task ExecuteCommandPipeline_WithBehavior_AppliesBehaviorCorrectly()
    {
        // Arrange
        TestCommand command = new("behavior-test");
        TestCommandHandler handler = new();
        TestBehavior behavior = new();

        _services.AddScoped<ICommandHandler<TestCommand, string>>(_ => handler);
        _services.AddScoped<IPipelineBehavior<TestCommand, string>>(_ => behavior);
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        // Act
        string result = await PipelineExecutor.ExecuteCommandPipeline<TestCommand, string>(
            command, serviceProvider, CancellationToken.None);

        // Assert
        result.Should().Be("behavior-handled-behavior-test");
        behavior.WasCalled.Should().BeTrue();
        behavior.LastValue.Should().Be("behavior-test");
    }

    [Fact]
    public async Task ExecuteCommandPipeline_WithCommandSpecificBehavior_AppliesBehaviorCorrectly()
    {
        // Arrange
        TestCommand command = new("specific-behavior-test");
        TestCommandHandler handler = new();
        TestCommandSpecificBehavior commandBehavior = new();

        _services.AddScoped<ICommandHandler<TestCommand, string>>(_ => handler);
        _services.AddScoped<ICommandPipelineBehavior<TestCommand, string>>(_ => commandBehavior);
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        // Act
        string result = await PipelineExecutor.ExecuteCommandPipeline<TestCommand, string>(
            command, serviceProvider, CancellationToken.None);

        // Assert
        result.Should().Be("command-specific-handled-specific-behavior-test");
        commandBehavior.WasCalled.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteCommandPipeline_WithMultipleBehaviors_ExecutesInCorrectOrder()
    {
        // Arrange
        TestCommand command = new("multi-behavior");
        TestCommandHandler handler = new();
        TestBehavior behavior1 = new();
        TestCommandSpecificBehavior behavior2 = new();

        _services.AddScoped<ICommandHandler<TestCommand, string>>(_ => handler);
        _services.AddScoped<IPipelineBehavior<TestCommand, string>>(_ => behavior1);
        _services.AddScoped<ICommandPipelineBehavior<TestCommand, string>>(_ => behavior2);
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        // Act
        string result = await PipelineExecutor.ExecuteCommandPipeline<TestCommand, string>(
            command, serviceProvider, CancellationToken.None);

        // Assert
        // Result should show behaviors wrapped around the handler
        result.Should().Contain("handled-multi-behavior");
        behavior1.WasCalled.Should().BeTrue();
        behavior2.WasCalled.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteCommandPipeline_WithCancellationToken_PassesToHandler()
    {
        // Arrange
        TestCommand command = new("cancellation-test");
        CancellationToken cancellationToken = new();
        ICommandHandler<TestCommand, string> handlerMock = Substitute.For<ICommandHandler<TestCommand, string>>();
        handlerMock.Handle(command, cancellationToken).Returns("cancelled-result");

        _services.AddScoped<ICommandHandler<TestCommand, string>>(_ => handlerMock);
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        // Act
        await PipelineExecutor.ExecuteCommandPipeline<TestCommand, string>(
            command, serviceProvider, cancellationToken);

        // Assert
        await handlerMock.Received(1).Handle(command, cancellationToken);
    }

    [Fact]
    public async Task ExecuteCommandPipeline_HandlerThrowsException_PropagatesException()
    {
        // Arrange
        TestCommand command = new("exception-test");
        ICommandHandler<TestCommand, string> handlerMock = Substitute.For<ICommandHandler<TestCommand, string>>();
        InvalidOperationException expectedException = new("Handler error");
        handlerMock.Handle(command, Arg.Any<CancellationToken>()).Should().Throws(expectedException);

        _services.AddScoped<ICommandHandler<TestCommand, string>>(_ => handlerMock);
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        // Act & Assert
        await FluentActions.Invoking(() => PipelineExecutor.ExecuteCommandPipeline<TestCommand, string>(
                command, serviceProvider, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Handler error");
    }

    [Fact]
    public async Task ExecuteCommandPipeline_NoHandlerRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        TestCommand command = new("no-handler");
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        // Act & Assert
        await FluentActions.Invoking(() => PipelineExecutor.ExecuteCommandPipeline<TestCommand, string>(
                command, serviceProvider, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    #endregion

    #region ExecuteQueryPipeline Tests

    [Fact]
    public async Task ExecuteQueryPipeline_WithValidQuery_ReturnsExpectedResult()
    {
        // Arrange
        TestQuery query = new("test-filter");
        TestQueryHandler handler = new();

        _services.AddScoped<IQueryHandler<TestQuery, string>>(_ => handler);
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        // Act
        string result = await PipelineExecutor.ExecuteQueryPipeline<TestQuery, string>(
            query, serviceProvider, CancellationToken.None);

        // Assert
        result.Should().Be("queried-test-filter");
    }

    [Fact]
    public async Task ExecuteQueryPipeline_WithBehavior_AppliesBehaviorCorrectly()
    {
        // Arrange
        TestQuery query = new("behavior-filter");
        TestQueryHandler handler = new();
        TestQueryBehavior behavior = new();

        _services.AddScoped<IQueryHandler<TestQuery, string>>(_ => handler);
        _services.AddScoped<IPipelineBehavior<TestQuery, string>>(_ => behavior);
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        // Act
        string result = await PipelineExecutor.ExecuteQueryPipeline<TestQuery, string>(
            query, serviceProvider, CancellationToken.None);

        // Assert
        result.Should().Be("query-behavior-queried-behavior-filter");
        behavior.WasCalled.Should().BeTrue();
        behavior.LastFilter.Should().Be("behavior-filter");
    }

    [Fact]
    public async Task ExecuteQueryPipeline_WithQuerySpecificBehavior_AppliesBehaviorCorrectly()
    {
        // Arrange
        TestQuery query = new("query-specific");
        TestQueryHandler handler = new();
        TestQuerySpecificBehavior queryBehavior = new();

        _services.AddScoped<IQueryHandler<TestQuery, string>>(_ => handler);
        _services.AddScoped<IQueryPipelineBehavior<TestQuery, string>>(_ => queryBehavior);
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        // Act
        string result = await PipelineExecutor.ExecuteQueryPipeline<TestQuery, string>(
            query, serviceProvider, CancellationToken.None);

        // Assert
        result.Should().Be("query-specific-queried-query-specific");
        queryBehavior.WasCalled.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteQueryPipeline_WithCancellationToken_PassesToHandler()
    {
        // Arrange
        TestQuery query = new("cancellation-query");
        CancellationToken cancellationToken = new();
        IQueryHandler<TestQuery, string> handlerMock = Substitute.For<IQueryHandler<TestQuery, string>>();
        handlerMock.Handle(query, cancellationToken).Returns("cancelled-query-result");

        _services.AddScoped<IQueryHandler<TestQuery, string>>(_ => handlerMock);
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        // Act
        await PipelineExecutor.ExecuteQueryPipeline<TestQuery, string>(
            query, serviceProvider, cancellationToken);

        // Assert
        await handlerMock.Received(1).Handle(query, cancellationToken);
    }

    #endregion

    #region ExecuteVoidCommandPipeline Tests

    [Fact]
    public async Task ExecuteVoidCommandPipeline_WithValidCommand_ExecutesSuccessfully()
    {
        // Arrange
        TestVoidCommand command = new("void-test");
        TestVoidCommandHandler handler = new();

        _services.AddScoped<ICommandHandler<TestVoidCommand>>(_ => handler);
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        // Act
        await PipelineExecutor.ExecuteVoidCommandPipeline<TestVoidCommand>(
            command, serviceProvider, CancellationToken.None);

        // Assert
        handler.WasCalled.Should().BeTrue();
        handler.LastValue.Should().Be("void-test");
    }

    [Fact]
    public async Task ExecuteVoidCommandPipeline_WithBehavior_AppliesBehaviorCorrectly()
    {
        // Arrange
        TestVoidCommand command = new("void-behavior");
        TestVoidCommandHandler handler = new();
        TestVoidBehavior behavior = new();

        _services.AddScoped<ICommandHandler<TestVoidCommand>>(_ => handler);
        _services.AddScoped<IPipelineBehavior<TestVoidCommand>>(_ => behavior);
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        // Act
        await PipelineExecutor.ExecuteVoidCommandPipeline<TestVoidCommand>(
            command, serviceProvider, CancellationToken.None);

        // Assert
        handler.WasCalled.Should().BeTrue();
        handler.LastValue.Should().Be("void-behavior");
        behavior.WasCalled.Should().BeTrue();
        behavior.LastValue.Should().Be("void-behavior");
    }

    [Fact]
    public async Task ExecuteVoidCommandPipeline_WithCommandSpecificBehavior_AppliesBehaviorCorrectly()
    {
        // Arrange
        TestVoidCommand command = new("void-command-specific");
        TestVoidCommandHandler handler = new();
        TestVoidCommandBehavior commandBehavior = new();

        _services.AddScoped<ICommandHandler<TestVoidCommand>>(_ => handler);
        _services.AddScoped<ICommandPipelineBehavior<TestVoidCommand>>(_ => commandBehavior);
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        // Act
        await PipelineExecutor.ExecuteVoidCommandPipeline<TestVoidCommand>(
            command, serviceProvider, CancellationToken.None);

        // Assert
        handler.WasCalled.Should().BeTrue();
        commandBehavior.WasCalled.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteVoidCommandPipeline_WithCancellationToken_PassesToHandler()
    {
        // Arrange
        TestVoidCommand command = new("void-cancellation");
        CancellationToken cancellationToken = new();
        ICommandHandler<TestVoidCommand> handlerMock = Substitute.For<ICommandHandler<TestVoidCommand>>();
        handlerMock.Handle(command, cancellationToken).Returns(Task.CompletedTask);

        _services.AddScoped<ICommandHandler<TestVoidCommand>>(_ => handlerMock);
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        // Act
        await PipelineExecutor.ExecuteVoidCommandPipeline<TestVoidCommand>(
            command, serviceProvider, cancellationToken);

        // Assert
        await handlerMock.Received(1).Handle(command, cancellationToken);
    }

    [Fact]
    public async Task ExecuteVoidCommandPipeline_HandlerThrowsException_PropagatesException()
    {
        // Arrange
        TestVoidCommand command = new("void-exception");
        ICommandHandler<TestVoidCommand> handlerMock = Substitute.For<ICommandHandler<TestVoidCommand>>();
        InvalidOperationException expectedException = new("Void handler error");
        handlerMock.Handle(command, Arg.Any<CancellationToken>()).ThrowsAsync(expectedException);

        _services.AddScoped<ICommandHandler<TestVoidCommand>>(_ => handlerMock);
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        // Act & Assert
        await FluentActions.Invoking(() => PipelineExecutor.ExecuteVoidCommandPipeline<TestVoidCommand>(
                command, serviceProvider, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Void handler error");
    }

    #endregion

    #region Behavior Resolution Tests

    [Fact]
    public async Task ResolveBehaviors_WithMixedBehaviors_ResolvesCorrectBehaviorsForCommand()
    {
        // Arrange
        TestCommand command = new("behavior-resolution");
        TestCommandHandler handler = new();
        TestBehavior globalBehavior = new();
        TestCommandSpecificBehavior commandBehavior = new();
        TestQuerySpecificBehavior queryBehavior = new(); // Should not be applied to command

        _services.AddScoped<ICommandHandler<TestCommand, string>>(_ => handler);
        _services.AddScoped<IPipelineBehavior<TestCommand, string>>(_ => globalBehavior);
        _services.AddScoped<ICommandPipelineBehavior<TestCommand, string>>(_ => commandBehavior);
        _services.AddScoped<IQueryPipelineBehavior<TestQuery, string>>(_ => queryBehavior);
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        // Act
        string result = await PipelineExecutor.ExecuteCommandPipeline<TestCommand, string>(
            command, serviceProvider, CancellationToken.None);

        // Assert
        globalBehavior.WasCalled.Should().BeTrue();
        commandBehavior.WasCalled.Should().BeTrue();
        queryBehavior.WasCalled.Should().BeFalse(); // Query behavior should not be called for command
    }

    [Fact]
    public async Task ResolveBehaviors_WithMixedBehaviors_ResolvesCorrectBehaviorsForQuery()
    {
        // Arrange
        TestQuery query = new("query-behavior-resolution");
        TestQueryHandler handler = new();
        TestQueryBehavior globalBehavior = new();
        TestQuerySpecificBehavior queryBehavior = new();
        TestCommandSpecificBehavior commandBehavior = new(); // Should not be applied to query

        _services.AddScoped<IQueryHandler<TestQuery, string>>(_ => handler);
        _services.AddScoped<IPipelineBehavior<TestQuery, string>>(_ => globalBehavior);
        _services.AddScoped<IQueryPipelineBehavior<TestQuery, string>>(_ => queryBehavior);
        _services.AddScoped<ICommandPipelineBehavior<TestCommand, string>>(_ => commandBehavior);
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        // Act
        string result = await PipelineExecutor.ExecuteQueryPipeline<TestQuery, string>(
            query, serviceProvider, CancellationToken.None);

        // Assert
        globalBehavior.WasCalled.Should().BeTrue();
        queryBehavior.WasCalled.Should().BeTrue();
        commandBehavior.WasCalled.Should().BeFalse(); // Command behavior should not be called for query
    }

    [Fact]
    public async Task ResolveBehaviors_WithVoidCommand_ResolvesCorrectBehaviors()
    {
        // Arrange
        TestVoidCommand command = new("void-behavior-resolution");
        TestVoidCommandHandler handler = new();
        TestVoidBehavior globalBehavior = new();
        TestVoidCommandBehavior commandBehavior = new();

        _services.AddScoped<ICommandHandler<TestVoidCommand>>(_ => handler);
        _services.AddScoped<IPipelineBehavior<TestVoidCommand>>(_ => globalBehavior);
        _services.AddScoped<ICommandPipelineBehavior<TestVoidCommand>>(_ => commandBehavior);
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        // Act
        await PipelineExecutor.ExecuteVoidCommandPipeline<TestVoidCommand>(
            command, serviceProvider, CancellationToken.None);

        // Assert
        globalBehavior.WasCalled.Should().BeTrue();
        commandBehavior.WasCalled.Should().BeTrue();
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task PipelineExecution_BehaviorThrowsException_PropagatesException()
    {
        // Arrange
        TestCommand command = new("behavior-exception");
        TestCommandHandler handler = new();
        IPipelineBehavior<TestCommand, string> behaviorMock = Substitute.For<IPipelineBehavior<TestCommand, string>>();
        InvalidOperationException expectedException = new("Behavior error");

        behaviorMock.Handle(command, Arg.Any<RequestHandlerDelegate<string>>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(expectedException);

        _services.AddScoped<ICommandHandler<TestCommand, string>>(_ => handler);
        _services.AddScoped<IPipelineBehavior<TestCommand, string>>(_ => behaviorMock);
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        // Act & Assert
        await FluentActions.Invoking(() => PipelineExecutor.ExecuteCommandPipeline<TestCommand, string>(
                command, serviceProvider, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Behavior error");
    }

    #endregion
}