namespace CRISP.Core.Identity;

/// <summary>
/// Represents an API key that can authenticate automated clients.
/// </summary>
[ExportTsInterface]
public sealed record ApiKey : BaseModel
{
    /// <summary>
    /// Gets the display name used to identify the API key.
    /// </summary>
    public string Name { get; init; } = default!;

    /// <summary>
    /// Gets the roles granted to requests authenticated with the API key.
    /// </summary>
    public Roles[] Roles { get; init; } = [];

    /// <summary>
    /// Gets a value indicating whether the API key can currently be used.
    /// </summary>
    public bool IsActive { get; init; } = true;

    /// <summary>
    /// Gets the UTC timestamp when the API key was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the UTC timestamp when the API key expires, when an expiration is configured.
    /// </summary>
    public DateTime? ExpiresAtUtc { get; init; }

    /// <summary>
    /// Gets the UTC timestamp when the API key was last used, when known.
    /// </summary>
    public DateTime? LastUsedAtUtc { get; init; }

    /// <summary>
    /// Gets the optional per-minute request limit for the API key.
    /// </summary>
    public int? RateLimitPerMinute { get; init; }
}
