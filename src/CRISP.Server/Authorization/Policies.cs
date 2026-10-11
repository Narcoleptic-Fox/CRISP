namespace CRISP.Server.Authorization;

/// <summary>
/// Central definition of all authorization policy names and role constants.
/// </summary>
public static class Policies
{
    /// <summary>
    /// Policy name for endpoints readable by viewers, editors, and administrators.
    /// </summary>
    public const string ReadAccess = "Api.Read";

    /// <summary>
    /// Policy name for endpoints writable by editors and administrators.
    /// </summary>
    public const string WriteAccess = "Api.Write";

    /// <summary>
    /// Policy name for endpoints restricted to administrators.
    /// </summary>
    public const string AdminAccess = "Api.Admin";
}
