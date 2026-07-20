namespace CRISP.Server.Features;

/// <summary>
/// Defines a feature module that can register services and map endpoints.
/// </summary>
public interface IFeature
{
    /// <summary>
    /// Registers feature-specific services with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The configured service collection.</returns>
    IServiceCollection AddFeature(IServiceCollection services);

    /// <summary>
    /// Maps feature-specific endpoints to the application route builder.
    /// </summary>
    /// <param name="app">The endpoint route builder to configure.</param>
    /// <returns>The endpoint route builder.</returns>
    IEndpointRouteBuilder MapFeature(IEndpointRouteBuilder app);
}
