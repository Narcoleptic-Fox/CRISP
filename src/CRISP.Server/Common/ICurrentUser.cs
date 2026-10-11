using CRISP.Core.Identity;

namespace CRISP.Server.Common;

public interface ICurrentUser
{
    /// <summary>
    /// Gets the audit display name for the current principal — the user's username,
    /// the API key's name, or "System" when no identity is present. Matches the
    /// string written to CreatedBy/UpdatedBy by the audit interceptor.
    /// </summary>
    string Username { get; }

    /// <summary>
    /// Gets the email claim of the current principal, when present. Returns null
    /// for API-key callers or unauthenticated contexts.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Resolves the provisioned <see cref="User"/> record for the current principal.
    /// Returns null when the caller is an API key or no matching account exists.
    /// </summary>
    ValueTask<User?> Get(CancellationToken cancellationToken = default);
}
