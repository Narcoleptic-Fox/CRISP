using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Crisp.Endpoints;

/// <summary>
/// Defines an endpoint that maps a request to an HTTP route.
/// Following the procedural flow: Request → Route → Handler → Response
/// </summary>
public interface IEndpoint
{
    /// <summary>
    /// The HTTP route pattern (e.g., "/api/todos/{id}").
    /// </summary>
    string Pattern { get; }

    /// <summary>
    /// The HTTP method (GET, POST, PUT, DELETE, etc.).
    /// </summary>
    string HttpMethod { get; }

    /// <summary>
    /// The request type this endpoint handles.
    /// </summary>
    Type RequestType { get; }

    /// <summary>
    /// The response type this endpoint returns (null for void commands).
    /// </summary>
    Type? ResponseType { get; }

    /// <summary>
    /// Maps the endpoint to the route builder.
    /// </summary>
    RouteHandlerBuilder Map(IEndpointRouteBuilder app);
}