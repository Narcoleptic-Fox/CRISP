using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Reflection;

namespace CRISP.Client.Common;

public static class ModuleExtensions
{
    private static IEnumerable<IModule> _modules = [];

    /// <summary>
    /// Registers the modules for the application.
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder instance.</param>
    public static void AddModules<T>(this WebAssemblyHostBuilder builder)
    {
        _modules = RegisterModules(typeof(T).Assembly);
        foreach (IModule module in _modules)
        {
            module.AddModule(builder);
        }
    }

    private static IEnumerable<IModule> RegisterModules(Assembly assembly) =>
        assembly.GetTypes()
                .Where(t => typeof(IModule).IsAssignableFrom(t) && !t.IsAbstract)
                .Select(Activator.CreateInstance)
                .Cast<IModule>()
                .ToList();
}
