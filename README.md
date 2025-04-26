# CRISP - Command, Response, Interface-driven, Service-oriented Pattern

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)]()
[![NuGet](https://img.shields.io/badge/nuget-v1.0.0-blue)]()

CRISP is a lightweight, modern framework for building scalable and maintainable .NET applications using a pattern that combines CQRS (Command Query Responsibility Segregation) with service-oriented architecture principles. It leverages source generators for automatic dependency registration, reducing boilerplate code.

## Features

- **Dependency Injection Made Easy**: Automatic registration of services through source generators
- **CQRS Implementation**: Clean separation of commands (write operations) and queries (read operations)
- **Interface-Driven Design**: Promotes loose coupling and testability
- **Cross-Platform**: Supports both .NET 9.0 and .NET Standard 2.0
- **Lightweight**: Minimal dependencies with focus on performance
- **Vertical Layers Support**: Organize code by business domains

## Getting Started

### Installation

Install the CRISP NuGet package:

```bash
dotnet add package CRISP
```

### Basic Usage

1. **Create Command and Handler**

```csharp
// Define your command
public class CreateUserCommand : IRequest<Guid>
{
    public string Name { get; set; }
    public string Email { get; set; }
}

// Create a handler
public class CreateUserHandler : IRequestHandler<CreateUserCommand, Guid>
{
    public ValueTask<Guid> Handle(CreateUserCommand request)
    {
        // Implementation logic
        return new ValueTask<Guid>(Guid.NewGuid());
    }
}
```

2. **Create Query and Handler**

```csharp
// Define your query
public class GetUserQuery : Query<UserModel>
{
    public Guid Id { get; set; }
}

// Create a handler
public class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserModel>
{
    public ValueTask<UserModel> Handle(GetUserQuery request)
    {
        // Implementation logic
        return new ValueTask<UserModel>(new UserModel { Id = request.Id, Name = "Test User" });
    }
}
```

3. **Use in your application**

```csharp
// In Program.cs or Startup.cs
services.AddCrispServices(); // Auto-registers all CRISP services

// In your controller or service
public class UserController
{
    private readonly IRequestService<CreateUserCommand, Guid> _createUserService;
    private readonly IQueryService<GetUserQuery, UserModel> _getUserService;

    public UserController(
        IRequestService<CreateUserCommand, Guid> createUserService,
        IQueryService<GetUserQuery, UserModel> getUserService)
    {
        _createUserService = createUserService;
        _getUserService = getUserService;
    }

    public async Task<Guid> CreateUser(string name, string email)
    {
        var command = new CreateUserCommand { Name = name, Email = email };
        return await _createUserService.Send(command);
    }

    public async Task<UserModel> GetUser(Guid id)
    {
        var query = new GetUserQuery { Id = id };
        return await _getUserService.Send(query);
    }
}
```

## Key Concepts

### Commands vs. Queries

- **Commands**: Write operations that change state but don't return data
- **Queries**: Read operations that return data but don't change state

### Event Handling

```csharp
// Define an event
public class UserCreatedEvent : IEvent
{
    public Guid UserId { get; set; }
    public string Name { get; set; }
}

// Create an event handler
public class NotifyUserCreatedHandler : IEventHandler<UserCreatedEvent>
{
    public ValueTask Handle(UserCreatedEvent @event)
    {
        // Implementation logic (e.g., send email)
        return ValueTask.CompletedTask;
    }
}

// Publish an event
await eventService.Publish(new UserCreatedEvent { UserId = id, Name = name });
```

### Filtered Queries

```csharp
// Define a filtered query
public class GetUsersQuery : FilteredQuery<UserModel>
{
    public string NameFilter { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

// Create handler
public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, FilteredResponse<UserModel>>
{
    public ValueTask<FilteredResponse<UserModel>> Handle(GetUsersQuery request)
    {
        // Implementation logic for filtering and paging
        return new ValueTask<FilteredResponse<UserModel>>(new FilteredResponse<UserModel>
        {
            Items = /* filtered items */,
            TotalItems = /* total count */
        });
    }
}
```

## Advanced Usage

### Custom Service Lifetimes

Control the lifetime of your services:

```csharp
[ServiceLifetime(ServiceLifetime.Scoped)]
public class UserService : IRequestService<CreateUserCommand, Guid>
{
    // Implementation
}
```

### Validation

Implement validation for your commands and queries:

```csharp
public class CreateUserCommandValidator : CommandValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}
```

### Error Handling

```csharp
public class CustomRequestHandler : IRequestHandler<MyCommand, MyResponse>
{
    public ValueTask<MyResponse> Handle(MyCommand request)
    {
        try
        {
            // Implementation logic
            return new ValueTask<MyResponse>(new MyResponse());
        }
        catch (Exception ex)
        {
            throw new DomainException("Operation failed", ex);
        }
    }
}
```

## Architecture Overview

CRISP is designed to support a clean, modular architecture:

```
Application
├── Commands
│   ├── CreateUserCommand
│   └── UpdateUserCommand
├── Queries
│   ├── GetUserQuery
│   └── ListUsersQuery
├── Handlers
│   ├── CreateUserHandler
│   └── GetUserHandler
├── Events
│   ├── UserCreatedEvent
│   └── UserCreatedEventHandler
└── Services
    ├── IUserService
    └── UserService
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE.txt) file for details.

## Further Reading

For more detailed information about the CRISP pattern, check out the [documentation](docs/CRISP.md).