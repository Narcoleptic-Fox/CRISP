using CRISP.Requests;
using CRISP.Responses;
using Microsoft.Extensions.Logging;

namespace CRISP.Samples;

public class SampleGenericRequestService<TRequest, TResponse> : IRequestService<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IResponse
{
    private readonly ILogger<SampleGenericRequestService<TRequest, TResponse>> _logger;
    private readonly IRequestHandler<TRequest, TResponse> _requestHandler;

    public SampleGenericRequestService(
        ILogger<SampleGenericRequestService<TRequest, TResponse>> logger,
        IRequestHandler<TRequest, TResponse> requestHandler)
    {
        _logger = logger;
        _requestHandler = requestHandler;
    }

    public ValueTask<TResponse> Send(TRequest request)
    {
        _logger.LogInformation("Sending request of type: {RequestType}", typeof(TRequest).Name);
        return _requestHandler.Handle(request);
    }

    public void Dispose() =>
        // Cleanup any resources if needed
        _logger.LogInformation("Disposing {ServiceName}", GetType().Name);
}