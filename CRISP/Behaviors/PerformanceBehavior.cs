using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CRISP.Behaviors;

/// <summary>
/// Pipeline behavior that monitors and logs performance metrics for requests.
/// </summary>
/// <typeparam name="TRequest">The type of request being handled.</typeparam>
/// <typeparam name="TResponse">The type of response from the handler.</typeparam>
public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private readonly Stopwatch _timer;
    private readonly long _thresholdMs;

    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="thresholdMs">The threshold in milliseconds after which to log a warning (default: 500ms).</param>
    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger, long thresholdMs = 500)
    {
        _logger = logger;
        _timer = new Stopwatch();
        _thresholdMs = thresholdMs;
    }

    /// <inheritdoc/>
    public async ValueTask<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;

        _timer.Start();

        try
        {
            TResponse? response = await next(cancellationToken);
            return response;
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        finally
        {
            _timer.Stop();

            long elapsedMs = _timer.ElapsedMilliseconds;

            // Log detailed timing information
            if (elapsedMs > _thresholdMs)
            {
                _logger.LogWarning(
                    "Long running request: {RequestName} ({ElapsedMilliseconds}ms) {@Request}",
                    requestName, elapsedMs, request);
            }
            else
            {
                _logger.LogDebug(
                    "Request performance: {RequestName} ({ElapsedMilliseconds}ms)",
                    requestName, elapsedMs);
            }
        }
    }
}