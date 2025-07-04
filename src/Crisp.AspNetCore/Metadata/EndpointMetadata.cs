using Crisp.Endpoints;
using System.Reflection;

namespace Crisp.Metadata;

/// <summary>
/// Metadata for an endpoint, including route pattern, HTTP method, and other configurations.
/// </summary>
public class EndpointMetadata
{
    /// <summary>
    /// The type of the request associated with the endpoint.
    /// </summary>
    public Type RequestType { get; init; } = null!;
    /// <summary>
    /// The type of the response associated with the endpoint, if any.
    /// </summary>
    public Type? ResponseType { get; init; }
    /// <summary>
    /// The route pattern that the endpoint matches.
    /// </summary>
    public string RoutePattern { get; init; } = null!;
    /// <summary>
    /// The HTTP method (e.g., GET, POST) that the endpoint supports.
    /// </summary>
    public string HttpMethod { get; init; } = null!;
    /// <summary>
    /// An optional tag used for grouping or categorizing the endpoint.
    /// </summary>
    public string? Tag { get; init; }
    /// <summary>
    /// A brief summary of the endpoint's purpose or behavior.
    /// </summary>
    public string? Summary { get; init; }
    /// <summary>
    /// A brief description of the endpoint's purpose or behavior.
    /// </summary>
    public string? Description { get; init; }
    /// <summary>
    /// The list of HTTP status codes that the endpoint can produce.
    /// </summary>
    public List<int> ProducesStatusCodes { get; init; } = [];

    /// <summary>
    /// Creates metadata from a request type using attributes or conventions.
    /// </summary>
    public static EndpointMetadata FromRequestType(Type requestType)
    {
        // Check for custom attributes
        RouteAttribute? routeAttr = requestType.GetCustomAttribute<RouteAttribute>();
        HttpMethodAttribute? httpMethodAttr = requestType.GetCustomAttribute<HttpMethodAttribute>();

        // Use attributes if present, otherwise fall back to conventions
        string pattern = routeAttr?.Pattern
            ?? EndpointConventions.DetermineRoutePattern(requestType);

        string httpMethod = httpMethodAttr?.Method
            ?? EndpointConventions.DetermineHttpMethod(requestType);

        return new EndpointMetadata
        {
            RequestType = requestType,
            RoutePattern = pattern,
            HttpMethod = httpMethod,
            Tag = EndpointConventions.ExtractTag(requestType),
            Summary = ExtractSummary(requestType),
            ProducesStatusCodes = DetermineStatusCodes(httpMethod)
        };
    }

    private static string? ExtractSummary(Type type) =>
        // Could read from XML docs or attributes
        null;

    private static List<int> DetermineStatusCodes(string httpMethod) => httpMethod switch
    {
        "GET" => [200, 404, 400],
        "POST" => [201, 400, 409],
        "PUT" => [200, 404, 400],
        "DELETE" => [204, 404, 400],
        "PATCH" => [200, 404, 400],
        _ => [200, 400]
    };
}