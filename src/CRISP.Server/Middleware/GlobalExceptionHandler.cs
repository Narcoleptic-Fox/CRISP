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
        var (statusCode, title) = MapException(exception);

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

        // Only include detail in development
        if (environment.IsDevelopment())
        {
            problemDetails.Detail = exception.Message;
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
        }

        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static (int StatusCode, string Title) MapException(Exception exception) => exception switch
    {
        ArgumentException => (StatusCodes.Status400BadRequest, "Bad Request"),
        UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Forbidden"),
        KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
        InvalidOperationException => (StatusCodes.Status409Conflict, "Conflict"),
        OperationCanceledException => (499, "Client Closed Request"),
        _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred"),
    };
}
