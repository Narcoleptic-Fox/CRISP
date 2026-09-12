using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using CRISP.Server.Common;
using CRISP.Server.Middleware;

namespace CRISP.Server.Tests.Middleware;

[Trait("Category", "Unit")]
public class GlobalExceptionHandlerTests
{
    private static async Task<JsonElement> Handle(Exception exception, string environmentName)
    {
        var environment = new StubEnvironment { EnvironmentName = environmentName };

        var context = new DefaultHttpContext { Request = { Path = "/api/v1/items" } };
        var body = new MemoryStream();
        context.Response.Body = body;

        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance, environment);
        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);
        handled.ShouldBeTrue();

        body.Position = 0;
        return (await JsonSerializer.DeserializeAsync<JsonElement>(body));
    }

    private static string? Detail(JsonElement problem) =>
        problem.TryGetProperty("detail", out var detail) && detail.ValueKind is not JsonValueKind.Null
            ? detail.GetString()
            : null;

    // The bug this exists to stop: staging returned a bare "Conflict" with no detail, so every
    // message the command services author for the user — which name is taken, which record is
    // missing — was withheld from the only person who could act on it.
    [Theory]
    [InlineData("Staging")]
    [InlineData("Production")]
    public async Task Rejection_ReturnsItsMessageOutsideDevelopment(string environmentName)
    {
        var problem = await Handle(
            RejectedException.Conflict("Item with name 'Widget' already exists."), environmentName);

        problem.GetProperty("status").GetInt32().ShouldBe(StatusCodes.Status409Conflict);
        problem.GetProperty("title").GetString().ShouldBe("Conflict");
        Detail(problem).ShouldBe("Item with name 'Widget' already exists.");
    }

    [Fact]
    public async Task EveryRejection_CarriesItsStatusTitleAndMessage()
    {
        foreach (var (rejection, status, title) in new (RejectedException, int, string)[]
        {
            (RejectedException.BadRequest("the reason"), StatusCodes.Status400BadRequest, "Bad Request"),
            (RejectedException.Forbidden("the reason"), StatusCodes.Status403Forbidden, "Forbidden"),
            (RejectedException.NotFound("the reason"), StatusCodes.Status404NotFound, "Not Found"),
            (RejectedException.Conflict("the reason"), StatusCodes.Status409Conflict, "Conflict"),
        })
        {
            var problem = await Handle(rejection, "Production");

            problem.GetProperty("status").GetInt32().ShouldBe(status);
            problem.GetProperty("title").GetString().ShouldBe(title);
            Detail(problem).ShouldBe("the reason");
        }
    }

    // A framework exception still maps to the status it implies, but EF Core and LINQ throw the
    // same types with text that names entities and keys, so its message is not trusted to be fit
    // for the caller. The type cannot tell a service's rejection from one of those.
    [Theory]
    [InlineData(typeof(ArgumentException), StatusCodes.Status400BadRequest)]
    [InlineData(typeof(UnauthorizedAccessException), StatusCodes.Status403Forbidden)]
    [InlineData(typeof(KeyNotFoundException), StatusCodes.Status404NotFound)]
    [InlineData(typeof(InvalidOperationException), StatusCodes.Status409Conflict)]
    public async Task FrameworkException_KeepsItsStatusButWithholdsItsMessageOutsideDevelopment(
        Type exceptionType, int expectedStatus)
    {
        var exception = (Exception)Activator.CreateInstance(exceptionType, "the reason")!;

        var problem = await Handle(exception, "Production");

        problem.GetProperty("status").GetInt32().ShouldBe(expectedStatus);
        Detail(problem).ShouldBeNull();
    }

    [Fact]
    public async Task AnEntityFrameworkTrackingMessage_NeverReachesTheClientInProduction()
    {
        var problem = await Handle(
            new InvalidOperationException(
                "The instance of entity type 'UserEntity' cannot be tracked because another instance " +
                "with the key value '{Id: 3f2a}' is already being tracked."),
            "Production");

        problem.GetProperty("status").GetInt32().ShouldBe(StatusCodes.Status409Conflict);
        Detail(problem).ShouldBeNull();
    }

    [Fact]
    public async Task FrameworkException_IsStillDiagnosableInDevelopment()
    {
        var problem = await Handle(new KeyNotFoundException("the reason"), "Development");

        Detail(problem).ShouldBe("the reason");
    }

    // An unmapped exception is a fault, and its message is the one that leaks schema and paths.
    [Fact]
    public async Task UnexpectedException_WithholdsItsMessageOutsideDevelopment()
    {
        var problem = await Handle(
            new DbUpdateFailure("Connection to 'db-cluster...' refused"), "Production");

        problem.GetProperty("status").GetInt32().ShouldBe(StatusCodes.Status500InternalServerError);
        problem.GetProperty("title").GetString().ShouldBe("An unexpected error occurred");
        Detail(problem).ShouldBeNull();
    }

    [Fact]
    public async Task UnexpectedException_IsStillDiagnosableInDevelopment()
    {
        var problem = await Handle(
            new DbUpdateFailure("Connection to 'db-cluster...' refused"), "Development");

        Detail(problem).ShouldBe("Connection to 'db-cluster...' refused");
        problem.TryGetProperty("stackTrace", out _).ShouldBeTrue();
    }

    // Stack traces describe the server rather than the request, so they stay development-only even
    // for the rejections whose message is now returned.
    [Fact]
    public async Task Rejection_NeverCarriesAStackTraceOutsideDevelopment()
    {
        var problem = await Handle(RejectedException.Conflict("nope"), "Production");

        problem.TryGetProperty("stackTrace", out _).ShouldBeFalse();
    }

    private sealed class DbUpdateFailure(string message) : Exception(message);

    private sealed class StubEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;
        public string ApplicationName { get; set; } = nameof(GlobalExceptionHandlerTests);
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
