using CRISP.Backend;
using CRISP.Events;
using CRISP.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CRISP.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddCrispFromAssemblies_RegistersAllComponents()
    {
        // Arrange
        ServiceCollection services = new();

        // Add required logger registration
        services.AddSingleton<ILoggerFactory>(new NullLoggerFactory());
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

        // Act
        services.AddCrispFromAssemblies(typeof(TestModule).Assembly);

        // Assert
        ServiceProvider provider = services.BuildServiceProvider();

        // Verify all component types are registered
        provider.GetService<IMediator>().ShouldNotBeNull();
        provider.GetService<IEventDispatcher>().ShouldNotBeNull();

        // Verify module is registered
        IEnumerable<IModule> modules = provider.GetServices<IModule>();
        modules.ShouldContain(m => m.GetType() == typeof(TestModule));
    }

    public class TestModule : IModule
    {
        public string ModuleName => "TestModule";

        public void RegisterServices(IServiceCollection services)
        {
            // This is just a test module for testing registration
        }
    }

    // Sample handlers and validators for testing registration
    public class TestCommand : IRequest<string> { }

    public class TestCommandHandler : IRequestHandler<TestCommand, string>
    {
        public ValueTask<string> Handle(TestCommand request, CancellationToken cancellationToken) => new("Test");
    }

    public class TestValidator : IValidator<TestCommand>
    {
        public ValidationResult Validate(TestCommand request) => new();
    }

    public class TestEvent : IEvent { }

    public class TestEventHandler : IEventHandler<TestEvent>
    {
        public ValueTask Handle(TestEvent @event, CancellationToken cancellationToken) => new();
    }
}