using Crisp.Common;
using Crisp.Queries;
using Crisp.State;

namespace Crisp.Pipeline;

/// <summary>
/// Pipeline behavior that triggers UI updates after state changes.
/// Ensures components re-render when needed.
/// </summary>
public class StateUpdateBehavior<TRequest, TResponse>(StateContainer stateContainer) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : notnull // Added constraint to ensure TResponse is not nullable
{
    private readonly StateContainer _stateContainer = stateContainer;

    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        TResponse response = await next();

        // Store query results in state container for caching
        if (request is IQuery<TResponse>)
        {
            string cacheKey = $"{request.GetType().Name}:{request.GetHashCode()}";
            _stateContainer.SetState(cacheKey, response);
        }

        return response!;
    }
}
