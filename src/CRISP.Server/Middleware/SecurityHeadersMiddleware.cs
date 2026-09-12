using CRISP.Server.Infrastructure.Storage;
using Microsoft.Extensions.Options;

namespace CRISP.Server.Middleware;

/// <summary>
/// Adds security response headers per OWASP recommendations.
/// </summary>
public sealed class SecurityHeadersMiddleware
{
    // API responses are never rendered by the browser, so they keep the most restrictive policy
    // as defense-in-depth. See BuildSpaContentSecurityPolicy for the SPA's.
    private const string ApiContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none'";

    private readonly RequestDelegate _next;

    // Both policies are fixed for the app's lifetime (they depend only on configuration), so they
    // are built once here rather than per request.
    private readonly string _spaContentSecurityPolicy;
    private readonly string _permissionsPolicy;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityHeadersMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="storageOptions">Object storage settings, read to derive the bucket origin.</param>
    /// <param name="headerOptions">The per-deployment header settings.</param>
    public SecurityHeadersMiddleware(
        RequestDelegate next,
        IOptions<ObjectStorageOptions> storageOptions,
        IOptions<SecurityHeadersOptions> headerOptions
    )
    {
        _next = next;
        _spaContentSecurityPolicy = BuildSpaContentSecurityPolicy(
            storageOptions.Value,
            headerOptions.Value
        );
        _permissionsPolicy = BuildPermissionsPolicy(headerOptions.Value);
    }

