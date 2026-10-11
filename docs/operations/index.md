---
title: Operations
description: Running, configuring, migrating, testing, building and deploying a CRISP solution. The runbook.
nav_order: 4
nav_icon: server
---

# Operations

These pages are the runbook: what to type, in what order, and what a healthy result looks like,
so that a failure is recognisable as one. They assume a generated solution; the template
repository itself follows the same steps with `CRISP` as the name.

| Page | Use it when |
| --- | --- |
| [Running locally](/operations/running-locally/) | You need the server, client and a database on your machine |
| [Configuration](/operations/configuration/) | You need to know what a setting does or where a secret goes |
| [Migrations](/operations/migrations/) | The schema changed, or you are pointing at a new database |
| [Testing](/operations/testing/) | You are adding a test, or one is failing for an environmental reason |
| [Continuous integration](/operations/ci/) | A workflow is red, or you are adding a job |
| [Deployment](/operations/deployment/) | You are building the container or shipping a release |

## The two invariants

1. **The server never migrates at startup.** The Migrator does, as a separate step with its own
   exit code: `0` applied and verified, `1` something is still pending, `2` no database configured.
2. **Secrets are never in a committed file.** Settings files name them (`${DBPASS}`), the
   environment supplies them, and a missing one fails at startup with the variable's name.
