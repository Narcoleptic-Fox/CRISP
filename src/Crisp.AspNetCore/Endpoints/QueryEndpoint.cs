using Crisp.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Crisp.Endpoints;

/// <summary>
/// Represents an endpoint for a query.
/// Queries always use GET and bind from route/query parameters.
/// </summary>
internal sealed class QueryEndpoint<TQuery, TResponse> : QueryEndpointBase<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    public QueryEndpoint(string pattern) : base(pattern)
    {
    }
    
    public QueryEndpoint() : base()
    {
    }
}