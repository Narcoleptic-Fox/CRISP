using System.Net;
using CRISP.Server.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shouldly;

namespace CRISP.Server.Tests.Middleware;

/// <summary>
/// Pins who is allowed to tell this app where a request came from.
///
/// The per-IP rate limiter partitions on the address these options produce, so "believes the wrong
/// header" and "believes no header" are both real failures: the first lets a client pick their own
/// bucket, the second leaves every user sharing the load balancer's.
/// </summary>
[Trait("Category", "Unit")]
public sealed class TrustedProxyExtensionsTests
{
    private const string VpcRange = "10.0.0.0/16";
    private static readonly IPAddress LoadBalancer = IPAddress.Parse("10.0.5.20");
    private static readonly IPAddress RealClient = IPAddress.Parse("73.12.34.56");

    /// <summary>Runs the real middleware over the options <see cref="TrustedProxyExtensions"/> builds.</summary>
    private static async Task<IPAddress?> RemoteIpAfterMiddleware(
        IPAddress? peer,
        string? forwardedFor,
        params string[] trustedNetworks)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(trustedNetworks
                .Select((network, index) =>
                    new KeyValuePair<string, string?>(
                        $"{TrustedProxyExtensions.TrustedProxyNetworksKey}:{index}", network)))
            .Build();

        var services = new ServiceCollection();
        services.AddOptions();
        services.AddForwardedHeaders(configuration);
        using var provider = services.BuildServiceProvider();

        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = peer;
        if (forwardedFor is not null)
        {
            context.Request.Headers[ForwardedHeadersDefaults.XForwardedForHeaderName] = forwardedFor;
        }

        var middleware = new ForwardedHeadersMiddleware(
            _ => Task.CompletedTask,
            NullLoggerFactory.Instance,
            provider.GetRequiredService<IOptions<ForwardedHeadersOptions>>());

        await middleware.Invoke(context);
        return context.Connection.RemoteIpAddress;
    }

    /// <summary>The point of the change: behind a trusted proxy, the user's own address is restored.</summary>
    [Fact]
    public async Task ATrustedProxyIsBelieved()
    {
        var remoteIp = await RemoteIpAfterMiddleware(LoadBalancer, RealClient.ToString(), VpcRange);

        remoteIp.ShouldBe(RealClient);
    }

    /// <summary>
    /// The trust check is against the peer, not the header. A caller connecting directly from the
    /// public internet cannot nominate their own address — otherwise anyone could rotate through
    /// invented IPs and never meet the per-IP limit at all.
    /// </summary>
    [Fact]
    public async Task AnUntrustedCallerCannotNominateTheirOwnAddress()
    {
        var attacker = IPAddress.Parse("203.0.113.9");

        var remoteIp = await RemoteIpAfterMiddleware(attacker, "10.0.0.1", VpcRange);

        remoteIp.ShouldBe(attacker);
    }

    /// <summary>
    /// A single hop is consumed, and the load balancer appends rather than replaces — so a client
    /// who forges a value gets their real address appended after it, and the real one is what wins.
    /// </summary>
    [Fact]
    public async Task AForgedEntryAheadOfTheProxysIsIgnored()
    {
        var remoteIp = await RemoteIpAfterMiddleware(
            LoadBalancer, $"1.2.3.4, {RealClient}", VpcRange);

        remoteIp.ShouldBe(RealClient);
    }

    /// <summary>
    /// Nothing configured must mean nothing believed, so an environment with no proxy — local
    /// development, and every environment before Terraform supplies the range — behaves as it did.
    /// </summary>
    [Fact]
    public async Task NoConfiguredNetworkMeansNoHeaderIsHonoured()
    {
        var remoteIp = await RemoteIpAfterMiddleware(LoadBalancer, RealClient.ToString());

        remoteIp.ShouldBe(LoadBalancer);
    }

    private static ForwardedHeadersOptions BuildOptions(params string[] trustedNetworks)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(trustedNetworks
                .Select((network, index) =>
                    new KeyValuePair<string, string?>(
                        $"{TrustedProxyExtensions.TrustedProxyNetworksKey}:{index}", network)))
            .Build();

        var services = new ServiceCollection();
        services.AddOptions();
        services.AddForwardedHeaders(configuration);
        using var provider = services.BuildServiceProvider();

        return provider.GetRequiredService<IOptions<ForwardedHeadersOptions>>().Value;
    }

    /// <summary>
    /// Host is deliberately absent: ClientAppResolver decides which SPA to serve from Request.Host,
    /// and forwarding it would let a header choose the application.
    /// </summary>
    [Fact]
    public void OnlyTheForwardedForHeaderIsHonoured()
    {
        var options = BuildOptions(VpcRange);

        options.ForwardedHeaders.ShouldBe(ForwardedHeaders.XForwardedFor);
        options.ForwardLimit.ShouldBe(1);
    }

    /// <summary>
    /// Guards the trap this configuration is built around: ForwardedHeadersMiddleware only checks
    /// the peer when a known proxy or network is configured, so leaving the lists empty makes it
    /// trust every caller. Switching the feature off is what "believe no one" actually looks like.
    /// </summary>
    [Fact]
    public void WithNothingTrustedTheFeatureIsOffRatherThanUnrestricted()
    {
        var options = BuildOptions();

        options.ForwardedHeaders.ShouldBe(ForwardedHeaders.None);
    }

    [Theory]
    [InlineData("not-a-cidr")]
    [InlineData("10.0.0.0")]
    [InlineData("10.0.0.0/")]
    [InlineData("10.0.0.0/sixteen")]
    [InlineData("10.0.0.0/33")]
    public void AMalformedRangeFailsRatherThanBeingSkipped(string cidr)
    {
        // Skipping it silently would leave the limiter pooled while the configuration claimed
        // otherwise — a failure nobody would see until a print run started returning 429s.
        Should.Throw<InvalidOperationException>(() => TrustedProxyExtensions.ParseNetworks([cidr]));
    }

    [Fact]
    public void BlankEntriesAreIgnored()
    {
        var networks = TrustedProxyExtensions.ParseNetworks(["", "  ", VpcRange]);

        networks.Count.ShouldBe(1);
    }

    [Fact]
    public void IPv6RangesAreAccepted()
    {
        var networks = TrustedProxyExtensions.ParseNetworks(["fd00::/8"]);

        networks.Count.ShouldBe(1);
    }

    /// <summary>
    /// A range carrying host bits below its prefix is masked to the network address rather than
    /// rejected, so '10.0.1.5/16' still trusts the whole VPC. Pinned because it is the one place the
    /// move to <see cref="System.Net.IPNetwork"/> could have changed who is trusted, and did not.
    /// </summary>
    [Theory]
    [InlineData("10.0.1.5/16", "10.0.0.0/16")]
    [InlineData("fd00::1/8", "fd00::/8")]
    public void HostBitsBelowThePrefixAreMaskedRatherThanRejected(string cidr, string expected)
    {
        var networks = TrustedProxyExtensions.ParseNetworks([cidr]);

        networks.Count.ShouldBe(1);
        networks[0].ToString().ShouldBe(expected);
    }

    /// <summary>The masking is not cosmetic — a peer inside the range is still matched.</summary>
    [Fact]
    public async Task ARangeWrittenWithHostBitsStillTrustsItsProxies()
    {
        var remoteIp = await RemoteIpAfterMiddleware(
            LoadBalancer, RealClient.ToString(), "10.0.1.5/16");

        remoteIp.ShouldBe(RealClient);
    }
}
