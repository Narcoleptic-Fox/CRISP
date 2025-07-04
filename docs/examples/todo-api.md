# CRISP Todo API Example

A complete working example showing CRISP patterns in action, based on the actual TodoApi sample.

## Project Structure
```
TodoApi/
├── Program.cs
├── TodoApi.csproj
├── TodoApi.http
├── Features/
│   └── Todo/
│       ├── Commands/
│       │   ├── CreateTodo.cs
│       │   ├── UpdateTodo.cs
│       │   └── DeleteTodo.cs
│       ├── Queries/
│       │   ├── GetTodo.cs
│       │   └── GetTodos.cs
│       ├── Models/
│       │   ├── TodoDto.cs
│       │   └── TodoEntity.cs
│       ├── ITodoRepository.cs
│       └── InMemoryTodoRepository.cs
├── Properties/
│   └── launchSettings.json
└── Security/
    └── SecurityExamples.cs
```

## Key Files

### `Program.cs`
```csharp
using TodoApi.Features.Todo;

namespace TodoApi;
public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Add CRISP framework
        builder.Services.AddCrisp(crisp =>
        {
            crisp.RegisterHandlersFromAssemblies(typeof(Program).Assembly);
        });

        // Add additional services
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ITodoRepository, InMemoryTodoRepository>();

        WebApplication app = builder.Build();

        // Configure the HTTP request pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();

        // Map CRISP endpoints
        app.MapCrisp();

        app.Run();
    }
}
```

### `Models/TodoDto.cs`
```csharp
namespace TodoApi.Features.Todo.Models;

public record TodoDto(
    int Id,
    string Title,
    string? Description,
    bool IsCompleted,
    DateTime CreatedAt,
    DateTime? CompletedAt);

public record CreateTodoDto(
    string Title,
    string? Description);

public record UpdateTodoDto(
    string? Title,
    string? Description,
    bool? IsCompleted);
```

### `Models/TodoEntity.cs`
```csharp
namespace TodoApi.Features.Todo.Models;

public class TodoEntity
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}
```

### `ITodoRepository.cs`
```csharp
using TodoApi.Features.Todo.Models;

namespace TodoApi.Features.Todo;

public interface ITodoRepository
{
    Task<TodoEntity> CreateAsync(TodoEntity todo);
    Task<TodoEntity?> GetByIdAsync(int id);
    Task<IEnumerable<TodoEntity>> GetAllAsync();
    Task<TodoEntity?> UpdateAsync(int id, TodoEntity todo);
    Task<bool> DeleteAsync(int id);
}
```

## Commands

### `Commands/CreateTodo.cs`
```csharp
using Crisp.Commands;
using System.ComponentModel.DataAnnotations;
using TodoApi.Features.Todo.Models;

namespace TodoApi.Features.Todo.Commands;

public record CreateTodoCommand(
    [Required]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters")]
    string Title,

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    string? Description) : ICommand<TodoDto>;

public class CreateTodo : ICommandHandler<CreateTodoCommand, TodoDto>
{
    private readonly ITodoRepository _repository;

    public CreateTodo(ITodoRepository repository) => _repository = repository;

    public async Task<TodoDto> Handle(CreateTodoCommand request, CancellationToken cancellationToken)
    {
        TodoEntity todo = new()
        {
            Title = request.Title,
            Description = request.Description,
            IsCompleted = false
        };

        TodoEntity createdTodo = await _repository.CreateAsync(todo);

        return new TodoDto(
            createdTodo.Id,
            createdTodo.Title,
            createdTodo.Description,
            createdTodo.IsCompleted,
            createdTodo.CreatedAt,
            createdTodo.CompletedAt);
    }
}
```

### `Commands/UpdateTodo.cs`
```csharp
using Crisp.Commands;
using System.ComponentModel.DataAnnotations;
using TodoApi.Features.Todo.Models;

namespace TodoApi.Features.Todo.Commands;

public record UpdateTodoCommand(
    int Id,
    
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters")]
    string? Title,
    
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    string? Description,
    
    bool? IsCompleted) : ICommand<TodoDto?>;

public class UpdateTodo : ICommandHandler<UpdateTodoCommand, TodoDto?>
{
    private readonly ITodoRepository _repository;

    public UpdateTodo(ITodoRepository repository) => _repository = repository;

    public async Task<TodoDto?> Handle(UpdateTodoCommand request, CancellationToken cancellationToken)
    {
        TodoEntity? existingTodo = await _repository.GetByIdAsync(request.Id);
        if (existingTodo == null)
            return null;

        TodoEntity updatedTodo = new()
        {
            Id = existingTodo.Id,
            Title = request.Title ?? existingTodo.Title,
            Description = request.Description ?? existingTodo.Description,
            IsCompleted = request.IsCompleted ?? existingTodo.IsCompleted,
            CreatedAt = existingTodo.CreatedAt,
            CompletedAt = (request.IsCompleted == true && !existingTodo.IsCompleted) 
                ? DateTime.UtcNow 
                : existingTodo.CompletedAt
        };

        TodoEntity? result = await _repository.UpdateAsync(request.Id, updatedTodo);
        if (result == null)
            return null;

        return new TodoDto(
            result.Id,
            result.Title,
            result.Description,
            result.IsCompleted,
            result.CreatedAt,
            result.CompletedAt);
    }
}
```

