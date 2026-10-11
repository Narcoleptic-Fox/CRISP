using System.Diagnostics;

namespace CRISP.Server.Middleware;

/// <summary>
/// Logs structured request/response data: method, path, status, duration, user, tenant, trace ID.
/// Placed after authentication so user identity is available.
/// </summary>
public sealed class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    /// <summary>
    /// Logs request metadata after the downstream pipeline completes.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>A task that completes when request processing has finished.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var startTimestamp = Stopwatch.GetTimestamp();

        try
        {
            await next(context);
        }
        finally
        {
            var elapsed = Stopwatch.GetElapsedTime(startTimestamp);
            var user = context.User.Identity?.Name ?? "anonymous";
            var statusCode = context.Response.StatusCode;

            var level = statusCode switch
            {
                >= 500 => LogLevel.Error,
                >= 400 => LogLevel.Warning,
                _ => LogLevel.Information,
            };

            logger.Log(level,
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms " +
                "[User: {User}, TraceId: {TraceId}]",
                context.Request.Method,
                context.Request.Path,
                statusCode,
                elapsed.TotalMilliseconds.ToString("F1"),
                user,
                context.TraceIdentifier);
        }
    }
}

/// <summary>
/// Provides middleware registration helpers for structured request logging.
/// </summary>
public static class RequestLoggingMiddlewareExtensions
{
    /// <summary>
    /// Adds structured request logging to the request pipeline.
    /// </summary>
    /// <param name="app">The application builder to configure.</param>
    /// <returns>The application builder.</returns>
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
        => app.UseMiddleware<RequestLoggingMiddleware>();
}
