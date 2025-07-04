namespace Crisp.Common;

/// <summary>
/// Base interface for all requests (commands and queries).
/// </summary>
/// <typeparam name="TResponse">The type of response returned from the request handler.</typeparam>
public interface IRequest<out TResponse> : IRequest
{
}

/// <summary>
/// Base interface for requests that do not return a response.
/// </summary>
public interface IRequest
{
}
