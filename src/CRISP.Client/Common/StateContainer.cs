using Microsoft.JSInterop;
using System.Text.Json;

namespace CRISP.Client.Common;

public class StateContainer<TState>
        where TState : IState, new()
{
    private readonly IJSRuntime _runtime;

    public StateContainer(IJSRuntime jS)
    {
        _runtime = jS;
    }

    public async ValueTask<TState> GetStateFromStorage()
    {
        string? container = await _runtime.InvokeAsync<string>(TState.LocalStorage ? "localStorage.getItem" : "sessionStorage.getItem", TState.StorageKey);
        return container is null ? new TState() : JsonSerializer.Deserialize<TState>(container) ?? new TState();
    }

    public async ValueTask SetStateToStorage(TState state)
    {
        await _runtime.InvokeVoidAsync(TState.LocalStorage ? "localStorage.setItem" : "sessionStorage.setItem", TState.StorageKey, JsonSerializer.Serialize(state));
    }
}
