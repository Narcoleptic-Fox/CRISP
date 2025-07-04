using Crisp.Common;
using Crisp.State;

namespace Crisp.Pipeline;

/// <summary>
/// Pipeline behavior that automatically manages loading state.
/// </summary>
/// <param name="state">The state manager to handle loading states.</param>
public class LoadingStateBehavior<TRequest, TResponse>(ICrispState state) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICrispState _state = state;

    /// <inheritdoc/>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        string key = request.GetType().Name;

        try
        {
            _state.SetLoading(key, true);
            return await next();
        }
        finally
        {
            _state.SetLoading(key, false);
        }
    }
}
