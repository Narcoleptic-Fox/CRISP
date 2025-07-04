using Crisp;
using Crisp.Commands;
using Crisp.Endpoints;
using Crisp.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Microsoft.AspNetCore.Routing;

/// <summary>
/// Extension methods for mapping CRISP endpoints.
/// </summary>
public static class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps all CRISP command and query endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapCrisp(
        this IEndpointRouteBuilder endpoints,
        Action<EndpointOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        EndpointMapper endpointMapper = endpoints.ServiceProvider.GetRequiredService<EndpointMapper>();
        CrispOptions options = endpoints.ServiceProvider.GetRequiredService<CrispOptions>();
        configure?.Invoke(options.Endpoints);

        if (options.Endpoints.AutoDiscoverEndpoints)
        {
            // Map all endpoints
            endpointMapper.MapEndpoints(endpoints);
        }

        return endpoints;
    }

    /// <summary>
    /// Maps CRISP endpoints from specific assemblies.
    /// </summary>
    public static IEndpointRouteBuilder MapCrispFromAssemblies(
        this IEndpointRouteBuilder endpoints,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentNullException.ThrowIfNull(assemblies);
        EndpointMapper endpointMapper = endpoints.ServiceProvider.GetRequiredService<EndpointMapper>();
        CrispOptions options = endpoints.ServiceProvider.GetRequiredService<CrispOptions>();

        // Discover endpoints from assemblies
        endpointMapper.DiscoverEndpoints(assemblies);

        // Create the API group
        RouteGroupBuilder group = endpoints.MapGroup(options.Endpoints.RoutePrefix);

        // Map all endpoints
        endpointMapper.MapEndpoints(group);

        return endpoints;
    }

    /// <summary>
    /// Maps a specific CRISP endpoint manually.
    /// </summary>
    public static RouteHandlerBuilder MapCrispEndpoint<TRequest>(
        this IEndpointRouteBuilder endpoints,
        string? pattern = null,
        string? httpMethod = null)
        where TRequest : class
    {
        IEndpoint endpoint = EndpointFactory.Create<TRequest>(pattern, httpMethod);
        return endpoint.Map(endpoints);
    }
}

/// <summary>
/// Factory for creating endpoints.
/// </summary>
internal static class EndpointFactory
{
    public static IEndpoint Create<TRequest>(string? pattern = null, string? httpMethod = null)
        where TRequest : class
    {
        Type requestType = typeof(TRequest);

        // Determine the endpoint type based on interfaces
        if (requestType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>)))
        {
            Type responseType = requestType.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>))
                .GetGenericArguments()[0];

            Type endpointType = typeof(CommandEndpoint<,>).MakeGenericType(requestType, responseType);
            
            // Use appropriate constructor based on parameters
            if (pattern != null && httpMethod != null)
                return (IEndpoint)Activator.CreateInstance(endpointType, pattern, httpMethod)!;
            else
                return (IEndpoint)Activator.CreateInstance(endpointType)!;
        }

        if (requestType.GetInterfaces().Any(i => i == typeof(ICommand)))
        {
            Type endpointType = typeof(VoidCommandEndpoint<>).MakeGenericType(requestType);
            
            // Use appropriate constructor based on parameters
            if (pattern != null && httpMethod != null)
                return (IEndpoint)Activator.CreateInstance(endpointType, pattern, httpMethod)!;
            else
                return (IEndpoint)Activator.CreateInstance(endpointType)!;
        }

        if (requestType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>)))
        {
            Type responseType = requestType.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>))
                .GetGenericArguments()[0];

            Type endpointType = typeof(QueryEndpoint<,>).MakeGenericType(requestType, responseType);
            
            // Use appropriate constructor based on parameters
            if (pattern != null)
                return (IEndpoint)Activator.CreateInstance(endpointType, pattern)!;
            else
                return (IEndpoint)Activator.CreateInstance(endpointType)!;
        }

        throw new InvalidOperationException($"Type {requestType.Name} is not a valid command or query");
    }

    /// <summary>
    /// Maps a specific CRISP endpoint for testing purposes.
    /// </summary>
    public static object MapCrispEndpoint<T>(this IEndpointRouteBuilder endpoints) where T : class
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        var endpointMapper = endpoints.ServiceProvider.GetRequiredService<EndpointMapper>();
        var endpoint = endpointMapper.CreateEndpoint(typeof(T));
        return endpoint?.Map(endpoints.MapGroup("")) ?? throw new InvalidOperationException($"Could not create endpoint for {typeof(T).Name}");
    }

    /// <summary>
    /// Maps a command endpoint for testing purposes.
    /// </summary>
    public static object MapCommand<TCommand, TResponse>(this IEndpointRouteBuilder endpoints) 
        where TCommand : ICommand<TResponse>
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        var endpointMapper = endpoints.ServiceProvider.GetRequiredService<EndpointMapper>();
        return endpointMapper.MapCommandEndpoint<TCommand, TResponse>(endpoints);
    }

    /// <summary>
    /// Maps a command endpoint with custom route for testing purposes.
    /// </summary>
    public static object MapCommand<TCommand, TResponse>(this IEndpointRouteBuilder endpoints, string route) 
        where TCommand : ICommand<TResponse>
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        var endpoint = new CommandEndpoint<TCommand, TResponse>(route, "POST");
        return endpoint.Map(endpoints.MapGroup(""));
    }

    /// <summary>
    /// Maps a query endpoint for testing purposes.
    /// </summary>
    public static object MapQuery<TQuery, TResponse>(this IEndpointRouteBuilder endpoints) 
        where TQuery : IQuery<TResponse>
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        var endpointMapper = endpoints.ServiceProvider.GetRequiredService<EndpointMapper>();
        return endpointMapper.MapQueryEndpoint<TQuery, TResponse>(endpoints);
    }

    /// <summary>
    /// Maps a query endpoint with custom route for testing purposes.
    /// </summary>
    public static object MapQuery<TQuery, TResponse>(this IEndpointRouteBuilder endpoints, string route) 
        where TQuery : IQuery<TResponse>
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        var endpoint = new QueryEndpoint<TQuery, TResponse>(route);
        return endpoint.Map(endpoints.MapGroup(""));
    }

    /// <summary>
    /// Maps a void command endpoint for testing purposes.
    /// </summary>
    public static object MapVoidCommand<TCommand>(this IEndpointRouteBuilder endpoints) 
        where TCommand : ICommand
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        var endpoint = new VoidCommandEndpoint<TCommand>("/test", "POST");
        return endpoint.Map(endpoints.MapGroup(""));
    }

    /// <summary>
    /// Maps a void command endpoint with custom route for testing purposes.
    /// </summary>
    public static object MapVoidCommand<TCommand>(this IEndpointRouteBuilder endpoints, string route) 
        where TCommand : ICommand
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        var endpoint = new VoidCommandEndpoint<TCommand>(route, "POST");
        return endpoint.Map(endpoints.MapGroup(""));
    }
}