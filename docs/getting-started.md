# From Zero to CRISP in 10 Minutes

Let's build a working API with CRISP. No theory, just code.

## 1. Create a New Project

```bash
dotnet new webapi -n TodoApi
cd TodoApi
dotnet add package Crisp.AspNetCore
```

## 2. Define Your First Command

Create `Features/Todos/CreateTodo.cs`:

```csharp
using Crisp.Commands;

namespace TodoApi.Features.Todos;

// The command - what you want to do
public record CreateTodoCommand(string Title, string? Description = null) : ICommand<TodoResponse>;

// The response - what you get back
public record TodoResponse(int Id, string Title, string? Description, bool Completed);

// The handler - how it's done
public class CreateTodoHandler : ICommandHandler<CreateTodoCommand, TodoResponse>
{
    private static int _nextId = 1;
    
    public Task<TodoResponse> Handle(CreateTodoCommand command, CancellationToken cancellationToken)
    {
        var todo = new TodoResponse(
            _nextId++,
            command.Title,
            command.Description,
            false
        );
        
        return Task.FromResult(todo);
    }
}
```

## 3. Wire It Up

Replace `Program.cs`:

```csharp
using TodoApi.Features.Todos;

var builder = WebApplication.CreateBuilder(args);

// Add CRISP
builder.Services.AddCrisp(crisp =>
{
    crisp.RegisterHandlersFromAssemblies(typeof(Program).Assembly);
});

var app = builder.Build();

// Map CRISP endpoints
app.MapCrisp();

app.Run();
```

## 4. Run It!

```bash
dotnet run
```

Your API is live! Test it:

```bash
curl -X POST https://localhost:5001/api/todos/create \
  -H "Content-Type: application/json" \
  -d '{"title":"Learn CRISP","description":"It''s pretty cool"}'
```

## 5. Add a Query

Create `Features/Todos/GetTodos.cs`:

```csharp
using Crisp.Queries;

namespace TodoApi.Features.Todos;

// Get all todos
public record GetTodosQuery() : IQuery<List<TodoResponse>>;

public class GetTodosHandler : IQueryHandler<GetTodosQuery, List<TodoResponse>>
{
    private static readonly List<TodoResponse> _todos = new();
    
    public Task<List<TodoResponse>> Handle(GetTodosQuery query, CancellationToken cancellationToken)
        => Task.FromResult(_todos.ToList());
}
```

That's it! The endpoint is automatically available at `/api/todos/get`.

## 6. Add Validation (Optional)

Just use Data Annotations:

```csharp
using System.ComponentModel.DataAnnotations;

public record CreateTodoCommand(
    [Required]
    [StringLength(200, MinimumLength = 1)]
    string Title, 
    
    [StringLength(1000)]
    string? Description = null) : ICommand<TodoResponse>;
```

Validation happens automatically in the pipeline!

CRISP automatically uses your validators!

## 7. Add Pipeline Behavior (Optional)

Want logging? Add it to the pipeline:

```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        => _logger = logger;
    
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next)
    {
        _logger.LogInformation("Handling {RequestType}", typeof(TRequest).Name);
        var response = await next();
        _logger.LogInformation("Handled {RequestType}", typeof(TRequest).Name);
        return response;
    }
}

// Register it
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
```

## 🎯 What Just Happened?

1. **Commands/Queries** define what you want to do
2. **Handlers** contain your business logic
3. **Pipeline** handles cross-cutting concerns
4. **CRISP** wires it all together

No controllers. No service interfaces. Just your logic.

## 📚 Next Steps

- [Pipeline Behaviors](concepts/pipeline.md) - Add logging, validation, retry policies
- [Vertical Layers](concepts/vertical-layers.md) - Organize larger applications
- [Blazor Integration](blazor-guide.md) - Full-stack CRISP
- [Testing Guide](testing.md) - Test handlers in isolation

## 💡 Tips

- Keep handlers focused - one job per handler
- Use records for immutable commands/queries
- Let the pipeline handle cross-cutting concerns
- Start simple, add behaviors as needed

Ready to dive deeper? Check out the [Core Concepts](concepts/)!