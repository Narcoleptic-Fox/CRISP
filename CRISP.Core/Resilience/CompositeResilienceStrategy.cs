namespace CRISP.Core.Resilience;

/// <summary>
/// A resilience strategy that combines multiple strategies into a chain.
/// </summary>
public class CompositeResilienceStrategy : IResilienceStrategy
{
    private readonly IEnumerable<IResilienceStrategy> _strategies;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeResilienceStrategy"/> class.
    /// </summary>
    /// <param name="strategies">The resilience strategies to combine.</param>
    public CompositeResilienceStrategy(IEnumerable<IResilienceStrategy> strategies)
    {
        _strategies = strategies?.ToList() ?? throw new ArgumentNullException(nameof(strategies));

        if (!_strategies.Any())
        {
            throw new ArgumentException("At least one resilience strategy must be provided.", nameof(strategies));
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeResilienceStrategy"/> class.
    /// </summary>
    /// <param name="strategies">The resilience strategies to combine.</param>
    public CompositeResilienceStrategy(params IResilienceStrategy[] strategies)
        : this((IEnumerable<IResilienceStrategy>)strategies)
    {
    }

    /// <inheritdoc />
    public async ValueTask<T> Execute<T>(Func<CancellationToken, ValueTask<T>> operation, CancellationToken cancellationToken = default)
    {
        // Create a nested chain of strategy executions
        Func<CancellationToken, ValueTask<T>> compositeOperation = operation;

        // Apply each strategy in reverse order to create the execution chain
        foreach (IResilienceStrategy? strategy in _strategies.Reverse())
        {
            IResilienceStrategy currentStrategy = strategy;
            Func<CancellationToken, ValueTask<T>> currentOperation = compositeOperation;

            compositeOperation = async (ct) => await currentStrategy.Execute(currentOperation, ct);
        }

        // Execute the composite operation
        return await compositeOperation(cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask Execute(Func<CancellationToken, ValueTask> operation, CancellationToken cancellationToken = default)
    {
        // Create a nested chain of strategy executions
        Func<CancellationToken, ValueTask> compositeOperation = operation;

        // Apply each strategy in reverse order to create the execution chain
        foreach (IResilienceStrategy? strategy in _strategies.Reverse())
        {
            IResilienceStrategy currentStrategy = strategy;
            Func<CancellationToken, ValueTask> currentOperation = compositeOperation;

            compositeOperation = async (ct) => await currentStrategy.Execute(currentOperation, ct);
        }

        // Execute the composite operation
        await compositeOperation(cancellationToken);
    }
}