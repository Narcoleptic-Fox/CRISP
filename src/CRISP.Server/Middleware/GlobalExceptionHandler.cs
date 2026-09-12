using CRISP.Server.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CRISP.Server.Middleware;

/// <summary>
/// Global exception handler that maps exceptions to RFC 7807 ProblemDetails responses.
/// Never leaks stack traces or internal details in non-development environments.
/// </summary>
public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment) : IExceptionHandler
{
    /// <summary>
    /// Handles an unhandled exception and writes a ProblemDetails response.
    /// </summary>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <param name="exception">The exception to handle.</param>
    /// <param name="cancellationToken">A token that cancels response writing.</param>
    /// <returns><c>true</c> when the exception was handled.</returns>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, isExpected) = MapException(exception);

        logger.LogError(exception,
            "Unhandled exception {ExceptionType}: {Message}",
            exception.GetType().Name,
            exception.Message);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Instance = httpContext.Request.Path,
            Type = $"https://httpstatuses.com/{statusCode}",
        };

        // A RejectedException carries a message written for the person who caused it — which name
        // is already taken, which record is missing — and the client shows it verbatim. Withholding
        // it outside development left the user a bare "Conflict" and no way to act on it.
        //
        // Everything else keeps its generic title. Framework exceptions are withheld even when they
        // map to a 4xx: EF Core and LINQ throw InvalidOperationException and friends with text that
        // names entities, keys and internals, and the type alone cannot tell a service's rejection
        // from one of those. That is why rejections have their own type.
        if (isExpected || environment.IsDevelopment())
        {
            problemDetails.Detail = exception.Message;
        }

        // Stack traces stay development-only regardless: they describe the server, never the request.
        if (environment.IsDevelopment())
        {
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
        }

        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    /// <summary>
    /// Maps an exception to its status, title, and whether its message is fit to return.
    /// </summary>
    /// <remarks>
    /// RejectedException is how command services refuse a request they understand, and its message
    /// is authored as user-facing text. The framework types still map to the status they imply (a
    /// KeyNotFoundException is a 404 whoever threw it) but are not trusted to carry a message fit
    /// for the caller. Everything unmapped is a fault: the status is reported, the message is not.
    /// </remarks>
    private static (int StatusCode, string Title, bool IsExpected) MapException(Exception exception) => exception switch
    {
        RejectedException rejected => (rejected.StatusCode, rejected.Title, true),
        ArgumentException => (StatusCodes.Status400BadRequest, "Bad Request", false),
        UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Forbidden", false),
        KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found", false),
        InvalidOperationException => (StatusCodes.Status409Conflict, "Conflict", false),
        OperationCanceledException => (499, "Client Closed Request", false),
        _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred", false),
    };
}
