namespace Crisp.State;

/// <summary>
/// Container for managing shared state between components.
/// Useful for caching query results and sharing data.
/// </summary>
public class StateContainer
{
    private readonly Dictionary<string, object> _state = [];

    /// <summary>
    /// Event triggered when the state changes.
    /// </summary>
    public event EventHandler<StateChangedEventArgs>? StateChanged;

    /// <summary>
    /// Sets a value in the state container.
    /// </summary>
    public void SetState<T>(string key, T value) where T : notnull
    {
        _state[key] = value;
        OnStateChanged(key, value);
    }

    /// <summary>
    /// Gets a value from the state container.
    /// </summary>
    public T? GetState<T>(string key) where T : class => _state.TryGetValue(key, out object? value) ? value as T : null;

    /// <summary>
    /// Removes a value from the state container.
    /// </summary>
    public bool RemoveState(string key)
    {
        bool removed = _state.Remove(key);
        if (removed)
            OnStateChanged(key, null);
        return removed;
    }

    /// <summary>
    /// Clears all state.
    /// </summary>
    public void Clear()
    {
        _state.Clear();
        OnStateChanged("*", null);
    }

    /// <summary>
    /// Triggers the <see cref="StateChanged"/> event when the state changes.
    /// </summary>
    /// <param name="key">The key of the state that changed.</param>
    /// <param name="value">The new value of the state, or null if the state was removed.</param>
    protected virtual void OnStateChanged(string key, object? value) =>
        StateChanged?.Invoke(this, new StateChangedEventArgs(key, value));
}

/// <summary>
/// Event args for state changes.
/// </summary>
public class StateChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the key of the state that changed.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets the new value of the state, or null if the state was removed.
    /// </summary>
    public object? Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StateChangedEventArgs"/> class.
    /// </summary>
    /// <param name="key">The key of the state that changed.</param>
    /// <param name="value">The new value of the state, or null if the state was removed.</param>
    public StateChangedEventArgs(string key, object? value) =>
        (Key, Value) = (key, value);
}
