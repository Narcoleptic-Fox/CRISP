using CRISP.Core.Abstractions;
using CRISP.Core.Interfaces;
using CRISP.Core.Responses;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CRISP.Core.Tests.Abstractions;

public class BaseHandlerTests
{
    #region BaseHandler<TRequest> Tests

    [Fact]
    public async Task BaseHandler_Handle_Success_ReturnsSuccessResponse()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        var handler = new TestCommandHandler(logger.Object);
        var request = new TestCommand();

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.Message.Should().NotBeNull();
        logger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Handling request")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        logger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Successfully handled")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task BaseHandler_Handle_WithException_ReturnsFailureResponse()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        var handler = new TestFailingCommandHandler(logger.Object);
        var request = new TestCommand();

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeFalse();
        response.Message.Should().Contain("Test exception");
        logger.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region BaseHandler<TRequest, TResponse> Tests

    [Fact]
    public async Task BaseHandlerWithResponse_Handle_Success_ReturnsSuccessResponseWithData()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        var handler = new TestQueryHandler(logger.Object);
        var request = new TestQuery();

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.Message.Should().NotBeNull();
        response.Data.Should().NotBeNull();
        response.Data.Message.Should().Be("Test data");
        logger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Handling request")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        logger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Successfully handled")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task BaseHandlerWithResponse_Handle_WithException_ReturnsFailureResponse()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        var handler = new TestFailingQueryHandler(logger.Object);
        var request = new TestQuery();

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeFalse();
        response.Message.Should().Contain("Test exception");
        response.Data.Should().BeNull();
        logger.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Test Classes

    // Command (no response data)
    public class TestCommand : IRequest<Response>
    {
    }

    public class TestCommandHandler : BaseHandler<TestCommand>
    {
        public TestCommandHandler(ILogger logger) : base(logger)
        {
        }

        protected override ValueTask Process(TestCommand request, CancellationToken cancellationToken)
        {
            // Simulate some work
            return ValueTask.CompletedTask;
        }
    }

    public class TestFailingCommandHandler : BaseHandler<TestCommand>
    {
        public TestFailingCommandHandler(ILogger logger) : base(logger)
        {
        }

        protected override ValueTask Process(TestCommand request, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("Test exception");
        }
    }

    // Query (with response data)
    public class TestQueryResult
    {
        public string Message { get; set; } = string.Empty;
    }

    public class TestQuery : IRequest<Response<TestQueryResult>>
    {
    }

    public class TestQueryHandler : BaseHandler<TestQuery, TestQueryResult>
    {
        public TestQueryHandler(ILogger logger) : base(logger)
        {
        }

        protected override ValueTask<TestQueryResult> Process(TestQuery request, CancellationToken cancellationToken)
        {
            // Return some test data
            return ValueTask.FromResult(new TestQueryResult { Message = "Test data" });
        }
    }

    public class TestFailingQueryHandler : BaseHandler<TestQuery, TestQueryResult>
    {
        public TestFailingQueryHandler(ILogger logger) : base(logger)
        {
        }

        protected override ValueTask<TestQueryResult> Process(TestQuery request, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("Test exception");
        }
    }

    #endregion
}
