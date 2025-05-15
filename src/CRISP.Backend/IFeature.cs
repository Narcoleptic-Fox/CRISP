using CRISP;
using Microsoft.AspNetCore.Routing;
using System.Reflection;

namespace CRISP
{
    /// <summary>
    /// Represents a cohesive set of related endpoints that can be mapped to an application's routing system.
    /// A feature serves as a boundary for a specific business capability or domain area in the application.
    /// </summary>
    /// <remarks>
    /// Features provide a higher level of organization than individual endpoints, allowing related
    /// endpoints to be grouped together and registered as a unit. This supports a modular architecture
    /// where each feature can be developed, tested, and maintained independently.
    ///
    /// Implementations should:
    /// - Register all related endpoints in the MapEndpoints method
    /// - Follow single responsibility principle by focusing on one business capability
    /// - Contain only endpoints related to the same domain concept
    /// </remarks>
    public interface IFeature : IModule
    {
        /// <summary>
        /// Maps all endpoints associated with this feature to the application's routing system.
        /// </summary>
        /// <param name="endpoints">The endpoint route builder to map endpoints to.</param>
        /// <remarks>
        /// This method is called during application startup to register all endpoints
        /// belonging to this feature. It should configure routes, HTTP methods, authorization
        /// requirements, and any other endpoint-specific settings.
        /// </remarks>
        void MapEndpoints(IEndpointRouteBuilder endpoints);
    }
}

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Provides extension methods for registering CRISP features with ASP.NET Core applications.
    /// </summary>
    public static class FeatureExtensions
    {
        /// <summary>
        /// Discovers and maps all CRISP endpoints from features found in the specified assemblies.
        /// </summary>
        /// <param name="app">The web application to map endpoints to.</param>
        /// <param name="assemblies">The assemblies to scan for CRISP features. If none are provided, the calling assembly is used.</param>
        /// <returns>The web application for method chaining.</returns>
        /// <remarks>
        /// This method uses reflection to:
        /// 1. Discover all classes that implement IFeature in the specified assemblies
        /// 2. Instantiate each feature using its parameterless constructor
        /// 3. Call MapEndpoints on each feature to register its endpoints
        ///
        /// This enables a clean, convention-based approach to endpoint registration that supports
        /// modular architecture and promotes separation of concerns.
        /// </remarks>
        public static WebApplication MapCrispEndpoints(this WebApplication app, params Assembly[] assemblies)
        {
            List<IFeature> features = FeatureDiscovery(assemblies);
            foreach (IFeature feature in features)
            {
                feature.MapEndpoints(app);
            }
            return app;
        }

        /// <summary>
        /// Discovers all implementations of IFeature in the specified assemblies.
        /// </summary>
        /// <param name="assemblies">The assemblies to scan for feature implementations.</param>
        /// <returns>A list of instantiated feature objects.</returns>
        private static List<IFeature> FeatureDiscovery(params Assembly[] assemblies) =>
            assemblies.SelectMany(a => a.GetTypes())
                .Where(t => typeof(IFeature).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                .Select(Activator.CreateInstance)
                .Cast<IFeature>()
                .ToList();
    }
}