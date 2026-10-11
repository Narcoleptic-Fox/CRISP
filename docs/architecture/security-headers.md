---
title: Security headers, forwarded headers and rate limiting
description: The OWASP response headers, how the SPA Content-Security-Policy is derived from configuration rather than hand-listed, trusted proxies, and the two-layer rate limiter.
nav_order: 8
---

# Security headers, forwarded headers and rate limiting

Three pieces of middleware decide what the browser may do with the responses, whose address a
request is attributed to, and how many requests that address or identity may make. Each of them
failed in production in a way that only showed up deployed; this page records what they do now
and why.

## Response headers

`SecurityHeadersMiddleware` sets on every response:

| Header | Value |
| --- | --- |
| `X-Content-Type-Options` | `nosniff` |
| `X-Frame-Options` | `DENY` |
| `Referrer-Policy` | `strict-origin-when-cross-origin` |
| `Permissions-Policy` | every capability denied; `camera=(self)` only when `SecurityHeaders:AllowCamera` is true. Without it `getUserMedia` is refused with no prompt, so an app that scans barcodes or takes photos must turn it on |
| `Content-Security-Policy` | `default-src 'none'; frame-ancestors 'none'` for `/api/*`; the SPA policy below for everything else |
| `Cache-Control`, `Pragma` | `no-store` for authenticated responses |

## The SPA Content-Security-Policy is derived

The policy served with the SPA shell is built once at startup from configuration:

- `script-src 'self' 'wasm-unsafe-eval'`: the second token lets the browser compile WebAssembly
  (barcode decoders, PDF layout engines ship as wasm). It does **not** re-enable `eval()` or inline
  scripts.
- `worker-src 'self' blob:`: libraries that build a worker from an object URL need `blob:`, and
  naming `worker-src` keeps `blob:` out of `script-src`.
- `style-src 'self' 'unsafe-inline'`: component libraries inject styles at runtime; scripts stay
  locked.
- `img-src 'self' data: blob:` plus the **object-storage origin**: presigned image URLs are
  rendered straight from the bucket, and `blob:` covers the preview of a file the user has just
  picked.
- `connect-src 'self' data: https://login.microsoftonline.com` plus the storage origin (presigned
  uploads PUT straight to the bucket) plus every entry in `SecurityHeaders:ConnectSources`.

The storage origin is computed from `ObjectStorageOptions` the same way the AWS SDK addresses the
bucket (`ServiceUrl`, path-style, or virtual-hosted), because the URL the server signs is the URL
the browser is asked to reach. Deriving it is what keeps the policy true by construction; the first
time it was hand-maintained, the bucket's CORS rule allowed the request and the page's own policy
blocked it before it was sent.

Each configured `ConnectSources` entry is reduced to scheme and host. A CSP source with a path or a
trailing slash is a path restriction, not an origin: `https://x/` matches only the root. A value
that is not an absolute `http(s)` URL is dropped rather than allowed to void the directive.

::: callout warning "This policy only governs pages this server serves"
In Development the SPA is served by Vite, which sets no CSP, so a `connect-src` problem does not
reproduce locally. It appears the first time the app is served from `wwwroot` by a deployed
container. The unit tests in `SecurityHeadersMiddlewareTests` are the check.
:::

## Forwarded headers

Behind a load balancer every request's `RemoteIpAddress` is the balancer. `AddForwardedHeaders`
reads CIDR ranges from `RateLimiting:TrustedProxyNetworks` and honours `X-Forwarded-For` only
when the immediate peer is inside one of them.

With nothing configured the feature is **off** (`ForwardedHeaders.None`). That is deliberate and
not the same as leaving the trusted lists empty: the framework middleware skips its trust check
entirely when both lists are empty and applies the header from any caller, which would let a client
choose its own rate-limit partition.

## Rate limiting

Two limiters are chained. The per-IP window is the pre-authentication abuse guard; the per-user
window governs fair usage once identity is known.

| Layer | Partition | Default |
| --- | --- | --- |
| Global | one bucket | `RateLimiting:GlobalPermitLimitPerMinute` = 50 |
| Per IP (anonymous) | remote address | `RateLimiting:AnonymousPermitLimitPerMinute` = 20 |
| Per user (authenticated) | user name | `RateLimiting:AuthenticatedPermitLimitPerMinute` = 100 |

`ResolvePerIpPartitionKey` returns no partition (no limit) for loopback and for any
**authenticated** request. Authenticated users share an address with everyone behind the same NAT
or load balancer; keeping them in the per-IP window meant the second user to do anything heavy hit
the limit the first one had spent. The function is pure so the rule is tested without a server.

The integration test factory lifts all three limits: the suite makes hundreds of requests a minute as
a handful of users, and a fast database turned the limits into `429`s mid-run.
