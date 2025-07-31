using Crisp.Commands;
using Crisp.Endpoints;
using Microsoft.AspNetCore.TestHost;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Crisp.AspNetCore.Tests.Endpoints;

public class CommandEndpointTests : TestBase
{
    [Fact]
    public void Constructor_ShouldSetDefaultPatternAndMethod()
    {
        // Act
        CommandEndpoint<CmdTestCommand, CmdTestResponse> endpoint = new();

        // Assert
        endpoint.Pattern.Should().NotBeNullOrEmpty();
        endpoint.HttpMethod.Should().NotBeNullOrEmpty();
        endpoint.RequestType.Should().Be(typeof(CmdTestCommand));
        endpoint.ResponseType.Should().Be(typeof(CmdTestResponse));
    }

    [Fact]
    public void Constructor_WithCustomValues_ShouldUseProvided()
    {
        // Act
        CommandEndpoint<CmdTestCommand, CmdTestResponse> endpoint = new("/custom", "POST");

        // Assert
        endpoint.Pattern.Should().Be("/custom");
        endpoint.HttpMethod.Should().Be("POST");
    }

    [Fact]
    public void Constructor_WithNullPattern_ShouldThrow() =>
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CommandEndpoint<CmdTestCommand, CmdTestResponse>(null!, "POST"));

    [Fact]
    public void Constructor_WithNullHttpMethod_ShouldThrow() =>
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CommandEndpoint<CmdTestCommand, CmdTestResponse>("/test", null!));

    [Fact]
    public void Map_ShouldRegisterEndpointWithCorrectConfiguration()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddRouting();
        services.AddCrisp()
            .AddHandlersFromAssembly(typeof(CommandEndpointTests).Assembly);

        WebApplication app = WebApplication.CreateBuilder().Build();
        app.Services.GetRequiredService<IServiceProvider>();

        CommandEndpoint<CmdTestCommand, CmdTestResponse> endpoint = new();

        // Act
        RouteHandlerBuilder builder = endpoint.Map(app);

        // Assert
        builder.Should().NotBeNull();
    }

    [Fact]
    public void Map_WithNullApp_ShouldThrow()
    {
        // Arrange
        CommandEndpoint<CmdTestCommand, CmdTestResponse> endpoint = new();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => endpoint.Map(null!));
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("DELETE")]
    public async Task Handle_GetOrDelete_ShouldBindFromRoute(string httpMethod)
    {
        // Arrange
        using TestServer server = CreateTestServer(services =>
        {
            services.AddSingleton<ICommandHandler<CmdTestCommand, CmdTestResponse>, CmdTestCommandHandler>();
            services.AddSingleton<ICommandDispatcher, TestCommandDispatcher>();
        });

        HttpClient client = server.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(new HttpRequestMessage(new HttpMethod(httpMethod), "/api/cmdtest"));

        // Assert
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_InvalidBody_ShouldThrowBadHttpRequest()
    {
        // Arrange
        IWebHostBuilder builder = new WebHostBuilder()
            .UseEnvironment("Testing")
            .ConfigureServices(services =>
            {
                services.AddLogging();
                services.AddRouting();
                services.AddCrisp(builder =>
                {
                    builder.ConfigureOptions(options =>
                    {
                        options.Endpoints.AutoDiscoverEndpoints = false;
                    });
                });
                services.AddSingleton<ICommandHandler<CmdTestCommand, CmdTestResponse>, CmdTestCommandHandler>();
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseCrispExceptionHandling();
                app.UseCrisp();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapCommand<CmdTestCommand, CmdTestResponse>("/api/cmdtest");
                });
            });

        using TestServer server = new(builder);
        HttpClient client = server.CreateClient();
        StringContent content = new("invalid json", Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await client.PostAsync("/api/cmdtest", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Handle_DeleteCommand_ShouldReturnNoContent()
    {
        // Arrange
        IWebHostBuilder builder = new WebHostBuilder()
            .UseEnvironment("Testing")
            .ConfigureServices(services =>
            {
                services.AddLogging();
                services.AddRouting();
                services.AddCrisp(builder =>
                {
                    builder.ConfigureOptions(options =>
                    {
                        options.Endpoints.AutoDiscoverEndpoints = false;
                    });
                });
                services.AddSingleton<ICommandHandler<CmdDeleteTestCommand>, CmdDeleteTestCommandHandler>();
                services.AddSingleton<ICommandDispatcher, TestCommandDispatcher>();
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseCrisp();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapCrispEndpoint<CmdDeleteTestCommand>("/api/test/delete/{id}", "DELETE");
                });
            });

        using TestServer server = new(builder);
        HttpClient client = server.CreateClient();

        // Act
        HttpResponseMessage response = await client.DeleteAsync("/api/test/delete/123");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public void VoidCommandEndpoint_Constructor_ShouldSetProperties()
    {
        // Act
        VoidCommandEndpoint<CmdTestVoidCommand> endpoint = new();

        // Assert
        endpoint.RequestType.Should().Be(typeof(CmdTestVoidCommand));
        endpoint.ResponseType.Should().BeNull();
        endpoint.Pattern.Should().NotBeNullOrEmpty();
        endpoint.HttpMethod.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void VoidCommandEndpoint_Constructor_WithCustomValues_ShouldUseProvided()
    {
        // Act
        VoidCommandEndpoint<CmdTestVoidCommand> endpoint = new("/custom-void", "DELETE");

        // Assert
        endpoint.Pattern.Should().Be("/custom-void");
        endpoint.HttpMethod.Should().Be("DELETE");
    }

    [Fact]
    public void VoidCommandEndpoint_Map_ShouldRegisterEndpoint()
    {
        // Arrange
        WebApplication app = WebApplication.CreateBuilder().Build();
        VoidCommandEndpoint<CmdTestVoidCommand> endpoint = new();

        // Act
        RouteHandlerBuilder builder = endpoint.Map(app);

        // Assert
        builder.Should().NotBeNull();
    }

    [Fact]
    public async Task VoidCommandEndpoint_Handle_ShouldReturnNoContent()
    {
        // Arrange
        using TestServer server = CreateTestServer(services =>
        {
            services.AddSingleton<ICommandHandler<CmdTestVoidCommand>, CmdTestVoidCommandHandler>();
            services.AddSingleton<ICommandDispatcher, TestCommandDispatcher>();
        });

        HttpClient client = server.CreateClient();
        CmdTestVoidCommand command = new("test action");
        string json = JsonSerializer.Serialize(command);
        StringContent content = new(json, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await client.PostAsync("/api/cmdtestvoid", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    // Test classes
    public record CmdTestCommand(string Message = "") : ICommand<CmdTestResponse>;
    public record CmdCreateTestCommand(string Name) : ICommand<CmdTestResponse>;
    public record CmdDeleteTestCommand(int Id) : ICommand;
    public record CmdTestResponse(string Result);
    public record CmdTestVoidCommand(string Action = "") : ICommand;

    public class CmdTestCommandHandler : ICommandHandler<CmdTestCommand, CmdTestResponse>
    {
        public Task<CmdTestResponse> Handle(CmdTestCommand request, CancellationToken cancellationToken) => Task.FromResult(new CmdTestResponse($"Handled: {request.Message}"));
    }

    public class CmdCreateTestCommandHandler : ICommandHandler<CmdCreateTestCommand, CmdTestResponse>
    {
        public Task<CmdTestResponse> Handle(CmdCreateTestCommand request, CancellationToken cancellationToken) => Task.FromResult(new CmdTestResponse($"Created: {request.Name}"));
    }

    public class CmdDeleteTestCommandHandler : ICommandHandler<CmdDeleteTestCommand>
    {
        public Task Handle(CmdDeleteTestCommand request, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    public class CmdTestVoidCommandHandler : ICommandHandler<CmdTestVoidCommand>
    {
        public Task Handle(CmdTestVoidCommand request, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    public class TestCommandDispatcher : ICommandDispatcher
    {
        public Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default) => command switch
        {
            CmdTestCommand tc => Task.FromResult((TResponse)(object)new CmdTestResponse($"Handled: {tc.Message}")),
            CmdCreateTestCommand ctc => Task.FromResult((TResponse)(object)new CmdTestResponse($"Created: {ctc.Name}")),
            _ => throw new NotImplementedException($"Handler not implemented for {typeof(TResponse).Name}")
        };

        public Task Send(ICommand command, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}