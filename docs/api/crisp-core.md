# Crisp.Core API Reference

The core package containing all fundamental abstractions and interfaces for the CRISP framework.

## Commands & Queries

### ICommand\<TResponse\>
**Namespace:** `Crisp.Commands`  
**Purpose:** Base interface for commands that return a response

```csharp
public interface ICommand<TResponse> : IRequest<TResponse>
{
    // Marker interface - no members
}
```

**Example:**
```csharp
public record CreateTodoCommand(string Title, string Description) : ICommand<int>;
```

### ICommand
**Namespace:** `Crisp.Commands`  
**Purpose:** Base interface for void commands (no response)

```csharp
public interface ICommand : IRequest
{
    // Marker interface - no members
}
```

**Example:**
```csharp
public record DeleteTodoCommand(int Id) : ICommand;
```

### ICommandDispatcher
**Namespace:** `Crisp.Commands`  
**Purpose:** Dispatches commands to their respective handlers

```csharp
public interface ICommandDispatcher
{
    Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);
    Task Send(ICommand command, CancellationToken cancellationToken = default);
}
```

**Example:**
```csharp
[ApiController]
public class TodoController : ControllerBase
{
    private readonly ICommandDispatcher _commandDispatcher;
    
    public TodoController(ICommandDispatcher commandDispatcher)
        => _commandDispatcher = commandDispatcher;
    
    [HttpPost]
    public async Task<IActionResult> CreateTodo(CreateTodoCommand command)
    {
        var todoId = await _commandDispatcher.Send(command);
        return Ok(new { Id = todoId });
    }
}
```

### ICommandHandler\<TCommand, TResponse\>
**Namespace:** `Crisp.Commands`  
**Purpose:** Handler for commands that return a response

```csharp
public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    // Inherits Handle method from IRequestHandler
}
```

**Example:**
```csharp
public class CreateTodoHandler : ICommandHandler<CreateTodoCommand, int>
{
    private readonly ITodoRepository _repository;
    
    public CreateTodoHandler(ITodoRepository repository)
        => _repository = repository;
    
    public async Task<int> Handle(CreateTodoCommand command, CancellationToken cancellationToken = default)
    {
        var todo = new Todo(command.Title, command.Description);
        await _repository.AddAsync(todo);
        return todo.Id;
    }
}
```

### IQuery\<TResponse\>
**Namespace:** `Crisp.Queries`  
**Purpose:** Base interface for queries that return a response

```csharp
public interface IQuery<TResponse> : IRequest<TResponse>
{
    // Marker interface - no members
}
```

**Example:**
```csharp
public record GetTodoQuery(int Id) : IQuery<Todo>;
public record GetAllTodosQuery : IQuery<List<Todo>>;
```

### IQueryDispatcher
**Namespace:** `Crisp.Queries`  
**Purpose:** Dispatches queries to their respective handlers

```csharp
public interface IQueryDispatcher
{
    Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);
}
```

### IQueryHandler\<TQuery, TResponse\>
**Namespace:** `Crisp.Queries`  
**Purpose:** Handler for queries

```csharp
public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    // Inherits Handle method from IRequestHandler
}
```

**Example:**
```csharp
public class GetTodoHandler : IQueryHandler<GetTodoQuery, Todo>
{
    private readonly ITodoRepository _repository;
    
    public GetTodoHandler(ITodoRepository repository)
        => _repository = repository;
    
    public async Task<Todo> Handle(GetTodoQuery query, CancellationToken cancellationToken = default)
    {
        var todo = await _repository.GetByIdAsync(query.Id);
        return todo ?? throw new NotFoundException(nameof(Todo), query.Id.ToString());
    }
}
```

## Pipeline System

### IPipelineBehavior\<TRequest, TResponse\>
**Namespace:** `Crisp.Pipeline`  
**Purpose:** Base interface for pipeline behaviors with responses

```csharp
public interface IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken = default);
}
```

**Example:**
```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        => _logger = logger;
    
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling {RequestType} with data: {@Request}", typeof(TRequest).Name, request);
        
        var stopwatch = Stopwatch.StartNew();
        var response = await next(cancellationToken);
        stopwatch.Stop();
        
        _logger.LogInformation("Handled {RequestType} in {ElapsedMs}ms", typeof(TRequest).Name, stopwatch.ElapsedMilliseconds);
        
        return response;
    }
}
```

### RequestHandlerDelegate\<TResponse\>
**Namespace:** `Crisp.Pipeline`  
**Purpose:** Delegate for pipeline continuation with response

```csharp
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>(CancellationToken cancellationToken = default);
```

## Result Types

### Result
**Namespace:** `Crisp.Common`  
**Purpose:** Represents operation results (success/failure)

```csharp
public class Result
{
    public bool IsSuccess { get; protected set; }
    public string? Error { get; protected set; }
    
    public static Result Success() => new() { IsSuccess = true };
    public static Result Failure(string error) => new() { IsSuccess = false, Error = error };
}
```

### Result\<T\>
**Namespace:** `Crisp.Common`  
**Purpose:** Represents operation results with value

```csharp
public class Result<T> : Result
{
    public T? Value { get; protected set; }
    
    public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };
    public static new Result<T> Failure(string error) => new() { IsSuccess = false, Error = error };
}
```

**Example:**
```csharp
public class TodoService
{
    public async Task<Result<Todo>> GetTodoAsync(int id)
    {
        var todo = await _repository.GetByIdAsync(id);
        return todo != null 
            ? Result<Todo>.Success(todo)
            : Result<Todo>.Failure($"Todo with ID {id} not found");
    }
}
```

