---
title: Configuration
description: Every setting the server and the Migrator read, its default, and where the value should come from in each environment.
nav_order: 2
---

# Configuration

Settings come from `appsettings.json`, then `appsettings.{Environment}.json`, then environment
variables (`Section__Key`). Committed files carry structure and safe defaults; secrets arrive from
the environment. Use this page to find what a key does; the page for the concern explains why.

## Where values come from

| Environment | Non-secrets | Secrets |
| --- | --- | --- |
| Development | `appsettings.Development.json` (committed) | `${NAME}` tokens in the connection string, expanded from the shell (`DBPASS`); `dotnet user-secrets` for the rest |
| CI | Workflow `env:` | Workflow `env:` for throwaway containers; repository secrets otherwise |
| Deployed | `appsettings.json` plus container environment | The platform's secret store injected as environment variables |

`${NAME}` expansion applies to `ConnectionStrings:default` only, wherever the server or the
Migrator read it. A token whose variable is unset stops startup with
`The connection string references ${NAME} but the environment variable 'NAME' is not set.`

## Server

### Database

| Key | Default | Notes |
| --- | --- | --- |
| `ConnectionStrings:default` | none (required) | Provider dialect; see [Database providers](/architecture/database-providers/). Env form: `ConnectionStrings__default` |

### Authentication (`--auth entra`)

| Key | Default | Notes |
| --- | --- | --- |
| `AzureAd:Instance` | `https://login.microsoftonline.com/` | |
| `AzureAd:TenantId` | placeholder | From the app registration |
| `AzureAd:ClientId` | placeholder | From the app registration |
| `AzureAd:Audience` | placeholder | What the API expects in `aud` |
| `AzureAd:Domain` | placeholder | Tenant domain |

API keys need no configuration; they are rows in `ApiKeys`, stored as SHA-256 hashes.

### Rate limiting and proxies

| Key | Default | Notes |
| --- | --- | --- |
| `RateLimiting:GlobalPermitLimitPerMinute` | 50 | One bucket for the whole process |
| `RateLimiting:AuthenticatedPermitLimitPerMinute` | 100 | Per user name |
| `RateLimiting:AnonymousPermitLimitPerMinute` | 20 | Per remote address; authenticated and loopback callers are exempt |
| `RateLimiting:TrustedProxyNetworks` | empty (feature off) | CIDR list; `X-Forwarded-For` is honoured only from these |

### Security headers and CORS

| Key | Default | Notes |
| --- | --- | --- |
| `SecurityHeaders:ConnectSources` | empty | Extra origins for `connect-src`; each reduced to scheme and host |
| `SecurityHeaders:AllowCamera` | `false` | `Permissions-Policy: camera=(self)` when true |
| `Cors:AllowedOrigins` | the two local client URLs | Only matters when the client is served from another origin |

### Object storage

| Key | Default | Notes |
| --- | --- | --- |
| `ObjectStorage:Provider` | `None` | `None` or `S3` |
| `ObjectStorage:BucketName` | empty | Required for `S3` |
| `ObjectStorage:Region` | `us-east-1` | |
| `ObjectStorage:ServiceUrl` | unset | S3-compatible endpoint (MinIO, R2) |
| `ObjectStorage:ForcePathStyle` | `false` | Required by most S3-compatible servers |
| `ObjectStorage:AccessKey`, `ObjectStorage:SecretKey` | unset | Omit on AWS to use the SDK credential chain (instance role, profile) |
| `ObjectStorage:DefaultPresignedUrlExpirationMinutes` | 15 | |

### Email

| Key | Default | Notes |
| --- | --- | --- |
| `Email:Provider` | `None` | `None`, `Smtp` or `Ses` |
| `Email:From` | `noreply@example.com` | |
| `Email:Region` | `us-east-1` | SES only |
| `Email:Smtp:Host`, `Port` (25), `EnableSsl` (false), `Username`, `Password` | | SMTP only; the password is a secret |

### Events

| Key | Default | Notes |
| --- | --- | --- |
| `Events:MaxAttempts` | 3 | Handler retries before the event is dropped with an error log |
| `Events:RetryBaseDelay` | `00:00:02` | Doubles per attempt |

### Present but not read

`AWS:Profile`, `AWS:Region` and `Site:TimeZone` are in `appsettings.json` because production
applications built on the template use them (SDK profile selection, display time zone). The
template's own code reads neither. Delete them or start reading them; do not leave them as
documentation.

## Migrator

Read from the process environment, or from `.env` / `.env.local` beside the project (both
gitignored; `.env.example` shows the shape).

| Variable | Default | Notes |
| --- | --- | --- |
| `ConnectionStrings__default` | | Wins over the parts below; `${NAME}` tokens expand |
| `DATABASE_HOST`, `DATABASE_NAME`, `DATABASE_USER` | | All three required when no connection string is given |
| `DATABASE_PASSWORD` | | |
| `DATABASE_PORT` | 5432 | Set it for MySQL (3306) and SQL Server (1433) |
| `BASTION_HOST` | unset | When set, an SSH tunnel is opened and the parts form are required |
| `BASTION_USERNAME`, `BASTION_KEY_PATH` | | Private key file must exist |
| `LOCAL_FORWARD_PORT` | 33700 | Local end of the tunnel |

## Tests

| Variable | Notes |
| --- | --- |
| `CRISP_TEST_POSTGRES_CONNECTION_STRING` (or `MYSQL`, `SQLSERVER`) | Use this server instead of a Testcontainer. The database **must** be named `crisp_tests`; the fixture refuses anything else because it truncates every table |
