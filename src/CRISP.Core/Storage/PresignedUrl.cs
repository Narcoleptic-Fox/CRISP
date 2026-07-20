namespace CRISP.Core.Storage;

/// <summary>
/// Represents a temporary URL and request requirements for direct object storage access.
/// </summary>
/// <param name="Url">The temporary URL clients should call.</param>
/// <param name="Method">The HTTP method required for the URL.</param>
/// <param name="ExpiresAt">The timestamp when the URL expires.</param>
/// <param name="RequiredHeaders">The headers clients must include when calling the URL.</param>
[ExportTsInterface]
public sealed record PresignedUrl(
    string Url,
    string Method,
    DateTimeOffset ExpiresAt,
    IReadOnlyDictionary<string, string> RequiredHeaders);