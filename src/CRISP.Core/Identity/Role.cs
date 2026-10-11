namespace CRISP.Core.Identity;

/// <summary>
/// Defines the application roles used for coarse-grained authorization.
/// </summary>
[ExportTsEnum]
public enum Roles
{
    /// <summary>
    /// Grants read-only access to application data.
    /// </summary>
    Viewer,

    /// <summary>
    /// Grants permission to create or modify application data.
    /// </summary>
    Editor,

    /// <summary>
    /// Grants administrative access to application features and user management.
    /// </summary>
    Admin,
}
