# Testing Strategy

The stack is **xUnit v3 on Microsoft.Testing.Platform (MTP)** — not MSTest, not classic
xUnit, and **never EF `UseInMemoryDatabase`**. Two test projects mirror the two source
projects.

## Core.Tests — fast unit tests

Validators and query helpers. Validator tests use FluentValidation's test extensions +
Shouldly:

```csharp
public sealed class ItemValidatorTests
{
    [Fact]
    public void CreateItemValidator_WithValidCommand_IsValid()
    {
        var result = new CreateItemValidator().TestValidate(
            new CreateItem { Name = "Widget", Description = "A standard widget", Owner = "Ada Lovelace" });
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateItemValidator_WithBlankName_ReturnsExpectedError()
    {
        var result = new CreateItemValidator().TestValidate(new CreateItem { Name = string.Empty });
        result.ShouldHaveValidationErrorFor(nameof(CreateItem.Name)).WithErrorMessage("Item name is required.");
        result.IsValid.ShouldBeFalse();
    }
}
```

## Server.Tests — handlers, middleware, and real-MySQL integration

Unit tests for the middleware and helpers (`RouteIdBinder`, `GlobalExceptionHandler`,
`SecurityHeadersMiddleware`, rate-limit partitioning, forwarded headers, scratch keys, audit
`Describe`) plus **integration tests against real MySQL**.

`IntegrationTestFixture` (one per collection) starts a `mysql:8.4` Testcontainer, or uses
`CRISP_TEST_MYSQL_CONNECTION_STRING` when set (CI does this with a service container). It applies
the **real migrations** — never `EnsureCreated`, which would hide a migration that disagrees with
the model — and resets state between tests with **Respawn** (ignoring `__EFMigrationsHistory`).
It refuses any database not named `crisp_tests`, because Respawn truncates every table it finds.

`CrispServerApplicationFactory` hosts the real `Program` in-process. Authentication in tests is
the `TestUser` scheme: send `X-Test-User: someone@example.test` and the request authenticates as
that email. The scheme fabricates **only** the identity; the role claim comes from the real
`BearerTokenRoleClaimsEnricher` reading the real `Users` table, so a test that seeds a Viewer
exercises stored role → claim → policy → endpoint rather than a faked principal.
`fixture.SeedUserAsync(Roles.Editor)` seeds and returns the email; `fixture.CreateClient(email)`
gives an `HttpClient` carrying the header.

Anything that uses `ExecuteUpdateAsync`, raw SQL, query filters, interceptors, or
provider-specific behaviour **must** be an integration test — the in-memory provider cannot model
it and is not referenced. Rule-only tests (validators) belong in `Core.Tests`.

`ItemEndpointTests` is the suite a new feature's tests copy.

## Template.Tests — the template itself

`tests/CRISP.Template.Tests` exists only in the template repository. It installs the template from
the checkout into a throwaway hive, instantiates every option combination, checks exclusions and
renames, builds the result and runs its Core tests. Slow (minutes); its own CI job.

## Running tests

Use the explicit `--project` flag — positional project paths are rejected on this SDK; MTP
support is enabled via `<TestingPlatformDotnetTestSupport>true`:

```powershell
dotnet test --project tests/CRISP.Core.Tests/CRISP.Core.Tests.csproj
dotnet test --project tests/CRISP.Server.Tests/CRISP.Server.Tests.csproj   # needs Docker or CRISP_TEST_MYSQL_CONNECTION_STRING
```

Test project packages: `xunit.v3.mtp-v2` (4.x — assembly parallelism is set with
`[assembly: Parallelization(Mode = ParallelMode.None)]` because the integration tests share one
database), `Testcontainers.MySql`, `Respawn`, `Shouldly`, `Microsoft.AspNetCore.Mvc.Testing`.
