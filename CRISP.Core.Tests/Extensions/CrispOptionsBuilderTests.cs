using CRISP.Core.Options;
using Microsoft.Extensions.DependencyInjection;

namespace CRISP.Core.Tests.Extensions;

public class CrispOptionsBuilderTests
{
    #region ConfigureResilience

    [Fact]
    public void ConfigureResilience_SetsResilienceOptions()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddCrispCore();
        CrispOptionsBuilder builder = new(services);

        // Act
        builder.ConfigureResilience(options =>
        {
            options.Retry.MaxRetryAttempts = 10;
            options.CircuitBreaker.FailureThreshold = 8;
            options.Timeout.TimeoutSeconds = 60;
        });

        // Assert
        ServiceProvider provider = services.BuildServiceProvider();
        ResilienceOptions resilienceOptions = provider.GetRequiredService<ResilienceOptions>();
        resilienceOptions.ShouldNotBeNull();
        resilienceOptions.Retry.MaxRetryAttempts.ShouldBe(10);
        resilienceOptions.CircuitBreaker.FailureThreshold.ShouldBe(8);
        resilienceOptions.Timeout.TimeoutSeconds.ShouldBe(60);
    }

    [Fact]
    public void ConfigureResilience_ReturnsBuilder_ForChaining()
    {
        // Arrange
        ServiceCollection services = new();
        CrispOptionsBuilder builder = new(services);

        // Act
        CrispOptionsBuilder returnedBuilder = builder.ConfigureResilience(options =>
        {
            options.Retry.MaxRetryAttempts = 5;
        });

        // Assert
        returnedBuilder.ShouldBeSameAs(builder);
    }

    #endregion

    #region ConfigureEvents

    [Fact]
    public void ConfigureEvents_SetsEventOptions()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddCrispCore();
        CrispOptionsBuilder builder = new(services);

        // Act
        builder.ConfigureEvents(options =>
        {
            options.UseChannels = true;
        });

        // Assert
        ServiceProvider provider = services.BuildServiceProvider();
        EventOptions eventOptions = provider.GetRequiredService<EventOptions>();
        eventOptions.ShouldNotBeNull();
        eventOptions.UseChannels.ShouldBeTrue();
    }

    [Fact]
    public void ConfigureEvents_ReturnsBuilder_ForChaining()
    {
        // Arrange
        ServiceCollection services = new();
        CrispOptionsBuilder builder = new(services);

        // Act
        CrispOptionsBuilder returnedBuilder = builder.ConfigureEvents(options =>
        {
            options.UseChannels = true;
        });

        // Assert
        returnedBuilder.ShouldBeSameAs(builder);
    }

    #endregion

    #region ConfigureChannelEvents

    [Fact]
    public void ConfigureChannelEvents_SetsChannelEventOptions()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddCrispCore();
        CrispOptionsBuilder builder = new(services);

        // Act
        builder.ConfigureChannelEvents(options =>
        {
            options.ChannelCapacity = 200;
        });

        // Assert
        ServiceProvider provider = services.BuildServiceProvider();
        ChannelEventOptions channelOptions = provider.GetRequiredService<ChannelEventOptions>();
        channelOptions.ShouldNotBeNull();
        channelOptions.ChannelCapacity.ShouldBe(200);
    }

    [Fact]
    public void ConfigureChannelEvents_ReturnsBuilder_ForChaining()
    {
        // Arrange
        ServiceCollection services = new();
        CrispOptionsBuilder builder = new(services);

        // Act
        CrispOptionsBuilder returnedBuilder = builder.ConfigureChannelEvents(options =>
        {
            options.ChannelCapacity = 100;
        });

        // Assert
        returnedBuilder.ShouldBeSameAs(builder);
    }

    #endregion

    #region UseChannelEventProcessing

    [Fact]
    public void UseChannelEventProcessing_WithoutConfig_EnablesChannelProcessing()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddCrispCore();
        CrispOptionsBuilder builder = new(services);

        // Act
        builder.UseChannelEventProcessing();

        // Assert
        ServiceProvider provider = services.BuildServiceProvider();
        EventOptions eventOptions = provider.GetRequiredService<EventOptions>();
        eventOptions.ShouldNotBeNull();
        eventOptions.UseChannels.ShouldBeTrue();
    }

    [Fact]
    public void UseChannelEventProcessing_WithConfig_SetsChannelEventOptions()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddCrispCore();
        CrispOptionsBuilder builder = new(services);

        // Act
        builder.UseChannelEventProcessing(options =>
        {
            options.ChannelCapacity = 500;
        });

        // Assert
        ServiceProvider provider = services.BuildServiceProvider();
        EventOptions eventOptions = provider.GetRequiredService<EventOptions>();
        ChannelEventOptions channelOptions = provider.GetRequiredService<ChannelEventOptions>();

        eventOptions.UseChannels.ShouldBeTrue();
        channelOptions.ChannelCapacity.ShouldBe(500);
    }

    [Fact]
    public void UseChannelEventProcessing_ReturnsBuilder_ForChaining()
    {
        // Arrange
        ServiceCollection services = new();
        CrispOptionsBuilder builder = new(services);

        // Act
        CrispOptionsBuilder returnedBuilder = builder.UseChannelEventProcessing();

        // Assert
        returnedBuilder.ShouldBeSameAs(builder);
    }

    #endregion

    #region ConfigureMediator

    [Fact]
    public void ConfigureMediator_SetsMediatorOptions()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddCrispCore();
        CrispOptionsBuilder builder = new(services);

        // Act
        builder.ConfigureMediator(options =>
        {
            options.AllowMultipleHandlers = true;
            options.DefaultTimeoutSeconds = 120;
        });

        // Assert
        ServiceProvider provider = services.BuildServiceProvider();
        MediatorOptions mediatorOptions = provider.GetRequiredService<MediatorOptions>();
        mediatorOptions.ShouldNotBeNull();
        mediatorOptions.AllowMultipleHandlers.ShouldBeTrue();
        mediatorOptions.DefaultTimeoutSeconds.ShouldBe(120);
    }

    [Fact]
    public void ConfigureMediator_ReturnsBuilder_ForChaining()
    {
        // Arrange
        ServiceCollection services = new();
        CrispOptionsBuilder builder = new(services);

        // Act
        CrispOptionsBuilder returnedBuilder = builder.ConfigureMediator(options =>
        {
            options.DefaultTimeoutSeconds = 60;
        });

        // Assert
        returnedBuilder.ShouldBeSameAs(builder);
    }

    #endregion

    #region ConfigureValidation

    [Fact]
    public void ConfigureValidation_SetsValidationOptions()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddCrispCore();
        CrispOptionsBuilder builder = new(services);

        // Act
        builder.ConfigureValidation(options =>
        {
            options.SkipValidationIfNoValidatorsRegistered = true;
        });

        // Assert
        ServiceProvider provider = services.BuildServiceProvider();
        ValidationOptions validationOptions = provider.GetRequiredService<ValidationOptions>();
        validationOptions.ShouldNotBeNull();
        validationOptions.SkipValidationIfNoValidatorsRegistered.ShouldBeTrue();
    }

    [Fact]
    public void ConfigureValidation_ReturnsBuilder_ForChaining()
    {
        // Arrange
        ServiceCollection services = new();
        CrispOptionsBuilder builder = new(services);

        // Act
        CrispOptionsBuilder returnedBuilder = builder.ConfigureValidation(options =>
        {
            options.SkipValidationIfNoValidatorsRegistered = true;
        });

        // Assert
        returnedBuilder.ShouldBeSameAs(builder);
    }

    #endregion
}
