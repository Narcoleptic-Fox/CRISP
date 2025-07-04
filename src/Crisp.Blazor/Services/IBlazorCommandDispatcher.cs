using Crisp.Commands;

namespace Crisp.Services;

/// <summary>
/// Command dispatcher for Blazor applications with UI state management.
/// </summary>
public interface IBlazorCommandDispatcher : ICommandDispatcher
{
    /// <summary>
    /// Sends a command with loading state management.
    /// </summary>
    Task<TResponse> SendWithLoadingState<TResponse>(
        ICommand<TResponse> command,
        string? loadingKey = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a void command with loading state management.
    /// </summary>
    Task SendWithLoadingState(
        ICommand command,
        string? loadingKey = null,
        CancellationToken cancellationToken = default);
}