namespace CRISP.Core.Storage;

/// <summary>
/// Represents file metadata exposed through Core contracts.
/// </summary>
/// <remarks>
/// This model intentionally omits provider-specific object keys so storage paths remain internal to the server.
/// </remarks>
[ExportTsInterface]
public sealed record StoredFile
{
    /// <summary>
    /// Gets the unique identifier for the stored file metadata record.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the original or display file name.
    /// </summary>
    public string Name { get; init; } = default!;

    /// <summary>
    /// Gets the file MIME content type.
    /// </summary>
    public string ContentType { get; init; } = default!;

    /// <summary>
    /// Gets the file size in bytes.
    /// </summary>
    public long SizeBytes { get; init; }

    /// <summary>
    /// Gets the timestamp when the file was uploaded.
    /// </summary>
    public DateTimeOffset UploadedOn { get; init; }

    /// <summary>
    /// Gets the identifier or display name of the user who uploaded the file.
    /// </summary>
    public string UploadedBy { get; init; } = string.Empty;
}
