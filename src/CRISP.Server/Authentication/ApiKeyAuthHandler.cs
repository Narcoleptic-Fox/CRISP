using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using CRISP.Server.Data;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace CRISP.Server.Authentication;

/// <summary>
/// Authenticates requests that provide an API key in the configured request header.
/// </summary>
public sealed class ApiKeyAuthHandler(
    IOptionsMonitor<ApiKeyAuthOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IServiceScopeFactory scopeFactory)
    : AuthenticationHandler<ApiKeyAuthOptions>(options, logger, encoder)
{
    /// <summary>
    /// Validates the request API key and creates a claims principal for active stored keys.
    /// </summary>
    /// <returns>The authentication result for the current request.</returns>
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(ApiKeyAuthOptions.HeaderName, out var apiKeyValue))
            return AuthenticateResult.NoResult(); // Let other schemes handle it

        var apiKey = apiKeyValue.ToString();
        if (string.IsNullOrWhiteSpace(apiKey))
            return AuthenticateResult.Fail("API key is empty.");

        // Extract prefix for efficient lookup (first 8 chars)
        if (apiKey.Length < 8)
            return AuthenticateResult.Fail("Invalid API key format.");

        var prefix = apiKey[..8];
        var keyHash = ApiKeyAuthOptions.HashKey(apiKey);

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var storedKey = await db.ApiKeys
            .AsNoTracking()
            .FirstOrDefaultAsync(k =>
                k.Prefix == prefix &&
                k.KeyHash == keyHash &&
                k.IsActive);

        if (storedKey is null)
            return AuthenticateResult.Fail("Invalid API key.");

        if (storedKey.ExpiresAtUtc.HasValue && storedKey.ExpiresAtUtc.Value < DateTime.UtcNow)
            return AuthenticateResult.Fail("API key has expired.");

        // Update last used (fire-and-forget, non-critical)
        await Task.Run(async () =>
        {
            using var updateScope = scopeFactory.CreateScope();
            var updateDb = updateScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await updateDb.ApiKeys
                .Where(k => k.Id == storedKey.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(k => k.LastUsedAtUtc, DateTime.UtcNow));
        });

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, storedKey.Name),
            new(ClaimTypes.NameIdentifier, $"apikey:{storedKey.Id}"),
            new("api_key_id", storedKey.Id.ToString()),
            new("auth_type", "api_key"),
        };

        if (storedKey.RateLimitPerMinute.HasValue)
            claims.Add(new Claim("rate_limit", storedKey.RateLimitPerMinute.Value.ToString()));

        foreach (var role in storedKey.Roles)
            claims.Add(new Claim(ClaimTypes.Role, role.ToString()));

        var identity = new ClaimsIdentity(claims, ApiKeyAuthOptions.DefaultScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, ApiKeyAuthOptions.DefaultScheme);

        return AuthenticateResult.Success(ticket);
    }
}
