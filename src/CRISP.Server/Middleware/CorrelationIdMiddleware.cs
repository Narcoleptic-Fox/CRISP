namespace CRISP.Server.Middleware;

/// <summary>
/// Reads X-Correlation-ID from inbound request or generates a new one.
/// Sets it on HttpContext.TraceIdentifier (so it flows into logs) and
/// returns it in the response header for the caller to correlate.
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    /// <summary>
    /// The request and response header that carries the correlation identifier.
    /// </summary>
    public const string HeaderName = "X-Correlation-ID";

    /// <summary>
    /// Applies the inbound correlation identifier to the current HTTP context and response.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>A task that completes when the downstream pipeline has completed.</returns>
    public Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(HeaderName, out var correlationId)
            && !string.IsNullOrWhiteSpace(correlationId))
        {
            context.TraceIdentifier = correlationId!;
        }

        // Return the correlation ID in the response so callers can trace
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = context.TraceIdentifier;
            return Task.CompletedTask;
        });

        return next(context);
    }
}

/// <summary>
/// Provides middleware registration helpers for correlation IDs.
/// </summary>
public static class CorrelationIdMiddlewareExtensions
{
    /// <summary>
    /// Adds correlation ID handling to the request pipeline.
    /// </summary>
    /// <param name="app">The application builder to configure.</param>
    /// <returns>The application builder.</returns>
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
        => app.UseMiddleware<CorrelationIdMiddleware>();
}
