using Crisp.Commands;
using Crisp.Common;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace Crisp.Pipeline;

/// <summary>
/// Lean pipeline behavior for logging command/query execution.
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;
        string requestType = IsCommand(typeof(TRequest)) ? "Command" : "Query";
        
        Stopwatch stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Processing {RequestType} {RequestName}", requestType, requestName);
            
            TResponse response = await next();
            stopwatch.Stop();
            
            _logger.LogInformation("{RequestType} {RequestName} completed in {ElapsedMs}ms", 
                requestType, requestName, stopwatch.ElapsedMilliseconds);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to process {RequestType} {RequestName} after {ElapsedMs}ms", 
                requestType, requestName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    private static bool IsCommand(Type requestType)
    {
        // Check if it implements ICommand (void command)
        if (requestType.IsAssignableTo(typeof(ICommand)))
            return true;

        // Check if it implements ICommand<T> (command with response)
        return requestType.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));
    }
}
