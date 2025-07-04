using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Crisp.AspNetCore.HealthChecks;

/// <summary>
/// Health check that verifies critical CRISP dependencies are available.
/// </summary>
public class CrispDependencyHealthCheck : IHealthCheck
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="CrispDependencyHealthCheck"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public CrispDependencyHealthCheck(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Checks the health of CRISP dependencies.
    /// </summary>
    /// <param name="context">Health check context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Health check result.</returns>
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>();
        var issues = new List<string>();

        try
        {
            // Check for command dispatcher
            var commandDispatcher = _serviceProvider.GetService<Crisp.Commands.ICommandDispatcher>();
            data["CommandDispatcherAvailable"] = commandDispatcher != null;
            if (commandDispatcher == null)
                issues.Add("Command dispatcher not available");

            // Check for query dispatcher
            var queryDispatcher = _serviceProvider.GetService<Crisp.Queries.IQueryDispatcher>();
            data["QueryDispatcherAvailable"] = queryDispatcher != null;
            if (queryDispatcher == null)
                issues.Add("Query dispatcher not available");

            // Check for CRISP options
            var options = _serviceProvider.GetService<CrispOptions>();
            data["OptionsAvailable"] = options != null;
            if (options == null)
                issues.Add("CRISP options not available");

            // Check optional services
            CheckOptionalService<Microsoft.Extensions.Caching.Memory.IMemoryCache>("MemoryCache", data);
            CheckOptionalService<Microsoft.Extensions.Caching.Distributed.IDistributedCache>("DistributedCache", data);

            if (issues.Any())
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    $"Critical CRISP dependencies missing: {string.Join(", ", issues)}",
                    data: data));
            }

            return Task.FromResult(HealthCheckResult.Healthy(
                "All critical CRISP dependencies are available",
                data: data));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "Failed to check CRISP dependencies",
                ex,
                data));
        }
    }

    private void CheckOptionalService<T>(string serviceName, Dictionary<string, object> data)
    {
        try
        {
            var service = _serviceProvider.GetService<T>();
            data[$"{serviceName}Available"] = service != null;
        }
        catch
        {
            data[$"{serviceName}Available"] = false;
        }
    }
}