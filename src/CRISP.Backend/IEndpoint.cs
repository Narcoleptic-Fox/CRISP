using Microsoft.AspNetCore.Routing;

namespace CRISP
{
    /// <summary>
    /// Defines an interface for mapping endpoints to a route group builder.
    /// This is the foundation interface for all CRISP endpoints, providing a standard
    /// mechanism to register routes with the ASP.NET Core routing system.
    /// </summary>
    public interface IEndpoint
    {
        /// <summary>
        /// Maps endpoints to the specified <see cref="RouteGroupBuilder"/>.
        /// Implementing classes should define their routing configuration within this method,
        /// including HTTP methods, route patterns, and handler methods.
        /// </summary>
        /// <param name="builder">The route group builder to which endpoints will be mapped.</param>
        /// <remarks>
        /// This static abstract method follows the .NET 7+ pattern for interfaces with static abstract members,
        /// allowing for a clean integration with ASP.NET Core minimal APIs while maintaining CRISP architecture principles.
        /// When implementing this method, consider:
        /// - Proper HTTP method selection (GET for queries, POST/PUT/DELETE for commands)
        /// - RESTful route naming conventions
        /// - Appropriate status code responses
        /// - Security requirements (authorization, CORS)
        /// </remarks>
        static abstract void Map(RouteGroupBuilder builder);
    }
}
