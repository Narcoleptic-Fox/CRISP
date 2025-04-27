using Microsoft.Extensions.DependencyInjection;

namespace CRISP.Core.Interfaces;

/// <summary>
/// Defines a module in the CRISP architecture.
/// A module represents a vertical layer in the application.
/// </summary>
public interface IModule
{
    /// <summary>
    /// Registers the module's services with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    void RegisterServices(IServiceCollection services);
}