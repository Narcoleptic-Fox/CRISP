namespace CRISP.Core.Constants;

/// <summary>
/// Provides custom claim type names used by authorization policies.
/// </summary>
public static class ClaimNames
{
    /// <summary>
    /// The claim containing the permission values granted to the authenticated principal.
    /// </summary>
    public const string Permissions = nameof(Permissions);
}
