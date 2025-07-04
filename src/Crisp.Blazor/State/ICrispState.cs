namespace Crisp.State;

/// <summary>
/// Manages UI state for CRISP operations in Blazor.
/// </summary>
public interface ICrispState
{
    /// <summary>
    /// Gets whether a specific operation is loading.
    /// </summary>
    bool IsLoading(string key);

    /// <summary>
    /// Gets whether any operation is loading.
    /// </summary>
    bool IsAnyLoading { get; }

    /// <summary>
    /// Sets the loading state for an operation.
    /// </summary>
    void SetLoading(string key, bool isLoading);

    /// <summary>
    /// Gets the last error for a specific operation.
    /// </summary>
    Exception? GetError(string key);

    /// <summary>
    /// Sets an error for an operation.
    /// </summary>
    void SetError(string key, Exception? error);

    /// <summary>
    /// Clears all errors.
    /// </summary>
    void ClearErrors();

    /// <summary>
    /// Event raised when state changes.
    /// </summary>
    event EventHandler? StateChanged;
}