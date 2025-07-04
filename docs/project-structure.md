# CRISP Project Structure

```
CRISP/
├── .git/
├── .gitattributes
├── .gitignore
├── .vs/
├── assets/
│   ├── company_logo.png
│   └── icon.png
├── blogs/
│   ├── old-1.md
│   └── old-2.md
├── docs/
│   ├── CRISP.md
│   ├── getting-started.md
│   ├── why-crisp.md
│   ├── migration-guide.md
│   ├── project-structure.md
│   ├── api/
│   │   ├── README.md
│   │   ├── crisp-core.md
│   │   ├── crisp-aspnetcore.md
│   │   ├── crisp-blazor.md
│   │   └── crisp-runtime.md
│   ├── concepts/
│   │   ├── 01_pipeline.md
│   │   ├── 02_commands-queries.md
│   │   ├── 03_endpoints.md
│   │   ├── 04_vertical-layers.md
│   │   ├── 05_flow-diagram.md
│   │   └── 06-behavior-registration.md
│   ├── examples/
│   │   └── todo-api.md
│   └── adr/
│       └── 001-procedural-philosophy.md
├── src/
│   ├── Crisp.Core/
│   │   ├── Crisp.Core.csproj
│   │   ├── CrispOptions.cs
│   │   ├── Commands/
│   │   │   ├── ICommand.cs
│   │   │   ├── ICommandHandler.cs
│   │   │   └── ICommandDispatcher.cs
│   │   ├── Queries/
│   │   │   ├── IQuery.cs
│   │   │   ├── IQueryHandler.cs
│   │   │   └── IQueryDispatcher.cs
│   │   ├── Events/
│   │   │   ├── IEvent.cs
│   │   │   ├── IEventHandler.cs
│   │   │   └── IEventPublisher.cs
│   │   ├── Pipeline/
│   │   │   ├── IPipelineBehavior.cs
│   │   │   └── RequestHandlerDelegate.cs
│   │   ├── Builder/
│   │   │   ├── ICrispBuilder.cs
│   │   │   └── HandlerRegistration.cs
│   │   ├── Common/
│   │   ├── Compatibility/
│   │   └── Exceptions/
│   ├── Crisp.AspNetCore/
│   │   ├── Crisp.AspNetCore.csproj
│   │   ├── ProblemDetails.cs
│   │   ├── Endpoints/
│   │   │   ├── IEndpoint.cs
│   │   │   ├── EndpointMapper.cs
│   │   │   ├── EndpointConventions.cs
│   │   │   ├── CommandEndpoint.cs
│   │   │   └── QueryEndpoint.cs
│   │   ├── Builder/
│   │   ├── Extensions/
│   │   ├── HealthChecks/
│   │   ├── Metadata/
│   │   ├── Middleware/
│   │   ├── OpenApi/
│   │   └── Security/
│   ├── Crisp.Blazor/
│   │   ├── Crisp.Blazor.csproj
│   │   ├── CrispBlazorOptions.cs
│   │   ├── Builder/
│   │   ├── Concurrency/
│   │   ├── Extensions/
│   │   ├── Notifications/
│   │   ├── Pipeline/
│   │   ├── Services/
│   │   └── State/
│   └── Crisp.Runtime/
│       ├── Crisp.Runtime.csproj
│       ├── Builder/
│       ├── Dispatchers/
│       │   ├── PreCompiledCommandDispatcher.cs
│       │   └── PreCompiledQueryDispatcher.cs
│       ├── Events/
│       └── Pipeline/
├── samples/
│   └── TodoApi/
│       ├── TodoApi.csproj
│       ├── Program.cs
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       ├── TodoApi.http
│       ├── Features/
│       │   └── Todo/
│       │       ├── Commands/
│       │       │   ├── CreateTodo.cs
│       │       │   ├── UpdateTodo.cs
│       │       │   └── DeleteTodo.cs
│       │       ├── Queries/
│       │       │   ├── GetTodo.cs
│       │       │   └── ListTodos.cs
│       │       ├── Models/
│       │       │   └── Todo.cs
│       │       ├── ITodoRepository.cs
│       │       └── InMemoryTodoRepository.cs
│       ├── Properties/
│       └── Security/
├── tests/
│   ├── Crisp.Core.Tests/
│   │   ├── Crisp.Core.Tests.csproj
│   │   ├── Commands/
│   │   │   └── CommandTests.cs
│   │   ├── Pipeline/
│   │   │   ├── PipelineBehaviorTests.cs
│   │   │   └── PipelineBuilderTests.cs
│   │   └── Validation/
│   │       └── ValidationTests.cs
│   ├── Crisp.AspNetCore.Tests/
│   │   ├── Crisp.AspNetCore.Tests.csproj
│   │   ├── Endpoints/
│   │   │   ├── EndpointMapperTests.cs
│   │   │   └── RouteConventionTests.cs
│   │   ├── Integration/
│   │   │   ├── WebApplicationFactoryTests.cs
│   │   │   └── EndToEndTests.cs
│   │   └── TestHelpers/
│   │       ├── TestCommand.cs
│   │       └── TestWebApplicationFactory.cs
│   ├── Crisp.Runtime.Tests/
│   │   └── Crisp.Runtime.Tests.csproj
│   ├── LoadTests/
│   └── TodoApi.IntegrationTests/
├── benchmarks/
│   └── Crisp.Benchmarks/
│       ├── Crisp.Benchmarks.csproj
│       ├── Program.cs
│       ├── PipelineBenchmarks.cs
│       └── DispatcherBenchmarks.cs
├── .editorconfig
├── .gitignore
├── Directory.Build.props
├── Directory.Packages.props
├── global.json
├── CRISP.sln
├── README.md
├── LICENSE
└── CONTRIBUTING.md
```

