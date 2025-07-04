using Crisp.Commands;
using Crisp.Queries;
using Crisp.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System.Collections.Concurrent;
using System.Reflection;

namespace Crisp.Endpoints;

/// <summary>
/// Maps commands and queries to HTTP endpoints following procedural flow.
/// Discovers all requests and creates appropriate endpoints automatically.
/// </summary>
public class EndpointMapper
{
    private readonly List<IEndpoint> _endpoints = [];
    private readonly HashSet<Type> _discoveredRequestTypes = [];
    private readonly CrispOptions _options;
    private static readonly HashSet<Type> _globalMappedTypes = [];
    private static readonly object _globalLock = new();

    public EndpointMapper(CrispOptions options) => _options = options;

    /// <summary>
    /// Gets all discovered endpoints.
    /// </summary>
    public IReadOnlyList<IEndpoint> GetEndpoints() => _endpoints.AsReadOnly();

    /// <summary>
    /// Discovers all commands and queries from the specified assemblies.
    /// </summary>
    public IEnumerable<IEndpoint> DiscoverEndpoints(params Assembly[] assemblies)
    {
        ConcurrentBag<IEndpoint> endpoints = [];
        object lockObj = new();
        
        Parallel.ForEach(assemblies, assembly =>
        {
            // Find all command and query types
            IEnumerable<Type> requestTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericTypeDefinition)
                .Where(t => RequestTypeDetector.IsCommand(t) || RequestTypeDetector.IsQuery(t));

            foreach (Type? requestType in requestTypes)
            {
                // Thread-safe check and add
                lock (lockObj)
                {
                    // Check if already discovered by this instance
                    if (_discoveredRequestTypes.Contains(requestType))
                        continue;
                    
                    _discoveredRequestTypes.Add(requestType);
                }
                    
                IEndpoint? endpoint = CreateEndpoint(requestType);
                if (endpoint != null)
                {
                    endpoints.Add(endpoint);
                }
            }
        });

        _endpoints.AddRange(endpoints);
        return endpoints;
    }

    /// <summary>
    /// Maps all discovered endpoints to the application.
    /// </summary>
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        // Create a single route group - don't apply prefix since patterns already include it
        RouteGroupBuilder apiGroup = app.MapGroup("")
            .WithOpenApi();

        if (_options.Endpoints.RequireAuthorization)
        {
            apiGroup.RequireAuthorization();
        }

        // Always ensure endpoints are discovered from all loaded assemblies if none were found yet
        // This handles test scenarios and ensures robust endpoint discovery
        if (_endpoints.Count == 0 && _options.Endpoints.AutoDiscoverEndpoints)
        {
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .ToArray();
            DiscoverEndpoints(allAssemblies);
        }

        // Map each endpoint that was discovered
        var mappedEndpoints = new HashSet<string>();
        foreach (IEndpoint endpoint in _endpoints)
        {
            // Create a unique key for the endpoint
            var endpointKey = $"{endpoint.HttpMethod}:{endpoint.Pattern}:{endpoint.RequestType.FullName}";
            
            // Skip if already mapped
            if (!mappedEndpoints.Add(endpointKey))
                continue;
                
            endpoint.Map(apiGroup);
            
            // Track globally to prevent duplicate registration across instances
            lock (_globalLock)
            {
                _globalMappedTypes.Add(endpoint.RequestType);
            }
        }
    }

    /// <summary>
    /// Creates an endpoint for the given request type.
    /// </summary>
    public IEndpoint? CreateEndpoint(Type requestType)
    {
        // Generate the route pattern for this request type
        string route = EndpointConventions.DetermineRoutePattern(requestType);

        // Check if it's a command
        Type? commandInterface = requestType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType &&
                           i.GetGenericTypeDefinition() == typeof(ICommand<>));

        if (commandInterface != null)
        {
            Type responseType = commandInterface.GetGenericArguments()[0];
            Type endpointType = typeof(CommandEndpoint<,>).MakeGenericType(requestType, responseType);
            return (IEndpoint?)Activator.CreateInstance(endpointType, route,
                EndpointConventions.DetermineHttpMethod(requestType));
        }

        // Check if it's a void command
        if (requestType.GetInterfaces().Any(i => i == typeof(ICommand)))
        {
            Type endpointType = typeof(VoidCommandEndpoint<>).MakeGenericType(requestType);
            return (IEndpoint?)Activator.CreateInstance(endpointType, route,
                EndpointConventions.DetermineHttpMethod(requestType));
        }

        // Check if it's a query
        Type? queryInterface = requestType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType &&
                           i.GetGenericTypeDefinition() == typeof(IQuery<>));

        if (queryInterface != null)
        {
            Type responseType = queryInterface.GetGenericArguments()[0];
            Type endpointType = typeof(QueryEndpoint<,>).MakeGenericType(requestType, responseType);
            return (IEndpoint?)Activator.CreateInstance(endpointType, route);
        }

        return null;
    }

    /// <summary>
    /// Creates an endpoint for the given request type and response type.
    /// </summary>
    public IEndpoint? CreateEndpoint(Type requestType, Type? responseType)
    {
        // Validate that the request type implements ICommand or IQuery
        bool isValidType = RequestTypeDetector.IsCommand(requestType) || RequestTypeDetector.IsQuery(requestType);
        if (!isValidType)
        {
            throw new InvalidOperationException($"Type {requestType.Name} does not implement ICommand or IQuery");
        }
        
        // For compatibility with existing tests - use single parameter version
        return CreateEndpoint(requestType);
    }

    /// <summary>
    /// Gets endpoint metadata for testing purposes.
    /// </summary>
    public TestEndpointMetadata GetEndpointMetadata(IEndpoint endpoint)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        
        return new TestEndpointMetadata
        {
            RequestType = endpoint.RequestType,
            ResponseType = endpoint.ResponseType,
            HttpMethod = endpoint.HttpMethod ?? "GET",
            RoutePattern = endpoint.Pattern
        };
    }

    /// <summary>
    /// Maps a command endpoint for testing purposes.
    /// </summary>
    public object MapCommandEndpoint<TCommand, TResponse>(IEndpointRouteBuilder app)
        where TCommand : ICommand<TResponse>
    {
        var endpoint = CreateEndpoint(typeof(TCommand));
        return endpoint?.Map(app.MapGroup("")) ?? throw new InvalidOperationException("Could not create command endpoint");
    }

    /// <summary>
    /// Maps a query endpoint for testing purposes.
    /// </summary>
    public object MapQueryEndpoint<TQuery, TResponse>(IEndpointRouteBuilder app)
        where TQuery : IQuery<TResponse>
    {
        var endpoint = CreateEndpoint(typeof(TQuery));
        return endpoint?.Map(app.MapGroup("")) ?? throw new InvalidOperationException("Could not create query endpoint");
    }

    /// <summary>
    /// Maps endpoints for testing purposes with the expected signature.
    /// </summary>
    public IEnumerable<object> MapEndpoints(IEndpointRouteBuilder app, IEnumerable<IEndpoint> endpoints)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(endpoints);
        
        var results = new List<object>();
        foreach (var endpoint in endpoints)
        {
            var result = endpoint.Map(app.MapGroup(""));
            results.Add(result);
        }
        return results;
    }
}