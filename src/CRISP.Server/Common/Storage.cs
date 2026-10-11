using CRISP.Core.Storage;

namespace CRISP.Server.Common;

/// <summary>
/// Defines object storage operations used by application features.
/// </summary>
public interface IObjectStorage
{
    /// <summary>
    /// Creates a temporary URL for uploading an object directly to storage.
    /// </summary>
    /// <param name="request">The upload URL request.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>A presigned URL and any headers required by the storage provider.</returns>
    Task<PresignedUrl> CreateUploadUrlAsync(ObjectUploadUrlRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a temporary URL for downloading an object directly from storage.
    /// </summary>
    /// <param name="request">The download URL request.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>A presigned URL for the requested object.</returns>
    Task<PresignedUrl> CreateDownloadUrlAsync(ObjectDownloadUrlRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an object from storage.
    /// </summary>
    /// <param name="objectKey">The provider object key to delete.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>A task that completes when the object has been deleted.</returns>
    Task DeleteAsync(string objectKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Copies an object within the configured bucket.
    /// </summary>
    /// <param name="sourceKey">The key to copy from.</param>
    /// <param name="destinationKey">The key to copy to.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>A task that completes when the object has been copied.</returns>
    /// <remarks>
    /// The copy is what makes an attachment record trustworthy: it is performed by the server and
    /// fails if the source is not there, so a record written after it cannot describe bytes that do
    /// not exist. Callers must therefore await this before persisting anything that refers to the
    /// destination.
    /// </remarks>
    Task CopyAsync(string sourceKey, string destinationKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads an object's stored metadata.
    /// </summary>
    /// <param name="objectKey">The provider object key to inspect.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>The object's metadata, or <c>null</c> when no such object exists.</returns>
    Task<StoredObject?> GetObjectMetadataAsync(string objectKey, CancellationToken cancellationToken = default);

    Task<string> UploadAsync(string objectKey, Stream content, string contentType, CancellationToken cancellationToken = default);
    Task<byte[]> DownloadAsync(string objectKey, CancellationToken cancellationToken = default);
}

/// <summary>
/// What storage itself reports about an object, as opposed to what a client claimed when asking to
/// upload it.
/// </summary>
/// <param name="SizeBytes">The stored size in bytes.</param>
/// <param name="ContentType">The stored MIME content type.</param>
public sealed record StoredObject(long SizeBytes, string ContentType);

/// <summary>
/// Represents the provider-facing request for creating an object upload URL.
/// </summary>
/// <param name="ObjectKey">The server-generated object key for the upload.</param>
/// <param name="ContentType">The MIME content type that must be used for the upload.</param>
/// <param name="ExpiresInMinutes">The requested URL lifetime in minutes.</param>
public sealed record ObjectUploadUrlRequest(
    string ObjectKey,
    string ContentType,
    int ExpiresInMinutes);

/// <summary>
/// Represents the provider-facing request for creating an object download URL.
/// </summary>
/// <param name="ObjectKey">The server-side object key for the requested file.</param>
/// <param name="ExpiresInMinutes">The requested URL lifetime in minutes.</param>
/// <param name="OriginalFileName">Original filename used to set Content-Disposition so the browser saves rather than inlines the response.</param>
public sealed record ObjectDownloadUrlRequest(
    string ObjectKey,
    int ExpiresInMinutes,
    string? OriginalFileName = null);