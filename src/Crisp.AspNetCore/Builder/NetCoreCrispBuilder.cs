using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Crisp.Builder;

/// <summary>
/// Builder class responsible for configuring and compiling the CRISP framework.
/// This class discovers handlers from assemblies, compiles optimized pipelines using expression trees,
/// and registers all necessary services for high-performance command/query execution.
/// </summary>
public class NetCoreCrispBuilder : CrispBuilderBase
{
    // Diagnostics
    /// <summary>
    /// Timer used to measure the compilation time of all pipelines.
    /// </summary>
    private readonly Stopwatch _compilationTimer = new();

    /// <summary>
    /// Initializes a new instance of the CrispBuilder with the specified service collection.
    /// </summary>
    /// <param name="services">The service collection to register dependencies with.</param>
    public NetCoreCrispBuilder(IServiceCollection services)
        : base(services)
    { }

    /// <summary>
    /// Gets the total compilation time in milliseconds for all pipelines.
    /// </summary>
    public long CompilationTime => _compilationTimer.ElapsedMilliseconds;

    /// <summary>
    /// Builds and configures the CRISP framework by discovering handlers, compiling pipelines,
    /// and registering all necessary services with the dependency injection container.
    /// This method should be called once during application startup.
    /// </summary>
    public override void Build()
    {
        _compilationTimer.Start();

        try
        {
            // Discover all command and query handlers from the registered assemblies
            DiscoverHandlers();

            // Register handlers with the DI container
            RegisterHandlers();

            // Register default pipeline behaviors
            RegisterDefaultPipelineBehaviors();

            // Compile optimized pipelines using expression trees
            CompilePipelines();

            // Register pre-compiled dispatchers
            RegisterDispatchers();

            _services.AddSingleton<ICrispBuilder>(this);
        }
        finally
        {
            _compilationTimer.Stop();
        }
    }


}