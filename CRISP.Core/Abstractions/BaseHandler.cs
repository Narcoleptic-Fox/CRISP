using CRISP.Core.Interfaces;
using CRISP.Core.Responses;
using Microsoft.Extensions.Logging;

namespace CRISP.Core.Abstractions;

/// <summary>
/// Base class for request handlers that return a typed response.
/// </summary>
/// <typeparam name="TRequest">The type of request being handled.</typeparam>
/// <typeparam name="TResponse">The type of response data returned.</typeparam>
public abstract class BaseHandler<TRequest, TResponse> : IRequestHandler<TRequest, Response<TResponse>> 
    where TRequest : IRequest<Response<TResponse>>
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseHandler{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    protected BaseHandler(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Handles the specified request.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A value task containing the response.</returns>
    public async ValueTask<Response<TResponse>> Handle(TRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Handling request {RequestType}", typeof(TRequest).Name);
            
            // Perform the actual handling
            var result = await Process(request, cancellationToken);
            
            _logger.LogInformation("Successfully handled request {RequestType}", typeof(TRequest).Name);
            
            return Response<TResponse>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling request {RequestType}: {ErrorMessage}", 
                typeof(TRequest).Name, ex.Message);
            
            return Response<TResponse>.Failure($"Error processing request: {ex.Message}");
        }
    }

    /// <summary>
    /// Processes the request to generate a response.
    /// </summary>
    /// <param name="request">The request to process.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response data.</returns>
    protected abstract ValueTask<TResponse> Process(TRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Base class for request handlers that do not return data.
/// </summary>
/// <typeparam name="TRequest">The type of request being handled.</typeparam>
public abstract class BaseHandler<TRequest> : IRequestHandler<TRequest, Response>
    where TRequest : IRequest<Response>
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseHandler{TRequest}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    protected BaseHandler(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Handles the specified request.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A value task containing the response.</returns>
    public async ValueTask<Response> Handle(TRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Handling request {RequestType}", typeof(TRequest).Name);
            
            // Perform the actual handling
            await Process(request, cancellationToken);
            
            _logger.LogInformation("Successfully handled request {RequestType}", typeof(TRequest).Name);
            
            return Response.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling request {RequestType}: {ErrorMessage}", 
                typeof(TRequest).Name, ex.Message);
            
            return Response.Failure($"Error processing request: {ex.Message}");
        }
    }

    /// <summary>
    /// Processes the request.
    /// </summary>
    /// <param name="request">The request to process.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A value task representing the operation.</returns>
    protected abstract ValueTask Process(TRequest request, CancellationToken cancellationToken);
}