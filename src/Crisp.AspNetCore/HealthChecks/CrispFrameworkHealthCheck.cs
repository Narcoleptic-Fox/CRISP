using Crisp.Builder;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Crisp.AspNetCore.HealthChecks;

/// <summary>
/// Health check for the CRISP framework itself.
/// Verifies that the framework is properly initialized and functioning.
/// </summary>
public class CrispFrameworkHealthCheck : IHealthCheck
{
    private readonly ICrispBuilder _crispBuilder;

    /// <summary>
    /// Initializes a new instance of the <see cref="CrispFrameworkHealthCheck"/> class.
    /// </summary>
    /// <param name="crispBuilder">The CRISP builder instance.</param>
    public CrispFrameworkHealthCheck(ICrispBuilder crispBuilder)
    {
        _crispBuilder = crispBuilder;
    }

    /// <summary>
    /// Checks the health of the CRISP framework.
    /// </summary>
    /// <param name="context">Health check context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Health check result.</returns>
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = new Dictionary<string, object>
            {
                ["CommandHandlers"] = _crispBuilder.CommandHandlerCount,
                ["QueryHandlers"] = _crispBuilder.QueryHandlerCount,
                ["CompiledPipelines"] = _crispBuilder.CompiledPipelineCount,
                ["FrameworkInitialized"] = true
            };

            // Check if we have any handlers registered
            if (_crispBuilder.CommandHandlerCount == 0 && _crispBuilder.QueryHandlerCount == 0)
            {
                return Task.FromResult(HealthCheckResult.Degraded(
                    "No command or query handlers are registered",
                    data: data));
            }

            // Check if pipelines are compiled
            if (_crispBuilder.CompiledPipelineCount == 0)
            {
                return Task.FromResult(HealthCheckResult.Degraded(
                    "No pipelines have been compiled",
                    data: data));
            }

            return Task.FromResult(HealthCheckResult.Healthy(
                "CRISP framework is running normally",
                data: data));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "CRISP framework health check failed",
                ex,
                new Dictionary<string, object>
                {
                    ["FrameworkInitialized"] = false,
                    ["Error"] = ex.Message
                }));
        }
    }
}