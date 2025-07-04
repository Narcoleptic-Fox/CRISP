using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Crisp;
using Crisp.Commands;
using Crisp.Queries;

namespace Crisp.AspNetCore.Tests;

public abstract class TestBase
{
    protected virtual IServiceProvider CreateServiceProvider(Action<IServiceCollection>? configureServices = null)
    {
        var services = new ServiceCollection();
        
        services.AddLogging(builder => builder.AddConsole());
        services.AddCrisp(builder =>
        {
            builder.ConfigureOptions(options =>
            {
                options.Endpoints.AutoDiscoverEndpoints = true;
            });
            builder.RegisterHandlersFromAssemblies(typeof(TestBase).Assembly);
        });
        
        configureServices?.Invoke(services);
        
        return services.BuildServiceProvider();
    }

    protected virtual TestServer CreateTestServer(Action<IServiceCollection>? configureServices = null)
    {
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
                        options.Endpoints.AutoDiscoverEndpoints = true;
                    });
                    builder.RegisterHandlersFromAssemblies(typeof(TestBase).Assembly);
                });
                configureServices?.Invoke(services);
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseCrisp();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapCrisp();
                });
            });

        return new TestServer(builder);
    }

    protected virtual TestServer CreateTestServerWithoutAutoDiscovery(Action<IServiceCollection>? configureServices = null)
    {
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
                    builder.RegisterHandlersFromAssemblies(typeof(TestBase).Assembly);
                });
                configureServices?.Invoke(services);
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseCrisp();
                app.UseEndpoints(endpoints =>
                {
                    // Manually register specific endpoints instead of auto-discovery
                    endpoints.MapCommand<BaseTestCommand, string>();
                    endpoints.MapVoidCommand<BaseVoidTestCommand>();
                    endpoints.MapQuery<BaseTestQuery, string>();
                });
            });

        return new TestServer(builder);
    }

    protected virtual HttpClient CreateTestClient(Action<IServiceCollection>? configureServices = null)
    {
        var server = CreateTestServer(configureServices);
        return server.CreateClient();
    }

    protected virtual WebApplication CreateWebApplication(Action<IServiceCollection>? configureServices = null)
    {
        var builder = WebApplication.CreateBuilder();
        
        builder.Services.AddLogging();
        builder.Services.AddRouting();
        builder.Services.AddCrisp(crispBuilder =>
        {
            crispBuilder.ConfigureOptions(options =>
            {
                options.Endpoints.AutoDiscoverEndpoints = true;
            });
            crispBuilder.RegisterHandlersFromAssemblies(typeof(TestBase).Assembly);
        });
        
        configureServices?.Invoke(builder.Services);
        
        var app = builder.Build();
        
        app.UseRouting();
        app.UseCrisp();
        app.MapCrisp();
        
        return app;
    }
}

public class TestWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> 
    where TProgram : class
{
    private readonly Action<IServiceCollection>? _configureServices;

    public TestWebApplicationFactory(Action<IServiceCollection>? configureServices = null)
    {
        _configureServices = configureServices;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            _configureServices?.Invoke(services);
        });
    }
}

public record BaseTestCommand(string Value) : ICommand<string>;
public record BaseTestQuery(int Id) : IQuery<string>;
public record BaseVoidTestCommand(string Value) : ICommand;

public class BaseTestCommandHandler : ICommandHandler<BaseTestCommand, string>
{
    public Task<string> Handle(BaseTestCommand request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult($"Handled: {request.Value}");
    }
}

public class BaseTestQueryHandler : IQueryHandler<BaseTestQuery, string>
{
    public Task<string> Handle(BaseTestQuery request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult($"Query result for ID: {request.Id}");
    }
}

public class BaseVoidTestCommandHandler : ICommandHandler<BaseVoidTestCommand>
{
    public Task Handle(BaseVoidTestCommand request, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}