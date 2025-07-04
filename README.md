<p align="center">
  <img src="https://github.com/Narcoleptic-Fox/CRISP/blob/master/assets/icon.png" alt="CRISP Framework Logo" width="200" height="200"/>
</p>

<p align="center">
  <img src="https://github.com/Narcoleptic-Fox/CRISP/blob/master/assets/comapny_logo.png" alt="Company Logo" height="50"/>
</p>

# CRISP - Command Response Interface Service Pattern

[![Build Status](https://img.shields.io/github/actions/workflow/status/Narcoleptic-Fox/CRISP/build.yml?branch=master&style=flat-square)](https://github.com/Narcoleptic-Fox/CRISP/actions)
[![NuGet](https://img.shields.io/nuget/v/Crisp?style=flat-square)](https://www.nuget.org/packages/Crisp/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Crisp%20?style=flat-square)](https://www.nuget.org/packages/Crisp/)
[![License](https://img.shields.io/github/license/Narcoleptic-Fox/CRISP?style=flat-square&label=license)](LICENSE)
[![Target Frameworks](https://img.shields.io/badge/targets-net8.0%20|%20net9.0-blue?style=flat-square)](https://github.com/Narcoleptic-Fox/CRISP/)
![Test Coverage](https://img.shields.io/codecov/c/github/Narcoleptic-Fox/CRISP?style=flat-square)

Clean, procedural architecture for ASP.NET Core & Blazor. No inheritance hierarchies. No magic. Just a clear pipeline from request to response.

## 🚀 Quick Start (30 seconds)

```bash
dotnet add package Crisp.AspNetCore
```

```csharp
// 1. Define a command
public record CreateTodoCommand(string Title) : ICommand<int>;

// 2. Handle it
public class CreateTodoHandler : ICommandHandler<CreateTodoCommand, int>
{
    public Task<int> Handle(CreateTodoCommand command, CancellationToken cancellationToken)
        => Task.FromResult(new Random().Next()); // Your logic here
}

// 3. Wire it up
builder.Services.AddCrisp();
app.MapCrisp();

// That's it! 🎉
```

## 🎯 What is CRISP?

CRISP is a design pattern that brings clarity to your codebase by establishing a simple, procedural flow:

```
Request → Validate → Handle → Response
```

No deep inheritance. No "enterprise" complexity. Just clean, testable, maintainable code.

## ✨ Core Features

- **🔄 Pipeline Pattern**: Every request follows the same predictable path
- **📦 Vertical Layers**: Organize by feature, not by file type
- **🎮 State Machines**: Built-in support for complex workflows
- **⚡ Multiple Protocols**: HTTP, gRPC, JSON-RPC from the same handlers
- **🧪 Testability First**: Every component is independently testable

## 📚 Documentation

- [Getting Started](docs/getting-started.md) - Your first CRISP application
- [Core Concepts](docs/concepts/) - Understand the pattern
- [Examples](examples/) - Real working code
- [API Reference](docs/api/) - Complete API documentation

## 💡 Why CRISP?

Traditional controllers and services often lead to scattered logic and tight coupling. CRISP provides structure without the overhead:

**Before (Controller mess):**
```csharp
[ApiController]
public class TodoController : ControllerBase
{
    private readonly ITodoService _service;
    private readonly IValidator _validator;
    private readonly ILogger _logger;
    // Constructor injection nightmare...
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TodoDto dto)
    {
        // Validation mixed with logic
        // Error handling everywhere
        // Hard to test
    }
}
```

**After (CRISP clarity):**
```csharp
public record CreateTodoCommand(string Title) : ICommand<int>;

public class CreateTodoHandler : ICommandHandler<CreateTodoCommand, int>
{
    public Task<int> Handle(CreateTodoCommand command, CancellationToken cancellationToken)
        => Task.FromResult(_todos.Add(command.Title));
}
// Validation, logging, retry policies - all handled by pipeline
```

## 🏗️ Real Examples

### Web API
```csharp
app.MapCrisp() // Auto-discovers all commands/queries
   .RequireAuthorization();
```

### Blazor
```razor
@inject ICommandDispatcher Commands

<button @onclick="CreateTodo">Add</button>

@code {
    async Task CreateTodo() 
        => await Commands.Send(new CreateTodoCommand("New Todo"));
}
```

### Game Server
```csharp
public record MovePlayerCommand(int PlayerId, Position To) : ICommand<GameState>;

// Same pattern works for games, web apps, microservices...
```

## 📦 Packages

| Package          | Description                     | NuGet                                                                                                             |
| ---------------- | ------------------------------- | ----------------------------------------------------------------------------------------------------------------- |
| Crisp.Core       | Core interfaces and pipeline    | [![NuGet](https://img.shields.io/nuget/v/Crisp.Core.svg)](https://www.nuget.org/packages/Crisp.Core/)             |
| Crisp.AspNetCore | ASP.NET Core integration        | [![NuGet](https://img.shields.io/nuget/v/Crisp.AspNetCore.svg)](https://www.nuget.org/packages/Crisp.AspNetCore/) |
| Crisp.Blazor     | Blazor components & integration | [![NuGet](https://img.shields.io/nuget/v/Crisp.Blazor.svg)](https://www.nuget.org/packages/Crisp.Blazor/)         |
| Crisp.Runtime    | Runtime optimizations           | [![NuGet](https://img.shields.io/nuget/v/Crisp.Runtime.svg)](https://www.nuget.org/packages/Crisp.Runtime/)       |

## 🤝 Contributing

CRISP is open source and we love contributions! Check out our [contributing guide](CONTRIBUTING.md).

## 📄 License

CRISP is licensed under the [MIT License](LICENSE).