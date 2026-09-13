# Storage, Auth, and Client Types

## File storage — presigned URLs

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

## Client type generation — TypeGen

Mark every Core contract that crosses the wire with `[ExportTsInterface]` (TypeGen, wired
through MSBuild). Build produces TypeScript interfaces the React client consumes, so the
client stays aligned with the server contract automatically. Adding a field to a Core record
flows to the client on the next build — **never hand-write a matching TS DTO**.

## Authentication & Authorization

- **Dual authentication scheme**: JWT Bearer (Entra / Microsoft.Identity.Web) **and** API key
  (`ApiKeyAuthHandler`). Registered via `AddApiAuthentication`.
- **Policy-based authorization** (`AddApiAuthorization`, `Authorization/Policies.cs`), roles
  `Viewer` / `Editor` / `Admin`. Apply with `.RequireAuthorization(...)` — feature groups call
  `RequireAuthorization()` by default; only explicitly anonymous endpoints (health checks) use
  `.AllowAnonymous()`.
- The Entra app registration is IT-managed — treat its config as fixed and keep secrets out of
  committed `appsettings` (use placeholders like `00000000-...` / `YOUR_DB_HOST`).

## `--auth none`

A solution generated without authentication has no `Authentication/`, `Authorization/`, Identity
feature, `Users` or `ApiKeys` tables, and no MSAL in the client: every endpoint is anonymous, the
audit interceptor and stored files record `System`, and `useCanWrite()` always answers yes. The
client swaps in `.noauth` versions of `main.tsx`, `lib/api.ts`, `protected-route.tsx`,
`useCurrentUser.ts` and `top-nav.tsx` by rename at generation time. For internal tools and
prototypes; anything reachable from outside wants `entra`.

## Security headers and the SPA Content-Security-Policy

`SecurityHeadersMiddleware` sets the OWASP headers on every response. API paths get
`default-src 'none'`; the SPA shell gets a policy **derived from configuration** rather than
hand-listed:

- the object-storage origin (virtual-hosted, path-style or `ServiceUrl`) is added to
  `connect-src` (presigned uploads) and `img-src` (presigned image URLs) when the S3 provider is on;
- `SecurityHeaders:ConnectSources` lists any other origin the browser calls directly, each reduced
  to scheme + host[:port] (a path or trailing slash would turn an origin into a single-URL rule);
- `worker-src 'self' blob:` and `script-src 'wasm-unsafe-eval'` are on (PDF renderers and barcode
  decoders need them; neither re-enables `eval()` or inline scripts);
- `SecurityHeaders:AllowCamera` (default **off**) flips `Permissions-Policy` to `camera=(self)`
  for apps that scan or photograph — `getUserMedia` is refused with no prompt otherwise.

`AddForwardedHeaders` trusts `X-Forwarded-For` only from the CIDR ranges in
`RateLimiting:TrustedProxyNetworks`; with nothing configured the feature is off, because the
framework middleware trusts *any* caller when both of its lists are empty. The per-IP rate limiter
exempts authenticated requests (the per-user limiter governs them) and loopback.

## Program.cs pipeline (order matters)

Registration order: database → authentication → authorization → rate limiting → CORS →
exception handling (`AddProblemDetails` + `AddExceptionHandler<GlobalExceptionHandler>`) →
supporting services (`HttpContextAccessor`, `TimeProvider.System`, email, storage, events,
`AddFeatures()`) → API versioning → FluentValidation → OpenAPI → Kestrel limits → health.

Middleware order: `UseForwardedHeaders` → `UseCorrelationId` → `UseExceptionHandler` / `UseStatusCodePages` →
`UseResponseCompression` → HTTPS/HSTS → `UseSecurityHeaders` → `UseCors` →
`UseAuthentication` → `UseRequestLogging` → `UseRateLimiter` → `UseAuthorization` →
`UseOutputCache` → `MapFeatures()` → health endpoints → `MapFallbackToFile("/index.html")`.
