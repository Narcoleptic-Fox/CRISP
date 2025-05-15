/// <summary>
/// Represents a request that returns a response of type <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TResponse">The type of response returned by the request.</typeparam>
public interface IRequest<out TResponse> : IRequest
{
}

/// <summary>
/// Marker interface for requests in the CRISP architecture.
/// This is the base for all command and query requests.
/// </summary>
public interface IRequest
{
}