using Crisp.Commands;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Crisp.Endpoints;

/// <summary>
/// Represents an endpoint for a command.
/// Handles the procedural flow: HTTP Request → Command → Dispatch → Response
/// </summary>
internal sealed class CommandEndpoint<TCommand, TResponse> : CommandEndpointBase<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    public CommandEndpoint(string pattern, string httpMethod) : base(pattern, httpMethod)
    {
    }
    
    public CommandEndpoint() : base()
    {
    }

    protected override async Task<IResult> HandleCommand(
        TCommand command, 
        ICommandDispatcher dispatcher, 
        CancellationToken cancellationToken)
    {
        TResponse response = await dispatcher.Send(command, cancellationToken);

        // Return appropriate response based on HTTP method
        return HttpMethod switch
        {
            "POST" => Results.Created($"{Pattern}/{GetResourceId(response)}", response),
            "DELETE" => Results.NoContent(),
            _ => Results.Ok(response)
        };
    }

    protected override RouteHandlerBuilder ConfigureEndpoint(RouteHandlerBuilder builder)
    {
        return base.ConfigureEndpoint(builder)
            .Accepts<TCommand>("application/json");
    }

    private static object? GetResourceId(TResponse response) =>
        // Try to get Id property if it exists
        response?.GetType().GetProperty("Id")?.GetValue(response);
}

/// <summary>
/// Represents an endpoint for a void command (no response).
/// </summary>
internal sealed class VoidCommandEndpoint<TCommand> : VoidCommandEndpointBase<TCommand>
    where TCommand : ICommand
{
    public VoidCommandEndpoint(string pattern, string httpMethod) : base(pattern, httpMethod)
    {
    }
    
    public VoidCommandEndpoint() : base()
    {
    }

    protected override RouteHandlerBuilder ConfigureEndpoint(RouteHandlerBuilder builder)
    {
        return base.ConfigureEndpoint(builder)
            .Accepts<TCommand>("application/json");
    }
}