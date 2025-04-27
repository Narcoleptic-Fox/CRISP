namespace CRISP.Core.Resilience;

/// <summary>
/// Defines a resilience strategy for executing operations with resilience patterns.
/// </summary>
public interface IResilienceStrategy
{
    /// <summary>
    /// Executes the operation with resilience patterns applied.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A value task containing the result of the operation.</returns>
    ValueTask<T> Execute<T>(Func<CancellationToken, ValueTask<T>> operation, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Executes the operation with resilience patterns applied.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A value task representing the asynchronous operation.</returns>
    ValueTask Execute(Func<CancellationToken, ValueTask> operation, CancellationToken cancellationToken = default);
}