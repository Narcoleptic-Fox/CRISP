using Crisp.Commands;
using Crisp.Exceptions;
using Crisp.Queries;
using Microsoft.AspNetCore.TestHost;
using System.Net;

namespace Crisp.AspNetCore.Tests.Extensions;

public class CrispApplicationBuilderExtensionsTests : TestBase
{
    [Fact]
    public void UseCrisp_ShouldRegisterMiddleware()
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Services.AddCrisp(crispBuilder =>
        {
            crispBuilder.RegisterHandlersFromAssemblies(typeof(CrispApplicationBuilderExtensionsTests).Assembly);
        });

        WebApplication app = builder.Build();

        // Act
        IApplicationBuilder result = app.UseCrisp(options =>
        {
            options.ValidateHandlers = false; // Disable validation for this test
        });

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(app);
    }

    [Fact]
    public void UseCrisp_WithNullApp_ShouldThrow() =>
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            CrispApplicationBuilderExtensions.UseCrisp(null!));

    [Fact]
    public void UseCrispExceptionHandling_ShouldRegisterExceptionMiddleware()
    {
        // Arrange
        WebApplication app = WebApplication.CreateBuilder().Build();

        // Act
        IApplicationBuilder result = app.UseCrispExceptionHandling();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(app);
    }

    [Fact]
    public void UseCrispExceptionHandling_WithNullApp_ShouldThrow() =>
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            CrispApplicationBuilderExtensions.UseCrispExceptionHandling(null!));

    [Fact]
    public async Task UseCrisp_ShouldMapEndpointsCorrectly()
    {
        // Arrange
        using TestServer server = CreateTestServer(services =>
        {
            services.AddSingleton<ICommandHandler<ExtAppTestCommand, ExtAppTestResponse>, ExtAppTestCommandHandler>();
            services.AddSingleton<IQueryHandler<ExtAppTestQuery, ExtAppTestResponse>, ExtAppTestQueryHandler>();
        });

        HttpClient client = server.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/api/extapptest?id=123&name=test");

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UseCrispExceptionHandling_ShouldHandleExceptions()
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
                        options.Endpoints.AutoDiscoverEndpoints = true;
                    });
                    builder.RegisterHandlersFromAssemblies(typeof(CrispApplicationBuilderExtensionsTests).Assembly);
                });
                services.AddSingleton<ICommandHandler<ExtExceptionTestCommand, ExtAppTestResponse>, ExtExceptionTestCommandHandler>();
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseCrispExceptionHandling();
                app.UseCrisp();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapCrisp();
                });
            });

        using TestServer server = new(builder);
        HttpClient client = server.CreateClient();

        // Act
        HttpResponseMessage response = await client.PostAsync("/api/extexceptiontest",
            new StringContent("{\"message\":\"test\"}", System.Text.Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task UseCrispExceptionHandling_WithNotFoundException_ShouldReturn404()
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
                        options.Endpoints.AutoDiscoverEndpoints = true;
                    });
                    builder.RegisterHandlersFromAssemblies(typeof(CrispApplicationBuilderExtensionsTests).Assembly);
                });
                services.AddSingleton<ICommandHandler<ExtNotFoundTestCommand, ExtAppTestResponse>, ExtNotFoundTestCommandHandler>();
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseCrispExceptionHandling();
                app.UseCrisp();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapCrisp();
                });
            });

        using TestServer server = new(builder);
        HttpClient client = server.CreateClient();

        // Act
        HttpResponseMessage response = await client.PostAsync("/api/shouldnotbefoundatall",
            new StringContent("{\"id\":999}", System.Text.Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UseCrisp_ShouldSupportMultipleEndpoints()
    {
        // Arrange
        using TestServer server = CreateTestServer(services =>
        {
            services.AddSingleton<ICommandHandler<ExtAppTestCommand, ExtAppTestResponse>, ExtAppTestCommandHandler>();
            services.AddSingleton<IQueryHandler<ExtAppTestQuery, ExtAppTestResponse>, ExtAppTestQueryHandler>();
            services.AddSingleton<ICommandHandler<ExtDeleteAnotherTestCommand>, ExtDeleteAnotherTestCommandHandler>();
            services.AddSingleton<ICommandDispatcher, ExtTestCommandDispatcher>();
            services.AddSingleton<IQueryDispatcher, ExtTestQueryDispatcher>();
        });

        HttpClient client = server.CreateClient();

        // Act & Assert
        HttpResponseMessage getResponse = await client.GetAsync("/api/extapptest?id=1&name=test");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        HttpResponseMessage postResponse = await client.PostAsync("/api/extapptest",
            new StringContent("{\"value\":\"test\"}", System.Text.Encoding.UTF8, "application/json"));
        postResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        HttpResponseMessage deleteResponse = await client.PostAsync("/api/extdeleteanothertest",
            new StringContent("{\"id\":123}", System.Text.Encoding.UTF8, "application/json"));
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    // Test classes
    public record ExtAppTestCommand(string Value) : ICommand<ExtAppTestResponse>;
    public record ExtAppTestQuery(int Id, string Name) : IQuery<ExtAppTestResponse>;
    public record ExtDeleteAnotherTestCommand(int Id) : ICommand;
    public record ExtExceptionTestCommand(string Message) : ICommand<ExtAppTestResponse>;
    public record ExtValidationTestCommand(string Value) : ICommand<ExtAppTestResponse>;
    public record ExtNotFoundTestCommand(int Id) : ICommand<ExtAppTestResponse>;
    public record ExtUnauthorizedTestCommand(string Action) : ICommand<ExtAppTestResponse>;
    public record ExtAppTestResponse(string Result);

    public class ExtAppTestCommandHandler : ICommandHandler<ExtAppTestCommand, ExtAppTestResponse>
    {
        public Task<ExtAppTestResponse> Handle(ExtAppTestCommand request, CancellationToken cancellationToken) => Task.FromResult(new ExtAppTestResponse($"App Command: {request.Value}"));
    }

    public class ExtAppTestQueryHandler : IQueryHandler<ExtAppTestQuery, ExtAppTestResponse>
    {
        public Task<ExtAppTestResponse> Handle(ExtAppTestQuery request, CancellationToken cancellationToken) => Task.FromResult(new ExtAppTestResponse($"App Query: Id={request.Id}, Name={request.Name}"));
    }

    public class ExtDeleteAnotherTestCommandHandler : ICommandHandler<ExtDeleteAnotherTestCommand>
    {
        public Task Handle(ExtDeleteAnotherTestCommand request, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    public class ExtExceptionTestCommandHandler : ICommandHandler<ExtExceptionTestCommand, ExtAppTestResponse>
    {
        public Task<ExtAppTestResponse> Handle(ExtExceptionTestCommand request, CancellationToken cancellationToken) => throw new InvalidOperationException("Test exception");
    }

    public class ExtValidationTestCommandHandler : ICommandHandler<ExtValidationTestCommand, ExtAppTestResponse>
    {
        public Task<ExtAppTestResponse> Handle(ExtValidationTestCommand request, CancellationToken cancellationToken) => string.IsNullOrEmpty(request.Value)
                ? throw new ValidationException("Value is required")
                : Task.FromResult(new ExtAppTestResponse($"Validated: {request.Value}"));
    }

    public class ExtNotFoundTestCommandHandler : ICommandHandler<ExtNotFoundTestCommand, ExtAppTestResponse>
    {
        public Task<ExtAppTestResponse> Handle(ExtNotFoundTestCommand request, CancellationToken cancellationToken) => throw new NotFoundException($"Item with ID {request.Id} not found");
    }

    public class ExtUnauthorizedTestCommandHandler : ICommandHandler<ExtUnauthorizedTestCommand, ExtAppTestResponse>
    {
        public Task<ExtAppTestResponse> Handle(ExtUnauthorizedTestCommand request, CancellationToken cancellationToken) => throw new UnauthorizedException("Access denied");
    }

    public class ExtTestCommandDispatcher : ICommandDispatcher
    {
        public Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default) => command switch
        {
            ExtAppTestCommand tc => Task.FromResult((TResponse)(object)new ExtAppTestResponse($"App Command: {tc.Value}")),
            ExtValidationTestCommand vtc => throw new ValidationException("Value is required"),
            ExtNotFoundTestCommand nftc => throw new NotFoundException($"Item with ID {nftc.Id} not found"),
            ExtUnauthorizedTestCommand utc => throw new UnauthorizedException("Access denied"),
            ExtExceptionTestCommand etc => throw new InvalidOperationException("Test exception"),
            _ => throw new NotImplementedException($"Handler not implemented for {typeof(TResponse).Name}")
        };

        public Task Send(ICommand command, CancellationToken cancellationToken = default) => command switch
        {
            ExtDeleteAnotherTestCommand => Task.CompletedTask,
            _ => throw new NotImplementedException($"Handler not implemented for {command.GetType().Name}")
        };
    }

    public class ExtTestQueryDispatcher : IQueryDispatcher
    {
        public Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default) => query switch
        {
            ExtAppTestQuery tq => Task.FromResult((TResponse)(object)new ExtAppTestResponse($"App Query: Id={tq.Id}, Name={tq.Name}")),
            _ => throw new NotImplementedException($"Handler not implemented for {typeof(TResponse).Name}")
        };
    }
}