    /// <summary>
    /// Adds security headers before the response is sent.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>A task that completes when request processing has finished.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(
            static state =>
            {
                var (httpContext, spaPolicy, permissionsPolicy) = ((
                    HttpContext,
                    string,
                    string
                ))state;
                var headers = httpContext.Response.Headers;

                // Prevent MIME-type sniffing
                headers["X-Content-Type-Options"] = "nosniff";

                // Prevent clickjacking
                headers["X-Frame-Options"] = "DENY";

                // Control referrer information
                headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

                // Restrict browser features
                headers["Permissions-Policy"] = permissionsPolicy;

                headers["Content-Security-Policy"] = httpContext.Request.Path.StartsWithSegments(
                    "/api"
                )
                    ? ApiContentSecurityPolicy
                    : spaPolicy;

                // Apply no-store after the auth middleware has established the final user.
                if (httpContext.User.Identity?.IsAuthenticated == true)
                {
                    headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
                    headers["Pragma"] = "no-cache";
                }

                return Task.CompletedTask;
            },
            (context, _spaContentSecurityPolicy, _permissionsPolicy)
        );

        await _next(context);
    }

    /// <summary>
    /// Builds the Permissions-Policy header. Every capability the app does not use stays denied.
    /// </summary>
    internal static string BuildPermissionsPolicy(SecurityHeadersOptions options)
    {
        // getUserMedia is refused outright — with no prompt — when the camera is denied here, so an
        // app that scans or photographs has to opt in. Same-origin only, never third parties.
        var camera = options.AllowCamera ? "camera=(self)" : "camera=()";
        return $"accelerometer=(), {camera}, geolocation=(), gyroscope=(), magnetometer=(), microphone=(), usb=()";
    }

    /// <summary>
    /// Builds the Content Security Policy served with the SPA shell and its assets.
    /// </summary>
    /// <remarks>
    /// The SPA shell (index.html) and its static assets must be allowed to load the Vite bundle,
    /// same-origin images, and the inline &lt;style&gt; block in index.html, otherwise the browser
    /// blocks the entire app.
    /// <para>
    /// 'unsafe-inline' is scoped to style-src only (never script-src): index.html ships an inline
    /// &lt;style&gt; and component libraries inject styles at runtime, which cannot be hashed ahead
    /// of time. Scripts remain locked to 'self' since Vite emits external module scripts.
    /// </para>
    /// <para>
    /// 'wasm-unsafe-eval' is what lets the browser compile WebAssembly (barcode decoders, PDF layout
    /// engines and the like ship as wasm). It permits WebAssembly only — it does NOT re-enable
    /// eval() or inline scripts.
    /// </para>
    /// <para>
    /// worker-src is named explicitly rather than left to fall back to script-src, because libraries
    /// that build a worker out of an object URL need blob: and scripts must not. Naming the
    /// directive keeps blob: workers legal while blob: &lt;script&gt; stays refused.
    /// </para>
    /// <para>
    /// This policy only governs pages this server serves. In development the SPA is served by Vite,
    /// which sets no CSP, so a connect-src problem will not reproduce locally — it appears only once
    /// the app is served from wwwroot by a deployed container. The tests are the check.
    /// </para>
    /// </remarks>
    internal static string BuildSpaContentSecurityPolicy(
        ObjectStorageOptions storage,
        SecurityHeadersOptions options
    )
    {
        // Everything the browser reaches object storage for, whether it fetches the bytes or renders
        // them. Derived rather than configured: this server issues those presigned URLs from the same
        // ObjectStorage settings, so reading them is what keeps the policy true by construction. A
        // hand-maintained entry is how this was missed the first time — the bucket's CORS rule
        // allowed the request and the page's own policy blocked it before it was ever sent.
        var storageOrigin = BuildStorageOrigin(storage);

        // MSAL exchanges the auth code for tokens via a fetch to the Microsoft identity platform, so
        // 'self' alone is never enough.
        //
        // data: is for wasm inlined into the bundle as a base64 data URL and instantiated by
        // *fetching* that URL — connect-src governs a fetch even when nothing leaves the machine.
        // This is not the usual data: risk, which is script-src: a page can only read bytes it
        // already carries, and there is no data: URL to exfiltrate to.
        var connectSources = new List<string>
        {
            "'self'",
            "data:",
            "https://login.microsoftonline.com",
        };

        // Attachments are uploaded and downloaded straight against the bucket through a presigned URL.
        if (storageOrigin is not null)
            connectSources.Add(storageOrigin);

        // Every other origin the browser calls directly, reduced to its origin.
        foreach (var source in options.ConnectSources)
        {
            if (BuildOrigin(source) is { } origin && !connectSources.Contains(origin))
                connectSources.Add(origin);
        }

        // Images the browser renders rather than fetches, which is a separate directive and was
        // missed when connect-src was fixed. Presigned storage URLs are used as an <img> source and
        // as a CSS background-image, and both are governed here.
        //
        // blob: covers the preview of a file the user has only just picked: it is turned into an
        // object URL and shown before anything is uploaded, and 'self' does not match a blob URL.
        var imageSources = new List<string> { "'self'", "data:", "blob:" };
        if (storageOrigin is not null)
            imageSources.Add(storageOrigin);

        return "default-src 'self'; "
            + "script-src 'self' 'wasm-unsafe-eval'; "
            + "worker-src 'self' blob:; "
            + "style-src 'self' 'unsafe-inline'; "
            + $"img-src {string.Join(' ', imageSources)}; "
            + "font-src 'self'; "
            + $"connect-src {string.Join(' ', connectSources)}; "
            + "base-uri 'self'; "
            + "form-action 'self'; "
            + "object-src 'none'; "
            + "frame-ancestors 'none'";
    }

    /// <summary>
    /// The origin presigned storage URLs are issued against, or <c>null</c> when no provider is
    /// configured.
    /// </summary>
    /// <remarks>
    /// Mirrors how the AWS SDK addresses the bucket in <c>S3ObjectStorage</c>, because the URL it
    /// signs is what the browser is asked to reach: an explicit ServiceUrl wins (an S3-compatible
    /// provider such as MinIO), path-style addressing puts the bucket in the path and leaves a bare
    /// regional endpoint, and the default is virtual-hosted style with the bucket as a subdomain.
    /// Only the origin is emitted — a CSP source carrying a path would restrict the policy to that
    /// one path.
    /// </remarks>
    internal static string? BuildStorageOrigin(ObjectStorageOptions storage)
    {
        // The null provider hands back an empty upload URL, which the client treats as "already
        // stored" and never fetches, so there is no origin to allow.
        if (
            !string.Equals(
                storage.Provider,
                ObjectStorageProviders.S3,
                StringComparison.OrdinalIgnoreCase
            )
        )
            return null;

        if (!string.IsNullOrWhiteSpace(storage.ServiceUrl))
            return BuildOrigin(storage.ServiceUrl);

        if (string.IsNullOrWhiteSpace(storage.BucketName))
            return null;

        var region = string.IsNullOrWhiteSpace(storage.Region) ? "us-east-1" : storage.Region;

        return storage.ForcePathStyle
            ? $"https://s3.{region}.amazonaws.com"
            : $"https://{storage.BucketName}.s3.{region}.amazonaws.com";
    }

    /// <summary>
    /// Reduces a configured URL to the origin (scheme + host[:port]) a CSP allow-list names, or
    /// <c>null</c> when it is blank or unparseable. A malformed value must not take the app down
    /// over a header, so it is treated as "not configured" — the fetch is then blocked and reported
    /// by the browser, which is at least visible.
    /// </summary>
    internal static string? BuildOrigin(string? url) =>
        Uri.TryCreate(url?.Trim(), UriKind.Absolute, out var parsed)
        && parsed.Scheme is "http" or "https"
            ? parsed.GetLeftPart(UriPartial.Authority)
            : null;
}

/// <summary>
/// Provides registration helpers for security response headers.
/// </summary>
public static class SecurityHeadersMiddlewareExtensions
{
    /// <summary>
    /// Binds <see cref="SecurityHeadersOptions"/> from the <c>SecurityHeaders</c> section.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddSecurityHeaders(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddOptions<SecurityHeadersOptions>()
            .Bind(configuration.GetSection(SecurityHeadersOptions.SectionName));
        return services;
    }

    /// <summary>
    /// Adds security response headers to the request pipeline.
    /// </summary>
    /// <param name="app">The application builder to configure.</param>
    /// <returns>The application builder.</returns>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app) =>
        app.UseMiddleware<SecurityHeadersMiddleware>();
}
