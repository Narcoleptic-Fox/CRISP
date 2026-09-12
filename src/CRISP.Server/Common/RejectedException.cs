namespace CRISP.Server.Common;

/// <summary>
/// A request the application understood and refused, carrying a message written for the caller.
/// </summary>
/// <remarks>
/// <para>
/// This is the one exception type handlers throw on purpose, and the only one whose
/// <see cref="Exception.Message"/> the <c>GlobalExceptionHandler</c> returns to the client outside
/// development. Framework exceptions (<see cref="InvalidOperationException"/>,
/// <see cref="KeyNotFoundException"/> and friends) still map to a status code, but their messages
/// are withheld in deployed environments: EF Core and LINQ throw the same types with text that
/// names entities, keys and internals, and an exception type cannot tell the two apart.
/// </para>
/// <para>
/// There is deliberately no hierarchy. Use the factories for the four rejections the API makes:
/// <see cref="BadRequest"/> (400), <see cref="Forbidden"/> (403), <see cref="NotFound"/> (404) and
/// <see cref="Conflict"/> (409). Do not subclass it per feature.
/// </para>
/// </remarks>
public sealed class RejectedException : Exception
{
    private RejectedException(int statusCode, string title, string message)
        : base(message)
    {
        StatusCode = statusCode;
        Title = title;
    }

    /// <summary>The HTTP status the response carries.</summary>
    public int StatusCode { get; }

    /// <summary>The ProblemDetails title for the status.</summary>
    public string Title { get; }

    /// <summary>The request was malformed in a way validation did not catch.</summary>
    /// <param name="message">What was wrong, written for the caller.</param>
    public static RejectedException BadRequest(string message) =>
        new(StatusCodes.Status400BadRequest, "Bad Request", message);

    /// <summary>The caller is authenticated but may not do this.</summary>
    /// <param name="message">What they may not do, written for the caller.</param>
    public static RejectedException Forbidden(string message) =>
        new(StatusCodes.Status403Forbidden, "Forbidden", message);

    /// <summary>The record the request named does not exist (or is not visible to the caller).</summary>
    /// <param name="message">Which record, written for the caller.</param>
    public static RejectedException NotFound(string message) =>
        new(StatusCodes.Status404NotFound, "Not Found", message);

    /// <summary>The request conflicts with current state: a duplicate, a stale version, an invalid transition.</summary>
    /// <param name="message">What it conflicts with, written for the caller.</param>
    public static RejectedException Conflict(string message) =>
        new(StatusCodes.Status409Conflict, "Conflict", message);
}
