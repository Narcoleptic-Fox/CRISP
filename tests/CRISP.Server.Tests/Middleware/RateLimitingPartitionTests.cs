using System.Net;
using CRISP.Server.Middleware;
using Shouldly;

namespace CRISP.Server.Tests.Middleware;

/// <summary>
/// Pins which requests the per-IP window applies to.
///
/// Layer 1 exists to blunt abuse arriving before anyone has identified themselves. Layer 2 then
/// governs fair usage per identity. Applying both to an authenticated user means the coarser of the
/// two decides, and the coarser one is keyed on something many users share.
/// </summary>
[Trait("Category", "Unit")]
public sealed class RateLimitingPartitionTests
{
    private static readonly IPAddress Client = IPAddress.Parse("73.12.34.56");

    [Fact]
    public void AnonymousTrafficIsPartitionedByItsAddress()
    {
        RateLimitingExtensions.ResolvePerIpPartitionKey(Client, isAuthenticated: false)
            .ShouldBe("73.12.34.56");
    }

    /// <summary>
    /// The change that stops one office sharing a bucket: a signed-in user is limited per identity
    /// by Layer 2, so the address they happen to share with colleagues stops mattering.
    /// </summary>
    [Fact]
    public void AuthenticatedTrafficIsLeftToTheIdentityLimiter()
    {
        RateLimitingExtensions.ResolvePerIpPartitionKey(Client, isAuthenticated: true)
            .ShouldBeNull();
    }

    [Theory]
    [InlineData("127.0.0.1")]
    [InlineData("::1")]
    public void LoopbackIsNeverLimited(string address)
    {
        RateLimitingExtensions.ResolvePerIpPartitionKey(IPAddress.Parse(address), isAuthenticated: false)
            .ShouldBeNull();
    }

    /// <summary>
    /// An address is not always available. Anonymous callers without one share a bucket rather than
    /// escaping the limiter — the safe direction for the guard that runs before authentication.
    /// </summary>
    [Fact]
    public void AnUnknownAddressStillGetsALimit()
    {
        RateLimitingExtensions.ResolvePerIpPartitionKey(null, isAuthenticated: false)
            .ShouldBe("unknown");
    }
}
