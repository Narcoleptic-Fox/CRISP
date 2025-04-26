namespace CRISP.Requests
{
    /// <summary>
    /// Represents a request that doesn't return a specific response (void operation).
    /// </summary>
    /// <remarks>
    /// Used for commands that perform actions but don't need to return data,
    /// such as create, update, or delete operations. This is a specialized form
    /// of the Command pattern within the CQRS architecture.
    /// </remarks>
    public interface IRequest : IBaseRequest { }

    /// <summary>
    /// Represents a request that returns a typed response.
    /// </summary>
    /// <typeparam name="TResponse">The type of response expected from this request.</typeparam>
    /// <remarks>
    /// Used for both commands that need to return data (like identifiers) and
    /// queries that retrieve information from the system. This forms a core part
    /// of the CQRS pattern implementation.
    /// </remarks>
    public interface IRequest<out TResponse> : IBaseRequest { }

    /// <summary>
    /// Base interface that enables generic type constraints across all request types.
    /// </summary>
    /// <remarks>
    /// This marker interface provides a common type for both void and returning requests,
    /// allowing for generic handling of request objects in the system infrastructure.
    /// It should not typically be implemented directly by domain requests.
    /// </remarks>
    public interface IBaseRequest { }
}
