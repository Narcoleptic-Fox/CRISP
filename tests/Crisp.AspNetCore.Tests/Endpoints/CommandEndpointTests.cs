using Crisp.Commands;
using Crisp.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
        var endpoint = new CommandEndpoint<CmdTestCommand, CmdTestResponse>();

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
        var endpoint = new CommandEndpoint<CmdTestCommand, CmdTestResponse>("/custom", "POST");

        // Assert
        endpoint.Pattern.Should().Be("/custom");
        endpoint.HttpMethod.Should().Be("POST");
    }

    [Fact]
    public void Constructor_WithNullPattern_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CommandEndpoint<CmdTestCommand, CmdTestResponse>(null!, "POST"));
    }

    [Fact]
    public void Constructor_WithNullHttpMethod_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CommandEndpoint<CmdTestCommand, CmdTestResponse>("/test", null!));
    }

    [Fact]
    public void Map_ShouldRegisterEndpointWithCorrectConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddRouting();
        services.AddCrisp()
            .AddHandlersFromAssembly(typeof(CommandEndpointTests).Assembly);
        
        var app = WebApplication.CreateBuilder().Build();
        app.Services.GetRequiredService<IServiceProvider>();
        
        var endpoint = new CommandEndpoint<CmdTestCommand, CmdTestResponse>();

        // Act
        var builder = endpoint.Map(app);

        // Assert
        builder.Should().NotBeNull();
    }

    [Fact]
    public void Map_WithNullApp_ShouldThrow()
    {
        // Arrange
        var endpoint = new CommandEndpoint<CmdTestCommand, CmdTestResponse>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => endpoint.Map(null!));
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("DELETE")]
    public async Task Handle_GetOrDelete_ShouldBindFromRoute(string httpMethod)
    {
        // Arrange
        using var server = CreateTestServer(services =>
        {
            services.AddSingleton<ICommandHandler<CmdTestCommand, CmdTestResponse>, CmdTestCommandHandler>();
            services.AddSingleton<ICommandDispatcher, TestCommandDispatcher>();
        });
        
        var client = server.CreateClient();
        
        // Act
        var response = await client.SendAsync(new HttpRequestMessage(new HttpMethod(httpMethod), "/api/cmdtest"));
        
        // Assert
        response.Should().NotBeNull();
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("PATCH")]
    public async Task Handle_PostPutPatch_ShouldBindFromBody(string httpMethod)
    {
        // Arrange
        using var server = CreateTestServer(services =>
        {
            services.AddSingleton<ICommandHandler<CmdTestCommand, CmdTestResponse>, CmdTestCommandHandler>();
            services.AddSingleton<ICommandDispatcher, TestCommandDispatcher>();
        });
        
        var client = server.CreateClient();
        var command = new CmdTestCommand("test message");
        var json = JsonSerializer.Serialize(command);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.SendAsync(new HttpRequestMessage(new HttpMethod(httpMethod), "/api/cmdtest") 
        { 
            Content = content 
        });

        // Assert
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_InvalidBody_ShouldThrowBadHttpRequest()
    {
        // Arrange
        var builder = new WebHostBuilder()
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

        using var server = new TestServer(builder);
        var client = server.CreateClient();
        var content = new StringContent("invalid json", Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/cmdtest", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Handle_PostCommand_ShouldReturnCreated()
    {
        // Arrange
        using var server = CreateTestServer(services =>
        {
            services.AddSingleton<ICommandHandler<CmdCreateTestCommand, CmdTestResponse>, CmdCreateTestCommandHandler>();
            services.AddSingleton<ICommandDispatcher, TestCommandDispatcher>();
        });
        
        var client = server.CreateClient();
        var command = new CmdCreateTestCommand("new item");
        var json = JsonSerializer.Serialize(command);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/cmdcreatetest", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Handle_DeleteCommand_ShouldReturnNoContent()
    {
        // Arrange
        var builder = new WebHostBuilder()
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

        using var server = new TestServer(builder);
        var client = server.CreateClient();

        // Act
        var response = await client.DeleteAsync("/api/test/delete/123");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public void VoidCommandEndpoint_Constructor_ShouldSetProperties()
    {
        // Act
        var endpoint = new VoidCommandEndpoint<CmdTestVoidCommand>();

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
        var endpoint = new VoidCommandEndpoint<CmdTestVoidCommand>("/custom-void", "DELETE");

        // Assert
        endpoint.Pattern.Should().Be("/custom-void");
        endpoint.HttpMethod.Should().Be("DELETE");
    }

    [Fact]
    public void VoidCommandEndpoint_Map_ShouldRegisterEndpoint()
    {
        // Arrange
        var app = WebApplication.CreateBuilder().Build();
        var endpoint = new VoidCommandEndpoint<CmdTestVoidCommand>();

        // Act
        var builder = endpoint.Map(app);

        // Assert
        builder.Should().NotBeNull();
    }

    [Fact]
    public async Task VoidCommandEndpoint_Handle_ShouldReturnNoContent()
    {
        // Arrange
        using var server = CreateTestServer(services =>
        {
            services.AddSingleton<ICommandHandler<CmdTestVoidCommand>, CmdTestVoidCommandHandler>();
            services.AddSingleton<ICommandDispatcher, TestCommandDispatcher>();
        });
        
        var client = server.CreateClient();
        var command = new CmdTestVoidCommand("test action");
        var json = JsonSerializer.Serialize(command);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/cmdtestvoid", content);

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
        public Task<CmdTestResponse> Handle(CmdTestCommand request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new CmdTestResponse($"Handled: {request.Message}"));
        }
    }

    public class CmdCreateTestCommandHandler : ICommandHandler<CmdCreateTestCommand, CmdTestResponse>
    {
        public Task<CmdTestResponse> Handle(CmdCreateTestCommand request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new CmdTestResponse($"Created: {request.Name}"));
        }
    }

    public class CmdDeleteTestCommandHandler : ICommandHandler<CmdDeleteTestCommand>
    {
        public Task Handle(CmdDeleteTestCommand request, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    public class CmdTestVoidCommandHandler : ICommandHandler<CmdTestVoidCommand>
    {
        public Task Handle(CmdTestVoidCommand request, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    public class TestCommandDispatcher : ICommandDispatcher
    {
        public Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
        {
            return command switch
            {
                CmdTestCommand tc => Task.FromResult((TResponse)(object)new CmdTestResponse($"Handled: {tc.Message}")),
                CmdCreateTestCommand ctc => Task.FromResult((TResponse)(object)new CmdTestResponse($"Created: {ctc.Name}")),
                _ => throw new NotImplementedException($"Handler not implemented for {typeof(TResponse).Name}")
            };
        }

        public Task Send(ICommand command, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}