using CRISP.Core.Interfaces;
using CRISP.Core.Options;
using CRISP.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace CRISP.Core.Tests.Services;

public class MediatorTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<ILogger<Mediator>> _loggerMock;
    private readonly MediatorOptions _options;

    public MediatorTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _loggerMock = new Mock<ILogger<Mediator>>();
        _options = new MediatorOptions
        {
            DefaultTimeoutSeconds = 30,
            AllowMultipleHandlers = false,
            EnableDetailedLogging = true,
            TrackRequestMetrics = true
        };
    }

    [Fact]
    public void Constructor_WithNullServiceProvider_ThrowsArgumentNullException()
    {
        // Arrange & Act
        Should.Throw<ArgumentNullException>(() => new Mediator(null!, _loggerMock.Object, _options))
            .ParamName.ShouldBe("serviceProvider");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act
        Should.Throw<ArgumentNullException>(() => new Mediator(_serviceProviderMock.Object, null!, _options))
            .ParamName.ShouldBe("logger");
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Arrange & Act
        Should.Throw<ArgumentNullException>(() => new Mediator(_serviceProviderMock.Object, _loggerMock.Object, null!))
            .ParamName.ShouldBe("options");
    }

    [Fact]
    public async Task Send_WithRequest_InvokesHandlerAndReturnsResponse()
    {
        // Arrange
        TestRequest request = new();
        string expectedResponse = "Test Response";

        Mock<IRequestHandler<TestRequest, string>> handlerMock = new();
        handlerMock.Setup(h => h.Handle(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        ServiceCollection serviceCollection = new();
        serviceCollection.AddSingleton(handlerMock.Object);
        ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

        _serviceProviderMock.Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<IRequest<string>, string>>)))
            .Returns(Array.Empty<IPipelineBehavior<IRequest<string>, string>>());

        _serviceProviderMock.Setup(sp => sp.GetService(typeof(IEnumerable<IRequestHandler<TestRequest, string>>)))
            .Returns(new[] { handlerMock.Object });

        Mediator mediator = new(_serviceProviderMock.Object, _loggerMock.Object, _options);

        // Act
        string response = await mediator.Send(request);

        // Assert
        response.ShouldBe(expectedResponse);
        handlerMock.Verify(h => h.Handle(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Send_WithRequestWithoutResponse_InvokesHandler()
    {
        // Arrange
        TestRequestWithoutResponse request = new();

        Mock<IRequestHandler<TestRequestWithoutResponse>> handlerMock = new();
        handlerMock.Setup(h => h.Handle(request, It.IsAny<CancellationToken>()))
            .Returns(new ValueTask());

        ServiceCollection serviceCollection = new();
        serviceCollection.AddSingleton(handlerMock.Object);
        ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

        // No other changes are needed as the BuildServiceProvider method is an extension method
        // provided by the Microsoft.Extensions.DependencyInjection namespace.
        _serviceProviderMock.Setup(sp => sp.GetService(typeof(IEnumerable<IRequestHandler<TestRequestWithoutResponse>>)))
            .Returns(new[] { handlerMock.Object });

        Mediator mediator = new(_serviceProviderMock.Object, _loggerMock.Object, _options);

        // Act
        await mediator.Send(request);

        // Assert
        handlerMock.Verify(h => h.Handle(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Send_WithNoHandlerRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        TestRequest request = new();

        _serviceProviderMock.Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<IRequest<string>, string>>)))
            .Returns(Array.Empty<IPipelineBehavior<IRequest<string>, string>>());

        _serviceProviderMock.Setup(sp => sp.GetService(typeof(IEnumerable<IRequestHandler<TestRequest, string>>)))
            .Returns(Array.Empty<IRequestHandler<TestRequest, string>>());

        Mediator mediator = new(_serviceProviderMock.Object, _loggerMock.Object, _options);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(async () => 
            await mediator.Send(request));
            
        exception.Message.ShouldBe($"No handler registered for {request.GetType().Name}");
    }

    [Fact]
    public async Task Send_WithMultipleHandlers_AndAllowMultipleHandlersFalse_ThrowsInvalidOperationException()
    {
        // Arrange
        TestRequest request = new();
        IRequestHandler<TestRequest, string>[] handlers = new[]
        {
            Mock.Of<IRequestHandler<TestRequest, string>>(),
            Mock.Of<IRequestHandler<TestRequest, string>>()
        };

        _serviceProviderMock.Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<IRequest<string>, string>>)))
            .Returns(Array.Empty<IPipelineBehavior<IRequest<string>, string>>());

        _serviceProviderMock.Setup(sp => sp.GetService(typeof(IEnumerable<IRequestHandler<TestRequest, string>>)))
            .Returns(handlers);

        Mediator mediator = new(_serviceProviderMock.Object, _loggerMock.Object, _options);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(async () => 
            await mediator.Send(request));
            
        exception.Message.ShouldBe($"Multiple handlers registered for {request.GetType().Name}. Consider setting AllowMultipleHandlers to true if this is intended.");
    }

    [Fact]
    public async Task Send_WithMultipleHandlers_AndAllowMultipleHandlersTrue_UsesFirstHandler()
    {
        // Arrange
        TestRequest request = new();
        string expectedResponse = "Test Response";

        Mock<IRequestHandler<TestRequest, string>> handler1Mock = new();
        handler1Mock.Setup(h => h.Handle(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        Mock<IRequestHandler<TestRequest, string>> handler2Mock = new();
        handler2Mock.Setup(h => h.Handle(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Another Response");

        IRequestHandler<TestRequest, string>[] handlers = new[]
        {
            handler1Mock.Object,
            handler2Mock.Object
        };

        _serviceProviderMock.Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<IRequest<string>, string>>)))
            .Returns(Array.Empty<IPipelineBehavior<IRequest<string>, string>>());

        _serviceProviderMock.Setup(sp => sp.GetService(typeof(IEnumerable<IRequestHandler<TestRequest, string>>)))
            .Returns(handlers);

        MediatorOptions options = new()
        {
            DefaultTimeoutSeconds = 30,
            AllowMultipleHandlers = true,
            EnableDetailedLogging = true,
            TrackRequestMetrics = true
        };

        Mediator mediator = new(_serviceProviderMock.Object, _loggerMock.Object, options);

        // Act
        string response = await mediator.Send(request);

        // Assert
        response.ShouldBe(expectedResponse);
        handler1Mock.Verify(h => h.Handle(request, It.IsAny<CancellationToken>()), Times.Once);
        handler2Mock.Verify(h => h.Handle(request, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Send_WithTimeout_ThrowsTimeoutException()
    {
        // Arrange
        TestRequest request = new();

        Mock<IRequestHandler<TestRequest, string>> handlerMock = new();
        handlerMock.Setup(h => h.Handle(request, It.IsAny<CancellationToken>()))
            .Returns(async (TestRequest _, CancellationToken ct) =>
            {
                await Task.Delay(2000, ct); // Delayed response
                return "Test Response";
            });

        _serviceProviderMock.Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<IRequest<string>, string>>)))
            .Returns(Array.Empty<IPipelineBehavior<IRequest<string>, string>>());

        _serviceProviderMock.Setup(sp => sp.GetService(typeof(IEnumerable<IRequestHandler<TestRequest, string>>)))
            .Returns(new[] { handlerMock.Object });

        // Configure a short timeout
        MediatorOptions options = new()
        {
            DefaultTimeoutSeconds = 1, // 1 second timeout
            AllowMultipleHandlers = false,
            EnableDetailedLogging = true,
            TrackRequestMetrics = true
        };

        Mediator mediator = new(_serviceProviderMock.Object, _loggerMock.Object, options);

        // Act & Assert
        var exception = await Should.ThrowAsync<TimeoutException>(async () => 
            await mediator.Send(request));
            
        exception.Message.ShouldBe($"Request {request.GetType().Name} timed out after {options.DefaultTimeoutSeconds} seconds");
    }

    public class TestRequest : IRequest<string> { }

    public class TestRequestWithoutResponse : IRequest { }
}