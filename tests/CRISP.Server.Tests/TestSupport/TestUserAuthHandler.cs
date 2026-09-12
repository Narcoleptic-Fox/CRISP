using System.Security.Claims;
using System.Text.Encodings.Web;
using CRISP.Server.Authentication;
using CRISP.Server.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRISP.Server.Tests.TestSupport;

/// <summary>
/// Authenticates a request as a seeded application user, named by the <c>X-Test-User</c> header.
/// </summary>
/// <remarks>
/// This deliberately does <em>not</em> fabricate role claims. It builds only the identity a
/// validated token would carry and then runs the real
/// <see cref="BearerTokenRoleClaimsEnricher.AddApplicationRoleClaimAsync"/> against the real
/// database — the same call the JWT bearer path makes on <c>OnTokenValidated</c>. So a test that
/// seeds a user with a particular role exercises the whole chain: stored value → enricher → claims →
/// policy → endpoint. Faking the claims here would skip the two steps most likely to be wrong.
/// </remarks>
internal sealed class TestUserAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder
) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    internal const string SchemeName = "TestUser";

    /// <summary>Carries the email of the user to authenticate as.</summary>
    internal const string UserHeader = "X-Test-User";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(UserHeader, out var header))
            return AuthenticateResult.NoResult();

        var email = header.ToString();
        if (string.IsNullOrWhiteSpace(email))
            return AuthenticateResult.Fail($"'{UserHeader}' was empty.");

        // What a validated bearer token gives us before enrichment: who you are, nothing about what
        // you may do. An unprovisioned identity legitimately stops here — authenticated, but holding
        // no role — which is exactly how production behaves.
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, email), new Claim(ClaimTypes.Email, email)],
            SchemeName
        );
        var principal = new ClaimsPrincipal(identity);

        var db = Context.RequestServices.GetRequiredService<ApplicationDbContext>();
        await BearerTokenRoleClaimsEnricher.AddApplicationRoleClaimAsync(
            principal,
            db,
            Context.RequestAborted
        );

        return AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName));
    }
}
