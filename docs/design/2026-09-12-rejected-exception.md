---
title: One RejectedException instead of an exception hierarchy
nav_title: RejectedException
description: Replace the typed business-exception hierarchy with a single sealed RejectedException carrying an HTTP status, and stop exposing framework exception messages to clients.
date: 2026-09-12
status: accepted
---

# One RejectedException instead of an exception hierarchy

**Decided:** 2026-09-12, option B, by Brock.

## Context

The template's `GlobalExceptionHandler` mapped a hierarchy of exceptions to status codes:
`NotFoundException`, `ConflictException`, `ForbiddenException`, `ValidationException`, each a
subclass of a business base. Production applications had grown the hierarchy differently (one added
`PreconditionFailedException`, another `GoneException`) and two of them exposed the message of
*every* exception to the client, including `DbUpdateException` text with table and column names.

Two problems were tangled together: which exceptions are safe to show, and how to tell the handler
which status to send.

## Options

|       | Option                                                                                                                                                                                                                                                                                                                                                  | Taken?                                                                                                                                                  |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A     | Keep the hierarchy; add an `ISafeToExpose` marker interface to the business base; map everything else to 500 with a generic message                                                                                                                                                                                                                     | No. A hierarchy invites growth; every new status is a new type, and a type cannot express "this one is safe and that one is not" better than a flag can |
| **B** | **One sealed `RejectedException(HttpStatusCode, string)` with static factories `BadRequest`, `Forbidden`, `NotFound`, `Conflict`. Its message is the only message that reaches a client outside Development. Framework exceptions map to a status by type (`DbUpdateConcurrencyException` to 409, and so on) with `Detail` filled only in Development** | **Yes**                                                                                                                                                 |
| C     | Result types (`Result<T>`) instead of exceptions                                                                                                                                                                                                                                                                                                        | No. Every handler signature changes, every endpoint unwraps, and the filter and interceptors that already throw would need a parallel path              |

## Decision

Option B. `RejectedException` is `sealed`; adding a status means adding a factory method, not a
type. "Rejected" was chosen over "Business" or "Domain" because it says what happened to the
request.

## Consequences

- A handler says `throw RejectedException.Conflict($"Item with name '{name}' already exists.")` and
  the client sees exactly that text with a 409. Nothing else a handler throws is shown.
- `ProblemDetails.Detail` for framework exceptions is present in Development and absent elsewhere;
  the integration tests assert both.
- Production applications migrating to this pattern replace their hierarchy with factory calls;
  the one with `PreconditionFailedException` adds a `PreconditionFailed` factory locally.
- Watch: a `RejectedException` message is user-facing text. Never put an id from another tenant,
  a connection string, or an exception's `ToString()` in one.
