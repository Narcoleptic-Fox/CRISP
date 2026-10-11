---
title: Library reference
description: Every third-party dependency in the stack and the reason it is there, by server, client and tests.
nav_order: 5
nav_icon: package
---

# Library reference

A dependency is a decision. These pages record the decision for each one: what it does here,
why this one and not an alternative, and what would have to change to remove it. Versions are not
listed; `Directory.Packages.props` and `src/CRISP.Client/package.json` are the only places a version
should ever be written, and the pages link to them.

| Page | Source of truth |
| --- | --- |
| [Server libraries](/reference/server-libraries/) | `Directory.Packages.props`, referenced from `src/*/*.csproj` |
| [Client libraries](/reference/client-libraries/) | `src/CRISP.Client/package.json` |
| [Test libraries](/reference/test-libraries/) | `Directory.Packages.props` (Testing group), `package.json` devDependencies |

## Adding a dependency

1. Say what it replaces. If the answer is "forty lines of our own code", write the forty lines.
2. Pin it centrally: `Directory.Packages.props` for NuGet (the build fails on a version in a
   `.csproj`), `package.json` for npm.
3. Add a row to the relevant page with the reason. A dependency no one can explain is the first
   one removed.
