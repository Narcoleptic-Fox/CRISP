# Core Layer

The Core layer is the foundation of CRISP architecture, containing immutable domain contracts, business rules, and events. It has no dependencies and represents the pure domain logic of your application.

## Overview

The Core layer serves as the center of the "onion" - everything else depends on it, but it depends on nothing. This ensures your domain logic remains pure and testable.

```
Core Layer Contents:
├── Common/
│   ├── BaseModel.cs           # Base immutable domain model
│   ├── BaseAuditableModel.cs  # Auditable domain model
│   ├── Commands.cs            # Command contracts
│   ├── Queries.cs             # Query contracts
│   └── Events.cs              # Domain event contracts
├── [Domain]/                  # Domain-specific contracts
│   ├── Models/                # Domain models (records)
│   ├── Commands/              # Domain commands
│   ├── Queries/               # Domain queries
│   ├── Events/                # Domain events
│   └── Validators/            # Core business validation
└── Constants/                 # Domain constants
```

## Domain Models

### Base Models

**BaseModel** - Foundation for all domain entities:
```csharp
public abstract record BaseModel
{
    public Guid Id { get; init; }
}
```

**BaseAuditableModel** - For entities requiring audit trails:
```csharp
public abstract record BaseAuditableModel : BaseModel
{
    public DateTime CreatedOn { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedOn { get; init; } = DateTime.UtcNow;
    public bool IsArchived { get; init; }
}
```

### Domain Models (Records)

Domain models are **immutable records** that represent your business concepts:

```csharp
namespace CRISP.Core.Identity;

public record User(
    Guid Id,
    string Email,
    string Name,
    bool IsActive
) : BaseAuditableModel
{
    // Core business validation
    public ValidationResult Validate()
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(Email))
            errors.Add("Email is required");
            
        if (!IsValidEmail(Email))
            errors.Add("Email format is invalid");
            
        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Name is required");
            
        return errors.Any() 
            ? ValidationResult.Failure(errors)
            : ValidationResult.Success();
    }
    
    private static bool IsValidEmail(string email) =>
        // Email validation logic
        !string.IsNullOrEmpty(email) && email.Contains('@');
}
```

### Key Characteristics:
- **Immutable**: Records ensure thread safety and prevent accidental mutations
- **Self-Validating**: Business rules are enforced within the model
- **Value Semantics**: Records provide value-based equality
- **No Dependencies**: Pure domain logic only

## Commands

Commands represent write operations that modify system state.

### Command Contracts

```csharp
// Base command interfaces
public interface ICommand;                    // Commands without return values
public interface ICommand<out TResponse>;     // Commands with return values

// Specialized command types
public abstract record CreateCommand : ICommand<Guid>;
public abstract record ModifyCommand : ICommand;
public sealed record DeleteCommand(Guid Id) : ModifyCommand;
public sealed record ArchiveCommand(Guid Id, string? Reason = null) : ModifyCommand;
```

### Domain-Specific Commands

```csharp
namespace CRISP.Core.Identity;

// User commands
public record CreateUser(
    string Email,
    string Name,
    string Password
) : CreateCommand
{
    public User ToUser() => new(
        Id: Guid.NewGuid(),
        Email: Email,
        Name: Name,
        IsActive: true
    );
}

public record UpdateUser(
    Guid Id,
    string? Email = null,
    string? Name = null,
    bool? IsActive = null
) : ModifyCommand(Id);

public record ChangeUserPassword(
    Guid Id,
    string CurrentPassword,
    string NewPassword
) : ModifyCommand(Id);
```

## Queries

Queries represent read operations that retrieve data without side effects.

### Query Contracts

```csharp
// Base query interface
public interface IQuery<TResponse> where TResponse : class;

// Standard query patterns
public sealed record SingularQuery<TResponse> : IQuery<TResponse>
    where TResponse : BaseModel;

public abstract record PagedQuery<TResponse> : IQuery<PagedResponse<TResponse>>
    where TResponse : BaseModel;
```

### Domain-Specific Queries

```csharp
namespace CRISP.Core.Identity;

// Single user queries
public record GetUserById(Guid Id) : SingularQuery<User>;
public record GetUserByEmail(string Email) : IQuery<User>;

// Collection queries
public record GetUsers : PagedQuery<User>
{
    public string? SearchTerm { get; init; }
    public bool? IsActive { get; init; }
    
    public override string ToQueryString()
    {
        var baseQuery = base.ToQueryString();
        var filters = new List<string>();
        
        if (!string.IsNullOrEmpty(SearchTerm))
            filters.Add($"searchTerm={Uri.EscapeDataString(SearchTerm)}");
            
        if (IsActive.HasValue)
            filters.Add($"isActive={IsActive.Value}");
            
        return filters.Any() 
            ? $"{baseQuery}&{string.Join("&", filters)}"
            : baseQuery;
    }
}
```

