using System.ComponentModel.DataAnnotations;

namespace CRISP.Server.Infrastructure.Storage;

/// <summary>
/// Configures object storage provider access and presigned URL behavior.
/// </summary>
public sealed class ObjectStorageOptions
{
    /// <summary>
    /// The configuration section name for object storage options.
    /// </summary>
    public const string SectionName = "ObjectStorage";

    /// <summary>
    /// Gets the configured object storage provider name.
    /// </summary>
    public string Provider { get; init; } = ObjectStorageProviders.None;

    /// <summary>
    /// Gets the object storage bucket name.
    /// </summary>
    public string BucketName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the object storage region.
    /// </summary>
    public string Region { get; init; } = "us-east-1";

    /// <summary>
    /// Gets the optional custom service URL used by S3-compatible providers.
    /// </summary>
    public string? ServiceUrl { get; init; }

    /// <summary>
    /// Gets the optional access key for explicit provider credentials.
    /// </summary>
    public string? AccessKey { get; init; }

    /// <summary>
    /// Gets the optional secret key for explicit provider credentials.
    /// </summary>
    public string? SecretKey { get; init; }

    /// <summary>
    /// Gets a value indicating whether path-style addressing should be used for S3 requests.
    /// </summary>
    public bool ForcePathStyle { get; init; }

    /// <summary>
    /// Gets the default lifetime for presigned URLs when a request does not supply one.
    /// </summary>
    [Range(1, 60)]
    public int DefaultPresignedUrlExpirationMinutes { get; init; } = 15;
}

/// <summary>
/// Provides supported object storage provider names.
/// </summary>
public static class ObjectStorageProviders
{
    /// <summary>
    /// Disables object storage operations.
    /// </summary>
    public const string None = "None";

    /// <summary>
    /// Uses Amazon S3 or an S3-compatible provider.
    /// </summary>
    public const string S3 = "S3";
}