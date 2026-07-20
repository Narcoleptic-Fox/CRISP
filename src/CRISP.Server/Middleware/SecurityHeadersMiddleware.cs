namespace CRISP.Server.Middleware;

/// <summary>
/// Adds security response headers per OWASP recommendations.
/// </summary>
public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    /// <summary>
    /// Adds security headers before the response is sent.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>A task that completes when request processing has finished.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(static state =>
        {
            var httpContext = (HttpContext)state;
            var headers = httpContext.Response.Headers;

            // Prevent MIME-type sniffing
            headers["X-Content-Type-Options"] = "nosniff";

            // Prevent clickjacking
            headers["X-Frame-Options"] = "DENY";

            // Control referrer information
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

            // Restrict browser features
            // The QR scanner calls getUserMedia, which the browser refuses outright if the camera is
            // denied here — no permission prompt is ever shown. Granted to same-origin only; every
            // other capability we don't use stays denied.
            headers["Permissions-Policy"] = "accelerometer=(), camera=(self), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), usb=()";

            // Content Security Policy.
            //
            // API responses are never rendered by the browser, so they keep the
            // most restrictive policy as defense-in-depth. The SPA shell
            // (index.html) and its static assets must be allowed to load the
            // Vite bundle, same-origin images, and the inline <style> block in
            // index.html, otherwise the browser blocks the entire app.
            //
            // 'unsafe-inline' is scoped to style-src only (never script-src):
            // index.html ships an inline <style> and component libraries inject
            // styles at runtime, which cannot be hashed ahead of time. Scripts
            // remain locked to 'self' since Vite emits external module scripts.
            //
            // 'wasm-unsafe-eval' is what lets the browser compile WebAssembly. The QR scanner falls
            // back to a wasm decoder where the platform has no BarcodeDetector (iOS Safari, desktop
            // Firefox), and without this Chrome blocks the compile outright. It CRISP WebAssembly
            // only — it does NOT re-enable eval() or inline scripts.
            headers["Content-Security-Policy"] = httpContext.Request.Path.StartsWithSegments("/api")
                ? "default-src 'none'; frame-ancestors 'none'"
                : "default-src 'self'; "
                  + "script-src 'self' 'wasm-unsafe-eval'; "
                  + "style-src 'self' 'unsafe-inline'; "
                  + "img-src 'self' data:; "
                  + "font-src 'self'; "
                  // MSAL exchanges the auth code for tokens via a fetch to the
                  // Microsoft identity platform, so 'self' alone is not enough.
                  + "connect-src 'self' https://login.microsoftonline.com; "
                  + "base-uri 'self'; "
                  + "form-action 'self'; "
                  + "object-src 'none'; "
                  + "frame-ancestors 'none'";

            // Apply no-store after the auth middleware has established the final user.
            if (httpContext.User.Identity?.IsAuthenticated == true)
            {
                headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
                headers["Pragma"] = "no-cache";
            }

            return Task.CompletedTask;
        }, context);

        await next(context);
    }
}

/// <summary>
/// Provides middleware registration helpers for security response headers.
/// </summary>
public static class SecurityHeadersMiddlewareExtensions
{
    /// <summary>
    /// Adds security response headers to the request pipeline.
    /// </summary>
    /// <param name="app">The application builder to configure.</param>
    /// <returns>The application builder.</returns>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        => app.UseMiddleware<SecurityHeadersMiddleware>();
}