## Service Contracts

Service contracts define how commands and queries are executed.

### Command Services

```csharp
// Service contracts for commands
public interface ICommandService<TCommand> where TCommand : ICommand
{
    ValueTask Send(TCommand command, CancellationToken cancellationToken = default);
}

public interface ICommandService<TCommand, TResponse> where TCommand : ICommand<TResponse>
{
    ValueTask<TResponse> Send(TCommand command, CancellationToken cancellationToken = default);
}

// Specialized service contracts
public interface ICreateService<TCreate> : ICommandService<TCreate, Guid>
    where TCreate : CreateCommand;

public interface IModifyService<TModify> : ICommandService<TModify>
    where TModify : ModifyCommand;
```

### Query Services

```csharp
// Service contract for queries
public interface IQueryService<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
    where TResponse : class
{
    ValueTask<TResponse> Send(TQuery query, CancellationToken cancellationToken = default);
}
```

## Domain Events

Domain events represent significant business occurrences.

### Event Contracts

```csharp
public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
}

public abstract record DomainEvent : IDomainEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}
```

### Domain-Specific Events

```csharp
namespace CRISP.Core.Identity;

public record UserCreated(
    Guid UserId,
    string Email,
    string Name
) : DomainEvent;

public record UserUpdated(
    Guid UserId,
    string Email,
    string Name
) : DomainEvent;

public record UserPasswordChanged(
    Guid UserId
) : DomainEvent;

public record UserArchived(
    Guid UserId,
    string? Reason
) : DomainEvent;
```

## Validation

### Core Validation Principles

1. **Business Rules in Models**: Domain models validate their own business rules
2. **Validation Results**: Standardized validation response pattern
3. **Immutable Validation**: Validation doesn't modify state
4. **Composable**: Validation rules can be combined

### Validation Implementation

```csharp
public record ValidationResult
{
    public bool IsSuccess { get; init; }
    public IEnumerable<string> Errors { get; init; } = [];
    
    public static ValidationResult Success() => new() { IsSuccess = true };
    public static ValidationResult Failure(params string[] errors) => 
        new() { IsSuccess = false, Errors = errors };
    public static ValidationResult Failure(IEnumerable<string> errors) =>
        new() { IsSuccess = false, Errors = errors };
}

// Usage in domain models
public record User(/* properties */) : BaseAuditableModel
{
    public ValidationResult Validate()
    {
        var errors = new List<string>();
        
        // Business rule validation
        if (string.IsNullOrWhiteSpace(Email))
            errors.Add("Email is required");
            
        // More validations...
        
        return errors.Any() 
            ? ValidationResult.Failure(errors)
            : ValidationResult.Success();
    }
}
```

## Best Practices

### DO:
- ✅ Keep models immutable using records
- ✅ Implement business validation in domain models
- ✅ Use descriptive names for commands and queries
- ✅ Include domain events for significant business actions
- ✅ Keep the Core layer dependency-free

### DON'T:
- ❌ Add infrastructure dependencies to Core
- ❌ Use mutable classes for domain models
- ❌ Put application logic in Core
- ❌ Reference external libraries in Core
- ❌ Include UI concerns in domain models

## Testing Core Layer

```csharp
[Test]
public void User_Validate_Should_Require_Email()
{
    // Arrange
    var user = new User(
        Id: Guid.NewGuid(),
        Email: "", // Invalid
        Name: "John Doe",
        IsActive: true
    );
    
    // Act
    var result = user.Validate();
    
    // Assert
    Assert.False(result.IsSuccess);
    Assert.Contains("Email is required", result.Errors);
}

[Test]
public void CreateUser_ToUser_Should_Generate_Valid_User()
{
    // Arrange
    var command = new CreateUser("test@example.com", "John Doe", "password");
    
    // Act
    var user = command.ToUser();
    
    // Assert
    Assert.Equal("test@example.com", user.Email);
    Assert.Equal("John Doe", user.Name);
    Assert.True(user.IsActive);
    Assert.NotEqual(Guid.Empty, user.Id);
}
```

## Next Steps

Continue with the [Infrastructure Layer](infrastructure-layer.md) to understand how Core contracts are implemented with persistent storage.
