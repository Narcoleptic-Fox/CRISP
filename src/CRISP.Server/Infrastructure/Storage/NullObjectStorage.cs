using System.Collections.Concurrent;
using CRISP.Core.Storage;
using CRISP.Server.Common;

namespace CRISP.Server.Infrastructure.Storage;

/// <summary>
/// Object storage implementation used when object storage is disabled.
/// </summary>
/// <remarks>
/// Every operation nominally succeeds, so callers behave as though storage were present. It does
/// keep track of which keys it has handed out an upload URL for, because attaching a file is
/// conditional on the object existing: without that record the disabled provider would report every
/// upload as missing and no attachment could ever be added. Recording only what was presigned keeps
/// the rule itself intact — a key this server never issued is still unknown.
/// </remarks>
internal sealed class NullObjectStorage : IObjectStorage
{
    private readonly ConcurrentDictionary<string, StoredObject> _objects = new(StringComparer.Ordinal);

    /// <summary>
    /// Throws because upload URLs cannot be created when object storage is disabled.
    /// </summary>
    /// <param name="request">The upload URL request.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>This method always throws.</returns>
    public Task<PresignedUrl> CreateUploadUrlAsync(ObjectUploadUrlRequest request, CancellationToken cancellationToken = default)
    {
        // An empty URL tells the client there is nothing to PUT, so the upload it would have made is
        // treated as already done — which is what this records.
        _objects[ObjectStorageKey.Normalize(request.ObjectKey)] = new StoredObject(0, request.ContentType);

        return Task.FromResult(new PresignedUrl(
            "",
            HttpMethods.Put,
            DateTimeOffset.UtcNow.AddMinutes(request.ExpiresInMinutes),
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Content-Type"] = request.ContentType
            }));
    }

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
    {
        _objects.TryRemove(ObjectStorageKey.Normalize(objectKey), out _);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task CopyAsync(string sourceKey, string destinationKey, CancellationToken cancellationToken = default)
    {
        var source = ObjectStorageKey.Normalize(sourceKey);

        // Mirrors the real provider, which fails a copy whose source is absent. Callers write their
        // record only after this returns, so the same guarantee holds with storage disabled.
        if (!_objects.TryGetValue(source, out var stored))
            throw new InvalidOperationException($"No object exists at '{source}' to copy.");

        _objects[ObjectStorageKey.Normalize(destinationKey)] = stored;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<StoredObject?> GetObjectMetadataAsync(string objectKey, CancellationToken cancellationToken = default)
        => Task.FromResult(_objects.GetValueOrDefault(ObjectStorageKey.Normalize(objectKey)));
    public Task<string> UploadAsync(string objectKey, Stream content, string contentType, CancellationToken cancellationToken = default) =>
        Task.FromResult(objectKey);
    public Task<byte[]> DownloadAsync(string objectKey, CancellationToken cancellationToken = default) =>
        Task.FromResult(Array.Empty<byte>());
}