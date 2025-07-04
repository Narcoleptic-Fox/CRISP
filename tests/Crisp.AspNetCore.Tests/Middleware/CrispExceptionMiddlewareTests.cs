using Crisp.Middleware;
using Crisp.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace Crisp.AspNetCore.Tests.Middleware;

public class CrispExceptionMiddlewareTests : TestBase
{
    [Fact]
    public async Task InvokeAsync_NoException_CallsNext()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var logger = Substitute.For<ILogger<CrispExceptionMiddleware>>();
        var nextCalled = false;
        
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new CrispExceptionMiddleware(next, logger, new CrispOptions(), Substitute.For<IHostEnvironment>());

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_ValidationException_Returns400()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        var logger = Substitute.For<ILogger<CrispExceptionMiddleware>>();
        
        RequestDelegate next = (ctx) => throw new ValidationException("Validation failed");

        var middleware = new CrispExceptionMiddleware(next, logger, new CrispOptions(), Substitute.For<IHostEnvironment>());

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        context.Response.ContentType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task InvokeAsync_NotFoundException_Returns404()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        var logger = Substitute.For<ILogger<CrispExceptionMiddleware>>();
        
        RequestDelegate next = (ctx) => throw new NotFoundException("Resource not found");

        var middleware = new CrispExceptionMiddleware(next, logger, new CrispOptions(), Substitute.For<IHostEnvironment>());

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        context.Response.ContentType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task InvokeAsync_UnauthorizedException_Returns401()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        var logger = Substitute.For<ILogger<CrispExceptionMiddleware>>();
        
        RequestDelegate next = (ctx) => throw new UnauthorizedException("Access denied");

        var middleware = new CrispExceptionMiddleware(next, logger, new CrispOptions(), Substitute.For<IHostEnvironment>());

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
        context.Response.ContentType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task InvokeAsync_GenericException_Returns500()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        var logger = Substitute.For<ILogger<CrispExceptionMiddleware>>();
        
        RequestDelegate next = (ctx) => throw new InvalidOperationException("Something went wrong");

        var middleware = new CrispExceptionMiddleware(next, logger, new CrispOptions(), Substitute.For<IHostEnvironment>());

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        context.Response.ContentType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task InvokeAsync_ProducesCorrectProblemDetails()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var responseStream = new MemoryStream();
        context.Response.Body = responseStream;
        
        var logger = Substitute.For<ILogger<CrispExceptionMiddleware>>();
        
        RequestDelegate next = (ctx) => throw new ValidationException("Invalid input data");

        var middleware = new CrispExceptionMiddleware(next, logger, new CrispOptions(), Substitute.For<IHostEnvironment>());

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        
        responseStream.Position = 0;
        var responseContent = await new StreamReader(responseStream).ReadToEndAsync();
        
        var problemDetails = JsonSerializer.Deserialize<JsonElement>(responseContent);
        problemDetails.GetProperty("title").GetString().Should().Be("Validation Error");
        problemDetails.GetProperty("detail").GetString().Should().Be("Invalid input data");
        problemDetails.GetProperty("status").GetInt32().Should().Be(400);
    }

    [Fact]
    public async Task InvokeAsync_WithNullContext_ShouldThrow()
    {
        // Arrange
        var logger = Substitute.For<ILogger<CrispExceptionMiddleware>>();
        RequestDelegate next = (ctx) => Task.CompletedTask;
        var middleware = new CrispExceptionMiddleware(next, logger, new CrispOptions(), Substitute.For<IHostEnvironment>());

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => middleware.InvokeAsync(null!));
    }

    [Fact]
    public async Task InvokeAsync_LogsExceptions()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        var logger = Substitute.For<ILogger<CrispExceptionMiddleware>>();
        
        var testException = new InvalidOperationException("Test exception for logging");
        RequestDelegate next = (ctx) => throw testException;

        var middleware = new CrispExceptionMiddleware(next, logger, new CrispOptions(), Substitute.For<IHostEnvironment>());

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Unhandled exception occurred")),
            testException,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task InvokeAsync_ForbiddenException_Returns403()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        var logger = Substitute.For<ILogger<CrispExceptionMiddleware>>();
        
        RequestDelegate next = (ctx) => throw new ForbiddenException("Access forbidden");

        var middleware = new CrispExceptionMiddleware(next, logger, new CrispOptions(), Substitute.For<IHostEnvironment>());

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.Forbidden);
        context.Response.ContentType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task InvokeAsync_ConflictException_Returns409()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        var logger = Substitute.For<ILogger<CrispExceptionMiddleware>>();
        
        RequestDelegate next = (ctx) => throw new ConflictException("Resource conflict");

        var middleware = new CrispExceptionMiddleware(next, logger, new CrispOptions(), Substitute.For<IHostEnvironment>());

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.Conflict);
        context.Response.ContentType.Should().Be("application/problem+json");
    }
}