using Crisp.Common;
using Crisp.Exceptions;
using Crisp.Pipeline;
using Microsoft.Extensions.Logging;
using NSubstitute.ExceptionExtensions;

namespace Crisp.Runtime.Tests.Pipeline;

public class ErrorHandlingBehaviorTests
{
    private readonly ILogger<ErrorHandlingBehavior<TestRequest, string>> _logger;
    private readonly CrispOptions _options;
    private readonly ErrorHandlingBehavior<TestRequest, string> _behavior;

    public ErrorHandlingBehaviorTests()
    {
        _logger = Substitute.For<ILogger<ErrorHandlingBehavior<TestRequest, string>>>();
        _options = new CrispOptions();
        _behavior = new ErrorHandlingBehavior<TestRequest, string>(_logger);
    }

    [Fact]
    public async Task Handle_SuccessfulExecution_ReturnsResult()
    {
        // Arrange
        TestRequest request = new();
        string expectedResponse = "success";
        RequestHandlerDelegate<string> next = Substitute.For<RequestHandlerDelegate<string>>();
        next().Returns(expectedResponse);

        // Act
        string result = await _behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        await next.Received(1)();
    }

    [Fact]
    public async Task Handle_ValidationException_ReturnsFailureResult()
    {
        // Arrange
        TestRequest request = new();
        ValidationException exception = new("Validation failed");
        RequestHandlerDelegate<string> next = Substitute.For<RequestHandlerDelegate<string>>();
        next().ThrowsAsync(exception);

        // Act
        await FluentActions.Invoking(() => _behavior.Handle(request, next, CancellationToken.None))
                  .Should().ThrowAsync<ValidationException>()
                  .WithMessage("Validation failed");
    }

    [Fact]
    public async Task Handle_NotFoundException_ReturnsNotFoundResult()
    {
        // Arrange
        TestRequest request = new();
        NotFoundException exception = new("Resource not found");
        RequestHandlerDelegate<string> next = Substitute.For<RequestHandlerDelegate<string>>();
        next().ThrowsAsync(exception);

        // Act
        await FluentActions.Invoking(() => _behavior.Handle(request, next, CancellationToken.None))
                  .Should().ThrowAsync<NotFoundException>()
                  .WithMessage("Resource not found");
    }

    [Fact]
    public async Task Handle_UnauthorizedException_ReturnsUnauthorizedResult()
    {
        // Arrange
        TestRequest request = new();
        UnauthorizedException exception = new("Access denied");
        RequestHandlerDelegate<string> next = Substitute.For<RequestHandlerDelegate<string>>();
        next().ThrowsAsync(exception);

        // Act
        await FluentActions.Invoking(() => _behavior.Handle(request, next, CancellationToken.None))
                  .Should().ThrowAsync<UnauthorizedException>()
                  .WithMessage("Access denied");
    }

    [Fact]
    public async Task Handle_OperationCanceledException_ReturnsCancelledResult()
    {
        // Arrange
        TestRequest request = new();
        OperationCanceledException exception = new("Operation was cancelled");
        RequestHandlerDelegate<string> next = Substitute.For<RequestHandlerDelegate<string>>();
        next().ThrowsAsync(exception);

        // Act
        await FluentActions.Invoking(() => _behavior.Handle(request, next, CancellationToken.None))
                  .Should().ThrowAsync<OperationCanceledException>()
                  .WithMessage("Operation was cancelled");
    }

    [Fact]
    public async Task Handle_TimeoutException_ReturnsTimeoutResult()
    {
        // Arrange
        TestRequest request = new();
        TimeoutException exception = new("Operation timed out");
        RequestHandlerDelegate<string> next = Substitute.For<RequestHandlerDelegate<string>>();
        next().ThrowsAsync(exception);

        // Act
        await FluentActions.Invoking(() => _behavior.Handle(request, next, CancellationToken.None))
                   .Should().ThrowAsync<TimeoutException>()
                   .WithMessage("Operation timed out");
    }

    [Fact]
    public async Task Handle_ArgumentException_ReturnsInvalidArgumentResult()
    {
        // Arrange
        TestRequest request = new();
        ArgumentException exception = new("Invalid argument provided");
        RequestHandlerDelegate<string> next = Substitute.For<RequestHandlerDelegate<string>>();
        next().ThrowsAsync(exception);

        // Act
        await FluentActions.Invoking(() => _behavior.Handle(request, next, CancellationToken.None))
                   .Should().ThrowAsync<ArgumentException>()
                   .WithMessage("Invalid argument provided");
    }

    [Fact]
    public async Task Handle_UnknownException_WithExposeInternalErrors_ReturnsDetailedError()
    {
        // Arrange
        TestRequest request = new();
        InvalidOperationException exception = new("Internal error details");
        RequestHandlerDelegate<string> next = Substitute.For<RequestHandlerDelegate<string>>();
        next().ThrowsAsync(exception);

        // Act
        await FluentActions.Invoking(() => _behavior.Handle(request, next, CancellationToken.None))
           .Should().ThrowAsync<InvalidOperationException>()
           .WithMessage("Internal error details");
    }

    [Fact]
    public async Task Handle_UnknownException_WithoutExposeInternalErrors_ReturnsGenericError()
    {
        // Arrange
        TestRequest request = new();
        InvalidOperationException exception = new("Internal error details");
        RequestHandlerDelegate<string> next = Substitute.For<RequestHandlerDelegate<string>>();
        next().ThrowsAsync(exception);

        // Act
        await FluentActions.Invoking(() => _behavior.Handle(request, next, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Internal error details");
    }

    [Fact]
    public async Task Handle_NonResultResponse_ThrowsForUnsupportedResponseType()
    {
        // Arrange
        ErrorHandlingBehavior<TestRequest, string> behavior = new(_logger);
        TestRequest request = new();
        ValidationException exception = new("Validation failed");
        RequestHandlerDelegate<string> next = Substitute.For<RequestHandlerDelegate<string>>();
        next().ThrowsAsync(exception);

        // Act & Assert
        await FluentActions.Invoking(() => behavior.Handle(request, next, CancellationToken.None))
            .Should().ThrowAsync<ValidationException>()
            .WithMessage("Validation failed");
    }

    public record TestRequest : IRequest<string>;
}