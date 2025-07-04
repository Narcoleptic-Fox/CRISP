using Crisp.Commands;
using Crisp.Pipeline;
using Crisp.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Crisp.Runtime.Tests.Pipeline;

public class CompiledPipelineTests
{
    private readonly IServiceProvider _serviceProvider;

    public CompiledPipelineTests()
    {
        _serviceProvider = Substitute.For<IServiceProvider>();
    }

    #region Test Commands and Queries

    public record TestCommand(string Value) : ICommand<string>;
    public record TestVoidCommand(string Value) : ICommand;
    public record TestQuery(string Filter) : IQuery<string>;

    #endregion

    #region CompiledPipeline<TResponse> Tests

    [Fact]
    public void CompiledPipeline_WithValidProperties_InitializesCorrectly()
    {
        // Arrange & Act
        var pipeline = new CompiledPipeline<string>
        {
            RequestType = typeof(TestCommand),
            ResponseType = typeof(string),
            HandlerType = typeof(object),
            Executor = (request, serviceProvider, cancellationToken) => Task.FromResult("test-result")
        };

        // Assert
        pipeline.RequestType.Should().Be(typeof(TestCommand));
        pipeline.ResponseType.Should().Be(typeof(string));
        pipeline.HandlerType.Should().Be(typeof(object));
        pipeline.Executor.Should().NotBeNull();
    }

    [Fact]
    public void CompiledPipeline_HandlerName_ReturnsHandlerTypeName()
    {
        // Arrange
        var pipeline = new CompiledPipeline<string>
        {
            HandlerType = typeof(TestCommand)
        };

        // Act & Assert
        pipeline.HandlerName.Should().Be("TestCommand");
    }

    [Fact]
    public void CompiledPipeline_RequestName_ReturnsRequestTypeName()
    {
        // Arrange
        var pipeline = new CompiledPipeline<string>
        {
            RequestType = typeof(TestQuery)
        };

        // Act & Assert
        pipeline.RequestName.Should().Be("TestQuery");
    }

    [Fact]
    public void CompiledPipeline_IsCommand_WithCommandType_ReturnsTrue()
    {
        // Arrange
        var pipeline = new CompiledPipeline<string>
        {
            RequestType = typeof(TestCommand)
        };

        // Act & Assert
        pipeline.IsCommand.Should().BeTrue();
    }

    [Fact]
    public void CompiledPipeline_IsCommand_WithQueryType_ReturnsFalse()
    {
        // Arrange
        var pipeline = new CompiledPipeline<string>
        {
            RequestType = typeof(TestQuery)
        };

        // Act & Assert
        pipeline.IsCommand.Should().BeFalse();
    }

    [Fact]
    public async Task CompiledPipeline_ExecuteAsync_CallsExecutor()
    {
        // Arrange
        const string expectedResult = "executor-result";
        bool executorCalled = false;
        object? capturedRequest = null;
        IServiceProvider? capturedServiceProvider = null;
        CancellationToken capturedToken = default;

        var pipeline = new CompiledPipeline<string>
        {
            RequestType = typeof(TestCommand),
            ResponseType = typeof(string),
            HandlerType = typeof(object),
            Executor = (request, serviceProvider, cancellationToken) =>
            {
                executorCalled = true;
                capturedRequest = request;
                capturedServiceProvider = serviceProvider;
                capturedToken = cancellationToken;
                return Task.FromResult(expectedResult);
            }
        };

        var testRequest = new TestCommand("test-value");
        var cancellationToken = new CancellationToken();

        // Act
        string result = await pipeline.ExecuteAsync(testRequest, _serviceProvider, cancellationToken);

        // Assert
        result.Should().Be(expectedResult);
        executorCalled.Should().BeTrue();
        capturedRequest.Should().Be(testRequest);
        capturedServiceProvider.Should().Be(_serviceProvider);
        capturedToken.Should().Be(cancellationToken);
    }

    [Fact]
    public async Task CompiledPipeline_ExecuteAsync_WithException_PropagatesException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Pipeline execution failed");
        var pipeline = new CompiledPipeline<string>
        {
            RequestType = typeof(TestCommand),
            ResponseType = typeof(string),
            HandlerType = typeof(object),
            Executor = (request, serviceProvider, cancellationToken) => throw expectedException
        };

        var testRequest = new TestCommand("test-value");

