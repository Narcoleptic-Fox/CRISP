---
title: The repository root is the template
nav_title: Single-source template
description: Make the live, building, tested repository the dotnet new content rather than a mirrored copy, and the three rules that follow from every conditional branch being live code.
date: 2026-09-13
status: accepted
---

# The repository root is the template

**Decided:** 2026-09-12 (root as content), extended 2026-09-13 (the four options)

## Context

The previous layout kept the template content under a `templates/` folder and the developed
solution beside it. The two drifted: the solution gained fixes the template never received. Every
production application had started from the template and then fixed the same things again.

At the same time, four choices were wanted at generation time: frontend or API only, with or
without the sample, which database, with or without authentication, plus Aspire. Any layout had to
support all of them without a combinatorial set of copies.

## Options

|       | Option                                                                                                                                                                                                 | Taken?                                                                                                                                               |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------- |
| A     | Keep `templates/`; add a CI check that diffs it against the solution                                                                                                                                   | No. The check would fail constantly and be disabled, or the template would be regenerated from the solution by a script that is itself a second copy |
| **B** | **The repository root is the template content. `.template.config/template.json` excludes what only the repository needs, and every option is a symbol whose branches are live code in the repository** | **Yes**                                                                                                                                              |
| C     | Generate the variants as separate packages (`crisp-mysql`, `crisp-api`)                                                                                                                                | No. Sixteen packages for four binary-ish options, each needing its own smoke test                                                                    |

## Decision

Option B. What builds and tests in the repository is exactly what is packed.

Because every branch is live code, three rules emerged, each learned by breaking it:

1. **No `//#else`, and no `//#if (!X)` with a live alternative.** Both branches compile here, so
   two definitions of one identifier collide. Declare once, assign conditionally.
2. **Alternatives that cannot share a file live beside the original under another name and are
   renamed in by the engine** (`appsettings.Development.sqlite.json`, `main.noauth.tsx`,
   `AppHost/Program.mysql.cs`, one `DatabaseProvider` per provider folder with a template-only
   `Compile Remove`). They are inert in the repository but still compile or type-check, so they
   cannot rot silently.
3. **A marker inside JSX or JSON is content, not a comment.** Conditionals go outside the element
   or the file is swapped by rename.

## Consequences

- One source of truth; a fix made while developing is in the next pack.
- The template smoke matrix (`tests/CRISP.Template.Tests`) is the only proof a variant works. It
  samples each non-default value against the defaults plus the everything-off corner rather than
  the cross product; an untested combination may break. Ten variants take about twenty minutes and
  need Docker.
- Template-only artefacts (`IsTemplateRepository` sections, the smoke tests, `ci-template.yml`,
  the docs site) must be excluded in two places, `template.json` and `CRISP.Templates.csproj`, and
  the smoke test checks the generated tree for leftovers.
- Generated solutions for non-default combinations ship no migration; the integration fixture
  refuses to run until one is added, and says how.
