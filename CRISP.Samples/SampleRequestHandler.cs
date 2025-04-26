using CRISP.Requests;
using Microsoft.Extensions.Logging;

namespace CRISP.Samples;

public class SampleRequestHandler : IRequestHandler<SampleRequest, SampleResponse>
{
    private readonly ILogger<SampleRequestHandler> _logger;

    public SampleRequestHandler(ILogger<SampleRequestHandler> logger) => _logger = logger;

    public ValueTask<SampleResponse> Handle(SampleRequest request)
    {
        _logger.LogInformation("Handling sample request with query: {Query}", request.Query);

        // Process the request (in a real application, this might involve database operations, etc.)
        SampleResponse response = new()
        {
            Result = $"Processed: {request.Query} - {DateTime.Now}",
            IsSuccess = true
        };

        return new ValueTask<SampleResponse>(response);
    }
}