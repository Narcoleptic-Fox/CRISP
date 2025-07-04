namespace Crisp.Endpoints;

/// <summary>
/// Metadata about an endpoint for testing purposes.
/// </summary>
public class TestEndpointMetadata
{
    /// <summary>
    /// The request type handled by this endpoint.
    /// </summary>
    public Type RequestType { get; set; } = null!;

    /// <summary>
    /// The response type returned by this endpoint.
    /// </summary>
    public Type? ResponseType { get; set; }

    /// <summary>
    /// The HTTP method used by this endpoint.
    /// </summary>
    public string HttpMethod { get; set; } = "GET";

    /// <summary>
    /// The route pattern for this endpoint.
    /// </summary>
    public string RoutePattern { get; set; } = string.Empty;
}