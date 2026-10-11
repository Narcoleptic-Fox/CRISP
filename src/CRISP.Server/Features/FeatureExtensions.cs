using Asp.Versioning;
using CRISP.Server.Middleware;
using System.Reflection;

namespace CRISP.Server.Features;

/// <summary>
/// Provides discovery and registration helpers for feature modules.
/// </summary>
public static class FeatureExtensions
{
    /// <summary>
    /// Registers discovered feature modules with the application service collection.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="assemblies">Optional assemblies to scan for feature modules.</param>
    public static void AddFeatures(this IServiceCollection services, params Assembly[] assemblies)
    {
        if (!assemblies.Any())
            assemblies = [typeof(Program).Assembly];
        var features = RegisterModules(typeof(Program).Assembly);
        foreach (IFeature module in features)
        {
            module.AddFeature(services);
        }
    }

    /// <summary>
    /// Maps discovered feature modules under the versioned API route group.
    /// </summary>
    /// <param name="app">The web application to configure.</param>
    /// <param name="assemblies">Optional assemblies to scan for feature modules.</param>
    public static void MapFeatures(this WebApplication app, params Assembly[] assemblies)
    {
        if (!assemblies.Any())
            assemblies = [typeof(Program).Assembly];
        var features = RegisterModules(typeof(Program).Assembly);

        var versionBuilder = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        RouteGroupBuilder group = app.MapGroup("/api/v{version:apiVersion}")
            .WithValidation()
            .WithApiVersionSet(versionBuilder);

        foreach (IFeature module in features)
        {
            module.MapFeature(group);
        }
    }

    private static IEnumerable<IFeature> RegisterModules(params Assembly[] assemblies) =>
        assemblies.SelectMany(assembly => assembly.GetTypes())
                .Where(t => typeof(IFeature).IsAssignableFrom(t) && !t.IsAbstract)
                .Select(Activator.CreateInstance)
                .Cast<IFeature>()
                .ToList();
}