## Validation

### IValidator\<T\>
**Namespace:** `Crisp.Validation`  
**Purpose:** Interface for validators

```csharp
public interface IValidator<T>
{
    ValidationResult Validate(T instance);
}
```

### ValidationResult
**Namespace:** `Crisp.Validation`  
**Purpose:** Contains validation results

```csharp
public class ValidationResult
{
    public IReadOnlyList<ValidationError> Errors { get; }
    public bool IsValid => !Errors.Any();
    
    public static ValidationResult Success() => new(Array.Empty<ValidationError>());
    public static ValidationResult Error(string propertyName, string errorMessage) 
        => new(new[] { new ValidationError(propertyName, errorMessage) });
}
```

### FluentValidatorBuilder\<T\>
**Namespace:** `Crisp.Validation`  
**Purpose:** Fluent API for building validators

```csharp
public class FluentValidatorBuilder<T> : IValidator<T>
{
    public PropertyRuleBuilder<T, TProp> RuleFor<TProp>(Expression<Func<T, TProp>> propertyExpression);
    public FluentValidatorBuilder<T> Must(Func<T, ValidationError?> validationFunc);
    public ValidationResult Validate(T instance);
}
```

**Example:**
```csharp
public class CreateTodoCommandValidator : FluentValidatorBuilder<CreateTodoCommand>
{
    public CreateTodoCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty("Title is required")
            .MaxLength(100, "Title cannot exceed 100 characters");
            
        RuleFor(x => x.Description)
            .MaxLength(500, "Description cannot exceed 500 characters");
    }
}
```

## Events

### IEvent
**Namespace:** `Crisp.Core.Events`  
**Purpose:** Marker interface for domain events

```csharp
public interface IEvent
{
    DateTime OccurredOn { get; }
}
```

### IEventHandler\<TEvent\>
**Namespace:** `Crisp.Core.Events`  
**Purpose:** Handler for domain events

```csharp
public interface IEventHandler<TEvent> where TEvent : IEvent
{
    Task Handle(TEvent @event, CancellationToken cancellationToken = default);
}
```

### IEventPublisher
**Namespace:** `Crisp.Core.Events`  
**Purpose:** Publisher for domain events

```csharp
public interface IEventPublisher
{
    Task Publish<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent;
}
```

**Example:**
```csharp
public record TodoCreatedEvent(int TodoId, string Title, DateTime OccurredOn) : IEvent;

public class TodoCreatedEventHandler : IEventHandler<TodoCreatedEvent>
{
    private readonly IEmailService _emailService;
    
    public TodoCreatedEventHandler(IEmailService emailService)
        => _emailService = emailService;
    
    public async Task Handle(TodoCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        await _emailService.SendWelcomeEmailAsync(@event.TodoId);
    }
}
```

## Exceptions

### CrispException
**Namespace:** `Crisp.Exceptions`  
**Purpose:** Base exception for CRISP-specific errors

```csharp
public class CrispException : Exception
{
    public CrispException() { }
    public CrispException(string message) : base(message) { }
    public CrispException(string message, Exception innerException) : base(message, innerException) { }
}
```

### NotFoundException
**Namespace:** `Crisp.Exceptions`  
**Purpose:** Exception for resource not found scenarios

```csharp
public class NotFoundException : CrispException
{
    public string ResourceType { get; }
    public string ResourceId { get; }
    
    public NotFoundException(string resourceType, string resourceId)
        : base($"{resourceType} with ID '{resourceId}' was not found.")
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }
}
```

### ValidationException
**Namespace:** `Crisp.Exceptions`  
**Purpose:** Exception for validation failures

```csharp
public class ValidationException : CrispException
{
    public ValidationResult ValidationResult { get; }
    
    public ValidationException(ValidationResult validationResult)
        : base("One or more validation errors occurred.")
    {
        ValidationResult = validationResult;
    }
}
```

### UnauthorizedException
**Namespace:** `Crisp.Exceptions`  
**Purpose:** Exception for unauthorized access

```csharp
public class UnauthorizedException : CrispException
{
    public UnauthorizedException() : base("Access denied.") { }
    public UnauthorizedException(string message) : base(message) { }
    public UnauthorizedException(string operation, string resource)
        : base($"Access denied for operation '{operation}' on resource '{resource}'.") { }
}
```

## Configuration

### CrispOptions
**Namespace:** `Crisp`  
**Purpose:** Main configuration options for CRISP framework

```csharp
public class CrispOptions
{
    public EndpointOptions Endpoints { get; set; } = new();
    public PipelineOptions Pipeline { get; set; } = new();
    public LoggingOptions Logging { get; set; } = new();
    public SerializationOptions Serialization { get; set; } = new();
    public ErrorHandlingOptions ErrorHandling { get; set; } = new();
    public CachingOptions Caching { get; set; } = new();
    public ResilienceOptions Resilience { get; set; } = new();
}
```

### CrispJsonOptions
**Namespace:** `Crisp.Serialization`  
**Purpose:** JSON serialization options for CRISP APIs

```csharp
public static class CrispJsonOptions
{
    public static JsonSerializerOptions Default { get; }
    public static JsonSerializerOptions Minimal { get; }
    public static JsonSerializerOptions ProblemDetails { get; }
    
    public static JsonSerializerOptions Create(Action<JsonSerializerOptions> configure);
}
```

**Example:**
```csharp
var options = CrispJsonOptions.Create(opt => 
{
    opt.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    opt.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});
```