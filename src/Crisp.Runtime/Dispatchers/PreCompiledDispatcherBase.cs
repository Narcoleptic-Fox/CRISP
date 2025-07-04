using Crisp.Pipeline;

namespace Crisp.Dispatchers;

/// <summary>
/// Abstract base class for pre-compiled dispatchers providing common pipeline execution logic.
/// Eliminates DRY violations between command and query dispatchers.
/// </summary>
internal abstract class PreCompiledDispatcherBase
{
    /// <summary>
    /// Dictionary containing compiled pipelines.
    /// Key: Request type, Value: Compiled pipeline with execution delegate.
    /// </summary>
    protected readonly IReadOnlyDictionary<Type, ICompiledPipeline> _pipelines;

    /// <summary>
    /// Service provider used for dependency resolution during pipeline execution.
    /// </summary>
    protected readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the PreCompiledDispatcherBase.
    /// </summary>
    /// <param name="pipelines">Pre-compiled pipelines for requests.</param>
    /// <param name="serviceProvider">Service provider for dependency resolution.</param>
    protected PreCompiledDispatcherBase(
        IReadOnlyDictionary<Type, ICompiledPipeline> pipelines,
        IServiceProvider serviceProvider)
    {
        _pipelines = pipelines ?? throw new ArgumentNullException(nameof(pipelines));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// Executes a request through the pre-compiled pipeline system.
    /// </summary>
    /// <typeparam name="TRequest">The type of request.</typeparam>
    /// <typeparam name="TResponse">The type of response.</typeparam>
    /// <param name="request">The request to execute.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The response from the pipeline execution.</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no handler is registered for the request type.</exception>
    protected async Task<TResponse> ExecutePipeline<TRequest, TResponse>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : notnull
    {
        
        Type requestType = request.GetType();

        if (!_pipelines.TryGetValue(requestType, out ICompiledPipeline? pipeline))
        {
            throw new InvalidOperationException(
                $"No handler registered for {GetRequestTypeName()} '{requestType.Name}'. " +
                $"Make sure the handler is registered and the assembly is scanned during startup.");
        }

        // Ensure this is a typed pipeline
        if (pipeline is not ICompiledPipeline<TResponse> typedPipeline)
        {
            throw GetTypeMismatchException(requestType, typeof(TResponse));
        }

        try
        {
            // Execute the pre-compiled pipeline
            return await typedPipeline.ExecuteAsync(request, _serviceProvider, cancellationToken);
        }
        catch (Exception ex) when (!(ex is ArgumentNullException || ex is InvalidOperationException || ex is OperationCanceledException))
        {
            // Wrap unexpected exceptions with more context
            throw new InvalidOperationException(
                $"Error executing {GetRequestTypeName()} '{requestType.Name}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Executes a void request through the pre-compiled pipeline system.
    /// </summary>
    /// <typeparam name="TRequest">The type of request.</typeparam>
    /// <param name="request">The request to execute.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no handler is registered for the request type.</exception>
    protected async Task ExecuteVoidPipeline<TRequest>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : notnull
    {
        
        Type requestType = request.GetType();

        if (!_pipelines.TryGetValue(requestType, out ICompiledPipeline? pipeline))
        {
            throw new InvalidOperationException(
                $"No handler registered for {GetRequestTypeName()} '{requestType.Name}'. " +
                $"Make sure the handler is registered and the assembly is scanned during startup.");
        }

        // Ensure this is a void pipeline
        if (pipeline is not ICompiledVoidPipeline voidPipeline)
        {
            throw new InvalidOperationException(
                $"{GetRequestTypeName()} '{requestType.Name}' is registered as a typed {GetRequestTypeName()} but was called as void. " +
                $"Use the generic Send<TResponse> method instead.");
        }

        try
        {
            // Execute the pre-compiled void pipeline
            await voidPipeline.ExecuteAsync(request, _serviceProvider, cancellationToken);
        }
        catch (Exception ex) when (!(ex is ArgumentNullException || ex is InvalidOperationException || ex is OperationCanceledException))
        {
            // Wrap unexpected exceptions with more context
            throw new InvalidOperationException(
                $"Error executing {GetRequestTypeName()} '{requestType.Name}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets the request type name for error messages (e.g., "command", "query").
    /// </summary>
    protected abstract string GetRequestTypeName();

    /// <summary>
    /// Gets the type mismatch exception for when a typed pipeline is expected but a void pipeline is found.
    /// </summary>
    protected abstract InvalidOperationException GetTypeMismatchException(Type requestType, Type responseType);
}