---
title: Server libraries
description: The NuGet packages in CRISP.Core, CRISP.Server, CRISP.Migrator and the Aspire projects, and why each is in the stack.
nav_order: 1
---

# Server libraries

Versions are pinned once in `Directory.Packages.props` with central package management and
transitive pinning on, so a vulnerable transitive dependency can be raised without waiting for its
parent. Packages marked *option* appear only in solutions generated with that option.

## CRISP.Core

| Package | Why |
| --- | --- |
| `FluentValidation` | The one validation mechanism. Rules are classes beside the command they validate, testable without a host, and run by the endpoint filter. Data annotations were rejected because they cannot express cross-field rules or rules that read configuration |
| `TypeGen` | Generates the client's TypeScript from Core records at build time. Chosen over OpenAPI-driven generators because it works from the C# types directly, needs no running server, and the output is one interface per record with no client wrapper to maintain |

## CRISP.Server

### ASP.NET Core and identity

| Package | Why |
| --- | --- |
| `Asp.Versioning.Http` | URL-segment API versioning (`/api/v1/`) on minimal APIs. Production applications have needed a `v2` of one feature while `v1` stayed deployed |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | Validates Entra ID access tokens for the bearer half of `DualAuth` |
| `Microsoft.Identity.Web`, `Microsoft.Identity.Web.DownstreamApi` | Entra ID configuration from the `AzureAd` section and token acquisition for calling downstream APIs on the user's behalf (`EnableTokenAcquisitionToCallDownstreamApi`). *option `--auth entra`* |
| `Microsoft.AspNetCore.Authentication.OpenIdConnect` | A dependency of `Microsoft.Identity.Web`; referenced explicitly so its version is pinned with the rest of the ASP.NET Core packages rather than floating with Identity.Web |
| `Microsoft.AspNetCore.OpenApi` | The OpenAPI document, served in Development only, with the security schemes added by `SecuritySchemeTransformer` |
| `Microsoft.AspNetCore.SpaProxy` | Starts Vite and proxies to it during `dotnet run`, so one command runs both halves. *option `--frontend react`* |
| `Microsoft.Extensions.Http.Resilience`, `Microsoft.Extensions.ServiceDiscovery` | Standard resilience handlers and service discovery for outbound `HttpClient`s; what ServiceDefaults configures when Aspire is on, and harmless defaults otherwise |
| `Microsoft.VisualStudio.Azure.Containers.Tools.Targets` | Lets Visual Studio's Docker launch profile build and debug the container from the Dockerfile |

### Entity Framework Core

| Package | Why |
| --- | --- |
| `Microsoft.EntityFrameworkCore.Design`, `Microsoft.EntityFrameworkCore.Tools` | `dotnet ef` migrations against this project; the design-time factory uses the provider's fixed configuration so no database is needed to scaffold |
| `Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore` | The database error page in Development, which turns "no such table" into "you have a pending migration" |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | The default provider. *option `--database postgres`* |
| `Microting.EntityFrameworkCore.MySql` | The Pomelo fork that targets EF Core 10; Pomelo itself stopped at 9. *option `--database mysql`* |
| `Microsoft.EntityFrameworkCore.SqlServer` | *option `--database sqlserver`* |
| `Microsoft.EntityFrameworkCore.Sqlite` | *option `--database sqlite`* |

### Infrastructure

| Package | Why |
| --- | --- |
| `AWSSDK.S3` | Presigned URLs, server-side copy and metadata for [file storage](/architecture/storage/). Also the client for any S3-compatible store (MinIO, R2) through `ServiceUrl` |
| `AWSSDK.SimpleEmailV2` | The `Ses` email provider. The `Smtp` provider uses the framework's `SmtpClient` and needs nothing |
| `FluentValidation.DependencyInjectionExtensions` | Registers every validator in Core by assembly scan so the endpoint filter can resolve them |
| `MimeTypes` | Extension to content-type lookup when presigning an upload, as a source-only package (`PrivateAssets=all`) so nothing ships at runtime |

## CRISP.Migrator

| Package | Why |
| --- | --- |
| `DotNetEnv` | Reads `.env` / `.env.local` beside the project so a developer can keep bastion and database settings out of the shell profile |
| `SSH.NET` | Opens the SSH tunnel through a bastion to a private database. Managed code, no `ssh` binary on the runner required |

## CRISP.AppHost and CRISP.ServiceDefaults (*option `--aspire`*)

| Package | Why |
| --- | --- |
| `Aspire.Hosting.AppHost` | The orchestrator |
| `Aspire.Hosting.PostgreSQL` / `MySql` / `SqlServer` | The database container resource for the chosen provider; only one is referenced |
| `Aspire.Hosting.JavaScript` | Runs the Vite client as an Aspire resource |
| `OpenTelemetry.*` | Traces, metrics and logs to the Aspire dashboard (OTLP exporter; ASP.NET Core, HTTP and runtime instrumentation) |

## Not in the stack, on purpose

| Candidate | Why not |
| --- | --- |
| MediatR or any mediator | One indirection fewer; the contract interface is injected directly and the compiler checks the wiring |
| AutoMapper or Mapster | The queryable projection is what EF translates; a mapper hides which columns are loaded |
| Serilog | The framework logger with structured templates is enough until a sink needs it; add it in the application, not the template |
| Polly directly | `Microsoft.Extensions.Http.Resilience` wraps it with sensible defaults |
