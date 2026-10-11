using System.Globalization;
using System.Net;
using Microsoft.AspNetCore.HttpOverrides;

// Microsoft.AspNetCore.HttpOverrides still exports its own obsolete IPNetwork, so the short name is
// ambiguous with both namespaces in scope. Bind it to the System.Net type that KnownIPNetworks takes.
using IPNetwork = System.Net.IPNetwork;

namespace CRISP.Server.Middleware;

/// <summary>
/// Teaches the app to read the client's real address out of <c>X-Forwarded-For</c> when — and only
/// when — the request reached it through a proxy we trust.
///
/// Deployed, every request arrives via the ALB, so <c>Connection.RemoteIpAddress</c> is the load
/// balancer for all of them. The per-IP rate limiter therefore partitioned every user into a single
/// bucket, and the 429 log recorded the ALB's address instead of anyone's.
///
/// The trust check is against the **immediate peer**, not against the address in the header: the
/// question is "did this connection come from a proxy I trust", and only then is the header believed.
/// A client on the public internet still gets their own partition, because the ALB is what we trust,
/// not them.
///
/// Named for the trust it establishes rather than the header it reads, which also keeps it from
/// colliding with <c>Microsoft.AspNetCore.Builder.ForwardedHeadersExtensions</c>.
/// </summary>
public static class TrustedProxyExtensions
{
    /// <summary>Configuration key holding the CIDR ranges whose members may set <c>X-Forwarded-For</c>.</summary>
    public const string TrustedProxyNetworksKey = "RateLimiting:TrustedProxyNetworks";

    /// <summary>
    /// Configures forwarded-header handling from <see cref="TrustedProxyNetworksKey"/>.
    ///
    /// With nothing configured the trust set is empty, so no forwarded header is ever honoured and
    /// behaviour is exactly what it was before this existed. That is the deliberate default: local
    /// development has no proxy, and an environment that has one has to say so.
    /// </summary>
    public static IServiceCollection AddForwardedHeaders(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var configured = configuration.GetSection(TrustedProxyNetworksKey).Get<string[]>() ?? [];
        var trusted = ParseNetworks(configured);

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            // Nothing to trust means the middleware must do nothing at all.
            //
            // This is emphatically NOT the same as leaving the trusted lists empty. ForwardedHeaders-
            // Middleware only performs its trust check when at least one known proxy or network is
            // configured — with both lists empty it skips the check and applies X-Forwarded-For from
            // ANY caller, which would let a client choose their own rate-limit partition. Turning the
            // feature off is the only configuration that actually means "believe no one".
            if (trusted.Count == 0)
            {
                options.ForwardedHeaders = ForwardedHeaders.None;
                return;
            }

            // X-Forwarded-For only. Host is deliberately NOT forwarded: ClientAppResolver picks which
            // SPA to serve from Request.Host, and the ALB already passes the original Host through
            // untouched — honouring X-Forwarded-Host would put "which app is this" under the control
            // of a header. Proto is not needed either; HTTPS redirection is development-only and
            // deployed runs UseHsts instead.
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor;

            // One hop: client -> ALB -> here. Nothing sits in front of the ALB (no CloudFront, no
            // WAF). This is also what defeats spoofing — the ALB *appends* to X-Forwarded-For, so a
            // client who sends a forged value gets their real address appended after it, and taking
            // a single entry from the right yields the address the ALB actually observed.
            options.ForwardLimit = 1;

            // The defaults trust loopback. Clear them so the trusted set is exactly what was
            // configured and nothing else — an empty list must mean "believe no one".
            options.KnownProxies.Clear();
            options.KnownIPNetworks.Clear();
            foreach (var network in trusted)
            {
                options.KnownIPNetworks.Add(network);
            }
        });

        return services;
    }

    /// <summary>
    /// Parses CIDR strings such as <c>10.0.0.0/16</c> into networks.
    ///
    /// Throws rather than skipping a malformed entry. A typo that silently dropped a network would
    /// leave every user pooled into one rate-limit bucket while the configuration claimed otherwise
    /// — the exact failure this is meant to remove, and an invisible one.
    /// </summary>
    /// <exception cref="InvalidOperationException">A value is not a valid CIDR range.</exception>
    public static IReadOnlyList<IPNetwork> ParseNetworks(IEnumerable<string> cidrs)
    {
        var networks = new List<IPNetwork>();

        foreach (var cidr in cidrs)
        {
            if (string.IsNullOrWhiteSpace(cidr)) continue;

            var parts = cidr.Split('/');
            if (parts.Length != 2
                || !IPAddress.TryParse(parts[0], out var prefix)
                || !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var prefixLength))
            {
                throw new InvalidOperationException(
                    $"'{cidr}' in {TrustedProxyNetworksKey} is not a CIDR range such as '10.0.0.0/16'.");
            }

            var maxPrefixLength = prefix.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6 ? 128 : 32;
            if (prefixLength > maxPrefixLength)
            {
                throw new InvalidOperationException(
                    $"'{cidr}' in {TrustedProxyNetworksKey} has a prefix length above {maxPrefixLength}.");
            }

            // Host bits below the prefix are masked away rather than rejected — '10.0.1.5/16' becomes
            // 10.0.0.0/16 — which is what the ASP.NET Core network type did too, so the swap changes
            // nothing about which peers are trusted.
            networks.Add(new IPNetwork(prefix, prefixLength));
        }

        return networks;
    }
}
