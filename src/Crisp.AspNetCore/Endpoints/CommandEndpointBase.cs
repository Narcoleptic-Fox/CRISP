using Crisp.Commands;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Crisp.Endpoints;

/// <summary>
/// Base class for command endpoints that return a response.
/// Commands use POST/PUT/DELETE and bind from request body.
/// </summary>
internal abstract class CommandEndpointBase<TCommand, TResponse> : EndpointBase<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    public override string HttpMethod { get; }

    protected CommandEndpointBase(string pattern, string httpMethod) : base(pattern)
    {
        ArgumentNullException.ThrowIfNull(httpMethod);
        HttpMethod = httpMethod;
    }

    protected CommandEndpointBase() : base()
    {
        HttpMethod = EndpointConventions.DetermineHttpMethod(typeof(TCommand));
    }

    protected override RouteHandlerBuilder CreateRouteHandlerBuilder(IEndpointRouteBuilder app)
    {
        return HttpMethod.ToUpperInvariant() switch
        {
            "POST" => app.MapPost(Pattern, HandlePostCommand),
            "PUT" => app.MapPut(Pattern, HandleBodyCommand),
            "DELETE" => app.MapDelete(Pattern, HandleRouteCommand),
            "PATCH" => app.MapPatch(Pattern, HandleBodyCommand),
            "GET" => app.MapGet(Pattern, HandleRouteCommand),
            _ => app.MapMethods(Pattern, [HttpMethod], HandleBodyCommand)
        };
    }

    protected override RouteHandlerBuilder ConfigureEndpoint(RouteHandlerBuilder builder)
    {
        return base.ConfigureEndpoint(builder)
            .Produces<TResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }

    /// <summary>
    /// Handles POST commands that bind from the request body.
    /// </summary>
    private async Task<IResult> HandlePostCommand(
        TCommand command, 
        ICommandDispatcher dispatcher, 
        CancellationToken cancellationToken)
    {
        return await HandleCommand(command, dispatcher, cancellationToken);
    }

    /// <summary>
    /// Handles commands that bind from the request body.
    /// </summary>
    private async Task<IResult> HandleBodyCommand(
        TCommand command, 
        ICommandDispatcher dispatcher, 
        CancellationToken cancellationToken)
    {
        return await HandleCommand(command, dispatcher, cancellationToken);
    }

    /// <summary>
    /// Handles commands that bind from route parameters.
    /// </summary>
    private async Task<IResult> HandleRouteCommand(
        HttpContext context, 
        ICommandDispatcher dispatcher, 
        CancellationToken cancellationToken)
    {
        TCommand command = await EndpointConventions.BindFromRoute<TCommand>(context);
        return await HandleCommand(command, dispatcher, cancellationToken);
    }

    /// <summary>
    /// Handles the command execution. Can be overridden for custom logic.
    /// </summary>
    protected virtual async Task<IResult> HandleCommand(
        TCommand command, 
        ICommandDispatcher dispatcher, 
        CancellationToken cancellationToken)
    {
        TResponse response = await dispatcher.Send(command, cancellationToken);
        return Results.Created($"{Pattern}", response);
    }
}

/// <summary>
/// Base class for void command endpoints that don't return a response.
/// </summary>
internal abstract class VoidCommandEndpointBase<TCommand> : EndpointBase<TCommand>
    where TCommand : ICommand
{
    public override string HttpMethod { get; }

    protected VoidCommandEndpointBase(string pattern, string httpMethod) : base(pattern)
    {
        ArgumentNullException.ThrowIfNull(httpMethod);
        HttpMethod = httpMethod;
    }

    protected VoidCommandEndpointBase() : base()
    {
        HttpMethod = EndpointConventions.DetermineHttpMethod(typeof(TCommand));
    }

    protected override RouteHandlerBuilder CreateRouteHandlerBuilder(IEndpointRouteBuilder app)
    {
        return HttpMethod.ToUpperInvariant() switch
        {
            "POST" => app.MapPost(Pattern, HandleVoidBodyCommand),
            "PUT" => app.MapPut(Pattern, HandleVoidBodyCommand),
            "DELETE" => app.MapDelete(Pattern, HandleVoidRouteCommand),
            "PATCH" => app.MapPatch(Pattern, HandleVoidBodyCommand),
            _ => app.MapMethods(Pattern, [HttpMethod], HandleVoidBodyCommand)
        };
    }

    protected override RouteHandlerBuilder ConfigureEndpoint(RouteHandlerBuilder builder)
    {
        return base.ConfigureEndpoint(builder)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }

    /// <summary>
    /// Handles void commands that bind from the request body.
    /// </summary>
    private async Task<IResult> HandleVoidBodyCommand(
        TCommand command, 
        ICommandDispatcher dispatcher, 
        CancellationToken cancellationToken)
    {
        return await HandleVoidCommand(command, dispatcher, cancellationToken);
    }

    /// <summary>
    /// Handles void commands that bind from route parameters.
    /// </summary>
    private async Task<IResult> HandleVoidRouteCommand(
        HttpContext context, 
        ICommandDispatcher dispatcher, 
        CancellationToken cancellationToken)
    {
        TCommand command = await EndpointConventions.BindFromRoute<TCommand>(context);
        return await HandleVoidCommand(command, dispatcher, cancellationToken);
    }

    /// <summary>
    /// Handles the void command execution. Can be overridden for custom logic.
    /// </summary>
    protected virtual async Task<IResult> HandleVoidCommand(
        TCommand command, 
        ICommandDispatcher dispatcher, 
        CancellationToken cancellationToken)
    {
        await dispatcher.Send(command, cancellationToken);
        return Results.NoContent();
    }
}