# CRISP memory bank

Session-to-session context that is not derivable from the code or git history. Read at session
start. Dated entries; delete what stops being true.

## Current state (2026-09-12)

- **Active branch: `modernize`** (from `origin/modernization_update`). Step 1 of the plan is done:
  builds, boots against MySQL, 25 Core tests green (sabotage-verified). Next: step 3 (remaining
  findings #7 #8 #9 #10 #11 #12 #13), then Migrator + Server.Tests, then templates/.
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
