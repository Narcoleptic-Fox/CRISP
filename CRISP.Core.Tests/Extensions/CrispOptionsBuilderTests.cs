using CRISP.Core.Events;
using CRISP.Core.Extensions;
using CRISP.Core.Options;
using CRISP.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CRISP.Core.Tests.Extensions;

public class CrispOptionsBuilderTests
{
    #region ConfigureResilience
    
    [Fact]
    public void ConfigureResilience_SetsResilienceOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddCrispCore();
        var builder = new CrispOptionsBuilder(services);
        
        // Act
        builder.ConfigureResilience(options => {
            options.Retry.MaxRetryAttempts = 10;
            options.CircuitBreaker.FailureThreshold = 8;
            options.Timeout.TimeoutSeconds = 60;
        });
        
        // Assert
        var provider = services.BuildServiceProvider();
        var resilienceOptions = provider.GetRequiredService<ResilienceOptions>();
        resilienceOptions.Should().NotBeNull();
        resilienceOptions.Retry.MaxRetryAttempts.Should().Be(10);
        resilienceOptions.CircuitBreaker.FailureThreshold.Should().Be(8);
        resilienceOptions.Timeout.TimeoutSeconds.Should().Be(60);
    }
    
    [Fact]
    public void ConfigureResilience_ReturnsBuilder_ForChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new CrispOptionsBuilder(services);
        
        // Act
        var returnedBuilder = builder.ConfigureResilience(options => {
            options.Retry.MaxRetryAttempts = 5;
        });
        
        // Assert
        returnedBuilder.Should().BeSameAs(builder);
    }
    
    #endregion
    
    #region ConfigureEvents
    
    [Fact]
    public void ConfigureEvents_SetsEventOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddCrispCore();
        var builder = new CrispOptionsBuilder(services);
        
        // Act
        builder.ConfigureEvents(options => {
            options.UseChannels = true;
        });
        
        // Assert
        var provider = services.BuildServiceProvider();
        var eventOptions = provider.GetRequiredService<EventOptions>();
        eventOptions.Should().NotBeNull();
        eventOptions.UseChannels.Should().BeTrue();
    }
    
    [Fact]
    public void ConfigureEvents_ReturnsBuilder_ForChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new CrispOptionsBuilder(services);
        
        // Act
        var returnedBuilder = builder.ConfigureEvents(options => {
            options.UseChannels = true;
        });
        
        // Assert
        returnedBuilder.Should().BeSameAs(builder);
    }
    
    #endregion
    
    #region ConfigureChannelEvents
    
    [Fact]
    public void ConfigureChannelEvents_SetsChannelEventOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddCrispCore();
        var builder = new CrispOptionsBuilder(services);
        
        // Act
        builder.ConfigureChannelEvents(options => {
            options.ChannelCapacity = 200;
        });
        
        // Assert
        var provider = services.BuildServiceProvider();
        var channelOptions = provider.GetRequiredService<ChannelEventOptions>();
        channelOptions.Should().NotBeNull();
        channelOptions.ChannelCapacity.Should().Be(200);
    }
    
    [Fact]
    public void ConfigureChannelEvents_ReturnsBuilder_ForChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new CrispOptionsBuilder(services);
        
        // Act
        var returnedBuilder = builder.ConfigureChannelEvents(options => {
            options.ChannelCapacity = 100;
        });
        
        // Assert
        returnedBuilder.Should().BeSameAs(builder);
    }
    
    #endregion
    
    #region UseChannelEventProcessing
    
    [Fact]
    public void UseChannelEventProcessing_WithoutConfig_EnablesChannelProcessing()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddCrispCore();
        var builder = new CrispOptionsBuilder(services);
        
        // Act
        builder.UseChannelEventProcessing();
        
        // Assert
        var provider = services.BuildServiceProvider();
        var eventOptions = provider.GetRequiredService<EventOptions>();
        eventOptions.Should().NotBeNull();
        eventOptions.UseChannels.Should().BeTrue();
    }
    
    [Fact]
    public void UseChannelEventProcessing_WithConfig_SetsChannelEventOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddCrispCore();
        var builder = new CrispOptionsBuilder(services);
        
        // Act
        builder.UseChannelEventProcessing(options => {
            options.ChannelCapacity = 500;
        });
        
        // Assert
        var provider = services.BuildServiceProvider();
        var eventOptions = provider.GetRequiredService<EventOptions>();
        var channelOptions = provider.GetRequiredService<ChannelEventOptions>();
        
        eventOptions.UseChannels.Should().BeTrue();
        channelOptions.ChannelCapacity.Should().Be(500);
    }
    
    [Fact]
    public void UseChannelEventProcessing_ReturnsBuilder_ForChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new CrispOptionsBuilder(services);
        
        // Act
        var returnedBuilder = builder.UseChannelEventProcessing();
        
        // Assert
        returnedBuilder.Should().BeSameAs(builder);
    }
    
    #endregion
    
    #region ConfigureMediator
    
    [Fact]
    public void ConfigureMediator_SetsMediatorOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddCrispCore();
        var builder = new CrispOptionsBuilder(services);
        
        // Act
        builder.ConfigureMediator(options => {
            options.AllowMultipleHandlers = true;
            options.DefaultTimeoutSeconds = 120;
        });
        
        // Assert
        var provider = services.BuildServiceProvider();
        var mediatorOptions = provider.GetRequiredService<MediatorOptions>();
        mediatorOptions.Should().NotBeNull();
        mediatorOptions.AllowMultipleHandlers.Should().BeTrue();
        mediatorOptions.DefaultTimeoutSeconds.Should().Be(120);
    }
    
    [Fact]
    public void ConfigureMediator_ReturnsBuilder_ForChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new CrispOptionsBuilder(services);
        
        // Act
        var returnedBuilder = builder.ConfigureMediator(options => {
            options.DefaultTimeoutSeconds = 60;
        });
        
        // Assert
        returnedBuilder.Should().BeSameAs(builder);
    }
    
    #endregion
    
    #region ConfigureValidation
    
    [Fact]
    public void ConfigureValidation_SetsValidationOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddCrispCore();
        var builder = new CrispOptionsBuilder(services);
        
        // Act
        builder.ConfigureValidation(options => {
            options.SkipValidationIfNoValidatorsRegistered = true;
        });
        
        // Assert
        var provider = services.BuildServiceProvider();
        var validationOptions = provider.GetRequiredService<ValidationOptions>();
        validationOptions.Should().NotBeNull();
        validationOptions.SkipValidationIfNoValidatorsRegistered.Should().BeTrue();
    }
    
    [Fact]
    public void ConfigureValidation_ReturnsBuilder_ForChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new CrispOptionsBuilder(services);
        
        // Act
        var returnedBuilder = builder.ConfigureValidation(options => {
            options.SkipValidationIfNoValidatorsRegistered = true;
        });
        
        // Assert
        returnedBuilder.Should().BeSameAs(builder);
    }
    
    #endregion
}
