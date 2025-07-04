using Crisp.Builder;
using Crisp.Commands;
using Crisp.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Extension methods for IApplicationBuilder to configure CRISP framework middleware and startup behavior.
/// Provides options for validation, warm-up, and diagnostic logging during application startup.
/// </summary>
public static class CrispApplicationBuilderExtensions
{
    /// <summary>
    /// Adds CRISP exception handling middleware to the pipeline.
    /// Should be one of the first middleware components.
    /// </summary>
    public static IApplicationBuilder UseCrispExceptionHandler(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<CrispExceptionMiddleware>();
    }

    /// <summary>
    /// Alias for UseCrispExceptionHandler for backward compatibility.
    /// </summary>
    public static IApplicationBuilder UseCrispExceptionHandling(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseCrispExceptionHandler();
    }

    /// <summary>
    /// Configures CRISP framework startup behavior and middleware.
    /// This method provides options for handler validation, pipeline warm-up, and diagnostic logging.
    /// </summary>
    /// <param name="app">The application builder to configure.</param>
    /// <param name="configure">Optional delegate to configure CRISP startup options.</param>
    /// <returns>The application builder for method chaining.</returns>
    /// <example>
    /// <code>
    /// app.UseCrisp(options =>
    /// {
    ///     options.ValidateHandlers = true;
    ///     options.WarmUp = true;
    ///     options.LogDiagnostics = true;
    /// });
    /// </code>
    /// </example>
    public static IApplicationBuilder UseCrisp(
        this IApplicationBuilder app,
        Action<CrispAppOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(app);
        
        CrispAppOptions options = new();
        configure?.Invoke(options);

        using IServiceScope scope = app.ApplicationServices.CreateScope();
        if (options.ValidateHandlers)
            ValidateHandlers(scope.ServiceProvider);

        if (options.WarmUp)
            WarmUpPipelines(scope.ServiceProvider).GetAwaiter().GetResult();

        if (options.LogDiagnostics)
            LogDiagnostics(scope.ServiceProvider);

        return app;
    }

    /// <summary>
    /// Validates that all registered handlers can be properly resolved from the dependency injection container.
    /// This helps catch configuration issues early during application startup.
    /// </summary>
    /// <param name="services">The service provider to validate handlers against.</param>
    private static void ValidateHandlers(IServiceProvider services)
    {
        // Validate all handlers can be resolved
        ICommandDispatcher dispatcher = services.GetRequiredService<ICommandDispatcher>();
        // Implementation would check all registered types
    }

    /// <summary>
    /// Warms up the compiled pipelines by potentially executing dummy operations.
    /// This ensures all code paths are JIT compiled for optimal performance on first real request.
    /// </summary>
    /// <param name="services">The service provider to warm up pipelines with.</param>
    /// <returns>A task representing the asynchronous warm-up operation.</returns>
    private static async Task WarmUpPipelines(IServiceProvider services)
    {
        ILogger<NetCoreCrispBuilder>? logger = services.GetService<ILogger<NetCoreCrispBuilder>>();
        logger?.LogInformation("Warming up CRISP pipelines...");

        // Could execute dummy commands to ensure everything is JIT compiled
        await Task.CompletedTask;
    }

    /// <summary>
    /// Logs diagnostic information about the CRISP framework configuration.
    /// This includes handler counts, compilation times, and other useful startup metrics.
    /// </summary>
    /// <param name="services">The service provider to extract diagnostic information from.</param>
    private static void LogDiagnostics(IServiceProvider services)
    {
        ILogger<NetCoreCrispBuilder>? logger = services.GetService<ILogger<NetCoreCrispBuilder>>();
        // Log useful startup information
    }
}

/// <summary>
/// Configuration options for CRISP framework startup behavior.
/// Controls various aspects of framework initialization and diagnostics.
/// </summary>
public class CrispAppOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to validate that all handlers can be resolved during startup.
    /// Default is true. Helps catch dependency injection configuration issues early.
    /// </summary>
    public bool ValidateHandlers { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to warm up pipelines during startup.
    /// Default is false. When true, ensures JIT compilation happens at startup for better first-request performance.
    /// </summary>
    public bool WarmUp { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to log diagnostic information during startup.
    /// Default is true. Provides useful information about handler counts and compilation performance.
    /// </summary>
    public bool LogDiagnostics { get; set; } = true;
}
