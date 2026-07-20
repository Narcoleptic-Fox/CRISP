using CRISP.Core.Storage;
using CRISP.Server.Common;

namespace CRISP.Server.Infrastructure.Storage;

/// <summary>
/// Object storage implementation used when object storage is disabled.
/// </summary>
internal sealed class NullObjectStorage : IObjectStorage
{
    /// <summary>
    /// Throws because upload URLs cannot be created when object storage is disabled.
    /// </summary>
    /// <param name="request">The upload URL request.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>This method always throws.</returns>
    public Task<PresignedUrl> CreateUploadUrlAsync(ObjectUploadUrlRequest request, CancellationToken cancellationToken = default)
        => Task.FromResult(new PresignedUrl(
            "",
            HttpMethods.Put,
            DateTimeOffset.UtcNow.AddMinutes(request.ExpiresInMinutes),
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Content-Type"] = request.ContentType
            })); //throw new InvalidOperationException("Object storage provider is disabled.");

    /// <summary>
    /// Throws because download URLs cannot be created when object storage is disabled.
    /// </summary>
    /// <param name="request">The download URL request.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>This method always throws.</returns>
    public Task<PresignedUrl> CreateDownloadUrlAsync(ObjectDownloadUrlRequest request, CancellationToken cancellationToken = default)
        => Task.FromResult(new PresignedUrl(
            "",
            HttpMethods.Get,
            DateTimeOffset.UtcNow.AddMinutes(request.ExpiresInMinutes),
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)));//throw new InvalidOperationException("Object storage provider is disabled.");

    /// <summary>
    /// Throws because objects cannot be deleted when object storage is disabled.
    /// </summary>
    /// <param name="objectKey">The provider object key to delete.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>This method always throws.</returns>
    public Task DeleteAsync(string objectKey, CancellationToken cancellationToken = default)
        => Task.CompletedTask; //throw new InvalidOperationException("Object storage provider is disabled.");
    public Task<string> UploadAsync(string objectKey, Stream content, string contentType, CancellationToken cancellationToken = default) =>
        Task.FromResult(objectKey);
    public Task<byte[]> DownloadAsync(string objectKey, CancellationToken cancellationToken = default) =>
        Task.FromResult(Array.Empty<byte>());
}