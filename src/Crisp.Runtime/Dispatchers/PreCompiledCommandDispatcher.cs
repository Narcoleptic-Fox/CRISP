using Crisp.Commands;
using Crisp.Pipeline;

namespace Crisp.Dispatchers;

/// <summary>
/// High-performance command dispatcher that uses pre-compiled pipelines for ultra-fast execution.
/// This dispatcher avoids reflection by using compiled expression trees generated at startup.
/// Supports both commands that return responses and void commands with a unified storage approach.
/// </summary>
internal class PreCompiledCommandDispatcher : PreCompiledDispatcherBase, ICommandDispatcher
{
    /// <summary>
    /// Initializes a new instance of the PreCompiledCommandDispatcher.
    /// </summary>
    /// <param name="pipelines">Pre-compiled pipelines for all commands (typed and void).</param>
    /// <param name="serviceProvider">Service provider for dependency resolution.</param>
    internal PreCompiledCommandDispatcher(
        IReadOnlyDictionary<Type, ICompiledPipeline> pipelines,
        IServiceProvider serviceProvider)
        : base(pipelines, serviceProvider)
    {
    }

    /// <inheritdoc/>
    public async Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        return await ExecutePipeline<ICommand<TResponse>, TResponse>(command, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task Send(ICommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        await ExecuteVoidPipeline(command, cancellationToken);
    }

    protected override string GetRequestTypeName() => "command";

    protected override InvalidOperationException GetTypeMismatchException(Type requestType, Type responseType)
    {
        return new InvalidOperationException(
            $"Command '{requestType.Name}' is registered as a void command but was called with a response type '{responseType.Name}'. " +
            $"Use the void Send method instead.");
    }
}
