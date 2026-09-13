# CRISP memory bank

Session-to-session context that is not derivable from the code or git history. Read at session
start. Dated entries; delete what stops being true.

## Current state (2026-09-12)

- **Active branch: `modernize`** (from `origin/modernization_update`), **not pushed**. All plan
  steps done plus the four options (2026-09-13). Step 10 (merge to `master`, tag, publish the
  nupkg) is Brock's call. Final state: 0 warnings, 25 Core + 118 Server tests green, 10-variant
  template smoke matrix green (19m47s; needs Docker). `feat/authz-eventing-tests` deleted.
- **The 26 commits up to `1f1b64d` carry a `Claude-Session:` trailer** that Brock's CLAUDE.md
  forbids. Stripping them is a history rewrite; Brock has not said yes. Commits after that have no
  trailer.
- **Defaults:** `--database postgres` (Brock 2026-09-12), `--auth entra`, `--frontend react`,
  `--include-sample true`, `--aspire false`. Local dev: this box has a native PostgreSQL 16 service
  on 5432 with an unknown password; use a container on another port and
  `ConnectionStrings__default` to override `appsettings.Development.json`.
- **Single-source rules** (review section 9): no `//#else` / no live negative branches; file
  alternatives are `*.<variant>.*` renamed in by the engine; never a marker inside JSX or JSON.
- **The repo root IS the template** (`.template.config/template.json`, `CRISP.Templates.csproj`).
  No `templates/` mirror. Options: `--frontend react|none`, `--skip-restore`. Anything
  template-repo-only (`tests/CRISP.Template.Tests`, `ci-template.yml`, README sections) is
  excluded or guarded with `IsTemplateRepository` (a computed symbol that is always false).
  `tests/CRISP.Template.Tests` (~2 min) instantiates both variants and builds them; run it before
  packing. Deferred options and why: review section 9.
- `.claude/skills/crisp-architecture/SKILL.md` is **force-added**: the user-level gitignore excludes
  `.claude/skills/`. Edit it like any tracked file; `git add -f` only matters for new files there.
- `SecurityHeaders:ConnectSources` and `SecurityHeaders:AllowCamera` are the new config knobs
  (replacing WAR's `Csp:ConnectSources` and the hardcoded `camera=(self)`). `PUT /Items/{id}`
  is the sample's update shape; `RouteIdBinder` fills `Id` before validation.
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
