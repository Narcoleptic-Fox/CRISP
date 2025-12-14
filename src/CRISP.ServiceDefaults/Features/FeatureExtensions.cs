using CRISP.ServiceDefaults.Middlwares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CRISP.ServiceDefaults.Features;

public static class FeatureExtensions
{
    private static IEnumerable<IFeature> _features = [];

    /// <summary>
    /// Registers the modules for the application.
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder instance.</param>
    public static void AddFeatures<T>(this IServiceCollection services)
    {
        _features = RegisterModules(typeof(T).Assembly);
        foreach (IFeature module in _features)
        {
            module.AddFeature(services);
        }
    }

    public static void MapFeatures(this WebApplication app)
    {
        RouteGroupBuilder group = app.MapGroup("/api")
                                     .AddEndpointFilter<ValidationEndpointFilter>();
        foreach (IFeature module in _features)
        {
            module.MapFeature(group);
        }
    }

    private static IEnumerable<IFeature> RegisterModules(Assembly assembly) =>
        assembly.GetTypes()
                .Where(t => typeof(IFeature).IsAssignableFrom(t) && !t.IsAbstract)
                .Select(Activator.CreateInstance)
                .Cast<IFeature>()
                .ToList();
}
