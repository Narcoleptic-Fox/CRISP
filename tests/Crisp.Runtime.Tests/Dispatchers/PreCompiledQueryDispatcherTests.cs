using Crisp.Dispatchers;
using Crisp.Pipeline;
using Crisp.Queries;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute.ExceptionExtensions;

namespace Crisp.Runtime.Tests.Dispatchers;

public class PreCompiledQueryDispatcherTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<Type, ICompiledPipeline> _pipelines;
    private readonly PreCompiledQueryDispatcher _dispatcher;

    public PreCompiledQueryDispatcherTests()
    {
        _serviceProvider = Substitute.For<IServiceProvider>();
        _pipelines = new Dictionary<Type, ICompiledPipeline>();
        _dispatcher = new PreCompiledQueryDispatcher(_pipelines, _serviceProvider);
    }

    #region Test Queries and Responses

    public record TestQuery(string Filter) : IQuery<string>;
    public record TestComplexQuery(int Id, string Name) : IQuery<TestResult>;
    public record TestResult(string Value, int Count);

    #endregion

    #region Send Method Tests

    [Fact]
    public async Task Send_WithValidQuery_ReturnsCorrectResponse()
    {
        // Arrange
        const string expectedResult = "query-result";
        var query = new TestQuery("test-filter");
        
        var mockPipeline = Substitute.For<ICompiledPipeline<string>>();
        mockPipeline.ExecuteAsync(query, _serviceProvider, Arg.Any<CancellationToken>())
            .Returns(expectedResult);
        
        _pipelines[typeof(TestQuery)] = mockPipeline;

        // Act
        string result = await _dispatcher.Send<string>(query);

        // Assert
        result.Should().Be(expectedResult);
        await mockPipeline.Received(1).ExecuteAsync(query, _serviceProvider, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Send_WithComplexQuery_ReturnsComplexResponse()
    {
        // Arrange
        var expectedResult = new TestResult("complex-value", 42);
        var query = new TestComplexQuery(123, "test-name");
        
        var mockPipeline = Substitute.For<ICompiledPipeline<TestResult>>();
        mockPipeline.ExecuteAsync(query, _serviceProvider, Arg.Any<CancellationToken>())
            .Returns(expectedResult);
        
        _pipelines[typeof(TestComplexQuery)] = mockPipeline;

        // Act
        TestResult result = await _dispatcher.Send<TestResult>(query);

        // Assert
        result.Should().Be(expectedResult);
        result.Value.Should().Be("complex-value");
        result.Count.Should().Be(42);
        await mockPipeline.Received(1).ExecuteAsync(query, _serviceProvider, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Send_WithCancellationToken_PassesTokenToPipeline()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var query = new TestQuery("test-filter");
        
        var mockPipeline = Substitute.For<ICompiledPipeline<string>>();
        mockPipeline.ExecuteAsync(query, _serviceProvider, cancellationToken)
            .Returns("result");
        
        _pipelines[typeof(TestQuery)] = mockPipeline;

        // Act
        await _dispatcher.Send<string>(query, cancellationToken);

        // Assert
        await mockPipeline.Received(1).ExecuteAsync(query, _serviceProvider, cancellationToken);
    }

    [Fact]
    public async Task Send_WithNullQuery_ThrowsArgumentNullException()
    {
        // Act & Assert
        await FluentActions.Invoking(() => _dispatcher.Send<string>(null!))
            .Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("query");
    }

    [Fact]
    public async Task Send_WithUnregisteredQuery_ThrowsInvalidOperationException()
    {
        // Arrange
        var query = new TestQuery("test-filter");
        
        // Act & Assert
        await FluentActions.Invoking(() => _dispatcher.Send<string>(query))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No handler registered for query 'TestQuery'. Make sure the handler is registered and the assembly is scanned during startup.");
    }

    [Fact]
    public async Task Send_WithMismatchedResponseType_ThrowsInvalidOperationException()
    {
        // Arrange
        var query = new TestQuery("test-filter");
        var mockPipeline = Substitute.For<ICompiledPipeline<int>>(); // Wrong type
        _pipelines[typeof(TestQuery)] = mockPipeline;

        // Act & Assert
        await FluentActions.Invoking(() => _dispatcher.Send<string>(query))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Query 'TestQuery' pipeline type mismatch. Expected response type 'String'.");
    }

    [Fact]
    public async Task Send_PipelineThrowsException_PropagatesException()
    {
        // Arrange
        var query = new TestQuery("test-filter");
        var mockPipeline = Substitute.For<ICompiledPipeline<string>>();
        var expectedException = new InvalidOperationException("Query pipeline error");
        
        mockPipeline.ExecuteAsync(query, _serviceProvider, Arg.Any<CancellationToken>())
            .ThrowsAsync(expectedException);
        
        _pipelines[typeof(TestQuery)] = mockPipeline;

        // Act & Assert
        await FluentActions.Invoking(() => _dispatcher.Send<string>(query))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Query pipeline error");
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_InitializesCorrectly()
    {
        // Arrange
        var pipelines = new Dictionary<Type, ICompiledPipeline>();
        var serviceProvider = Substitute.For<IServiceProvider>();

        // Act
        var dispatcher = new PreCompiledQueryDispatcher(pipelines, serviceProvider);

        // Assert
        dispatcher.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullPipelines_ThrowsArgumentNullException()
    {
        // Arrange
        var serviceProvider = Substitute.For<IServiceProvider>();

        // Act & Assert
        FluentActions.Invoking(() => new PreCompiledQueryDispatcher(null!, serviceProvider))
            .Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullServiceProvider_ThrowsArgumentNullException()
    {
        // Arrange
        var pipelines = new Dictionary<Type, ICompiledPipeline>();

        // Act & Assert
        FluentActions.Invoking(() => new PreCompiledQueryDispatcher(pipelines, null!))
            .Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Concurrent Access Tests

    [Fact]
    public async Task Send_ConcurrentQueries_HandlesCorrectly()
    {
        // Arrange
        var query1 = new TestQuery("filter1");
        var query2 = new TestQuery("filter2");
        
        var mockPipeline = Substitute.For<ICompiledPipeline<string>>();
        mockPipeline.ExecuteAsync(Arg.Any<TestQuery>(), _serviceProvider, Arg.Any<CancellationToken>())
            .Returns(callInfo => $"result-{((TestQuery)callInfo[0]).Filter}");
        
        _pipelines[typeof(TestQuery)] = mockPipeline;

        // Act
        var task1 = _dispatcher.Send<string>(query1);
        var task2 = _dispatcher.Send<string>(query2);
        
        var results = await Task.WhenAll(task1, task2);

        // Assert
        results.Should().Contain("result-filter1");
        results.Should().Contain("result-filter2");
        await mockPipeline.Received(2).ExecuteAsync(Arg.Any<TestQuery>(), _serviceProvider, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Send_MultipleDifferentQueryTypes_HandlesCorrectly()
    {
        // Arrange
        var stringQuery = new TestQuery("string-filter");
        var complexQuery = new TestComplexQuery(456, "complex-name");
        
        var stringPipeline = Substitute.For<ICompiledPipeline<string>>();
        stringPipeline.ExecuteAsync(stringQuery, _serviceProvider, Arg.Any<CancellationToken>())
            .Returns("string-result");
        
        var complexPipeline = Substitute.For<ICompiledPipeline<TestResult>>();
        complexPipeline.ExecuteAsync(complexQuery, _serviceProvider, Arg.Any<CancellationToken>())
            .Returns(new TestResult("complex-result", 789));
        
        _pipelines[typeof(TestQuery)] = stringPipeline;
        _pipelines[typeof(TestComplexQuery)] = complexPipeline;

        // Act
        var stringTask = _dispatcher.Send<string>(stringQuery);
        var complexTask = _dispatcher.Send<TestResult>(complexQuery);
        
        await Task.WhenAll(stringTask, complexTask);

        // Assert
        stringTask.Result.Should().Be("string-result");
        complexTask.Result.Should().BeEquivalentTo(new TestResult("complex-result", 789));
        
        await stringPipeline.Received(1).ExecuteAsync(stringQuery, _serviceProvider, Arg.Any<CancellationToken>());
        await complexPipeline.Received(1).ExecuteAsync(complexQuery, _serviceProvider, Arg.Any<CancellationToken>());
    }

    #endregion

    #region Integration with Real Pipelines

    [Fact]
    public async Task Send_WithRealCompiledPipeline_ExecutesCorrectly()
    {
        // Arrange
        var query = new TestQuery("integration-filter");
        var realPipeline = new CompiledPipeline<string>
        {
            RequestType = typeof(TestQuery),
            ResponseType = typeof(string),
            HandlerType = typeof(object), // Mock handler type
            Executor = (request, serviceProvider, cancellationToken) => 
                Task.FromResult($"processed-{((TestQuery)request).Filter}")
        };
        
        _pipelines[typeof(TestQuery)] = realPipeline;

        // Act
        string result = await _dispatcher.Send<string>(query);

        // Assert
        result.Should().Be("processed-integration-filter");
    }

    [Fact]
    public async Task Send_WithRealComplexPipeline_ExecutesCorrectly()
    {
        // Arrange
        var query = new TestComplexQuery(999, "integration-name");
        var realPipeline = new CompiledPipeline<TestResult>
        {
            RequestType = typeof(TestComplexQuery),
            ResponseType = typeof(TestResult),
            HandlerType = typeof(object), // Mock handler type
            Executor = (request, serviceProvider, cancellationToken) =>
            {
                var complexQuery = (TestComplexQuery)request;
                return Task.FromResult(new TestResult($"processed-{complexQuery.Name}", complexQuery.Id * 2));
            }
        };
        
        _pipelines[typeof(TestComplexQuery)] = realPipeline;

        // Act
        TestResult result = await _dispatcher.Send<TestResult>(query);

        // Assert
        result.Should().BeEquivalentTo(new TestResult("processed-integration-name", 1998));
    }

    #endregion

    #region Edge Cases and Error Scenarios

    [Fact]
    public async Task Send_WithCanceledToken_ThrowsOperationCanceledException()
    {
        // Arrange
        var query = new TestQuery("test-filter");
        var mockPipeline = Substitute.For<ICompiledPipeline<string>>();
        var canceledToken = new CancellationToken(true);
        
        mockPipeline.ExecuteAsync(query, _serviceProvider, canceledToken)
            .ThrowsAsync(new OperationCanceledException());
        
        _pipelines[typeof(TestQuery)] = mockPipeline;

        // Act & Assert
        await FluentActions.Invoking(() => _dispatcher.Send<string>(query, canceledToken))
            .Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Send_PipelineReturnsNull_ReturnsNull()
    {
        // Arrange
        var query = new TestQuery("null-result");
        var mockPipeline = Substitute.For<ICompiledPipeline<string>>();
        
        mockPipeline.ExecuteAsync(query, _serviceProvider, Arg.Any<CancellationToken>())
            .Returns((string?)null);
        
        _pipelines[typeof(TestQuery)] = mockPipeline;

        // Act
        string? result = await _dispatcher.Send<string>(query);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Send_WithGenericQuery_HandlesTypeParametersCorrectly()
    {
        // Arrange
        var query = new TestQuery("generic-test");
        var mockPipeline = Substitute.For<ICompiledPipeline<string>>();
        
        // Simulate pipeline execution with type verification
        mockPipeline.ExecuteAsync(Arg.Is<TestQuery>(q => q.Filter == "generic-test"), 
                                 _serviceProvider, 
                                 Arg.Any<CancellationToken>())
            .Returns("generic-result");
        
        _pipelines[typeof(TestQuery)] = mockPipeline;

        // Act
        string result = await _dispatcher.Send<string>(query);

        // Assert
        result.Should().Be("generic-result");
        await mockPipeline.Received(1).ExecuteAsync(
            Arg.Is<TestQuery>(q => q.Filter == "generic-test"), 
            _serviceProvider, 
            Arg.Any<CancellationToken>());
    }

    #endregion
}