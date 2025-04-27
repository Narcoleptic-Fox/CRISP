using CRISP.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CRISP.Core.Modules;

/// <summary>
/// Base implementation for modules in the CRISP architecture.
/// </summary>
public abstract class ModuleBase : IModule
{
    /// <summary>
    /// Gets the name of the module.
    /// </summary>
    public abstract string ModuleName { get; }

    /// <summary>
    /// Registers the module's services with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    public abstract void RegisterServices(IServiceCollection services);
}