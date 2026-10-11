namespace CRISP.Server.Common;

/// <summary>
/// The scratch area's key layout, and the ownership rule that layout encodes.
/// </summary>
/// <remarks>
/// Namespacing by user id is what makes a scratch key safe to accept back from a client: the server
/// re-derives the expected prefix from the caller rather than trusting the key, so the worst a
/// stolen or guessed key can do is name an object its holder may not attach. Keys are normalized
/// before comparison, so no amount of <c>..</c> or duplicated separators can dress one prefix up as
/// another.
/// </remarks>
public static class ScratchKeys
{
    private const string Prefix = "scratch";

    /// <summary>Builds a fresh scratch key for a user.</summary>
    /// <param name="userId">The user the key is issued to.</param>
    /// <param name="extension">The original file's extension, kept so the stored object is recognisable.</param>
    /// <returns>The new scratch key.</returns>
    public static string For(Guid userId, string extension) =>
        $"{Prefix}/{userId}/{Guid.NewGuid()}{extension}";

    /// <summary>Determines whether a key is a scratch key issued to the given user.</summary>
    /// <param name="objectKey">The key supplied by the caller.</param>
    /// <param name="userId">The user the key must belong to.</param>
    /// <returns><see langword="true"/> when the key is theirs to attach.</returns>
    public static bool BelongTo(string objectKey, Guid userId) =>
        ObjectStorageKey.Normalize(objectKey)
            .StartsWith($"{Prefix}/{userId}/", StringComparison.Ordinal);
}
