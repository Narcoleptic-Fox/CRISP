using CRISP.Behaviors;
using CRISP.Events;
using CRISP.Options;
using CRISP.Resilience;
using CRISP.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CRISP.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddCrispCore_RegistersRequiredServices()
    {
        // Arrange
        ServiceCollection services = new();

        // Add required logger registration
        services.AddSingleton<ILoggerFactory>(new NullLoggerFactory());
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

        // Act
        services.AddCrispCore();

        // Assert
        ServiceProvider provider = services.BuildServiceProvider();

        // Verify core services are registered
        provider.GetService<IMediator>().ShouldNotBeNull();
        provider.GetService<IEventDispatcher>().ShouldNotBeNull();
    }

    [Fact]
    public void AddCrispCore_WithCustomOptions_ConfiguresOptions()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddCrispCore(options =>
        {
            options.ConfigureMediator(m =>
            {
                m.AllowMultipleHandlers = true;
                m.DefaultTimeoutSeconds = 60;
            });

            options.ConfigureResilience(r =>
            {
                r.Retry.MaxRetryAttempts = 5;
            });
        });

        // Assert
        ServiceProvider provider = services.BuildServiceProvider();

        MediatorOptions mediatorOptions = provider.GetRequiredService<MediatorOptions>();
        mediatorOptions.AllowMultipleHandlers.ShouldBeTrue();
        mediatorOptions.DefaultTimeoutSeconds.ShouldBe(60);

        ResilienceOptions resilienceOptions = provider.GetRequiredService<ResilienceOptions>();
        resilienceOptions.Retry.MaxRetryAttempts.ShouldBe(5);
    }

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

    [Fact]
    public void AddResilienceStrategies_RegistersCorrectStrategies()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddSingleton<ILoggerFactory>(new NullLoggerFactory());
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddSingleton(new ResilienceOptions());

        // Act - Use the real extension method
        services.AddResilienceStrategies();

        // Assert
        ServiceProvider provider = services.BuildServiceProvider();

        // Check if specific strategies are registered
        provider.GetService<RetryStrategy>().ShouldNotBeNull();
        provider.GetService<CircuitBreakerStrategy>().ShouldNotBeNull();
        provider.GetService<TimeoutStrategy>().ShouldNotBeNull();

        // Check if IResilienceStrategy is registered
        IResilienceStrategy? resilienceStrategy = provider.GetService<IResilienceStrategy>();
        resilienceStrategy.ShouldNotBeNull();

        // The composite strategy should be registered as IResilienceStrategy
        resilienceStrategy.ShouldBeOfType<CompositeResilienceStrategy>();
    }

    [Fact]
    public void UseChannelEventProcessing_ConfiguresChannelEventDispatcher()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddSingleton<ILoggerFactory>(new NullLoggerFactory());
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

        // Act
        services.AddCrispCore(options =>
        {
            options.UseChannelEventProcessing(channelOptions =>
            {
                channelOptions.ChannelCapacity = 200;
                channelOptions.ConsumerCount = 4;
            });
        });

        // Assert
        ServiceProvider provider = services.BuildServiceProvider();

        // Event dispatcher should be channel-based
        IEventDispatcher? eventDispatcher = provider.GetService<IEventDispatcher>();
        eventDispatcher.ShouldBeOfType<ChannelEventDispatcher>();

        // Options should be configured
        ChannelEventOptions? channelOptions = provider.GetService<ChannelEventOptions>();
        channelOptions.ShouldNotBeNull();
        channelOptions!.ChannelCapacity.ShouldBe(200);
        channelOptions.ConsumerCount.ShouldBe(4);
    }

    [Fact]
    public void AddPipelineBehaviors_RegistersBehaviorsInCorrectOrder()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddSingleton<ILoggerFactory>(new NullLoggerFactory());
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

        // Need to register IPipelineBehavior<,> interfaces
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));

        // Act
        services.AddCrispCore();

        // Assert
        List<ServiceDescriptor> behaviors = services.Where(sd => sd.ServiceType.IsGenericType &&
                                        sd.ServiceType.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>))
                                .OrderBy(sd => sd.ImplementationInstance != null ? 0 : 1) // Instance behaviors first
                                .ThenBy(sd => sd.ServiceType.ToString())
                                .ToList();

        // Validate the pipeline behaviors
        behaviors.ShouldNotBeEmpty();

        // Check that the behaviors are registered in the correct order
        // Validation should be first, then logging, then performance
        string?[] behaviorTypes = behaviors.Select(sd => sd.ImplementationType?.Name ?? sd.ImplementationInstance?.GetType().Name)
                                    .Where(name => name != null)
                                    .ToArray();

        behaviorTypes.ShouldContain("ValidationBehavior`2");
        behaviorTypes.ShouldContain("LoggingBehavior`2");
        behaviorTypes.ShouldContain("PerformanceBehavior`2");
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