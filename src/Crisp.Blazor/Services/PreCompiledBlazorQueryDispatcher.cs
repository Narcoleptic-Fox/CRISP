using Crisp.Dispatchers;
using Crisp.Pipeline;
using Crisp.Queries;
using Crisp.State;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace Crisp.Services;

/// <summary>
/// High-performance precompiled query dispatcher for Blazor applications.
/// Includes caching support for optimal performance.
/// </summary>
internal sealed class PreCompiledBlazorQueryDispatcher : PreCompiledQueryDispatcher, IBlazorQueryDispatcher
{
    private readonly ICrispState _state;
    private readonly StateContainer _stateContainer;
    private readonly CrispBlazorOptions _options;

    // Query result cache
    private readonly IMemoryCache? _cache;

    // Track cache keys for invalidation
    private readonly ConcurrentDictionary<Type, HashSet<string>> _cacheKeysByType = new();

    public PreCompiledBlazorQueryDispatcher(
        IServiceProvider serviceProvider,
        ICrispState state,
        StateContainer stateContainer,
        IReadOnlyDictionary<Type, ICompiledPipeline> compiledPipelines,
        CrispBlazorOptions options,
        IMemoryCache? cache = null)
        : base(compiledPipelines, serviceProvider)
    {
        _state = state;
        _stateContainer = stateContainer;
        _options = options;
        _cache = cache;
    }

    public async Task<TResponse> SendWithLoadingState<TResponse>(
        IQuery<TResponse> query,
        string? loadingKey = null,
        CancellationToken cancellationToken = default)
    {
        string key = loadingKey ?? query.GetType().Name;

        try
        {
            _state.SetLoading(key, true);
            return await Send(query, cancellationToken);
        }
        catch (Exception ex)
        {
            _state.SetError(key, ex);
            throw;
        }
        finally
        {
            _state.SetLoading(key, false);
        }
    }
}