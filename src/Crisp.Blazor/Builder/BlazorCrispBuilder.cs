using Crisp.Commands;
using Crisp.Queries;
using Crisp.Services;
using Crisp.State;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Crisp.Builder;

/// <summary>
/// Builder specifically for Blazor applications with precompilation support.
/// </summary>
public class BlazorCrispBuilder : CrispBuilderBase
{
    /// <inheritdoc/>
    public new CrispBlazorOptions Options { get; protected set; } = new();

    /// <inheritdoc/>
    public ICrispBuilder ConfigureOptions(Action<CrispBlazorOptions> configureOptions)
    {
        configureOptions(Options);
        return this;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlazorCrispBuilder"/> class.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    public BlazorCrispBuilder(IServiceCollection services)
        : base(services)
    { }

    /// <summary>
    /// Builds and registers Blazor-specific services with precompiled pipelines.
    /// </summary>
    public override void Build()
    {
        // Discover all command and query handlers from the registered assemblies
        DiscoverHandlers();

        // Register handlers with the DI container
        RegisterHandlers();

        // Register default pipeline behaviors
        RegisterDefaultPipelineBehaviors();

        // Compile optimized pipelines using expression trees
        CompilePipelines();

        // Register Blazor-specific services
        _services.AddScoped<ICrispState, CrispState>();
        _services.AddScoped<StateContainer>();

        // Register precompiled Blazor dispatchers
        _services.AddScoped<IBlazorCommandDispatcher>(provider =>
        {
            ICrispState state = provider.GetRequiredService<ICrispState>();
            NavigationManager navigation = provider.GetRequiredService<NavigationManager>();

            return new PreCompiledBlazorCommandDispatcher(
                provider,
                state,
                navigation,
                _compiledPipelines);
        });

        _services.AddScoped<IBlazorQueryDispatcher>(provider =>
        {
            ICrispState state = provider.GetRequiredService<ICrispState>();
            StateContainer stateContainer = provider.GetRequiredService<StateContainer>();
            return new PreCompiledBlazorQueryDispatcher(
                provider,
                state,
                stateContainer,
                _compiledPipelines,
                Options);
        });

        // Also register as base interfaces
        _services.AddScoped<ICommandDispatcher>(provider =>
            provider.GetRequiredService<IBlazorCommandDispatcher>());
        _services.AddScoped<IQueryDispatcher>(provider =>
            provider.GetRequiredService<IBlazorQueryDispatcher>());
    }
}
