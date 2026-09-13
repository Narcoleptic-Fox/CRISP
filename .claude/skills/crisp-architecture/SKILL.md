---
name: crisp-architecture
description: Use when adding, modifying, or reviewing a feature in this solution (CRISP.Core / CRISP.Server) — writing a command, query, endpoint, EF entity, validator, event handler, or full vertical slice, wiring an IFeature, or checking code against the conventions in this solution. Applies to ASP.NET Core minimal-API CQRS code with immutable Core models and direct service injection (no mediator).
---

# CRISP architecture

CRISP is a vertical-slice, CQRS-flavoured ASP.NET Core architecture: explicit command/query
contracts, **direct service injection (no mediator)**, immutable Core models, minimal-API endpoints
with the handler service co-located in the same file. The `Item` slice in this repository is the
worked example you copy.

**Core principle:** copy the existing `Item` slice and rename — do not invent structure. When in
doubt about an exact signature, open the real file and mirror it. This skill points at the code and
the docs; it does not carry copies of either, so it cannot drift from them.

## When to use

- Adding or changing a feature: a command, query, endpoint, service handler, EF entity, validator,
  or event handler. Scaffolding a vertical slice or wiring an `IFeature`.
- Reviewing C# in this solution for convention adherence.
- The user mentions CRISP, vertical slice, `IFeature`, `ICreateService` / `IQueryService`.

**Not for:** the React client, infrastructure/Terraform, or non-CRISP .NET projects.

## Where things are

| Need | Read |
| --- | --- |
| Overview, project split, feature checklist, the rules | `docs/index.md` |
| CQRS primitives | `docs/contracts.md`, `src/CRISP.Core/Common/` |
| Slice anatomy, `IFeature`, keyed `DeleteCommand`, `PUT /{id}` | `docs/vertical-slice.md`, `src/CRISP.Server/Features/Items/` |
| Model ↔ entity, interceptors | `docs/models-and-entities.md`, `src/CRISP.Server/Data/` |
| Validation filter, `RouteIdBinder`, `RejectedException`, status map | `docs/validation-and-errors.md`, `src/CRISP.Server/Middleware/`, `src/CRISP.Server/Common/RejectedException.cs` |
| Event bus | `docs/events.md`, `src/CRISP.Server/Infrastructure/Events/` |
| Storage, auth, CSP, pipeline order | `docs/storage-auth-types.md`, `src/CRISP.Server/Program.cs` |
| Tests and how to run them | `docs/testing.md`, `tests/CRISP.Server.Tests/Integration/ItemEndpointTests.cs` |

## The worked example — files to copy

```
src/CRISP.Core/Items/                       Item.cs CreateItem.cs UpdateItem.cs GetItem.cs GetItems.cs GetItemOwnerNames.cs
src/CRISP.Server/Data/Entities/ItemEntity.cs
src/CRISP.Server/Data/Configurations/ItemConfiguration.cs
src/CRISP.Server/Events/ItemEvents.cs
src/CRISP.Server/Features/Items/            ItemFeature.cs + one <Operation>Endpoint.cs per operation + Events/ItemRenamedLogger.cs
tests/CRISP.Core.Tests/ItemValidatorTests.cs
tests/CRISP.Server.Tests/Integration/ItemEndpointTests.cs
```

## Non-negotiable rules

The high-judgment guardrails an agent gets wrong by default:

- **No mediator.** Inject the concrete contract (`ICreateService<CreateItem>`) into the endpoint
  and call `service.Send(...)`. Never add MediatR / `IMediator.Send`.
- **No automatic mapping.** Map model ⇄ entity by hand (`ToX()` / `ProjectToX()`). No AutoMapper.
- **Two types per concept.** Core model = immutable `record`; Server entity = mutable `class`.
  Don't collapse them; don't make the Core model mutable.
- **Validation is FluentValidation only**, run by the group-level `ValidationEndpointFilter`.
  Handlers do **not** call validators — they enforce only DB-dependent business rules (uniqueness,
  existence). There is no domain `Validate()` / `ValidationResult`.
- **Reject with `RejectedException.BadRequest / Forbidden / NotFound / Conflict(message)`.** It is
  the only exception whose message reaches the client outside Development. Do not throw
  `KeyNotFoundException` / `InvalidOperationException` for a user-facing rejection (they still map
  to a status, but their message is withheld because EF and LINQ throw the same types with entity
  and key names), and do not add exception subclasses.
- **Updates map `PUT /{id:guid}`** and send `command with { Id = id }`. `RouteIdBinder` has
  already put the route id on the command before validation; the body never repeats it.
- **Never hand-set audit/soft-delete fields** (`CreatedBy`/`UpdatedOn`/`IsDeleted`/`SystemMessage`)
  — `AuditLogInterceptor` / `SoftDeleteInterceptor` own them on `SaveChanges`.
- **Side effects via the in-process event bus**, not inline: publish a `BaseEvent` through
  `IEventPublisher`; react in a separate `IEventHandler<T>` registered in the feature.
- **Tests never use the EF in-memory provider.** Integration tests run against Testcontainers
  MySQL + Respawn through the real migrations; authenticate with the `X-Test-User` header. Run
  with `dotnet test --project <csproj>` (positional paths rejected; MTP).
- **Schema changes ship as a migration** (`dotnet ef migrations add`) applied by `CRISP.Migrator`.
  The server never migrates at startup.

When in doubt on exact signatures, open the matching `Item` file and mirror it — naming, file
layout, DI registration, and exception choices included.
