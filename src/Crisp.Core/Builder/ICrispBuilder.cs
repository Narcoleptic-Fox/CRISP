using System.Reflection;

namespace Crisp.Builder;

/// <summary>
/// Core builder interface for CRISP configuration.
/// Transport-agnostic handler discovery and registration.
/// </summary>
public interface ICrispBuilder
{
    /// <summary>
    /// Gets the total number of discovered command handlers.
    /// </summary>
    int CommandHandlerCount { get; }

    /// <summary>
    /// Gets the total number of compiled pipelines.
    /// </summary>
    int CompiledPipelineCount { get; }

    /// <summary>
    /// Gets the total number of discovered query handlers.
    /// </summary>
    int QueryHandlerCount { get; }

    /// <summary>
    /// Gets the configuration options for CRISP.
    /// </summary>
    CrispOptions Options { get; }

    /// <summary>
    /// Configures the options for the CRISP framework.
    /// </summary>
    /// <param name="configureOptions">An action to configure the <see cref="CrispOptions"/>.</param>
    /// <returns>The current <see cref="ICrispBuilder"/> instance for method chaining.</returns>
    ICrispBuilder ConfigureOptions(Action<CrispOptions> configureOptions);

    /// <summary>
    /// Adds an assembly to scan for handlers.
    /// </summary>
    ICrispBuilder AddAssembly(Assembly assembly);

    /// <summary>
    /// Adds an assembly to scan for handlers.
    /// </summary>
    ICrispBuilder AddAssemblies(params Assembly[] assembly);

    /// <summary>
    /// Gets discovered handler mappings.
    /// </summary>
    IReadOnlyList<HandlerRegistration> GetHandlerMappings();

    /// <summary>
    /// Gets the assemblies registered for handler discovery.
    /// </summary>
    Assembly[] GetAssemblies();

    /// <summary>
    /// Registers all command and query handlers from the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly to scan for handlers.</param>
    /// <returns>The current CrispBuilder instance for method chaining.</returns>
    ICrispBuilder RegisterHandlersFromAssemblies(params Assembly[] assembly);

    /// <summary>
    /// Registers all command and query handlers from the assembly containing the specified type.
    /// </summary>
    /// <typeparam name="T">A type from the assembly to scan.</typeparam>
    /// <returns>The current CrispBuilder instance for method chaining.</returns>
    ICrispBuilder RegisterHandlersFromAssemblyContaining<T>() =>
        RegisterHandlersFromAssemblies(typeof(T).Assembly);

    /// <summary>
    /// Builds the configuration.
    /// </summary>
    void Build();
}
