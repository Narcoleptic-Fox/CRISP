namespace Crisp.Commands;

/// <summary>
/// Dispatches commands to their respective handlers.
/// </summary>
public interface ICommandDispatcher
{    /// <summary>
     /// Dispatches a command to its handler and returns a response.
     /// </summary>
     /// <typeparam name="TResponse">The type of the response.</typeparam>
     /// <param name="command">The command to dispatch.</param>
     /// <param name="cancellationToken">Cancellation token</param>
     /// <returns>The response from the command handler.</returns>
    Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dispatches a command to its handler with no response.
    /// </summary>
    /// <param name="command">The command to dispatch.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task Send(ICommand command, CancellationToken cancellationToken = default);
}
