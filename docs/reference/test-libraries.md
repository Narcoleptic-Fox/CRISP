---
title: Test libraries
description: The test frameworks and helpers on both sides, and the rule that the EF in-memory provider is never one of them.
nav_order: 3
---

# Test libraries

## .NET

| Package | Why |
| --- | --- |
| `xunit.v3.mtp-v2` | xUnit v3 running on Microsoft.Testing.Platform. `dotnet test --project` runs the test executable directly; `global.json` sets `"test": { "runner": "Microsoft.Testing.Platform" }`. Not MSTest, not classic xUnit |
| `Shouldly` | Assertions that read as sentences and report both values on failure (`actual.ShouldBe(expected)`) |
| `Microsoft.AspNetCore.Mvc.Testing` | `WebApplicationFactory`, the basis of `CrispServerApplicationFactory`, which boots the real `Program` with test configuration injected early enough to be seen |
| `Testcontainers.PostgreSql` / `MySql` / `MsSql` | Starts a real database engine in Docker for the integration suite. One package per provider; a solution receives the one for its `--database` |
| `Respawn` | Resets the database between tests by deleting rows in dependency order, far faster than dropping and recreating, while keeping the schema the migrations produced |

### Not used, deliberately

| Candidate | Why not |
| --- | --- |
| `Microsoft.EntityFrameworkCore.InMemory` | Cannot model global query filters, interceptors or provider SQL. A suite on it proves the wrong database works |
| `Moq` / `NSubstitute` | Handlers are tested against the real database and middleware against a real pipeline; the few seams that need a fake (`IObjectStorage`, `IEmailSender`, the clock) have hand-written test doubles in `TestSupport/` |
| `AutoFixture.Xunit3`, `coverlet.collector` | Pinned in `Directory.Packages.props` for applications that want them; no project in the template references them |

## Client

| Package | Why |
| --- | --- |
| `vitest`, `@vitest/coverage-v8` | The test runner Vite projects use; shares the Vite config |
| `@testing-library/react`, `@testing-library/user-event`, `@testing-library/jest-dom` | Component tests that interact the way a user does |
| `jsdom` | The DOM for those tests |

::: callout warning "The client suite runs with `--passWithNoTests`"
There are no client tests in the template yet, so `npm run test:run` passes by having nothing
to fail. Remove that flag the day the first test exists.
:::
