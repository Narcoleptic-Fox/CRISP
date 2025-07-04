using Crisp.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Crisp.Endpoints;

/// <summary>
/// Base class for query endpoints.
/// Queries always use GET and bind from route/query parameters.
/// </summary>
internal abstract class QueryEndpointBase<TQuery, TResponse> : EndpointBase<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    public override string HttpMethod => "GET";

    protected QueryEndpointBase(string pattern) : base(pattern)
    {
    }

    protected QueryEndpointBase() : base()
    {
    }

    protected override RouteHandlerBuilder CreateRouteHandlerBuilder(IEndpointRouteBuilder app)
    {
        return app.MapGet(Pattern, HandleQuery);
    }

    protected override RouteHandlerBuilder ConfigureEndpoint(RouteHandlerBuilder builder)
    {
        return base.ConfigureEndpoint(builder)
            .Produces<TResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }

    /// <summary>
    /// Handles the query execution. Can be overridden for custom logic.
    /// </summary>
    protected virtual async Task<IResult> HandleQuery(
        HttpContext context, 
        IQueryDispatcher dispatcher, 
        CancellationToken cancellationToken)
    {
        try
        {
            // Step 1: Bind query from route and query string
            TQuery query = EndpointConventions.BindFromRouteAndQuery<TQuery>(context);

            // Step 2: Dispatch the query
            TResponse response = await dispatcher.Send(query, cancellationToken);

            // Step 3: Return the response
            return Results.Ok(response);
        }
        catch (BadHttpRequestException ex)
        {
            // Return BadRequest for binding/validation errors
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}