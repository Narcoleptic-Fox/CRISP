using Crisp.Common;
using Microsoft.Extensions.Logging;

namespace Crisp.Pipeline;

/// <summary>
/// Lean pipeline behavior for basic error handling and logging.
/// </summary>
public class ErrorHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<ErrorHandlingBehavior<TRequest, TResponse>> _logger;

    public ErrorHandlingBehavior(ILogger<ErrorHandlingBehavior<TRequest, TResponse>> logger) => _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next(cancellationToken);
        }
        catch (Exception ex)
        {
            string requestName = typeof(TRequest).Name;
            string correlationId = Guid.NewGuid().ToString();

            _logger.LogError(ex, "Error handling {RequestName}. CorrelationId: {CorrelationId}",
                requestName, correlationId);

            // Re-throw to let HTTP layer handle with TypedResults
            throw;
        }
    }
}
