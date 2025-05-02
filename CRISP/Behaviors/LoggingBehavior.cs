using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CRISP.Behaviors;

/// <summary>
/// Pipeline behavior that logs information about requests and responses.
/// </summary>
/// <typeparam name="TRequest">The type of request being handled.</typeparam>
/// <typeparam name="TResponse">The type of response from the handler.</typeparam>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggingBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger) => _logger = logger;

    /// <inheritdoc/>
    public async ValueTask<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;
        string requestGuid = Guid.NewGuid().ToString();

        _logger.LogInformation("[START] Request {RequestName} {RequestGuid}", requestName, requestGuid);

        Stopwatch sw = Stopwatch.StartNew();
        try
        {
            TResponse? response = await next(cancellationToken);
            sw.Stop();

            _logger.LogInformation(
                "[END] Request {RequestName} {RequestGuid} completed in {ElapsedMilliseconds}ms",
                requestName, requestGuid, sw.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(
                ex,
                "[ERROR] Request {RequestName} {RequestGuid} failed after {ElapsedMilliseconds}ms",
                requestName, requestGuid, sw.ElapsedMilliseconds);

            throw;
        }
    }
}