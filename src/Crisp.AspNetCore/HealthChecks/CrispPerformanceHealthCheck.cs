using Crisp.Commands;
using Crisp.Common;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Crisp.AspNetCore.HealthChecks;

/// <summary>
/// Health check that verifies CRISP framework performance characteristics.
/// Tests actual command/query execution to ensure the framework is responsive.
/// </summary>
public class CrispPerformanceHealthCheck : IHealthCheck
{
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly ILogger<CrispPerformanceHealthCheck> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CrispPerformanceHealthCheck"/> class.
    /// </summary>
    /// <param name="commandDispatcher">The command dispatcher.</param>
    /// <param name="logger">The logger.</param>
    public CrispPerformanceHealthCheck(
        ICommandDispatcher commandDispatcher,
        ILogger<CrispPerformanceHealthCheck> logger)
    {
        _commandDispatcher = commandDispatcher;
        _logger = logger;
    }

    /// <summary>
    /// Performs a performance health check.
    /// </summary>
    /// <param name="context">Health check context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Health check result.</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var data = new Dictionary<string, object>();

        try
        {
            // Execute a simple health check command
            var command = new HealthCheckCommand();
            
            await _commandDispatcher.Send(command, cancellationToken);
            
            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;
            
            data["ResponseTimeMs"] = elapsedMs;
            data["PerformanceTestPassed"] = true;

            // Determine health based on response time
            if (elapsedMs > 5000) // 5 seconds
            {
                return HealthCheckResult.Unhealthy(
                    $"CRISP performance is severely degraded. Response time: {elapsedMs}ms",
                    data: data);
            }
            
            if (elapsedMs > 1000) // 1 second
            {
                return HealthCheckResult.Degraded(
                    $"CRISP performance is degraded. Response time: {elapsedMs}ms",
                    data: data);
            }

            return HealthCheckResult.Healthy(
                $"CRISP performance is normal. Response time: {elapsedMs}ms",
                data: data);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            _logger.LogError(ex, "CRISP performance health check failed");
            
            return HealthCheckResult.Unhealthy(
                "CRISP performance health check failed",
                ex,
                new Dictionary<string, object>
                {
                    ["ResponseTimeMs"] = stopwatch.ElapsedMilliseconds,
                    ["PerformanceTestPassed"] = false,
                    ["Error"] = ex.Message
                });
        }
    }
}

/// <summary>
/// Simple command for health check testing.
/// </summary>
public record HealthCheckCommand : ICommand
{
}

/// <summary>
/// Handler for the health check command.
/// </summary>
public class HealthCheckCommandHandler : ICommandHandler<HealthCheckCommand>
{
    /// <summary>
    /// Handles the health check command.
    /// </summary>
    /// <param name="command">The health check command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A completed task.</returns>
    public Task Handle(HealthCheckCommand command, CancellationToken cancellationToken = default)
    {
        // Simple operation to test framework responsiveness
        return Task.CompletedTask;
    }
}