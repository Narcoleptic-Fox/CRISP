namespace CRISP.Core.Identity;

/// <summary>
/// Represents user-specific application preferences.
/// </summary>
[ExportTsInterface]
public sealed record UserSettings
{
    /// <summary>
    /// Gets a value indicating whether the user allows notifications.
    /// </summary>
    public bool AllowNotifications { get; init; } = true;
}
