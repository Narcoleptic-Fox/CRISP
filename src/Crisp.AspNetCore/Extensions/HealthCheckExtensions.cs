using Crisp.AspNetCore.HealthChecks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Crisp.AspNetCore.Extensions;

/// <summary>
/// Extension methods for adding CRISP health checks.
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Adds CRISP framework health checks to the service collection.
    /// </summary>
    /// <param name="builder">The health checks builder.</param>
    /// <param name="name">The name of the health check. Default is "crisp".</param>
    /// <param name="failureStatus">The failure status for unhealthy checks.</param>
    /// <param name="tags">Tags for the health check.</param>
    /// <returns>The health checks builder for chaining.</returns>
    public static IHealthChecksBuilder AddCrispFramework(
        this IHealthChecksBuilder builder,
        string name = "crisp",
        HealthStatus? failureStatus = null,
        IEnumerable<string>? tags = null) => builder.AddCheck<CrispFrameworkHealthCheck>(
            name,
            failureStatus,
            tags);

    /// <summary>
    /// Adds CRISP dependency health checks to the service collection.
    /// </summary>
    /// <param name="builder">The health checks builder.</param>
    /// <param name="name">The name of the health check. Default is "crisp-dependencies".</param>
    /// <param name="failureStatus">The failure status for unhealthy checks.</param>
    /// <param name="tags">Tags for the health check.</param>
    /// <returns>The health checks builder for chaining.</returns>
    public static IHealthChecksBuilder AddCrispDependencies(
        this IHealthChecksBuilder builder,
        string name = "crisp-dependencies",
        HealthStatus? failureStatus = null,
        IEnumerable<string>? tags = null) => builder.AddCheck<CrispDependencyHealthCheck>(
            name,
            failureStatus,
            tags);

    /// <summary>
    /// Adds CRISP performance health checks to the service collection.
    /// </summary>
    /// <param name="builder">The health checks builder.</param>
    /// <param name="name">The name of the health check. Default is "crisp-performance".</param>
    /// <param name="failureStatus">The failure status for unhealthy checks.</param>
    /// <param name="tags">Tags for the health check.</param>
    /// <returns>The health checks builder for chaining.</returns>
    public static IHealthChecksBuilder AddCrispPerformance(
        this IHealthChecksBuilder builder,
        string name = "crisp-performance",
        HealthStatus? failureStatus = null,
        IEnumerable<string>? tags = null) => builder.AddCheck<CrispPerformanceHealthCheck>(
            name,
            failureStatus,
            tags);

    /// <summary>
    /// Adds all CRISP health checks to the service collection.
    /// </summary>
    /// <param name="builder">The health checks builder.</param>
    /// <param name="tags">Tags for all health checks.</param>
    /// <returns>The health checks builder for chaining.</returns>
    public static IHealthChecksBuilder AddCrispHealthChecks(
        this IHealthChecksBuilder builder,
        IEnumerable<string>? tags = null)
    {
        IEnumerable<string> crispTags = tags?.Concat(new[] { "crisp" }) ?? new[] { "crisp" };

        return builder
            .AddCrispFramework(tags: crispTags)
            .AddCrispDependencies(tags: crispTags)
            .AddCrispPerformance(tags: crispTags);
    }

    /// <summary>
    /// Adds CRISP health checks with database connectivity check.
    /// </summary>
    /// <param name="builder">The health checks builder.</param>
    /// <param name="connectionString">Database connection string.</param>
    /// <param name="name">The name of the database health check.</param>
    /// <param name="tags">Tags for all health checks.</param>
    /// <returns>The health checks builder for chaining.</returns>
    public static IHealthChecksBuilder AddCrispHealthChecksWithDatabase(
        this IHealthChecksBuilder builder,
        string connectionString,
        string name = "database",
        IEnumerable<string>? tags = null)
    {
        IEnumerable<string> allTags = tags?.Concat(new[] { "crisp", "database" }) ?? new[] { "crisp", "database" };

        return builder
            .AddCrispHealthChecks(allTags)
            /*.AddSqlServer(connectionString, name: name, tags: allTags)*/;
    }

    /// <summary>
    /// Adds health check endpoints to the application.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <param name="pattern">The health check endpoint pattern. Default is "/health".</param>
    /// <param name="detailedPattern">The detailed health check endpoint pattern. Default is "/health/detailed".</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication MapCrispHealthChecks(
        this WebApplication app,
        string pattern = "/health",
        string detailedPattern = "/health/detailed")
    {
        // Basic health check endpoint
        app.MapHealthChecks(pattern, new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var response = new
                {
                    status = report.Status.ToString(),
                    totalDuration = report.TotalDuration.TotalMilliseconds
                };
                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
            }
        });

        // Detailed health check endpoint
        app.MapHealthChecks(detailedPattern, new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var response = new
                {
                    status = report.Status.ToString(),
                    totalDuration = report.TotalDuration.TotalMilliseconds,
                    checks = report.Entries.Select(kvp => new
                    {
                        name = kvp.Key,
                        status = kvp.Value.Status.ToString(),
                        duration = kvp.Value.Duration.TotalMilliseconds,
                        description = kvp.Value.Description,
                        data = kvp.Value.Data,
                        exception = kvp.Value.Exception?.Message
                    })
                };
                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
            }
        });

        return app;
    }
}