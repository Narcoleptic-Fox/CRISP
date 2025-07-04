using Crisp.Commands;
using Crisp.Dispatchers;
using Crisp.Pipeline;
using Crisp.State;
using Microsoft.AspNetCore.Components;

namespace Crisp.Services;

/// <summary>
/// High-performance precompiled command dispatcher for Blazor applications.
/// Uses expression trees compiled at startup for optimal performance.
/// </summary>
internal sealed class PreCompiledBlazorCommandDispatcher : PreCompiledCommandDispatcher, IBlazorCommandDispatcher
{
    private readonly ICrispState _state;
    private readonly NavigationManager _navigation;

    public PreCompiledBlazorCommandDispatcher(
        IServiceProvider serviceProvider,
        ICrispState state,
        NavigationManager navigation,
        IReadOnlyDictionary<Type, ICompiledPipeline> compiledPipelines)
        : base(compiledPipelines, serviceProvider)
    {
        _state = state;
        _navigation = navigation;
    }

    public async Task<TResponse> SendWithLoadingState<TResponse>(
        ICommand<TResponse> command,
        string? loadingKey = null,
        CancellationToken cancellationToken = default)
    {
        string key = loadingKey ?? command.GetType().Name;

        try
        {
            _state.SetLoading(key, true);
            return await Send(command, cancellationToken);
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

    public async Task SendWithLoadingState(
        ICommand command,
        string? loadingKey = null,
        CancellationToken cancellationToken = default)
    {
        string key = loadingKey ?? command.GetType().Name;

        try
        {
            _state.SetLoading(key, true);
            await Send(command, cancellationToken);
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