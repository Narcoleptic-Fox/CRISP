using CRISP.Core.Identity;

namespace CRISP.Server.Data.Entities;

/// <summary>
/// Represents a persisted API key credential and its authorization metadata.
/// </summary>
public sealed class ApiKeyEntity : BaseEntity
{
    /// <summary>
    /// Gets or sets the display name for the API key.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the stored hash of the raw API key value.
    /// </summary>
    public required string KeyHash { get; set; }

    /// <summary>
    /// Gets or sets the lookup prefix stored separately from the hash for efficient queries.
    /// </summary>
    public required string Prefix { get; set; }

    /// <summary>
    /// Gets or sets the roles granted to the API key.
    /// </summary>
    public Roles[] Roles { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether the API key can currently authenticate requests.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the UTC timestamp when the API key was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the UTC timestamp when the API key expires, when configured.
    /// </summary>
    public DateTime? ExpiresAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the API key was last used.
    /// </summary>
    public DateTime? LastUsedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the optional per-minute request limit for this API key.
    /// </summary>
    public int? RateLimitPerMinute { get; set; }
}
