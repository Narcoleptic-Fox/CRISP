# CRISP - CQRS Pattern Template

A .NET solution template implementing the CRISP CQRS architectural pattern with .NET Aspire, feature folders, Entity Framework Core, and optional React frontend.

## Installation

```bash
dotnet new install CRISP
```

## Usage

### Create a new project with defaults

```bash
dotnet new crisp --name MyApp
```

### Available Options

| Option | Description | Default |
|--------|-------------|---------|
| `--framework` | Target framework (net10.0, net9.0) | net10.0 |
| `--include-sample` | Include Todo sample feature | true |
| `--frontend` | Frontend framework (react, none) | react |
| `--database` | Database provider (postgres, sqlserver, sqlite) | postgres |
| `--auth` | Include ASP.NET Core Identity | true |
| `--redis` | Include Redis for caching | true |
| `--aspire` | Use .NET Aspire orchestration | true |

### Examples

**API-only with SQL Server:**
```bash
dotnet new crisp --name MyApi --frontend none --database sqlserver
```

**Minimal setup without sample or auth:**
```bash
dotnet new crisp --name MyApp --include-sample false --auth false
```

**Full stack with PostgreSQL (default):**
```bash
dotnet new crisp --name MyApp
```

## Project Structure

```
MyApp/
├── src/
│   ├── MyApp.AppHost/          # .NET Aspire orchestration
│   ├── MyApp.Core/             # Contracts, commands, queries
│   ├── MyApp.Server/           # API, Blazor, EF Core
│   │   ├── Components/         # Blazor components
│   │   ├── Data/               # EF Core entities & migrations
│   │   └── Features/           # CQRS feature folders
│   ├── MyApp.ServiceDefaults/  # Shared Aspire configuration
│   └── MyApp.Web/              # React frontend (if included)
├── styles/                     # Shared CSS variables
└── tailwind.config.js          # Shared Tailwind config
```

## The CRISP Pattern

CRISP (Command Responsibility Isolation with Segregated Processing) is a CQRS implementation that:

- **Separates commands and queries** into distinct contracts
- **Uses feature folders** to organize code by domain
- **Auto-discovers features** via IFeature interface
- **Supports output caching** with tag-based invalidation
- **Integrates with .NET Aspire** for orchestration and observability

## License

MIT License - Copyright (c) Narcoleptic Fox LLC
