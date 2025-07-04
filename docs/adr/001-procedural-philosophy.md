# ADR-001: Why Procedural Over Pure OOP

## Status
Accepted

## Date
2024-01-15

## Context

Modern .NET applications often embrace deep object hierarchies, abstract base classes, and complex inheritance patterns. While OOP has its place, we observed that many applications become unnecessarily complex when everything must be an object.

HTTP requests are inherently procedural: Request → Process → Response. Fighting this nature leads to convoluted code.

## Decision

CRISP embraces a **procedural-first** design philosophy while using objects as data containers and organizational tools.

## Rationale

### 1. **HTTP is Procedural**
```
POST /api/todos
→ Validate request
→ Execute business logic  
→ Return response
```

This is a sequence of steps, not an object interaction.

### 2. **Clarity Over Cleverness**

**Traditional OOP Approach:**
```csharp
public abstract class BaseEntity 
{
    public virtual void Validate() { }
}

public class Todo : BaseEntity, IAuditable, ISoftDeletable
{
    public override void Validate() 
    {
        base.Validate();
        // Where do I even look for all validation?
    }
}
```

**CRISP Procedural Approach:**
```csharp
// Data is just data
public record Todo(int Id, string Title, bool Completed);

// Operations are just functions
public class CreateTodoHandler : ICommandHandler<CreateTodoCommand, Todo>
{
    public Task<Todo> Handle(CreateTodoCommand command)
    {
        // Linear flow you can follow
        return Task.FromResult(new Todo(1, command.Title, false));
    }
}
```

### 3. **Performance**

- No virtual dispatch overhead
- No deep object graphs
- Predictable memory layout
- Better for game development and high-performance scenarios

### 4. **Testability**

```csharp
// Test just tests the function, not an object hierarchy
[Test]
public async Task CreateTodo_WithValidTitle_ReturnsTodo()
{
    var handler = new CreateTodoHandler();
    var result = await handler.Handle(new CreateTodoCommand("Test"));
    Assert.AreEqual("Test", result.Title);
}
```

### 5. **Composition Over Inheritance**

Instead of inheritance, we use pipeline behaviors:
```csharp
Request 
  → ValidationBehavior
  → LoggingBehavior  
  → Handler (pure business logic)
  → Response
```

Each behavior is independent and testable.

## Consequences

### Positive
- ✅ Code is easier to follow (top-to-bottom flow)
- ✅ Onboarding is faster (junior devs understand procedures)
- ✅ Debugging is straightforward (clear stack traces)
- ✅ Performance is predictable
- ✅ Testing is simple

### Negative
- ❌ Some C# developers expect deep OOP
- ❌ Can't leverage some OOP patterns (visitor, etc.)
- ❌ May need to repeat some code (mitigated by behaviors)

### Neutral
- 🔄 Different mental model from traditional .NET
- 🔄 Requires explaining philosophy to teams

## Examples

### Game Development
```csharp
// Procedural game loop - natural and fast
public async Task GameLoop()
{
    var input = await ReadInput();
    var command = ParseCommand(input);
    var newState = await ProcessCommand(command);
    await RenderState(newState);
}
```

### Web API
```csharp
// Each endpoint is a procedure
app.MapPost("/todos", async (CreateTodoCommand cmd, ICommandDispatcher dispatcher) 
    => await dispatcher.Send(cmd));
```

### Why Not Pure Functional?

We're not dogmatic. We use:
- Immutable records (functional idea)
- Pure handlers when possible (functional idea)  
- But also use state when needed (procedural idea)
- And organize with classes (OOP idea)

**CRISP is procedural-first, not procedural-only.**

## References

- [Out of the Tar Pit](http://curtclifton.net/papers/MoseleyMarks06a.pdf) - Complexity in software
- [Write Simple Code](https://www.youtube.com/watch?v=SER1Z0fyUkI) - Casey Muratori
- Go's design philosophy - Simplicity and clarity

## Decision

We will design CRISP with a procedural-first mindset, using objects for organization but not as the primary abstraction mechanism.