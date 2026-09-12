using System.Globalization;
using System.Net;
using System.Threading.RateLimiting;

namespace CRISP.Server.Middleware;

/// <summary>
/// Provides service registration helpers for API rate limiting.
/// </summary>
public static class RateLimitingExtensions
{
    /// <summary>
    /// Registers chained per-IP and per-identity rate limiters for API requests.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration containing rate limiting settings.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddApiRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var rateLimitSection = configuration.GetSection("RateLimiting");

        var globalPermitLimit = rateLimitSection.GetValue("GlobalPermitLimitPerMinute", 50);
        var authenticatedPermitLimit = rateLimitSection.GetValue("AuthenticatedPermitLimitPerMinute", 100);
        var anonymousPermitLimit = rateLimitSection.GetValue("AnonymousPermitLimitPerMinute", 20);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // OnRejected: include Retry-After header and log
            options.OnRejected = (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);
                }

                context.HttpContext.RequestServices.GetService<ILoggerFactory>()?
                    .CreateLogger("RateLimiting")
                    .LogWarning(
                        "Rate limit exceeded for {User} on {Path} from {IP}",
                        context.HttpContext.User.Identity?.Name ?? "anonymous",
                        context.HttpContext.Request.Path,
                        context.HttpContext.Connection.RemoteIpAddress);

                return ValueTask.CompletedTask;
            };

            // Chained global limiter:
            // 1. Per-IP fixed window (prevents abuse before auth)
            // 2. Per-user/API-key token bucket (fair usage after auth)
            options.GlobalLimiter = PartitionedRateLimiter.CreateChained(
                // Layer 1: Per-IP protection
                PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {
                    var partitionKey = ResolvePerIpPartitionKey(
                        httpContext.Connection.RemoteIpAddress,
                        httpContext.User.Identity?.IsAuthenticated == true);

                    if (partitionKey is null)
                        return RateLimitPartition.GetNoLimiter("unlimited");

                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: partitionKey,
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = globalPermitLimit,
                            Window = TimeSpan.FromMinutes(1),
                            AutoReplenishment = true,
                        });
                }),
                // Layer 2: Per-identity token bucket
                PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {
                    var user = httpContext.User;

                    if (user.Identity?.IsAuthenticated != true)
                    {
                        return RateLimitPartition.GetTokenBucketLimiter(
                            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anon",
                            factory: _ => new TokenBucketRateLimiterOptions
                            {
                                TokenLimit = anonymousPermitLimit,
                                ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                                TokensPerPeriod = anonymousPermitLimit,
                                AutoReplenishment = true,
                            });
                    }

                    // Check for custom rate limit from API key claims
                    var rateLimitClaim = user.FindFirst("rate_limit")?.Value;
                    var userLimit = rateLimitClaim is not null
                        ? int.Parse(rateLimitClaim, CultureInfo.InvariantCulture)
                        : authenticatedPermitLimit;

                    var partitionKey = user.Identity.Name ?? "authenticated";

                    return RateLimitPartition.GetTokenBucketLimiter(
                        partitionKey: partitionKey,
                        factory: _ => new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = userLimit,
                            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                            TokensPerPeriod = userLimit,
                            AutoReplenishment = true,
                        });
                }));
        });

        return services;
    }

    /// <summary>
    /// The per-IP partition a request belongs to, or <c>null</c> when Layer 1 should not limit it.
    ///
    /// Pure, and separate from the limiter wiring, so the rule can be tested without a server.
    ///
    /// Two requests are exempt:
    /// <list type="bullet">
    /// <item>Loopback, so local development is never throttled.</item>
    /// <item>Anything already authenticated. Once we know who someone is, identity is a strictly
    /// better partition key than an address, and Layer 2 governs them per-user. Keeping the per-IP
    /// window on authenticated traffic pools everyone behind one corporate NAT — or, before
    /// forwarded headers were honoured, every user behind the load balancer — into a single bucket,
    /// so the second user to do anything heavy hit the limit the first one spent.</item>
    /// </list>
    ///
    /// Anonymous traffic keeps the per-IP window, which is the pre-auth abuse guard it was for.
    /// </summary>
    /// <param name="remoteIp">The connecting address, after forwarded headers have been applied.</param>
    /// <param name="isAuthenticated">Whether the request carries an established identity.</param>
    /// <returns>The partition key, or <c>null</c> for no limit.</returns>
    public static string? ResolvePerIpPartitionKey(IPAddress? remoteIp, bool isAuthenticated)
    {
        if (remoteIp is not null && IPAddress.IsLoopback(remoteIp)) return null;
        if (isAuthenticated) return null;

        return remoteIp?.ToString() ?? "unknown";
    }
}
