namespace Crisp;

/// <summary>
/// Configuration options for CRISP Blazor WASM integration.
/// </summary>
public class CrispBlazorOptions : CrispOptions
{
    /// <summary>
    /// Whether to automatically manage loading states. Default is true.
    /// </summary>
    public bool EnableLoadingState { get; set; } = true;

    /// <summary>
    /// Whether to automatically trigger UI updates. Default is true.
    /// </summary>
    public bool EnableStateUpdates { get; set; } = true;

    /// <summary>
    /// Whether to use error boundary behavior. Default is true.
    /// </summary>
    public bool EnableErrorBoundary { get; set; } = true;
}
