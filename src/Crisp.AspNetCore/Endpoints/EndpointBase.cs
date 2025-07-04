using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Crisp.Endpoints;

/// <summary>
/// Abstract base class for all endpoints providing common functionality.
/// Implements Template Method pattern for endpoint mapping.
/// </summary>
internal abstract class EndpointBase<TRequest, TResponse> : IEndpoint
{
    public string Pattern { get; }
    public abstract string HttpMethod { get; }
    public Type RequestType => typeof(TRequest);
    public Type? ResponseType => typeof(TResponse);

    protected EndpointBase(string pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        Pattern = pattern;
    }

    protected EndpointBase()
    {
        Pattern = EndpointConventions.DetermineRoutePattern(typeof(TRequest));
    }

    public RouteHandlerBuilder Map(IEndpointRouteBuilder app)
    {
        // Template method - delegate to specific implementation
        var builder = CreateRouteHandlerBuilder(app);
        
        // Apply common endpoint configuration
        return ConfigureEndpoint(builder);
    }

    /// <summary>
    /// Creates the route handler builder for the specific HTTP method.
    /// This is implemented by derived classes.
    /// </summary>
    protected abstract RouteHandlerBuilder CreateRouteHandlerBuilder(IEndpointRouteBuilder app);

    /// <summary>
    /// Configures the endpoint with common settings like OpenAPI, tags, etc.
    /// </summary>
    protected virtual RouteHandlerBuilder ConfigureEndpoint(RouteHandlerBuilder builder)
    {
        return builder
            .WithName(GenerateEndpointName())
            .WithSummary(EndpointConventions.GetSummary(typeof(TRequest)))
            .WithOpenApi()
            .WithTags(EndpointConventions.ExtractTag(typeof(TRequest)));
    }

    /// <summary>
    /// Generates a unique endpoint name.
    /// </summary>
    protected virtual string GenerateEndpointName()
    {
        return $"{HttpMethod}_{typeof(TRequest).Name}_{Guid.NewGuid().ToString("N")[..8]}";
    }
}

/// <summary>
/// Base class for endpoints that don't return a response (void commands).
/// </summary>
internal abstract class EndpointBase<TRequest> : IEndpoint
{
    public string Pattern { get; }
    public abstract string HttpMethod { get; }
    public Type RequestType => typeof(TRequest);
    public Type? ResponseType => null;

    protected EndpointBase(string pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        Pattern = pattern;
    }

    protected EndpointBase()
    {
        Pattern = EndpointConventions.DetermineRoutePattern(typeof(TRequest));
    }

    public RouteHandlerBuilder Map(IEndpointRouteBuilder app)
    {
        // Template method - delegate to specific implementation
        var builder = CreateRouteHandlerBuilder(app);
        
        // Apply common endpoint configuration
        return ConfigureEndpoint(builder);
    }

    /// <summary>
    /// Creates the route handler builder for the specific HTTP method.
    /// This is implemented by derived classes.
    /// </summary>
    protected abstract RouteHandlerBuilder CreateRouteHandlerBuilder(IEndpointRouteBuilder app);

    /// <summary>
    /// Configures the endpoint with common settings like OpenAPI, tags, etc.
    /// </summary>
    protected virtual RouteHandlerBuilder ConfigureEndpoint(RouteHandlerBuilder builder)
    {
        return builder
            .WithName(GenerateEndpointName())
            .WithSummary(EndpointConventions.GetSummary(typeof(TRequest)))
            .WithOpenApi()
            .WithTags(EndpointConventions.ExtractTag(typeof(TRequest)));
    }

    /// <summary>
    /// Generates a unique endpoint name.
    /// </summary>
    protected virtual string GenerateEndpointName()
    {
        return $"{HttpMethod}_{typeof(TRequest).Name}_{Guid.NewGuid().ToString("N")[..8]}";
    }
}