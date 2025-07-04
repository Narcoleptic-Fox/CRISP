namespace Crisp.Builder;

/// <summary>
/// Represents the registration details for a handler, including its type, interface, request, and response types.
/// </summary>
public class HandlerRegistration
{
    /// <summary>
    /// Gets the type of the handler.
    /// </summary>
    public Type HandlerType { get; init; } = default!;

    /// <summary>
    /// Gets the interface type implemented by the handler.
    /// </summary>
    public Type HandlerInterface { get; init; } = default!;

    /// <summary>
    /// Gets the type of the request handled by the handler.
    /// </summary>
    public Type RequestType { get; init; } = default!;

    /// <summary>
    /// Gets the type of the response returned by the handler.
    /// </summary>
    public Type ResponseType { get; init; } = default!;

    /// <summary>
    /// Indicates whether the handler processes commands.
    /// </summary>
    public bool IsCommand { get; init; }

    /// <summary>
    /// Indicates whether the handler produces a response.
    /// </summary>
    public bool HasResponse { get; init; }
}
