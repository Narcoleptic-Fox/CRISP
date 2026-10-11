using CRISP.Core.Storage;

namespace CRISP.Server.Data.ValueObjects;

/// <summary>
/// Represents persisted object storage metadata for a file attached to an entity.
/// </summary>
public sealed record StoredFileObject
{
    /// <summary>
    /// Gets the unique identifier for the stored file metadata record.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the provider object key used to access the file in storage.
    /// </summary>
    public string ObjectKey { get; init; } = string.Empty;

    /// <summary>
    /// Gets the original file name supplied by the client.
    /// </summary>
    public string OriginalFileName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the MIME content type for the file.
    /// </summary>
    public string ContentType { get; init; } = string.Empty;

    /// <summary>
    /// Gets the file size in bytes.
    /// </summary>
    public long SizeBytes { get; init; }

    /// <summary>
    /// Gets the timestamp when the file was uploaded.
    /// </summary>
    public DateTimeOffset UploadedOn { get; init; }

    /// <summary>
    /// Gets the user that uploaded the file.
    /// </summary>
    public string UploadedBy { get; init; } = string.Empty;

    /// <summary>
    /// Converts persisted storage metadata into the Core file contract.
    /// </summary>
    /// <returns>The Core stored file model without provider object-key details.</returns>
    public StoredFile ToStoredFile()
    {
        return new StoredFile
        {
            Id = Id,
            Name = OriginalFileName,
            ContentType = ContentType,
            SizeBytes = SizeBytes,
            UploadedOn = UploadedOn,
            UploadedBy = UploadedBy
        };
    }
}

/// <summary>
/// Provides projection helpers for stored file metadata.
/// </summary>
public static class StoredFileObjectExtensions
{
    extension(IQueryable<StoredFileObject> objects)
    {
        /// <summary>
        /// Projects stored file objects to Core stored file models in a queryable expression.
        /// </summary>
        /// <returns>A queryable projection to <see cref="StoredFile"/>.</returns>
        public IQueryable<StoredFile> ProjectToStoredFile() =>
            objects.Select(o => new StoredFile
            {
                Id = o.Id,
                Name = o.OriginalFileName,
                ContentType = o.ContentType,
                SizeBytes = o.SizeBytes,
                UploadedOn = o.UploadedOn,
                UploadedBy = o.UploadedBy
            });
    }
}
