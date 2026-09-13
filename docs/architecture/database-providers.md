---
title: Database providers
description: How PostgreSQL, MySQL, SQL Server and SQLite each live in one provider class, what differs between them, and how migrations and tests follow the choice.
nav_order: 9
---

# Database providers

A CRISP solution targets one relational provider, chosen at generation time with `--database`.
Everything that differs between providers lives in a single `DatabaseProvider` class per provider
and a matching `TestDatabase` class in the test project. The rest of the code base calls the
provider and knows nothing else about it.

## One class, same shape, four folders

```
src/CRISP.Server/Data/Providers/
  Postgres/DatabaseProvider.cs     (default)
  MySql/DatabaseProvider.cs
  SqlServer/DatabaseProvider.cs
  Sqlite/DatabaseProvider.cs  + SqliteForeignKeyInterceptor.cs
tests/CRISP.Server.Tests/TestSupport/Providers/<Provider>/TestDatabase.cs
```

Each `DatabaseProvider` exposes the same members:

| Member | What varies |
| --- | --- |
| `BuildConnectionString(host, port, database, user, password)` | The dialect: `Host=;Port=` against `server=;port=` against `Server=host,port` against `Data Source=` |
| `CreateConfigurator(connectionString)` | Called once at registration; MySQL probes `ServerVersion` here, once, rather than per context |
| `ConfigureDesignTime(options)` | A fixed configuration so `dotnet ef migrations add` needs no running database |
| `ObjectKeyColumn(property)` | MySQL needs `varchar(1024)` in `latin1` to fit a 1024-character key under its 3072-byte index limit; SQL Server needs `varchar` rather than `nvarchar` under its 1700-byte limit |
| `ConfigureConventions(builder)` | SQLite stores `DateTimeOffset` as UTC ticks (below); the others do nothing |

A generated solution receives exactly one provider folder and one `TestDatabase`. The template
repository keeps all four and compiles PostgreSQL through a template-only `Compile Remove`.

## Migrations follow the provider

Migrations are provider-specific SQL. The template ships `InitialCreate` for the default
combination (PostgreSQL, sample included, authentication on). Any other combination arrives with no
migration, and the first step is to create one:

```bash
dotnet tool restore
dotnet ef migrations add InitialCreate --project src/CRISP.Server --startup-project src/CRISP.Server --output-dir Data/Migrations
```

The design-time factory uses `DatabaseProvider.ConfigureDesignTime`, so this needs no database.
The integration fixture refuses to run when no migrations exist and prints the command above rather
than silently `EnsureCreated`-ing a schema no deploy will ever see.

## SQLite differences

SQLite is a file, not a server. `BuildConnectionString` ignores host, port, user and password and
uses the database name as the path. Foreign keys are off per connection by default;
`SqliteForeignKeyInterceptor` turns them on as each connection opens.

SQLite also stores `DateTimeOffset` as ISO-8601 text, and EF Core refuses to translate an `ORDER BY`
over it: *SQLite does not support expressions of type 'DateTimeOffset' in ORDER BY clauses*. The
activity feed orders by `Timestamp`, so the SQLite provider's `ConfigureConventions` stores every
`DateTimeOffset` as UTC ticks in an `INTEGER` column. The offset is not kept; every timestamp this
application writes is UTC.

## Tests per provider

`TestDatabase.StartAsync` starts a Testcontainer for the provider (`postgres:16`, `mysql:8.4`,
`mcr.microsoft.com/mssql/server:2022-latest`) or, when `CRISP_TEST_<PROVIDER>_CONNECTION_STRING` is
set, uses that server. It refuses any database not named `crisp_tests`, because the reset truncates
every table it finds. Resets go through Respawn for the three servers; SQLite has no Respawn adapter,
so a reset deletes the file and re-applies the migrations.

## Choosing

| Provider | Choose it when |
| --- | --- |
| PostgreSQL (default) | Anything new. It is what the fleet standardised on. |
| MySQL | An existing MySQL estate, or an application that started on MySQL 8. |
| SQL Server | A Windows or Azure SQL shop. |
| SQLite | A desktop application, a single-user tool, or a test harness. Not for concurrent web traffic. |