## Key Files Content Preview

### `Directory.Build.props`
```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
  </PropertyGroup>

  <PropertyGroup>
    <Version>1.0.0</Version>
    <Authors>YourName</Authors>
    <Company>CRISP</Company>
    <Product>CRISP Framework</Product>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <PackageProjectUrl>https://github.com/yourusername/crisp</PackageProjectUrl>
    <RepositoryUrl>https://github.com/yourusername/crisp</RepositoryUrl>
  </PropertyGroup>
</Project>
```

### `CRISP.sln`
```
Microsoft Visual Studio Solution File, Format Version 12.00
Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "src", "src", "{guid}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Crisp.Core", "src\Crisp.Core\Crisp.Core.csproj", "{guid}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Crisp.AspNetCore", "src\Crisp.AspNetCore\Crisp.AspNetCore.csproj", "{guid}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Crisp.Blazor", "src\Crisp.Blazor\Crisp.Blazor.csproj", "{guid}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Crisp.Runtime", "src\Crisp.Runtime\Crisp.Runtime.csproj", "{guid}"
EndProject
Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "samples", "samples", "{guid}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "TodoApi", "samples\TodoApi\TodoApi.csproj", "{guid}"
EndProject
Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "tests", "tests", "{guid}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Crisp.Core.Tests", "tests\Crisp.Core.Tests\Crisp.Core.Tests.csproj", "{guid}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Crisp.AspNetCore.Tests", "tests\Crisp.AspNetCore.Tests\Crisp.AspNetCore.Tests.csproj", "{guid}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Crisp.Runtime.Tests", "tests\Crisp.Runtime.Tests\Crisp.Runtime.Tests.csproj", "{guid}"
EndProject
Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "benchmarks", "benchmarks", "{guid}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Crisp.Benchmarks", "benchmarks\Crisp.Benchmarks\Crisp.Benchmarks.csproj", "{guid}"
EndProject
```

### `src/Crisp.Core/Crisp.Core.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <PackageId>Crisp.Core</PackageId>
    <Description>Core interfaces and abstractions for CRISP pattern</Description>
  </PropertyGroup>
</Project>
```

### `src/Crisp.AspNetCore/Crisp.AspNetCore.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <PackageId>Crisp.AspNetCore</PackageId>
    <Description>ASP.NET Core integration for CRISP pattern</Description>
  </PropertyGroup>

  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
    <ProjectReference Include="..\Crisp.Core\Crisp.Core.csproj" />
  </ItemGroup>
</Project>
```

### `src/Crisp.Runtime/Crisp.Runtime.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <PackageId>Crisp.Runtime</PackageId>
    <Description>Runtime optimizations and pre-compiled dispatchers for CRISP pattern</Description>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\Crisp.Core\Crisp.Core.csproj" />
  </ItemGroup>
</Project>
```

This structure follows your preferences:
- **Minimal packages** (4 main ones: Core, AspNetCore, Blazor, Runtime)
- **Clear organization** (vertical features in examples)
- **Test coverage** (unit and integration tests)
- **Documentation** (comprehensive but practical)
- **Benchmarks** (because performance matters)
- **Runtime optimization** (with pre-compiled dispatchers)

The structure is procedural and straightforward - no complex hierarchies or unnecessary abstractions! 🚀