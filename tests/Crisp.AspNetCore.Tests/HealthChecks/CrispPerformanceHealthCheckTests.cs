using Crisp.AspNetCore.HealthChecks;
using Crisp.Commands;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NSubstitute.ExceptionExtensions;

namespace Crisp.AspNetCore.Tests.HealthChecks;

public class CrispPerformanceHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_FastResponse_ReturnsHealthy()
    {
        // Arrange
        ICommandDispatcher commandDispatcher = Substitute.For<ICommandDispatcher>();
        ILogger<CrispPerformanceHealthCheck> logger = Substitute.For<ILogger<CrispPerformanceHealthCheck>>();

        commandDispatcher.Send(Arg.Any<HealthCheckCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        CrispPerformanceHealthCheck healthCheck = new(commandDispatcher, logger);
        HealthCheckContext context = new();

        // Act
        HealthCheckResult result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Contain("CRISP performance is normal");
        result.Data.Should().ContainKey("ResponseTimeMs");
        result.Data.Should().ContainKey("PerformanceTestPassed").WhoseValue.Should().Be(true);

        long responseTime = (long)result.Data["ResponseTimeMs"];
        responseTime.Should().BeLessThan(1000); // Should be fast
    }

    [Fact]
    public async Task CheckHealthAsync_SlowResponse_ReturnsDegraded()
    {
        // Arrange
        ICommandDispatcher commandDispatcher = Substitute.For<ICommandDispatcher>();
        ILogger<CrispPerformanceHealthCheck> logger = Substitute.For<ILogger<CrispPerformanceHealthCheck>>();

        commandDispatcher.Send(Arg.Any<HealthCheckCommand>(), Arg.Any<CancellationToken>())
            .Returns(async _ =>
            {
                await Task.Delay(1500); // 1.5 seconds - should be degraded
            });

        CrispPerformanceHealthCheck healthCheck = new(commandDispatcher, logger);
        HealthCheckContext context = new();

        // Act
        HealthCheckResult result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Description.Should().Contain("CRISP performance is degraded");
        result.Data.Should().ContainKey("ResponseTimeMs");
        result.Data.Should().ContainKey("PerformanceTestPassed").WhoseValue.Should().Be(true);

        long responseTime = (long)result.Data["ResponseTimeMs"];
        responseTime.Should().BeGreaterThan(1000);
        responseTime.Should().BeLessThan(5000);
    }

    [Fact]
    public async Task CheckHealthAsync_VerySlowResponse_ReturnsUnhealthy()
    {
        // Arrange
        ICommandDispatcher commandDispatcher = Substitute.For<ICommandDispatcher>();
        ILogger<CrispPerformanceHealthCheck> logger = Substitute.For<ILogger<CrispPerformanceHealthCheck>>();

        commandDispatcher.Send(Arg.Any<HealthCheckCommand>(), Arg.Any<CancellationToken>())
            .Returns(async _ =>
            {
                await Task.Delay(6000); // 6 seconds - should be unhealthy
            });

        CrispPerformanceHealthCheck healthCheck = new(commandDispatcher, logger);
        HealthCheckContext context = new();

        // Act
        HealthCheckResult result = await healthCheck.CheckHealthAsync(context, CancellationToken.None);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Contain("CRISP performance is severely degraded");
        result.Data.Should().ContainKey("ResponseTimeMs");
        result.Data.Should().ContainKey("PerformanceTestPassed").WhoseValue.Should().Be(true);

        long responseTime = (long)result.Data["ResponseTimeMs"];
        responseTime.Should().BeGreaterThan(5000);
    }

    [Fact]
    public async Task CheckHealthAsync_CommandThrowsException_ReturnsUnhealthy()
    {
        // Arrange
        ICommandDispatcher commandDispatcher = Substitute.For<ICommandDispatcher>();
        ILogger<CrispPerformanceHealthCheck> logger = Substitute.For<ILogger<CrispPerformanceHealthCheck>>();

        commandDispatcher.Send(Arg.Any<HealthCheckCommand>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Command failed"));

        CrispPerformanceHealthCheck healthCheck = new(commandDispatcher, logger);
        HealthCheckContext context = new();

        // Act
        HealthCheckResult result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Be("CRISP performance health check failed");
        result.Exception.Should().BeOfType<InvalidOperationException>();
        result.Data.Should().ContainKey("ResponseTimeMs");
        result.Data.Should().ContainKey("PerformanceTestPassed").WhoseValue.Should().Be(false);
        result.Data.Should().ContainKey("Error").WhoseValue.Should().Be("Command failed");
    }

    [Fact]
    public async Task CheckHealthAsync_CancellationRequested_HandlesCancellation()
    {
        // Arrange
        ICommandDispatcher commandDispatcher = Substitute.For<ICommandDispatcher>();
        ILogger<CrispPerformanceHealthCheck> logger = Substitute.For<ILogger<CrispPerformanceHealthCheck>>();

        commandDispatcher.Send(Arg.Any<HealthCheckCommand>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException("Operation was cancelled"));

        CrispPerformanceHealthCheck healthCheck = new(commandDispatcher, logger);
        HealthCheckContext context = new();

        // Act
        HealthCheckResult result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Exception.Should().BeOfType<OperationCanceledException>();
        result.Data.Should().ContainKey("PerformanceTestPassed").WhoseValue.Should().Be(false);
    }

    [Fact]
    public async Task CheckHealthAsync_MultipleCallsInSequence_AllComplete()
    {
        // Arrange
        ICommandDispatcher commandDispatcher = Substitute.For<ICommandDispatcher>();
        ILogger<CrispPerformanceHealthCheck> logger = Substitute.For<ILogger<CrispPerformanceHealthCheck>>();

        commandDispatcher.Send(Arg.Any<HealthCheckCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        CrispPerformanceHealthCheck healthCheck = new(commandDispatcher, logger);
        HealthCheckContext context = new();

        // Act
        HealthCheckResult result1 = await healthCheck.CheckHealthAsync(context);
        HealthCheckResult result2 = await healthCheck.CheckHealthAsync(context);
        HealthCheckResult result3 = await healthCheck.CheckHealthAsync(context);

        // Assert
        result1.Status.Should().Be(HealthStatus.Healthy);
        result2.Status.Should().Be(HealthStatus.Healthy);
        result3.Status.Should().Be(HealthStatus.Healthy);

        await commandDispatcher.Received(3).Send(Arg.Any<HealthCheckCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CheckHealthAsync_VerifiesCorrectCommandType()
    {
        // Arrange
        ICommandDispatcher commandDispatcher = Substitute.For<ICommandDispatcher>();
        ILogger<CrispPerformanceHealthCheck> logger = Substitute.For<ILogger<CrispPerformanceHealthCheck>>();

        commandDispatcher.Send(Arg.Any<HealthCheckCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        CrispPerformanceHealthCheck healthCheck = new(commandDispatcher, logger);
        HealthCheckContext context = new();

        // Act
        await healthCheck.CheckHealthAsync(context);

        // Assert
        await commandDispatcher.Received(1).Send(
            Arg.Is<HealthCheckCommand>(cmd => cmd != null),
            Arg.Any<CancellationToken>());
    }
}