using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring CRISP services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all CRISP components from the specified assemblies.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="assemblies">The assemblies to scan for CRISP components.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCrispFromAssemblies(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        services.AddCrispCore();
        services.AddRequestHandlers(assemblies);
        services.AddPipelineBehaviors(assemblies);
        services.AddValidators(assemblies);
        services.AddModules(assemblies);
        services.AddEventHandlers(assemblies);

        return services;
    }

    /// <summary>
    /// Adds all CRISP components from the specified assemblies with custom options.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configureOptions">Action to configure options.</param>
    /// <param name="assemblies">The assemblies to scan for CRISP components.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCrispFromAssemblies(
        this IServiceCollection services,
        Action<CrispOptionsBuilder> configureOptions,
        params Assembly[] assemblies)
    {
        services.AddCrispCore(configureOptions);
        services.AddRequestHandlers(assemblies);
        services.AddPipelineBehaviors(assemblies);
        services.AddValidators(assemblies);
        services.AddModules(assemblies);
        services.AddEventHandlers(assemblies);
        services.AddResilienceStrategies();

        return services;
    }
}