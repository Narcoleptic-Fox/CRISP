using CRISP.Core.Storage;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using CRISP.Server.Common;

namespace CRISP.Server.Infrastructure.Storage;

/// <summary>
/// Object storage implementation backed by Amazon S3 or an S3-compatible service.
/// </summary>
internal sealed class S3ObjectStorage(IOptions<ObjectStorageOptions> options, ILogger<S3ObjectStorage> logger) : IObjectStorage
{
    private readonly ObjectStorageOptions _options = options.Value;

    /// <summary>
    /// Creates a presigned PUT URL for uploading an object.
    /// </summary>
    /// <param name="request">The upload URL request.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>A presigned URL and required headers for the upload.</returns>
    public async Task<PresignedUrl> CreateUploadUrlAsync(ObjectUploadUrlRequest request, CancellationToken cancellationToken = default)
    {
        var objectKey = ObjectStorageKey.Normalize(request.ObjectKey);
        var expiresAt = GetExpiresAt(request.ExpiresInMinutes);
        using var client = CreateClient();
        var presignedRequest = new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = objectKey,
            Verb = HttpVerb.PUT,
            ContentType = request.ContentType,
            Expires = expiresAt.UtcDateTime
        };

        var url = await client.GetPreSignedURLAsync(presignedRequest);
        logger.LogDebug("Created presigned upload URL for object {ObjectKey}.", objectKey);

        return new PresignedUrl(
            url,
            HttpMethods.Put,
            expiresAt,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Content-Type"] = request.ContentType
            });
    }

    /// <summary>
    /// Creates a presigned GET URL for downloading an object.
    /// </summary>
    /// <param name="request">The download URL request.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>A presigned URL for the download.</returns>
    public async Task<PresignedUrl> CreateDownloadUrlAsync(ObjectDownloadUrlRequest request, CancellationToken cancellationToken = default)
    {
        var objectKey = ObjectStorageKey.Normalize(request.ObjectKey);
        var expiresAt = GetExpiresAt(request.ExpiresInMinutes);
        using var client = CreateClient();
        var presignedRequest = new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = objectKey,
            Verb = HttpVerb.GET,
            Expires = expiresAt.UtcDateTime
        };

        // Force the browser to save (rather than inline-render) by signing a
        // response-content-disposition override into the URL. Falls back to a
        // sanitized version of the object key if the original filename isn't
        // provided.
        var downloadName = string.IsNullOrWhiteSpace(request.OriginalFileName)
            ? Path.GetFileName(objectKey)
            : request.OriginalFileName!;
        presignedRequest.ResponseHeaderOverrides.ContentDisposition =
            BuildContentDisposition(downloadName);

        var url = await client.GetPreSignedURLAsync(presignedRequest);
        logger.LogDebug("Created presigned download URL for object {ObjectKey}.", objectKey);

        return new PresignedUrl(
            url,
            HttpMethods.Get,
            expiresAt,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Deletes an object from the configured bucket.
    /// </summary>
    /// <param name="objectKey">The provider object key to delete.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>A task that completes when the delete request finishes.</returns>
    public async Task DeleteAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        var normalizedKey = ObjectStorageKey.Normalize(objectKey);
        using var client = CreateClient();
        await client.DeleteObjectAsync(new DeleteObjectRequest
        {
            BucketName = _options.BucketName,
            Key = normalizedKey
        }, cancellationToken);

        logger.LogInformation("Deleted object {ObjectKey} from bucket {BucketName}.", normalizedKey, _options.BucketName);
    }

    // RFC 5987 / 6266: filename* uses percent-encoded UTF-8 so non-ASCII names
    // survive; filename= keeps a sanitized ASCII fallback for legacy clients.
    private static string BuildContentDisposition(string fileName)
    {
        var ascii = new string(fileName.Select(c => c < 0x20 || c == '"' || c == '\\' ? '_' : c).ToArray());
        var encoded = Uri.EscapeDataString(fileName);
        return $"attachment; filename=\"{ascii}\"; filename*=UTF-8''{encoded}";
    }

    private DateTimeOffset GetExpiresAt(int expiresInMinutes)
    {
        var minutes = expiresInMinutes > 0 ? expiresInMinutes : _options.DefaultPresignedUrlExpirationMinutes;
        minutes = Math.Clamp(minutes, 1, 60);

        return DateTimeOffset.UtcNow.AddMinutes(minutes);
    }

    private AmazonS3Client CreateClient()
    {
        var config = new AmazonS3Config
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(_options.Region),
            ForcePathStyle = _options.ForcePathStyle
        };

        if (!string.IsNullOrWhiteSpace(_options.ServiceUrl))
        {
            config.ServiceURL = _options.ServiceUrl;
        }

        if (!string.IsNullOrWhiteSpace(_options.AccessKey) && !string.IsNullOrWhiteSpace(_options.SecretKey))
        {
            return new AmazonS3Client(new BasicAWSCredentials(_options.AccessKey, _options.SecretKey), config);
        }

        return new AmazonS3Client(config);
    }

    public async Task<string> UploadAsync(string objectKey, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        var normalizedKey = ObjectStorageKey.Normalize(objectKey);
        using var client = CreateClient();
        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = normalizedKey,
            InputStream = content,
            ContentType = contentType
        };
        try
        {
            await client.PutObjectAsync(request, cancellationToken);
            logger.LogInformation("Uploaded object {ObjectKey} to bucket {BucketName}.", normalizedKey, _options.BucketName);
            return normalizedKey;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to upload object {ObjectKey} to bucket {BucketName}.", normalizedKey, _options.BucketName);
            throw;
        }
    }

    public async Task<byte[]> DownloadAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        var normalizedKey = ObjectStorageKey.Normalize(objectKey);
        using var client = CreateClient();
        try
        {
            var response = await client.GetObjectAsync(_options.BucketName, normalizedKey, cancellationToken);
            using var memoryStream = new MemoryStream();
            await response.ResponseStream.CopyToAsync(memoryStream, cancellationToken);
            logger.LogInformation("Downloaded object {ObjectKey} from bucket {BucketName}.", normalizedKey, _options.BucketName);
            return memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to download object {ObjectKey} from bucket {BucketName}.", normalizedKey, _options.BucketName);
            throw;
        }
    }
}