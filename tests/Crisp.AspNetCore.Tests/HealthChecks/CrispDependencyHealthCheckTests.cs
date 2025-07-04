using Crisp.AspNetCore.HealthChecks;
using Crisp.Commands;
using Crisp.Queries;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Crisp.AspNetCore.Tests.HealthChecks;

public class CrispDependencyHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_WithAllDependencies_ReturnsHealthy()
    {
        // Arrange
        IServiceProvider serviceProvider = Substitute.For<IServiceProvider>();
        ICommandDispatcher commandDispatcher = Substitute.For<ICommandDispatcher>();
        IQueryDispatcher queryDispatcher = Substitute.For<IQueryDispatcher>();
        CrispOptions options = new();
        IMemoryCache memoryCache = Substitute.For<IMemoryCache>();
        IDistributedCache distributedCache = Substitute.For<IDistributedCache>();

        serviceProvider.GetService<ICommandDispatcher>().Returns(commandDispatcher);
        serviceProvider.GetService<IQueryDispatcher>().Returns(queryDispatcher);
        serviceProvider.GetService<CrispOptions>().Returns(options);
        serviceProvider.GetService<IMemoryCache>().Returns(memoryCache);
        serviceProvider.GetService<IDistributedCache>().Returns(distributedCache);

        CrispDependencyHealthCheck healthCheck = new(serviceProvider);
        HealthCheckContext context = new();

        // Act
        HealthCheckResult result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Be("All critical CRISP dependencies are available");
        result.Data.Should().ContainKey("CommandDispatcherAvailable").WhoseValue.Should().Be(true);
        result.Data.Should().ContainKey("QueryDispatcherAvailable").WhoseValue.Should().Be(true);
        result.Data.Should().ContainKey("OptionsAvailable").WhoseValue.Should().Be(true);
        result.Data.Should().ContainKey("MemoryCacheAvailable").WhoseValue.Should().Be(true);
        result.Data.Should().ContainKey("DistributedCacheAvailable").WhoseValue.Should().Be(true);
    }

    [Fact]
    public async Task CheckHealthAsync_MissingCommandDispatcher_ReturnsUnhealthy()
    {
        // Arrange
        IServiceProvider serviceProvider = Substitute.For<IServiceProvider>();
        IQueryDispatcher queryDispatcher = Substitute.For<IQueryDispatcher>();
        CrispOptions options = new();

        serviceProvider.GetService<ICommandDispatcher>().Returns((ICommandDispatcher?)null);
        serviceProvider.GetService<IQueryDispatcher>().Returns(queryDispatcher);
        serviceProvider.GetService<CrispOptions>().Returns(options);

        CrispDependencyHealthCheck healthCheck = new(serviceProvider);
        HealthCheckContext context = new();

        // Act
        HealthCheckResult result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Contain("Command dispatcher not available");
        result.Data.Should().ContainKey("CommandDispatcherAvailable").WhoseValue.Should().Be(false);
        result.Data.Should().ContainKey("QueryDispatcherAvailable").WhoseValue.Should().Be(true);
        result.Data.Should().ContainKey("OptionsAvailable").WhoseValue.Should().Be(true);
    }

    [Fact]
    public async Task CheckHealthAsync_MissingQueryDispatcher_ReturnsUnhealthy()
    {
        // Arrange
        IServiceProvider serviceProvider = Substitute.For<IServiceProvider>();
        ICommandDispatcher commandDispatcher = Substitute.For<ICommandDispatcher>();
        CrispOptions options = new();

        serviceProvider.GetService<ICommandDispatcher>().Returns(commandDispatcher);
        serviceProvider.GetService<IQueryDispatcher>().Returns((IQueryDispatcher?)null);
        serviceProvider.GetService<CrispOptions>().Returns(options);

        CrispDependencyHealthCheck healthCheck = new(serviceProvider);
        HealthCheckContext context = new();

        // Act
        HealthCheckResult result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Contain("Query dispatcher not available");
        result.Data.Should().ContainKey("CommandDispatcherAvailable").WhoseValue.Should().Be(true);
        result.Data.Should().ContainKey("QueryDispatcherAvailable").WhoseValue.Should().Be(false);
        result.Data.Should().ContainKey("OptionsAvailable").WhoseValue.Should().Be(true);
    }

    [Fact]
    public async Task CheckHealthAsync_MissingOptions_ReturnsUnhealthy()
    {
        // Arrange
        IServiceProvider serviceProvider = Substitute.For<IServiceProvider>();
        ICommandDispatcher commandDispatcher = Substitute.For<ICommandDispatcher>();
        IQueryDispatcher queryDispatcher = Substitute.For<IQueryDispatcher>();

        serviceProvider.GetService<ICommandDispatcher>().Returns(commandDispatcher);
        serviceProvider.GetService<IQueryDispatcher>().Returns(queryDispatcher);
        serviceProvider.GetService<CrispOptions>().Returns((CrispOptions?)null);

        CrispDependencyHealthCheck healthCheck = new(serviceProvider);
        HealthCheckContext context = new();

        // Act
        HealthCheckResult result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Contain("CRISP options not available");
        result.Data.Should().ContainKey("CommandDispatcherAvailable").WhoseValue.Should().Be(true);
        result.Data.Should().ContainKey("QueryDispatcherAvailable").WhoseValue.Should().Be(true);
        result.Data.Should().ContainKey("OptionsAvailable").WhoseValue.Should().Be(false);
    }

    [Fact]
    public async Task CheckHealthAsync_MultipleMissingDependencies_ReturnsUnhealthy()
    {
        // Arrange
        IServiceProvider serviceProvider = Substitute.For<IServiceProvider>();

        serviceProvider.GetService<ICommandDispatcher>().Returns((ICommandDispatcher?)null);
        serviceProvider.GetService<IQueryDispatcher>().Returns((IQueryDispatcher?)null);
        serviceProvider.GetService<CrispOptions>().Returns((CrispOptions?)null);

        CrispDependencyHealthCheck healthCheck = new(serviceProvider);
        HealthCheckContext context = new();

        // Act
        HealthCheckResult result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Contain("Command dispatcher not available");
        result.Description.Should().Contain("Query dispatcher not available");
        result.Description.Should().Contain("CRISP options not available");
        result.Data.Should().ContainKey("CommandDispatcherAvailable").WhoseValue.Should().Be(false);
        result.Data.Should().ContainKey("QueryDispatcherAvailable").WhoseValue.Should().Be(false);
        result.Data.Should().ContainKey("OptionsAvailable").WhoseValue.Should().Be(false);
    }

    [Fact]
    public async Task CheckHealthAsync_WithOptionalServices_ReportsAvailability()
    {
        // Arrange
        IServiceProvider serviceProvider = Substitute.For<IServiceProvider>();
        ICommandDispatcher commandDispatcher = Substitute.For<ICommandDispatcher>();
        IQueryDispatcher queryDispatcher = Substitute.For<IQueryDispatcher>();
        CrispOptions options = new();
        IMemoryCache memoryCache = Substitute.For<IMemoryCache>();

        serviceProvider.GetService<ICommandDispatcher>().Returns(commandDispatcher);
        serviceProvider.GetService<IQueryDispatcher>().Returns(queryDispatcher);
        serviceProvider.GetService<CrispOptions>().Returns(options);
        serviceProvider.GetService<IMemoryCache>().Returns(memoryCache);
        serviceProvider.GetService<IDistributedCache>().Returns((IDistributedCache?)null);

        CrispDependencyHealthCheck healthCheck = new(serviceProvider);
        HealthCheckContext context = new();

        // Act
        HealthCheckResult result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Data.Should().ContainKey("MemoryCacheAvailable").WhoseValue.Should().Be(true);
        result.Data.Should().ContainKey("DistributedCacheAvailable").WhoseValue.Should().Be(false);
    }

    [Fact]
    public async Task CheckHealthAsync_WithException_ReturnsUnhealthy()
    {
        // Arrange
        IServiceProvider serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(ICommandDispatcher)).Returns(x => { throw new InvalidOperationException("Service resolution failed"); });

        CrispDependencyHealthCheck healthCheck = new(serviceProvider);
        HealthCheckContext context = new();

        // Act
        HealthCheckResult result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Be("Failed to check CRISP dependencies");
        result.Exception.Should().BeOfType<InvalidOperationException>();
    }

    [Fact]
    public async Task CheckHealthAsync_OptionalServiceThrows_HandlesGracefully()
    {
        // Arrange
        IServiceProvider serviceProvider = Substitute.For<IServiceProvider>();
        ICommandDispatcher commandDispatcher = Substitute.For<ICommandDispatcher>();
        IQueryDispatcher queryDispatcher = Substitute.For<IQueryDispatcher>();
        CrispOptions options = new();

        serviceProvider.GetService<ICommandDispatcher>().Returns(commandDispatcher);
        serviceProvider.GetService<IQueryDispatcher>().Returns(queryDispatcher);
        serviceProvider.GetService<CrispOptions>().Returns(options);
        serviceProvider.GetService(typeof(IMemoryCache)).Returns(x => { throw new InvalidOperationException("Cache not available"); });

        CrispDependencyHealthCheck healthCheck = new(serviceProvider);
        HealthCheckContext context = new();

        // Act
        HealthCheckResult result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy); // Should still be healthy as cache is optional
        result.Data.Should().ContainKey("MemoryCacheAvailable").WhoseValue.Should().Be(false);
    }
}