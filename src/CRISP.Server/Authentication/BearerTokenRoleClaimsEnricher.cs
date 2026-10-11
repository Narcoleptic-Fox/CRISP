using CRISP.Server.Data;
using CRISP.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CRISP.Server.Authentication;

/// <summary>
/// Adds local application role claims to validated bearer identities.
/// </summary>
public static class BearerTokenRoleClaimsEnricher
{
    /// <summary>
    /// Looks up the active application user for the bearer identity and adds its role as a standard role claim.
    /// </summary>
    /// <param name="principal">The validated bearer principal.</param>
    /// <param name="db">The application database context.</param>
    /// <param name="cancellationToken">A token that cancels the lookup.</param>
    public static async Task AddApplicationRoleClaimAsync(
        ClaimsPrincipal principal,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        if (principal.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
            return;

        var userKey = GetUserLookupKey(principal);
        if (string.IsNullOrWhiteSpace(userKey))
            return;

        var user = await db.Set<UserEntity>()
            .AsNoTracking()
            .Where(u => u.IsActive && u.Email == userKey)
            .Select(u => new { u.Role })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
            return;

        AddRoleClaim(identity, user.Role.ToString());
    }

    /// <summary>
    /// Gets the local user lookup key from common Microsoft identity email claims.
    /// </summary>
    /// <param name="principal">The authenticated principal.</param>
    /// <returns>The email-like lookup value, when present.</returns>
    public static string? GetUserLookupKey(ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.Email) ??
            principal.FindFirstValue("preferred_username") ??
            principal.FindFirstValue("upn") ??
            principal.Identity?.Name;
    }

    /// <summary>
    /// Adds a standard role claim when the identity does not already contain the same role.
    /// </summary>
    /// <param name="identity">The claims identity to enrich.</param>
    /// <param name="role">The application role value.</param>
    public static void AddRoleClaim(ClaimsIdentity identity, string role)
    {
        if (identity.HasClaim(ClaimTypes.Role, role))
            return;

        identity.AddClaim(new Claim(ClaimTypes.Role, role));
    }
}