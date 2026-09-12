using System.Security.Claims;
using CRISP.Server.Infrastructure.Storage;
using CRISP.Server.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;

namespace CRISP.Server.Tests.Middleware;

[Trait("Category", "Unit")]
public sealed class SecurityHeadersMiddlewareTests
{
    // A virtual-hosted-style bucket. Its browser-facing origin is what the SPA CSP must allow for
    // direct-to-S3 upload (connect-src) and display (img-src).
    private static readonly ObjectStorageOptions S3Storage = new()
    {
        Provider = ObjectStorageProviders.S3,
        BucketName = "crisp-staging",
        Region = "us-east-1",
    };

    private const string S3Origin = "https://crisp-staging.s3.us-east-1.amazonaws.com";

    private static SecurityHeadersMiddleware CreateMiddleware(
        RequestDelegate next,
        ObjectStorageOptions? storage = null,
        SecurityHeadersOptions? headers = null
    ) =>
        new(
            next,
            Options.Create(storage ?? new ObjectStorageOptions()),
            Options.Create(headers ?? new SecurityHeadersOptions())
        );

    private static async Task<IHeaderDictionary> HeadersFor(
        string path,
        ObjectStorageOptions? storage = null,
        SecurityHeadersOptions? headers = null,
        bool authenticated = false
    )
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        var responseFeature = new TestResponseFeature();
        context.Features.Set<IHttpResponseFeature>(responseFeature);
        if (authenticated)
            context.User = new ClaimsPrincipal(
                new ClaimsIdentity([new Claim(ClaimTypes.Name, "someone")], "test")
            );

        var middleware = CreateMiddleware(_ => Task.CompletedTask, storage, headers);

        await middleware.InvokeAsync(context);
        await responseFeature.FireOnStartingAsync();

