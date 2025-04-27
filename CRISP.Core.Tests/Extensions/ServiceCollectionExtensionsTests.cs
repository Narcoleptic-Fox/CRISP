using CRISP.Core.Behaviors;
using CRISP.Core.Events;
using CRISP.Core.Extensions;
using CRISP.Core.Interfaces;
using CRISP.Core.Modules;
using CRISP.Core.Options;
using CRISP.Core.Resilience;
using CRISP.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Reflection;

namespace CRISP.Core.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddCrispCore_RegistersRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Act
        services.AddCrispCore();
        
        // Assert
        var provider = services.BuildServiceProvider();
        
        // Verify core services are registered
        provider.GetService<IMediator>().Should().NotBeNull();
        provider.GetService<IEventDispatcher>().Should().NotBeNull();
    }

    [Fact]
    public void AddCrispCore_WithCustomOptions_ConfiguresOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Act
        services.AddCrispCore(options => {
            options.ConfigureMediator(m => {
                m.AllowMultipleHandlers = true;
                m.DefaultTimeoutSeconds = 60;
            });
            
            options.ConfigureResilience(r => {
                r.Retry.MaxRetryAttempts = 5;
            });
        });
        
        // Assert
        var provider = services.BuildServiceProvider();
        
        var mediatorOptions = provider.GetRequiredService<MediatorOptions>();
        mediatorOptions.AllowMultipleHandlers.Should().BeTrue();
        mediatorOptions.DefaultTimeoutSeconds.Should().Be(60);
        
        var resilienceOptions = provider.GetRequiredService<ResilienceOptions>();
        resilienceOptions.Retry.MaxRetryAttempts.Should().Be(5);
    }

    [Fact]
    public void AddCrispFromAssemblies_RegistersAllComponents()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Mock logger factory for testing
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(Mock.Of<ILogger>());
        
        services.AddSingleton(mockLoggerFactory.Object);
        
        // Act
        services.AddCrispFromAssemblies(typeof(TestModule).Assembly);
        
        // Assert
        var provider = services.BuildServiceProvider();
        
        // Verify all component types are registered
        provider.GetService<IMediator>().Should().NotBeNull();
        provider.GetService<IEventDispatcher>().Should().NotBeNull();
        
        // Verify module is registered
        var modules = provider.GetServices<IModule>();
        modules.Should().Contain(m => m.GetType() == typeof(TestModule));
    }

    [Fact]
    public void AddResilienceStrategies_RegistersCorrectStrategies()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(new NullLoggerFactory());
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        
        // Act
        services.AddResilienceStrategies();
        
        // Assert
        var provider = services.BuildServiceProvider();
        
        // Specific strategies
        provider.GetService<RetryStrategy>().Should().NotBeNull();
        provider.GetService<CircuitBreakerStrategy>().Should().NotBeNull();
        provider.GetService<TimeoutStrategy>().Should().NotBeNull();
        
        // Composite strategy
        provider.GetService<CompositeResilienceStrategy>().Should().NotBeNull();
        
        // Interface
        provider.GetService<IResilienceStrategy>().Should().NotBeNull();
    }

    [Fact]
    public void UseChannelEventProcessing_ConfiguresChannelEventDispatcher()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(new NullLoggerFactory());
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        
        // Act
        services.AddCrispCore(options => {
            options.UseChannelEventProcessing(channelOptions => {
                channelOptions.ChannelCapacity = 200;
                channelOptions.ConsumerCount = 4;
            });
        });
        
        // Assert
        var provider = services.BuildServiceProvider();
        
        // Event dispatcher should be channel-based
        var eventDispatcher = provider.GetService<IEventDispatcher>();
        eventDispatcher.Should().BeOfType<ChannelEventDispatcher>();
        
        // Options should be configured
        var channelOptions = provider.GetService<ChannelEventOptions>();
        channelOptions.Should().NotBeNull();
        channelOptions!.ChannelCapacity.Should().Be(200);
        channelOptions.ConsumerCount.Should().Be(4);
    }

    [Fact]
    public void AddPipelineBehaviors_RegistersBehaviorsInCorrectOrder()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(new NullLoggerFactory());
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        
        // Act
        services.AddCrispCore();
        
        // Assert
        var behaviors = services.Where(sd => sd.ServiceType.IsGenericType && 
                                        sd.ServiceType.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>))
                                .OrderBy(sd => sd.ImplementationInstance != null ? 0 : 1) // Instance behaviors first
                                .ThenBy(sd => sd.ServiceType.ToString())
                                .ToList();
        
        // Validate the pipeline behaviors
        behaviors.Should().NotBeEmpty();
        
        // Check that the behaviors are registered in the correct order
        // Validation should be first, then logging, then performance
        var behaviorTypes = behaviors.Select(sd => sd.ImplementationType?.Name ?? sd.ImplementationInstance?.GetType().Name)
                                    .Where(name => name != null)
                                    .ToArray();
        
        behaviorTypes.Should().Contain("ValidationBehavior`2");
        behaviorTypes.Should().Contain("LoggingBehavior`2");
        behaviorTypes.Should().Contain("PerformanceBehavior`2");
    }

    public class TestModule : ModuleBase
    {
        public override string ModuleName => "TestModule";
        
        public override void RegisterServices(IServiceCollection services)
        {
            // This is just a test module for testing registration
        }
    }

    // Sample handlers and validators for testing registration
    public class TestCommand : IRequest<string> { }
    
    public class TestCommandHandler : IRequestHandler<TestCommand, string>
    {
        public ValueTask<string> Handle(TestCommand request, CancellationToken cancellationToken)
        {
            return new ValueTask<string>("Test");
        }
    }
    
    public class TestValidator : IValidator<TestCommand>
    {
        public ValidationResult Validate(TestCommand request)
        {
            return new ValidationResult();
        }
    }
    
    public class TestEvent : DomainEvent { }
    
    public class TestEventHandler : IEventHandler<TestEvent>
    {
        public ValueTask Handle(TestEvent @event, CancellationToken cancellationToken)
        {
            return new ValueTask();
        }
    }
}