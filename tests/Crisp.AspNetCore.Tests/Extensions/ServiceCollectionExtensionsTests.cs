using Crisp.Events;
using Crisp.Runtime.Events;
using System.Reflection;

namespace Crisp.AspNetCore.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddCrisp_ShouldRegisterRequiredServices()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddLogging();

        // Act
        services.AddCrisp(builder =>
        {
            builder.RegisterHandlersFromAssemblies(Assembly.GetExecutingAssembly());
        });

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        serviceProvider.GetService<IEventPublisher>().Should().NotBeNull();
        serviceProvider.GetService<IEventPublisher>().Should().BeOfType<ChannelEventPublisher>();
    }

    [Fact]
    public void AddCrisp_WithNullConfiguration_ShouldStillWork()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddLogging();
        // Act
        services.AddCrisp();

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        serviceProvider.GetService<IEventPublisher>().Should().NotBeNull();
    }

    [Fact]
    public void AddCrisp_ShouldConfigureOptions()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddCrisp(builder =>
        {
            builder.Options.Pipeline.EnableLogging = false;
            builder.Options.Pipeline.EnableErrorHandling = false;
        });

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        CrispOptions? options = serviceProvider.GetService<CrispOptions>();

        options.Should().NotBeNull();
        options!.Pipeline.EnableLogging.Should().BeFalse();
        options.Pipeline.EnableErrorHandling.Should().BeFalse();
    }

    [Fact]
    public void AddCrisp_WithOpenApiEnabled_ShouldConfigureSwagger()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddCrisp(builder =>
        {
            builder.Options.Endpoints.EnableOpenApi = true;
        });

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        serviceProvider.Should().NotBeNull();
    }
}