        // Act & Assert
        await FluentActions.Invoking(() => pipeline.ExecuteAsync(testRequest, _serviceProvider, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Pipeline execution failed");
    }

    [Fact]
    public async Task CompiledPipeline_ExecuteAsync_WithAsyncException_PropagatesException()
    {
        // Arrange
        var expectedException = new ArgumentException("Async pipeline error");
        var pipeline = new CompiledPipeline<string>
        {
            RequestType = typeof(TestCommand),
            ResponseType = typeof(string),
            HandlerType = typeof(object),
            Executor = async (request, serviceProvider, cancellationToken) =>
            {
                await Task.Delay(1, cancellationToken);
                throw expectedException;
            }
        };

        var testRequest = new TestCommand("test-value");

        // Act & Assert
        await FluentActions.Invoking(() => pipeline.ExecuteAsync(testRequest, _serviceProvider, CancellationToken.None))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Async pipeline error");
    }

    [Fact]
    public async Task CompiledPipeline_ExecuteAsync_WithCancellation_PropagatesCancellation()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        var pipeline = new CompiledPipeline<string>
        {
            RequestType = typeof(TestCommand),
            ResponseType = typeof(string),
            HandlerType = typeof(object),
            Executor = async (request, serviceProvider, cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(1000, cancellationToken);
                return "never-reached";
            }
        };

        var testRequest = new TestCommand("test-value");
        cancellationTokenSource.Cancel();

        // Act & Assert
        await FluentActions.Invoking(() => pipeline.ExecuteAsync(testRequest, _serviceProvider, cancellationTokenSource.Token))
            .Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region CompiledVoidPipeline Tests

    [Fact]
    public void CompiledVoidPipeline_WithValidProperties_InitializesCorrectly()
    {
        // Arrange & Act
        var pipeline = new CompiledVoidPipeline
        {
            RequestType = typeof(TestVoidCommand),
            HandlerType = typeof(object),
            Executor = (request, serviceProvider, cancellationToken) => Task.CompletedTask
        };

        // Assert
        pipeline.RequestType.Should().Be(typeof(TestVoidCommand));
        pipeline.ResponseType.Should().Be(typeof(void));
        pipeline.HandlerType.Should().Be(typeof(object));
        pipeline.Executor.Should().NotBeNull();
    }

    [Fact]
    public void CompiledVoidPipeline_HandlerName_ReturnsHandlerTypeName()
    {
        // Arrange
        var pipeline = new CompiledVoidPipeline
        {
            HandlerType = typeof(TestVoidCommand)
        };

        // Act & Assert
        pipeline.HandlerName.Should().Be("TestVoidCommand");
    }

    [Fact]
    public void CompiledVoidPipeline_RequestName_ReturnsRequestTypeName()
    {
        // Arrange
        var pipeline = new CompiledVoidPipeline
        {
            RequestType = typeof(TestVoidCommand)
        };

        // Act & Assert
        pipeline.RequestName.Should().Be("TestVoidCommand");
    }

    [Fact]
    public void CompiledVoidPipeline_IsCommand_AlwaysReturnsTrue()
    {
        // Arrange
        var pipeline = new CompiledVoidPipeline
        {
            RequestType = typeof(TestVoidCommand)
        };

        // Act & Assert
        pipeline.IsCommand.Should().BeTrue();
    }

    [Fact]
    public void CompiledVoidPipeline_ResponseType_IsVoid()
    {
        // Arrange
        var pipeline = new CompiledVoidPipeline();

        // Act & Assert
        pipeline.ResponseType.Should().Be(typeof(void));
    }

    [Fact]
    public async Task CompiledVoidPipeline_ExecuteAsync_CallsExecutor()
    {
        // Arrange
        bool executorCalled = false;
        object? capturedRequest = null;
        IServiceProvider? capturedServiceProvider = null;
        CancellationToken capturedToken = default;

        var pipeline = new CompiledVoidPipeline
        {
            RequestType = typeof(TestVoidCommand),
            HandlerType = typeof(object),
            Executor = (request, serviceProvider, cancellationToken) =>
            {
                executorCalled = true;
                capturedRequest = request;
                capturedServiceProvider = serviceProvider;
                capturedToken = cancellationToken;
                return Task.CompletedTask;
            }
        };

        var testRequest = new TestVoidCommand("test-value");
        var cancellationToken = new CancellationToken();

        // Act
        await pipeline.ExecuteAsync(testRequest, _serviceProvider, cancellationToken);

        // Assert
        executorCalled.Should().BeTrue();
        capturedRequest.Should().Be(testRequest);
        capturedServiceProvider.Should().Be(_serviceProvider);
        capturedToken.Should().Be(cancellationToken);
    }

    [Fact]
    public async Task CompiledVoidPipeline_ExecuteAsync_WithException_PropagatesException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Void pipeline execution failed");
        var pipeline = new CompiledVoidPipeline
        {
            RequestType = typeof(TestVoidCommand),
            HandlerType = typeof(object),
            Executor = (request, serviceProvider, cancellationToken) => throw expectedException
        };

        var testRequest = new TestVoidCommand("test-value");

        // Act & Assert
        await FluentActions.Invoking(() => pipeline.ExecuteAsync(testRequest, _serviceProvider, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Void pipeline execution failed");
    }

    [Fact]
    public async Task CompiledVoidPipeline_ExecuteAsync_WithAsyncException_PropagatesException()
    {
        // Arrange
        var expectedException = new ArgumentException("Async void pipeline error");
        var pipeline = new CompiledVoidPipeline
        {
            RequestType = typeof(TestVoidCommand),
            HandlerType = typeof(object),
            Executor = async (request, serviceProvider, cancellationToken) =>
            {
                await Task.Delay(1, cancellationToken);
                throw expectedException;
            }
        };

        var testRequest = new TestVoidCommand("test-value");

        // Act & Assert
        await FluentActions.Invoking(() => pipeline.ExecuteAsync(testRequest, _serviceProvider, CancellationToken.None))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Async void pipeline error");
    }

    [Fact]
    public async Task CompiledVoidPipeline_ExecuteAsync_WithCancellation_PropagatesCancellation()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        var pipeline = new CompiledVoidPipeline
        {
            RequestType = typeof(TestVoidCommand),
            HandlerType = typeof(object),
            Executor = async (request, serviceProvider, cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(1000, cancellationToken);
            }
        };

        var testRequest = new TestVoidCommand("test-value");
        cancellationTokenSource.Cancel();

        // Act & Assert
        await FluentActions.Invoking(() => pipeline.ExecuteAsync(testRequest, _serviceProvider, cancellationTokenSource.Token))
            .Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region Interface Compliance Tests

    [Fact]
    public void CompiledPipeline_ImplementsICompiledPipeline()
    {
        // Arrange & Act
        var pipeline = new CompiledPipeline<string>();

        // Assert
        pipeline.Should().BeAssignableTo<ICompiledPipeline>();
        pipeline.Should().BeAssignableTo<ICompiledPipeline<string>>();
    }

    [Fact]
    public void CompiledVoidPipeline_ImplementsICompiledVoidPipeline()
    {
        // Arrange & Act
        var pipeline = new CompiledVoidPipeline();

        // Assert
        pipeline.Should().BeAssignableTo<ICompiledPipeline>();
        pipeline.Should().BeAssignableTo<ICompiledVoidPipeline>();
    }

    [Fact]
    public void CompiledPipeline_AsInterface_ExposesCorrectMembers()
    {
        // Arrange
        var pipeline = new CompiledPipeline<string>
        {
            RequestType = typeof(TestCommand),
            ResponseType = typeof(string),
            HandlerType = typeof(object)
        };

        // Act
        ICompiledPipeline basePipeline = pipeline;

        // Assert
        basePipeline.RequestType.Should().Be(typeof(TestCommand));
        basePipeline.ResponseType.Should().Be(typeof(string));
        basePipeline.HandlerType.Should().Be(typeof(object));
        basePipeline.IsCommand.Should().BeTrue();
    }

    [Fact]
    public void CompiledVoidPipeline_AsInterface_ExposesCorrectMembers()
    {
        // Arrange
        var pipeline = new CompiledVoidPipeline
        {
            RequestType = typeof(TestVoidCommand),
            HandlerType = typeof(object)
        };

        // Act
        ICompiledPipeline basePipeline = pipeline;

        // Assert
        basePipeline.RequestType.Should().Be(typeof(TestVoidCommand));
        basePipeline.ResponseType.Should().Be(typeof(void));
        basePipeline.HandlerType.Should().Be(typeof(object));
        basePipeline.IsCommand.Should().BeTrue();
    }

    #endregion

    #region Edge Cases and Property Tests

    [Fact]
    public void CompiledPipeline_WithNullTypes_HandlesGracefully()
    {
        // Arrange & Act
        var pipeline = new CompiledPipeline<string>
        {
            RequestType = null!,
            ResponseType = null!,
            HandlerType = null!
        };

        // Assert - Properties should be accessible even if null
        pipeline.RequestType.Should().BeNull();
        pipeline.ResponseType.Should().BeNull();
        pipeline.HandlerType.Should().BeNull();
    }

    [Fact]
    public void CompiledVoidPipeline_WithNullTypes_HandlesGracefully()
    {
        // Arrange & Act
        var pipeline = new CompiledVoidPipeline
        {
            RequestType = null!,
            HandlerType = null!
        };

        // Assert
        pipeline.RequestType.Should().BeNull();
        pipeline.HandlerType.Should().BeNull();
        pipeline.ResponseType.Should().Be(typeof(void)); // This is always void
    }

    [Fact]
    public void CompiledPipeline_HandlerName_WithNullHandlerType_ThrowsException()
    {
        // Arrange
        var pipeline = new CompiledPipeline<string>
        {
            HandlerType = null!
        };

        // Act & Assert
        FluentActions.Invoking(() => pipeline.HandlerName)
            .Should().Throw<NullReferenceException>();
    }

    [Fact]
    public void CompiledPipeline_RequestName_WithNullRequestType_ThrowsException()
    {
        // Arrange
        var pipeline = new CompiledPipeline<string>
        {
            RequestType = null!
        };

        // Act & Assert
        FluentActions.Invoking(() => pipeline.RequestName)
            .Should().Throw<NullReferenceException>();
    }

    [Fact]
    public void CompiledPipeline_IsCommand_WithNullRequestType_ThrowsException()
    {
        // Arrange
        var pipeline = new CompiledPipeline<string>
        {
            RequestType = null!
        };

        // Act & Assert
        FluentActions.Invoking(() => pipeline.IsCommand)
            .Should().Throw<NullReferenceException>();
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task CompiledPipeline_MultipleExecutions_PerformsConsistently()
    {
        // Arrange
        var pipeline = new CompiledPipeline<string>
        {
            RequestType = typeof(TestCommand),
            ResponseType = typeof(string),
            HandlerType = typeof(object),
            Executor = (request, serviceProvider, cancellationToken) => Task.FromResult($"result-{((TestCommand)request).Value}")
        };

        var requests = Enumerable.Range(1, 10)
            .Select(i => new TestCommand($"test-{i}"))
            .ToArray();

        // Act
        var tasks = requests.Select(req => pipeline.ExecuteAsync(req, _serviceProvider, CancellationToken.None));
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(10);
        for (int i = 0; i < 10; i++)
        {
            results[i].Should().Be($"result-test-{i + 1}");
        }
    }

    [Fact]
    public async Task CompiledVoidPipeline_MultipleExecutions_PerformsConsistently()
    {
        // Arrange
        var executionCount = 0;
        var pipeline = new CompiledVoidPipeline
        {
            RequestType = typeof(TestVoidCommand),
            HandlerType = typeof(object),
            Executor = (request, serviceProvider, cancellationToken) =>
            {
                Interlocked.Increment(ref executionCount);
                return Task.CompletedTask;
            }
        };

        var requests = Enumerable.Range(1, 10)
            .Select(i => new TestVoidCommand($"test-{i}"))
            .ToArray();

        // Act
        var tasks = requests.Select(req => pipeline.ExecuteAsync(req, _serviceProvider, CancellationToken.None));
        await Task.WhenAll(tasks);

        // Assert
        executionCount.Should().Be(10);
    }

    #endregion

    #region Complex Type Tests

    public record ComplexCommand(int Id, string Name, List<string> Tags) : ICommand<ComplexResponse>;
    public record ComplexResponse(bool Success, Dictionary<string, object> Data);

    [Fact]
    public async Task CompiledPipeline_WithComplexTypes_HandlesCorrectly()
    {
        // Arrange
        var expectedResponse = new ComplexResponse(true, new Dictionary<string, object> { { "key", "value" } });
        var pipeline = new CompiledPipeline<ComplexResponse>
        {
            RequestType = typeof(ComplexCommand),
            ResponseType = typeof(ComplexResponse),
            HandlerType = typeof(object),
            Executor = (request, serviceProvider, cancellationToken) =>
            {
                var cmd = (ComplexCommand)request;
                var response = new ComplexResponse(true, new Dictionary<string, object>
                {
                    { "id", cmd.Id },
                    { "name", cmd.Name },
                    { "tagCount", cmd.Tags.Count }
                });
                return Task.FromResult(response);
            }
        };

        var testRequest = new ComplexCommand(123, "Test", new List<string> { "tag1", "tag2" });

        // Act
        ComplexResponse result = await pipeline.ExecuteAsync(testRequest, _serviceProvider, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data["id"].Should().Be(123);
        result.Data["name"].Should().Be("Test");
        result.Data["tagCount"].Should().Be(2);
    }

    #endregion
}