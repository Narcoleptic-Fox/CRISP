# CRISP memory bank

Session-to-session context that is not derivable from the code or git history. Read at session
start. Dated entries; delete what stops being true.

## Current state (2026-09-12)

- **Active branch: `modernize`** (from `origin/modernization_update`). Steps 1-5 done 2026-09-12:
  boots, Migrator applies InitialCreate, 25 Core + 118 Server tests green (21 of them integration
  on Testcontainers MySQL), every fix sabotage-verified. Next: step 6 (`templates/` +
  `template.json` options), step 7 (carry docs/.editorconfig/Directory.Build.props from master),
  step 8 (skill into repo), step 9 (remaining version bumps incl. Microsoft.OpenApi NU1903).
- Integration tests need Docker (Testcontainers `mysql:8.4`) or `CRISP_TEST_MYSQL_CONNECTION_STRING`
  pointing at a database literally named `crisp_tests` (the fixture refuses anything else; Respawn
  truncates every table). Auth in tests: `X-Test-User: <email>` header → real enricher.
- `SecurityHeaders:ConnectSources` and `SecurityHeaders:AllowCamera` are the new config knobs
  (replacing WAR's `Csp:ConnectSources` and the hardcoded `camera=(self)`). `PUT /Items/{id}`
  is the sample's update shape; `RouteIdBinder` fills `Id` before validation.
- Local dev needs a MySQL: `docker run -d --name crisp-mysql -e MYSQL_ROOT_PASSWORD=password
  -e MYSQL_DATABASE=crisp -p 3307:3306 mysql:8.4` and the gitignored
  `src/CRISP.Server/appsettings.Development.json` with
  `server=127.0.0.1;port=3307;...` (use 127.0.0.1, not localhost: the IPv6 attempt stalls).
  The template should ship a Development settings file; today it does not.
- Run tests with `dotnet test --project tests/<proj>.csproj` (MTP; positional path is rejected).

- Branch `feat/authz-eventing-tests` holds the Sep 2026 working-tree commits (authz, eventing,
  55 in-memory tests, template gating). Not pushed. `master` == `origin/master` at `f76b5b3`.
- `origin/modernization_update` (Jul 20 2026, 1 commit) is **the baseline**: webapp-starter's
  production shape ported into CRISP namespaces with ~6 of the 17 review findings already
  applied. It dropped `templates/`, `docs/`, the `Item` sample and all Server tests, and was never
  booted (`UseOutputCache` without `AddOutputCache`). Branch from it; carry `templates/`, `docs/`,
  `.editorconfig`, `Directory.Build.props` forward from `master`. Gap table: review section 6.
- The template has drifted from production. Six production snapshots live at
  `D:\Dev\dotnet\CRISP_Production\*-2026-08-28`; `webapp-starter` is the canonical shape the
  `_claude-crisp` skill describes. Full review and 10-step plan:
  `docs/reviews/2026-09-12-production-consolidation.md`.

## Decisions made

- 2026-09-12 **Error signalling = option B**: `CRISP.Server.Common.RejectedException` (factories
  `BadRequest/Forbidden/NotFound/Conflict`) is the only exception whose message reaches the client
  outside development. Framework exceptions still map to a status, `Detail` dev-only. Reason: EF
  Core and LINQ throw `InvalidOperationException`/`KeyNotFoundException` with entity and key names.
  Services throw `RejectedException.X(...)`, never `new InvalidOperationException(...)` for a
  user-facing rejection. Update the crisp-architecture skill rule when it lands in the repo (step 8).

## Decisions pending (Brock's call, see review section 5)

Endpoint contract · route-id binding · Aspire default · auth stack · drop Blazor · MySQL option ·
test stack · permission model · event bus.

## Environment gotchas

- **Never `rm` a gitignored file you did not create.** `NuGet.Config` was untracked + ignored and
  got deleted with stale build output on 2026-09-12; reconstructed from the restore dgspec, but
  the original is gone. Only `bin/`, `obj/`, `node_modules/`, `dist/` are disposable.

- `grep` and `rg` are the same binary in this shell (alias) and it rejects GNU flags
  (`--include`, `-oh`). Use the Grep tool or PowerShell `Select-String`.
- PowerShell `-replace` is case-insensitive; use `-creplace` when normalising identifiers
  (`WAR` inside `Middleware` bit once).
- Repo has no `.gitattributes`; every commit warns LF→CRLF.
