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

    Task<string> UploadAsync(string objectKey, Stream content, string contentType, CancellationToken cancellationToken = default);
    Task<byte[]> DownloadAsync(string objectKey, CancellationToken cancellationToken = default);
}

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