namespace CRISP.Server.Middleware;

/// <summary>
/// The per-deployment knobs on the security response headers. Everything else in
/// <see cref="SecurityHeadersMiddleware"/> is fixed policy.
/// </summary>
public sealed class SecurityHeadersOptions
{
    /// <summary>The configuration section the options bind from.</summary>
    public const string SectionName = "SecurityHeaders";

    /// <summary>
    /// Origins the browser may call directly from the SPA, in addition to this server, the
    /// identity platform, and the configured object store (which is derived, not listed here).
    /// </summary>
    /// <remarks>
    /// Any value is reduced to its origin before use: a CSP source with a path is a path
    /// restriction, and one with a trailing slash matches only the root, so "https://x/" would
    /// allow exactly one URL rather than a host. Entries that are not absolute URLs are dropped
    /// rather than allowed to void the directive.
    /// </remarks>
    public string[] ConnectSources { get; init; } = [];

    /// <summary>
    /// Whether same-origin pages may use the camera (<c>camera=(self)</c> in Permissions-Policy).
    /// </summary>
    /// <remarks>
    /// Off by default: a page that calls <c>getUserMedia</c> under a denying policy is refused
    /// outright with no prompt, so an app that scans QR codes or takes photos has to turn this on,
    /// and one that does neither should not carry the grant.
    /// </remarks>
    public bool AllowCamera { get; init; }
}
