using Blazor.Concurrency.Background;
using Crisp.Commands;

namespace Crisp.Blazor.Concurrency.Commands;

/// <summary>
/// Command to execute a background task using web workers.
/// </summary>
public sealed record ExecuteBackgroundTaskCommand<T> : ICommand<BackgroundJobResult<T>>
{
    /// <summary>
    /// The task to execute in the background.
    /// </summary>
    public required ExecuteTaskRequest Task { get; init; }

    /// <summary>
    /// Optional progress callback for task updates.
    /// </summary>
    public Action<ParallelProgress>? OnProgress { get; init; }

    /// <summary>
    /// Cancellation token for the operation.
    /// </summary>
    public CancellationToken CancellationToken { get; init; } = default;
}