### `Commands/DeleteTodo.cs`
```csharp
using Crisp.Commands;

namespace TodoApi.Features.Todo.Commands;

public record DeleteTodoCommand(int Id) : ICommand<bool>;

public class DeleteTodo : ICommandHandler<DeleteTodoCommand, bool>
{
    private readonly ITodoRepository _repository;

    public DeleteTodo(ITodoRepository repository) => _repository = repository;

    public async Task<bool> Handle(DeleteTodoCommand request, CancellationToken cancellationToken)
    {
        return await _repository.DeleteAsync(request.Id);
    }
}
```

## Queries

### `Queries/GetTodo.cs`
```csharp
using Crisp.Queries;
using TodoApi.Features.Todo.Models;

namespace TodoApi.Features.Todo.Queries;

public record GetTodoQuery(int Id) : IQuery<TodoDto?>;

public class GetTodo : IQueryHandler<GetTodoQuery, TodoDto?>
{
    private readonly ITodoRepository _repository;

    public GetTodo(ITodoRepository repository) => _repository = repository;

    public async Task<TodoDto?> Handle(GetTodoQuery request, CancellationToken cancellationToken)
    {
        TodoEntity? todo = await _repository.GetByIdAsync(request.Id);
        if (todo == null)
            return null;

        return new TodoDto(
            todo.Id,
            todo.Title,
            todo.Description,
            todo.IsCompleted,
            todo.CreatedAt,
            todo.CompletedAt);
    }
}
```

### `Queries/GetTodos.cs`
```csharp
using Crisp.Queries;
using TodoApi.Features.Todo.Models;

namespace TodoApi.Features.Todo.Queries;

public record GetTodosQuery() : IQuery<IEnumerable<TodoDto>>;

public class GetTodos : IQueryHandler<GetTodosQuery, IEnumerable<TodoDto>>
{
    private readonly ITodoRepository _repository;

    public GetTodos(ITodoRepository repository) => _repository = repository;

    public async Task<IEnumerable<TodoDto>> Handle(GetTodosQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<TodoEntity> todos = await _repository.GetAllAsync();

        return todos.Select(todo => new TodoDto(
            todo.Id,
            todo.Title,
            todo.Description,
            todo.IsCompleted,
            todo.CreatedAt,
            todo.CompletedAt));
    }
}
```

## Repository Implementation

### `InMemoryTodoRepository.cs`
```csharp
using TodoApi.Features.Todo.Models;

namespace TodoApi.Features.Todo;

public class InMemoryTodoRepository : ITodoRepository
{
    private readonly Dictionary<int, TodoEntity> _todos = new();
    private int _nextId = 1;

    public Task<TodoEntity> CreateAsync(TodoEntity todo)
    {
        todo.Id = _nextId++;
        todo.CreatedAt = DateTime.UtcNow;
        _todos[todo.Id] = todo;
        return Task.FromResult(todo);
    }

    public Task<TodoEntity?> GetByIdAsync(int id)
    {
        _todos.TryGetValue(id, out TodoEntity? todo);
        return Task.FromResult(todo);
    }

    public Task<IEnumerable<TodoEntity>> GetAllAsync()
    {
        return Task.FromResult(_todos.Values.AsEnumerable());
    }

    public Task<TodoEntity?> UpdateAsync(int id, TodoEntity todo)
    {
        if (!_todos.ContainsKey(id))
            return Task.FromResult<TodoEntity?>(null);

        _todos[id] = todo;
        return Task.FromResult<TodoEntity?>(todo);
    }

    public Task<bool> DeleteAsync(int id)
    {
        return Task.FromResult(_todos.Remove(id));
    }
}
```

## API Testing

### `TodoApi.http`
```http
### Get all todos
GET https://localhost:7089/api/todo/gettodos

### Get a specific todo
GET https://localhost:7089/api/todo/gettodo/1

### Create a new todo
POST https://localhost:7089/api/todo/createtodo
Content-Type: application/json

{
  "title": "Learn CRISP",
  "description": "Build a todo API with CRISP framework"
}

### Update a todo
PUT https://localhost:7089/api/todo/updatetodo
Content-Type: application/json

{
  "id": 1,
  "title": "Learn CRISP Framework",
  "description": "Master the CRISP pattern for building APIs",
  "isCompleted": true
}

### Delete a todo
DELETE https://localhost:7089/api/todo/deletetodo
Content-Type: application/json

{
  "id": 1
}
```

## Running the Application

1. **Clone and run:**
   ```bash
   git clone https://github.com/Narcoleptic-Fox/CRISP
   cd CRISP/samples/TodoApi
   dotnet run
   ```

2. **Test the API:**
   - Open `TodoApi.http` in VS Code or Visual Studio
   - Execute the HTTP requests to test all endpoints

3. **Automatic endpoint discovery:**
   - All commands and queries are automatically mapped to endpoints
   - No controllers needed!
   - Validation happens automatically using Data Annotations

## Key Features Demonstrated

✅ **CQRS Pattern**: Commands (write) and Queries (read) are separated  
✅ **Automatic Validation**: Data Annotations provide input validation  
✅ **Dependency Injection**: Constructor injection works seamlessly  
✅ **Endpoint Auto-Discovery**: No manual routing required  
✅ **Clean Architecture**: Feature-based organization  
✅ **Testability**: Each handler can be unit tested independently  

## Next Steps

- Add authentication and authorization
- Implement caching for queries
- Add database persistence
- Implement event publishing
- Add comprehensive error handling

This example shows how CRISP makes building APIs simpler, cleaner, and more maintainable!
