using CRISP.Requests;
using CRISP.Responses;
using Microsoft.Extensions.Logging;

namespace CRISP.Samples;

/// <summary>
/// A generic request handler that can handle any request/response pair
/// </summary>
/// <typeparam name="TRequest">The type of request to handle</typeparam>
/// <typeparam name="TResponse">The type of response to return</typeparam>
public class GenericRequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IResponse, new()
{
    private readonly ILogger<GenericRequestHandler<TRequest, TResponse>> _logger;

    public GenericRequestHandler(ILogger<GenericRequestHandler<TRequest, TResponse>> logger) => _logger = logger;

    public ValueTask<TResponse> Handle(TRequest request)
    {
        _logger.LogInformation("Handling request of type {RequestType} with generic handler", typeof(TRequest).Name);
        Console.WriteLine($"Generic handler processed request of type {typeof(TRequest).Name}");

        // Create a new response instance
        TResponse response = new();

        return ValueTask.FromResult(response);
    }
}