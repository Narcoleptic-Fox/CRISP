---
title: File storage
description: Presigned-URL upload and download against S3-compatible storage, the IHasStoredFiles marker, server-side copy, and user-scoped scratch keys.
nav_order: 7
---

# File storage

Files are uploaded/downloaded **directly to object storage via presigned URLs**; the server
never proxies bytes. Core exposes provider-agnostic contracts (`StoredFile`,
`CreatePresignedUploadUrl`, `CreatePresignedDownloadUrl`, `PresignedUrl`); the Server hides
S3/MinIO keys behind `IObjectStorage` (`S3ObjectStorage`, with `NullObjectStorage` for local).

```csharp
public interface IObjectStorage
{
    Task<PresignedUrl> CreateUploadUrlAsync(ObjectUploadUrlRequest request, CancellationToken ct = default);
    Task<PresignedUrl> CreateDownloadUrlAsync(ObjectDownloadUrlRequest request, CancellationToken ct = default);
    Task DeleteAsync(string objectKey, CancellationToken ct = default);
    Task<string> UploadAsync(string objectKey, Stream content, string contentType, CancellationToken ct = default);
}
```

Any entity that owns files implements the `IHasStoredFiles` marker
(`List<StoredFileObject> Files`); the storage feature uses it to associate uploads with the
owning record. Object keys, buckets, and paths stay server-side — clients only ever see
`StoredFile.Id` and presigned URLs. Wire with `AddObjectStorageInfrastructure(...)`.

## Scratch keys and the attach step

A browser uploads to a **scratch key** first and attaches afterwards. `ScratchKeys.For(userId,
extension)` issues `scratch/{userId}/{guid}{ext}`; when the client later asks to attach that key to
a record, `ScratchKeys.BelongTo(key, userId)` re-derives the expected prefix from the caller and
compares it against the **normalised** key. No amount of `..` or doubled separators can make one
user's prefix look like another's, and a key this server never issued is simply not theirs.

`IObjectStorage.CopyAsync` performs the move from scratch to the final key on the server, and it
fails when the source is absent. The record describing the file is written only after the copy
returns, so a record can never describe bytes that do not exist. `GetObjectMetadataAsync` reports
what storage actually holds (size, content type) as opposed to what the client claimed when asking
for the upload URL.

`NullObjectStorage`, the provider for local work, remembers which keys it has presigned so the same
attach flow works with storage disabled, and refuses to copy a key it never issued.

## Configuration

| Key | Meaning |
| --- | --- |
| `ObjectStorage:Provider` | `None` (default) or `S3` |
| `ObjectStorage:BucketName` | The bucket |
| `ObjectStorage:Region` | AWS region, default `us-east-1` |
| `ObjectStorage:ServiceUrl` | An S3-compatible endpoint such as MinIO; when set, addressing uses it |
| `ObjectStorage:ForcePathStyle` | Path-style addressing (`https://s3.{region}.amazonaws.com/{bucket}/...`) |
| `ObjectStorage:AccessKey`, `SecretKey` | Static credentials; omit to use the AWS SDK's credential chain |
| `ObjectStorage:DefaultPresignedUrlExpirationMinutes` | Default 15 |

The configured bucket origin also feeds the client's Content-Security-Policy; see
[Security headers](/architecture/security-headers/).
