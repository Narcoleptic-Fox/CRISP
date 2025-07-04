using Crisp.Pipeline;
using Crisp.Queries;

namespace Crisp.Dispatchers;

/// <summary>
/// High-performance query dispatcher that uses pre-compiled pipelines for ultra-fast execution.
/// This dispatcher avoids reflection by using compiled expression trees generated at startup.
/// Optimized specifically for read operations that return data.
/// </summary>
internal class PreCompiledQueryDispatcher : PreCompiledDispatcherBase, IQueryDispatcher
{
    /// <summary>
    /// Initializes a new instance of the PreCompiledQueryDispatcher.
    /// </summary>
    /// <param name="pipelines">Pre-compiled pipelines for queries.</param>
    /// <param name="serviceProvider">Service provider for dependency resolution.</param>
    internal PreCompiledQueryDispatcher(
        IReadOnlyDictionary<Type, ICompiledPipeline> pipelines,
        IServiceProvider serviceProvider)
        : base(pipelines, serviceProvider)
    {
    }

    /// <summary>
    /// Sends a query through the pre-compiled pipeline and returns the response.
    /// This method provides ultra-fast execution by avoiding reflection entirely.
    /// </summary>
    /// <typeparam name="TResponse">The type of response the query returns.</typeparam>
    /// <param name="query">The query to execute.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The query response.</returns>
    /// <exception cref="ArgumentNullException">Thrown when query is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no handler is registered for the query type.</exception>
    public async Task<TResponse> Send<TResponse>(
        IQuery<TResponse> query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        return await ExecutePipeline<IQuery<TResponse>, TResponse>(query, cancellationToken);
    }

    protected override string GetRequestTypeName() => "query";

    protected override InvalidOperationException GetTypeMismatchException(Type requestType, Type responseType)
    {
        return new InvalidOperationException(
            $"Query '{requestType.Name}' pipeline type mismatch. Expected response type '{responseType.Name}'.");
    }
}
