---
title: The template
description: How the repository root is the dotnet new template, what each option does, and the three rules that let one source tree serve every combination.
nav_order: 11
---

# The template

The CRISP repository is itself the `dotnet new crisp` template. There is no mirrored copy of the
source under a `templates/` folder; `.template.config/template.json` at the root describes what is
excluded, what is conditional and what is renamed, and `CRISP.Templates.csproj` packs the tree.
What is built and tested in the repository is exactly what is packed.

## Options

| Option | Values | Default | Effect |
| --- | --- | --- | --- |
| `--frontend` | `react`, `none` | `react` | The client project, its esproj reference from the server, the TypeGen targets, SpaProxy and the client CI job |
| `--include-sample` | `true`, `false` | `true` | The `Item` slice on both sides, its tests, and the shipped migration (its tables are Items) |
| `--database` | `postgres`, `mysql`, `sqlserver`, `sqlite` | `postgres` | The provider folder on both sides, the EF package, the Testcontainers package, the CI service container, the development settings, and whether a migration ships |
| `--auth` | `entra`, `none` | `entra` | Authentication and authorization wholesale; see [Authentication](/architecture/authentication/) |
| `--aspire` | `true`, `false` | `false` | An AppHost and ServiceDefaults; see below |
| `--skip-restore` | | `false` | Skip the restore after generation |

Only combinations the smoke matrix instantiates are verified. It samples every non-default value
against the defaults, the "everything off" corner, and each provider with its full integration
suite; see [Testing](/operations/testing/).

## Three rules for a single-source template

Every conditional branch is live code in the repository, which builds and tests. That shapes how
a choice can be expressed:

1. **No `//#else`, and no `//#if (!X)` with a live alternative.** Both branches compile in the
   repository, so two `var database = ...` lines or two assertions on the same value are duplicate
   definitions. Declare unconditionally, assign conditionally:

   ```csharp title="ItemFeature.cs"
   var v1Authorized = v1;
   //#if (IncludeAuth)
   v1Authorized = v1Authorized.RequireAuthorization();
   //#endif
   ```

2. **Alternatives that cannot share a file live beside the original under another name and are
   renamed in.** `appsettings.Development.sqlite.json` becomes `appsettings.Development.json` when
   SQLite is chosen; `main.noauth.tsx` becomes `main.tsx` without authentication; each
   `AppHost/Program.<provider>.cs` becomes `Program.cs`. The alternatives are inert in the
   repository (ASP.NET never loads `appsettings.Development.mysql.json`; Vite bundles only
   `main.tsx`) and still type-check, so they cannot rot silently. Where a whole class varies, it gets
   its own folder with the same class name inside and a template-only `Compile Remove`; that is
   how the [database providers](/architecture/database-providers/) work.

3. **A marker inside JSX or JSON is content, not a comment.** `//#if` between JSX children
   renders as text; four `"default"` keys in one JSON object is a parse error. Conditionals go
   outside the element, or the file is swapped by rename.

## Aspire

`--aspire true` adds `CRISP.AppHost` and `CRISP.ServiceDefaults`. The AppHost runs the database
container for the chosen provider, the Migrator, the server (which waits for the Migrator to
complete, so a fresh clone comes up migrated) and the Vite client, with the Aspire dashboard for
logs, traces and metrics. The database resource is named `default` so the injected connection
string is `ConnectionStrings__default`, the key the server already reads. ServiceDefaults adds
OpenTelemetry, service discovery, resilient `HttpClient` defaults and the `/alive` probe.

It is a development convenience. Deployment is the [Dockerfile and the Migrator](/operations/deployment/).

## Working on the template itself

```bash
dotnet test --project tests/CRISP.Template.Tests/CRISP.Template.Tests.csproj   # ~20 minutes, needs Docker
dotnet pack CRISP.Templates.csproj -c Release -o artifacts
```

`TemplateSmokeTests` installs the template from the checkout into a throwaway hive, instantiates
every variant in its matrix, checks exclusions and renames, scaffolds a migration where none
ships, builds, runs the generated Core tests, and for the provider and no-auth variants runs the
generated Server suite against a real engine. The `ci-template.yml` workflow runs it and uploads
the `.nupkg`.

Anything that exists only for the template repository (this test project, that workflow, the
template-only sections of the README) is excluded from generated output or guarded with the
`IsTemplateRepository` symbol, which is always false in generated output.