        return context.Response.Headers;
    }

    private static async Task<string> PolicyFor(
        string path,
        ObjectStorageOptions? storage = null,
        SecurityHeadersOptions? headers = null
    ) => (await HeadersFor(path, storage, headers))["Content-Security-Policy"].ToString();

    [Fact]
    public async Task AddsCoreSecurityHeaders()
    {
        var headers = await HeadersFor("/");

        headers["X-Content-Type-Options"].ToString().ShouldBe("nosniff");
        headers["X-Frame-Options"].ToString().ShouldBe("DENY");
        headers["Referrer-Policy"].ToString().ShouldBe("strict-origin-when-cross-origin");
    }

    [Fact]
    public async Task ApiPaths_KeepTheRestrictivePolicy()
    {
        // Even with S3 and extra origins configured, the API policy stays locked down — nothing
        // from the SPA allow-list must leak into it.
        var csp = await PolicyFor(
            "/api/v1/items",
            S3Storage,
            new SecurityHeadersOptions { ConnectSources = ["https://other.example"] }
        );

        csp.ShouldBe("default-src 'none'; frame-ancestors 'none'");
    }

    [Fact]
    public async Task SpaPaths_AllowSelfAssetsAndInlineStyles()
    {
        var csp = await PolicyFor("/index.html");

        csp.ShouldContain("default-src 'self'");
        csp.ShouldContain("style-src 'self' 'unsafe-inline'");
        // MSAL token exchange must be allowed or sign-in breaks.
        csp.ShouldContain("connect-src 'self' data: https://login.microsoftonline.com;");
        csp.ShouldContain("frame-ancestors 'none'");
    }

    // The wasm decoder fallback cannot compile without this, and the failure only shows up in a
    // deployed environment. 'wasm-unsafe-eval' permits WebAssembly only.
    [Fact]
    public async Task ScriptSrc_AllowsWasm_WithoutAllowingEvalOrInlineScripts()
    {
        var csp = await PolicyFor("/index.html");

        csp.ShouldContain("script-src 'self' 'wasm-unsafe-eval';");
        csp.ShouldNotContain("'unsafe-eval'");
        csp.ShouldNotContain("script-src 'self' 'unsafe-inline'");
    }

    [Fact]
    public async Task WorkerSrc_AllowsBlobUrls_WithoutLooseningScriptSrc()
    {
        var csp = await PolicyFor("/index.html");

        csp.ShouldContain("worker-src 'self' blob:;");
        csp.ShouldNotContain("script-src 'self' blob:");
    }

    [Fact]
    public async Task ImgSrc_AllowsBlobUrls_ForPreviewsOfAPickedFile()
    {
        var csp = await PolicyFor("/index.html");

        csp.ShouldContain("img-src 'self' data: blob:;");
    }

    // Uploads PUT bytes to a presigned S3 URL and images bind presigned GET URLs to <img src>, both
    // cross-origin to the SPA. Without the bucket origin in connect-src and img-src the browser's
    // own CSP blocks the file feature before any request leaves — and the bucket's CORS rule, which
    // is where everyone looks first, is not at fault.
    [Fact]
    public async Task S3Bucket_ReachesConnectSrcAndImgSrc()
    {
        var csp = await PolicyFor("/index.html", S3Storage);

        csp.ShouldContain(
            $"connect-src 'self' data: https://login.microsoftonline.com {S3Origin};"
        );
        csp.ShouldContain($"img-src 'self' data: blob: {S3Origin};");
    }

    [Fact]
    public async Task NoStorageProvider_AddsNothing()
    {
        var csp = await PolicyFor("/index.html");

        csp.ShouldNotContain("amazonaws.com");
    }

    [Fact]
    public void PathStyleAddressing_AllowsTheRegionalEndpointRatherThanASubdomain()
    {
        var origin = SecurityHeadersMiddleware.BuildStorageOrigin(
            new ObjectStorageOptions
            {
                Provider = ObjectStorageProviders.S3,
                BucketName = "crisp-local",
                Region = "eu-central-1",
                ForcePathStyle = true,
            }
        );

        origin.ShouldBe("https://s3.eu-central-1.amazonaws.com");
    }

    [Fact]
    public void CustomServiceUrl_ContributesItsOriginOnly()
    {
        var origin = SecurityHeadersMiddleware.BuildStorageOrigin(
            new ObjectStorageOptions
            {
                Provider = ObjectStorageProviders.S3,
                BucketName = "crisp-local",
                ServiceUrl = "http://localhost:9000/",
            }
        );

        origin.ShouldBe("http://localhost:9000");
    }

    [Fact]
    public async Task ConfiguredOrigin_ReachesConnectSrc()
    {
        var csp = await PolicyFor(
            "/index.html",
            headers: new SecurityHeadersOptions
            {
                ConnectSources = ["https://staging-api.example.com"],
            }
        );

        csp.ShouldContain(
            "connect-src 'self' data: https://login.microsoftonline.com https://staging-api.example.com;"
        );
    }

    // A CSP source with a trailing slash or a path is a path restriction, not an origin —
    // "https://x/" matches only the root. Only the origin may reach the policy.
    [Theory]
    [InlineData("https://api.example.com/")]
    [InlineData("https://api.example.com/api/v1/projects")]
    [InlineData("  https://api.example.com  ")]
    public async Task ConfiguredSource_IsReducedToItsOrigin(string configured)
    {
        var csp = await PolicyFor(
            "/index.html",
            headers: new SecurityHeadersOptions { ConnectSources = [configured] }
        );

        csp.ShouldContain(" https://api.example.com;");
        csp.ShouldNotContain("https://api.example.com/");
    }

    // A malformed value must not take the app down over a header; it is treated as not configured.
    [Theory]
    [InlineData("not a url")]
    [InlineData("")]
    [InlineData("ftp://files.example.com")]
    public async Task MalformedSource_IsDropped(string configured)
    {
        var csp = await PolicyFor(
            "/index.html",
            headers: new SecurityHeadersOptions { ConnectSources = [configured] }
        );

        csp.ShouldContain("connect-src 'self' data: https://login.microsoftonline.com;");
    }

    [Fact]
    public async Task ConfiguredSources_AreNotRenderedFromImgSrc()
    {
        var csp = await PolicyFor(
            "/index.html",
            S3Storage,
            new SecurityHeadersOptions { ConnectSources = ["https://other.example"] }
        );

        csp.ShouldContain($"img-src 'self' data: blob: {S3Origin};");
    }

    // getUserMedia is refused outright — with no prompt — when Permissions-Policy denies the
    // camera. A template must not grant it by default; an app that scans has to say so.
    [Fact]
    public async Task Camera_IsDeniedUnlessAllowed()
    {
        var denied = (await HeadersFor("/"))["Permissions-Policy"].ToString();
        var allowed = (
            await HeadersFor("/", headers: new SecurityHeadersOptions { AllowCamera = true })
        )["Permissions-Policy"]
            .ToString();

        denied.ShouldContain("camera=()");
        allowed.ShouldContain("camera=(self)");
        allowed.ShouldContain("microphone=()");
        allowed.ShouldContain("geolocation=()");
    }

    [Fact]
    public async Task AuthenticatedUser_GetsNoCacheHeaders()
    {
        var headers = await HeadersFor("/", authenticated: true);

        headers["Cache-Control"].ToString().ShouldBe("no-store, no-cache, must-revalidate");
        headers["Pragma"].ToString().ShouldBe("no-cache");
    }

    [Fact]
    public async Task AnonymousUser_GetsNoCacheDirective()
    {
        var headers = await HeadersFor("/");

        headers.ContainsKey("Cache-Control").ShouldBeFalse();
    }

    private sealed class TestResponseFeature : IHttpResponseFeature
    {
        private readonly List<(Func<object, Task> Callback, object State)> _callbacks = [];

        public int StatusCode { get; set; } = StatusCodes.Status200OK;

        public string? ReasonPhrase { get; set; }

        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();

        public Stream Body { get; set; } = new MemoryStream();

        public bool HasStarted { get; private set; }

        public void OnCompleted(Func<object, Task> callback, object state) { }

        public void OnStarting(Func<object, Task> callback, object state)
        {
            _callbacks.Add((callback, state));
        }

        public async Task FireOnStartingAsync()
        {
            foreach (
                (Func<object, Task> callback, object state) in _callbacks.AsEnumerable().Reverse()
            )
            {
                await callback(state);
            }

            HasStarted = true;
        }
    }
}
