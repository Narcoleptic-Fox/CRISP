---
title: Getting started
description: Install the template, generate a solution, run it against a local database, and make your first authenticated request.
nav_order: 1
nav_icon: rocket
---

# Getting started

This page takes you from nothing to a running server and client with the sample feature in it.
It is for a developer who has not used CRISP before. At the end you will have a generated
solution running locally against a PostgreSQL container, the schema applied, and the Item pages
open in a browser.

## Prerequisites

- .NET SDK 10.0.300 or later (`dotnet --version`)
- Node.js 24 (`node --version`); the client pins it in `.nvmrc`
- Docker Desktop or another Docker engine, for the database container and the integration tests
- An Entra ID app registration if you keep the default `--auth entra`; use `--auth none` to skip
  sign-in while you evaluate

## Install the template

::: tabs

== tab "From NuGet"
```bash
dotnet new install CRISP
```

== tab "From a clone"
```bash
git clone https://github.com/Narcoleptic-Fox/CRISP.git
dotnet new install ./CRISP
```
:::

## Generate a solution

```bash
dotnet new crisp --name Inventory
cd Inventory
```

The defaults give you PostgreSQL, Entra + API-key authentication, the React client, the Item
sample and no Aspire. Every option is on the [template page](/architecture/template/).

::: tabs

== tab "API only"
```bash
dotnet new crisp --name Inventory --frontend none
```

== tab "No sign-in"
```bash
dotnet new crisp --name Inventory --auth none
```

== tab "MySQL"
```bash
dotnet new crisp --name Inventory --database mysql
```

== tab "With Aspire"
```bash
dotnet new crisp --name Inventory --aspire true
```
:::

## Start a database

The committed `appsettings.Development.json` points at `127.0.0.1:5432`, database `inventory`,
user `postgres`, and names the password as `${DBPASS}`. Set that variable, then start a container
that matches.

```bash
export DBPASS=password                      # PowerShell: $env:DBPASS = 'password'
docker run -d --name inventory-postgres \
  -e POSTGRES_PASSWORD="$DBPASS" -e POSTGRES_DB=inventory -p 5432:5432 postgres:16
```

::: callout info "Already running a PostgreSQL on 5432?"
Map the container to another port and override the connection string for this shell:
`ConnectionStrings__default="Host=127.0.0.1;Port=5433;Database=inventory;Username=postgres;Password=${DBPASS};"`.
The `${NAME}` token expands from the environment wherever the connection string is read.
:::

## Apply the schema

The server never migrates at startup. The Migrator does, and tells you what it applied.

```bash
DATABASE_HOST=127.0.0.1 DATABASE_NAME=inventory DATABASE_USER=postgres DATABASE_PASSWORD=$DBPASS \
  dotnet run --project src/Inventory.Migrator
```

Expect `1 pending migration(s)`, then `Done. 1 migration(s) applied in total`. A second run reports
`0 pending` and exits 0. Anything else is in [Migrations](/operations/migrations/).

## Run it

```bash
dotnet run --project src/Inventory.Server
```

In Development the server starts the Vite dev server through SpaProxy and opens the client.
Without a client (`--frontend none`) the OpenAPI document is at `/openapi/v1.json` and the health
probes at `/health` and `/healthz`. Neither probes the database; a `200` says the process is up, not that
it can reach its schema. The Migrator's exit code is the check for that.

With `--auth entra`, fill the `AzureAd` section of `appsettings.json` from your app registration
before the first sign-in; until then every API call returns `401`. With `--auth none` there is no
sign-in and every endpoint is open.

## Make a request

::: tabs

== tab "curl (API key)"
```bash
curl -H "X-API-Key: <key>" http://localhost:5051/api/v1/Items
```

== tab "curl (no auth)"
```bash
curl http://localhost:5051/api/v1/Items
```
:::

An empty page comes back: `{"items":[],"totalCount":0}`. Create one from the client, or:

```bash
curl -X POST http://localhost:5051/api/v1/Items \
  -H "Content-Type: application/json" -d '{"name":"Wrench","owner":"Shop"}'
```

The response is `201 Created` with the new id and a `Location` header. `GET /api/v1/Items/{id}/activity`
then shows the audit row the interceptor wrote.

## Run the tests

```bash
dotnet test --project tests/Inventory.Core.Tests/Inventory.Core.Tests.csproj
dotnet test --project tests/Inventory.Server.Tests/Inventory.Server.Tests.csproj
```

The Server suite starts a database container through Testcontainers, applies the real migrations,
and runs the Item and Identity suites against it. Use the explicit `--project` flag; positional
paths are rejected by this SDK.

## Next steps

- [Anatomy of a vertical slice](/architecture/vertical-slice/) before you add your first feature.
- [Configuration](/operations/configuration/) for every setting the server reads.
- [Deployment](/operations/deployment/) when it is time to ship.
