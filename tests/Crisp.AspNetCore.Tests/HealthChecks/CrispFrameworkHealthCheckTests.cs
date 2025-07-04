using Crisp.AspNetCore.HealthChecks;
using Crisp.Builder;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NSubstitute.ExceptionExtensions;

namespace Crisp.AspNetCore.Tests.HealthChecks;

public class CrispFrameworkHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_WithHandlersAndPipelines_ReturnsHealthy()
    {
        // Arrange
        ICrispBuilder crispBuilder = Substitute.For<ICrispBuilder>();
        crispBuilder.CommandHandlerCount.Returns(5);
        crispBuilder.QueryHandlerCount.Returns(10);
        crispBuilder.CompiledPipelineCount.Returns(15);

        CrispFrameworkHealthCheck healthCheck = new(crispBuilder);
        HealthCheckContext context = new();

        // Act
        HealthCheckResult result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Be("CRISP framework is running normally");
        result.Data.Should().ContainKey("CommandHandlers").WhoseValue.Should().Be(5);
        result.Data.Should().ContainKey("QueryHandlers").WhoseValue.Should().Be(10);
        result.Data.Should().ContainKey("CompiledPipelines").WhoseValue.Should().Be(15);
        result.Data.Should().ContainKey("FrameworkInitialized").WhoseValue.Should().Be(true);
    }

    [Fact]
    public async Task CheckHealthAsync_WithNoHandlers_ReturnsDegraded()
    {
        // Arrange
        ICrispBuilder crispBuilder = Substitute.For<ICrispBuilder>();
        crispBuilder.CommandHandlerCount.Returns(0);
        crispBuilder.QueryHandlerCount.Returns(0);
        crispBuilder.CompiledPipelineCount.Returns(0);

        CrispFrameworkHealthCheck healthCheck = new(crispBuilder);
        HealthCheckContext context = new();

        // Act
        HealthCheckResult result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Description.Should().Be("No command or query handlers are registered");
        result.Data.Should().ContainKey("CommandHandlers").WhoseValue.Should().Be(0);
        result.Data.Should().ContainKey("QueryHandlers").WhoseValue.Should().Be(0);
        result.Data.Should().ContainKey("CompiledPipelines").WhoseValue.Should().Be(0);
    }

    [Fact]
    public async Task CheckHealthAsync_WithHandlersButNoPipelines_ReturnsDegraded()
    {
        // Arrange
        ICrispBuilder crispBuilder = Substitute.For<ICrispBuilder>();
        crispBuilder.CommandHandlerCount.Returns(5);
        crispBuilder.QueryHandlerCount.Returns(5);
        crispBuilder.CompiledPipelineCount.Returns(0);

        CrispFrameworkHealthCheck healthCheck = new(crispBuilder);
        HealthCheckContext context = new();

        // Act
        HealthCheckResult result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Description.Should().Be("No pipelines have been compiled");
        result.Data.Should().ContainKey("CommandHandlers").WhoseValue.Should().Be(5);
        result.Data.Should().ContainKey("QueryHandlers").WhoseValue.Should().Be(5);
        result.Data.Should().ContainKey("CompiledPipelines").WhoseValue.Should().Be(0);
    }

    [Fact]
    public async Task CheckHealthAsync_WithException_ReturnsUnhealthy()
    {
        // Arrange
        ICrispBuilder crispBuilder = Substitute.For<ICrispBuilder>();
        crispBuilder.CommandHandlerCount.Returns(x => { throw new InvalidOperationException("Builder error"); });

        CrispFrameworkHealthCheck healthCheck = new(crispBuilder);
        HealthCheckContext context = new();

        // Act
        HealthCheckResult result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Be("CRISP framework health check failed");
        result.Exception.Should().BeOfType<InvalidOperationException>();
        result.Data.Should().ContainKey("FrameworkInitialized").WhoseValue.Should().Be(false);
        result.Data.Should().ContainKey("Error").WhoseValue.Should().Be("Builder error");
    }

    [Fact]
    public async Task CheckHealthAsync_WithOnlyCommands_ReturnsHealthy()
    {
        // Arrange
        ICrispBuilder crispBuilder = Substitute.For<ICrispBuilder>();
        crispBuilder.CommandHandlerCount.Returns(10);
        crispBuilder.QueryHandlerCount.Returns(0);
        crispBuilder.CompiledPipelineCount.Returns(10);

        CrispFrameworkHealthCheck healthCheck = new(crispBuilder);
        HealthCheckContext context = new();

        // Act
        HealthCheckResult result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Be("CRISP framework is running normally");
        result.Data.Should().ContainKey("CommandHandlers").WhoseValue.Should().Be(10);
        result.Data.Should().ContainKey("QueryHandlers").WhoseValue.Should().Be(0);
    }

    [Fact]
    public async Task CheckHealthAsync_WithOnlyQueries_ReturnsHealthy()
    {
        // Arrange
        ICrispBuilder crispBuilder = Substitute.For<ICrispBuilder>();
        crispBuilder.CommandHandlerCount.Returns(0);
        crispBuilder.QueryHandlerCount.Returns(8);
        crispBuilder.CompiledPipelineCount.Returns(8);

        CrispFrameworkHealthCheck healthCheck = new(crispBuilder);
        HealthCheckContext context = new();

        // Act
        HealthCheckResult result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Be("CRISP framework is running normally");
        result.Data.Should().ContainKey("CommandHandlers").WhoseValue.Should().Be(0);
        result.Data.Should().ContainKey("QueryHandlers").WhoseValue.Should().Be(8);
    }
}