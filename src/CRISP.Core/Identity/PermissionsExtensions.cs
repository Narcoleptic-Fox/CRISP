namespace CRISP.Core.Identity;
public static class PermissionsExtensions
{
    public static bool ContainsPermission(this IEnumerable<Permissions> permissions, Permissions permissionToCheck) =>
            permissions.Contains(permissionToCheck) || permissions.Contains(Permissions.AccessAll);

    public static string PackPermissionsIntoString(this IEnumerable<Permissions> permissions)
    {
        return permissions.Aggregate("", (s, permission) => s + (char)permission);
    }

    public static IEnumerable<Permissions> UnpackPermissionsFromString(this string packedPermissions)
    {
        if (packedPermissions == null)
            throw new ArgumentNullException(nameof(packedPermissions));
        foreach (char character in packedPermissions)
        {
            yield return ((Permissions)character);
        }
    }
}
