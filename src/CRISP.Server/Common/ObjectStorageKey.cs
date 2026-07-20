namespace CRISP.Server.Common;

/// <summary>
/// Provides validation and normalization helpers for object storage keys.
/// </summary>
public static class ObjectStorageKey
{
    /// <summary>
    /// Normalizes an object key into a storage-safe relative path.
    /// </summary>
    /// <param name="objectKey">The object key to normalize.</param>
    /// <returns>The normalized object key.</returns>
    /// <exception cref="ArgumentException">Thrown when the key is empty, too long, or contains unsafe path segments.</exception>
    public static string Normalize(string objectKey)
    {
        // A storage key must point to a concrete object; blank input would otherwise normalize to an unusable root key.
        if (string.IsNullOrWhiteSpace(objectKey))
            throw new ArgumentException("Object key is required.", nameof(objectKey));

        // Object storage keys use forward slashes as logical separators, so normalize Windows paths into the same form.
        var normalizedKey = objectKey.Trim().Replace('\\', '/');

        // Collapse repeated separators so equivalent keys resolve to one canonical value and do not create empty segments.
        while (normalizedKey.Contains("//", StringComparison.Ordinal))
        {
            normalizedKey = normalizedKey.Replace("//", "/", StringComparison.Ordinal);
        }

        // Keep keys relative to the configured bucket/prefix; callers should not be able to express an absolute path.
        normalizedKey = normalizedKey.TrimStart('/');
        var segments = normalizedKey.Split('/', StringSplitOptions.RemoveEmptyEntries);

        // Reject traversal segments after normalization so a key cannot escape its intended logical folder.
        if (segments.Length == 0 || segments.Any(segment => segment is "." or ".."))
            throw new ArgumentException("Object key contains an invalid path segment.", nameof(objectKey));

        // S3-compatible providers allow long keys, but 1024 characters is the practical object-key ceiling.
        if (normalizedKey.Length > 1024)
            throw new ArgumentException("Object key must not exceed 1024 characters.", nameof(objectKey));

        // Rejoin validated segments so the returned key is both normalized and free of empty path components.
        return string.Join('/', segments);
    }
}