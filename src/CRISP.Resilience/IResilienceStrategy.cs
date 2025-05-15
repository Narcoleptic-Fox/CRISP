namespace CRISP.Resilience;

/// <summary>
/// Defines a resilience strategy for executing operations with resilience patterns.
/// </summary>
/// <remarks>
/// Resilience strategies provide a framework for executing operations in a way that can
/// handle transient failures and other exceptional conditions. These strategies follow
/// cloud service best practices and implement patterns such as:
/// <list type="bullet">
/// <item>Retry with exponential backoff for transient failures</item>
/// <item>Circuit breakers to prevent cascading failures</item>
/// <item>Timeout handling to prevent blocked operations</item>
/// <item>Bulkhead isolation to contain failures</item>
/// </list>
/// Implementations of this interface provide specific resilience behaviors that
/// can be composed together for comprehensive resilience capabilities.
/// </remarks>
public interface IResilienceStrategy
{
    /// <summary>
    /// Executes the operation with resilience patterns applied.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A value task containing the result of the operation.</returns>
    /// <remarks>
    /// This method wraps the provided operation with resilience patterns implemented by
    /// the specific strategy, such as retries, circuit breaking, or timeouts. The operation
    /// may be executed multiple times depending on the strategy's behavior (e.g., for retries).
    /// 
    /// The caller should ensure that the operation is idempotent when using strategies
    /// that may retry the operation.
    /// </remarks>
    ValueTask<T> Execute<T>(Func<CancellationToken, ValueTask<T>> operation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the operation with resilience patterns applied.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A value task representing the asynchronous operation.</returns>
    /// <remarks>
    /// This method is the non-returning variant of Execute, applying the same resilience
    /// patterns to operations that don't return a result. As with the typed version, the
    /// operation may be executed multiple times depending on the strategy's behavior.
    /// 
    /// The caller should ensure that the operation is idempotent when using strategies
    /// that may retry the operation.
    /// </remarks>
    ValueTask Execute(Func<CancellationToken, ValueTask> operation, CancellationToken cancellationToken = default);
}