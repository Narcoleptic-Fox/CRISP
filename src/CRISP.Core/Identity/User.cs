namespace CRISP.Core.Identity;

/// <summary>
/// Represents an application user.
/// </summary>
[ExportTsInterface]
public sealed record User : BaseModel
{
    /// <summary>
    /// Gets the unique username used to identify the user in the application.
    /// </summary>
    public string Username { get; init; } = default!;

    /// <summary>
    /// Gets the user's email address.
    /// </summary>
    public string Email { get; init; } = default!;

    /// <summary>
    /// Gets the application role assigned to the user.
    /// </summary>
    public Roles Role { get; init; }

    /// <summary>
    /// Gets a value indicating whether the user account can currently sign in or be assigned work.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Gets the user's per-account preferences.
    /// </summary>
    public UserSettings Settings { get; init; } = new();
}
