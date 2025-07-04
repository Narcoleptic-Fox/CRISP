using System.Collections.Concurrent;

namespace Crisp.State;

/// <summary>
/// Default implementation of CRISP state management.
/// Thread-safe for Blazor Server scenarios.
/// </summary>
public class CrispState : ICrispState
{
    private readonly ConcurrentDictionary<string, bool> _loadingStates = new();
    private readonly ConcurrentDictionary<string, Exception> _errors = new();

    /// <inheritdoc/>
    public event EventHandler? StateChanged;

    /// <inheritdoc/>
    public bool IsLoading(string key) => _loadingStates.GetValueOrDefault(key);

    /// <inheritdoc/>
    public bool IsAnyLoading => _loadingStates.Any(kvp => kvp.Value);

    /// <inheritdoc/>
    public void SetLoading(string key, bool isLoading)
    {
        if (isLoading)
            _loadingStates[key] = true;
        else
            _loadingStates.TryRemove(key, out _);

        OnStateChanged();
    }

    /// <inheritdoc/>
    public Exception? GetError(string key) =>
        _errors.TryGetValue(key, out Exception? error) ? error : null;

    /// <inheritdoc/>
    public void SetError(string key, Exception? error)
    {
        if (error != null)
            _errors[key] = error;
        else
            _errors.TryRemove(key, out _);

        OnStateChanged();
    }

    /// <inheritdoc/>
    public void ClearErrors()
    {
        _errors.Clear();
        OnStateChanged();
    }

    /// <summary>
    /// Invokes the StateChanged event to notify subscribers of state changes.
    /// </summary>
    protected virtual void OnStateChanged() =>
        StateChanged?.Invoke(this, EventArgs.Empty);
}