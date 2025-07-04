using Crisp.Commands;

namespace Crisp.Pipeline;

/// <summary>
/// Represents a compiled pipeline for commands and queries that return a response.
/// Contains pre-compiled delegate for ultra-fast execution without reflection.
/// </summary>
internal class CompiledPipeline<TResponse> : ICompiledPipeline<TResponse>
{
    /// <summary>
    /// Gets or sets the type of the request (command or query).
    /// </summary>
    public Type RequestType { get; init; } = default!;

    /// <summary>
    /// Gets or sets the type of the response returned by the handler.
    /// </summary>
    public Type ResponseType { get; init; } = default!;

    /// <summary>
    /// Gets or sets the type of the handler that processes this request.
    /// </summary>
    public Type HandlerType { get; init; } = default!;

    /// <summary>
    /// Gets or sets the compiled execution delegate for this pipeline.
    /// This delegate includes the full pipeline with all behaviors and the handler.
    /// </summary>
    public Func<object, IServiceProvider, CancellationToken, Task<TResponse>> Executor { get; init; } = default!;

    // Metadata for diagnostics and debugging

    /// <summary>
    /// Gets the simple name of the handler type for diagnostic purposes.
    /// </summary>
    public string HandlerName => HandlerType.Name;

    /// <summary>
    /// Gets the simple name of the request type for diagnostic purposes.
    /// </summary>
    public string RequestName => RequestType.Name;

    /// <summary>
    /// Gets a value indicating whether this pipeline is for a command (true) or query (false).
    /// Determined by checking if the request type implements ICommand&lt;&gt;.
    /// </summary>
    public bool IsCommand => RequestType.GetInterfaces().Any(i =>
        i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));

    /// <summary>
    /// Executes the compiled pipeline and returns the response.
    /// </summary>
    public Task<TResponse> ExecuteAsync(object request, IServiceProvider serviceProvider, CancellationToken cancellationToken) => Executor(request, serviceProvider, cancellationToken);
}

/// <summary>
/// Represents a compiled pipeline for void commands (commands that don't return a response).
/// Contains pre-compiled delegate for ultra-fast execution without reflection.
/// </summary>
internal class CompiledVoidPipeline : ICompiledVoidPipeline
{
    /// <summary>
    /// Gets or sets the type of the void command request.
    /// </summary>
    public Type RequestType { get; init; } = default!;

    /// <summary>
    /// Gets or sets the type of the response (always void).
    /// </summary>
    public Type ResponseType => typeof(void);

    /// <summary>
    /// Gets or sets the type of the handler that processes this void command.
    /// </summary>
    public Type HandlerType { get; init; } = default!;

    /// <summary>
    /// Gets or sets the compiled execution delegate for this void pipeline.
    /// This delegate includes the full pipeline with all behaviors and the handler.
    /// </summary>
    public Func<object, IServiceProvider, CancellationToken, Task> Executor { get; init; } = default!;

    /// <summary>
    /// Gets the simple name of the handler type for diagnostic purposes.
    /// </summary>
    public string HandlerName => HandlerType.Name;

    /// <summary>
    /// Gets the simple name of the request type for diagnostic purposes.
    /// </summary>
    public string RequestName => RequestType.Name;

    /// <summary>
    /// Gets a value indicating whether this pipeline is for a command (always true for void commands).
    /// </summary>
    public bool IsCommand => true;

    /// <summary>
    /// Executes the compiled void pipeline.
    /// </summary>
    public Task ExecuteAsync(object request, IServiceProvider serviceProvider, CancellationToken cancellationToken) => Executor(request, serviceProvider, cancellationToken);
}
