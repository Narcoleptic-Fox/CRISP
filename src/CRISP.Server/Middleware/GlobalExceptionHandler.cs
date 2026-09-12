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

        // An expected exception carries a message written for the person who caused it — which name
        // is already taken, which record is missing — and the client shows it verbatim. Withholding it outside development left the user a bare "Conflict" and no way to
        // act on it. Anything unmapped keeps its generic title: those messages are internal and are
        // exactly the ones that leak schema, paths and connection strings.
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
    /// The four mapped types are how the command services reject a request they understand, and
    /// their messages are authored as user-facing text. Everything else is a fault: the status is
    /// reported, the message is not.
    /// </remarks>
    private static (int StatusCode, string Title, bool IsExpected) MapException(Exception exception) => exception switch
    {
        ArgumentException => (StatusCodes.Status400BadRequest, "Bad Request", true),
        UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Forbidden", true),
        KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found", true),
        InvalidOperationException => (StatusCodes.Status409Conflict, "Conflict", true),
        OperationCanceledException => (499, "Client Closed Request", false),
        _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred", false),
    };
}
