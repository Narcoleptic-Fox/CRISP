# Commands and Queries in CRISP

CRISP uses Command Query Responsibility Segregation (CQRS) to separate reads from writes. Simple concept, powerful results.

## 📝 Commands vs Queries

**Commands** change state:
```csharp
public record CreateUserCommand(string Name, string Email) : ICommand<User>;
public record DeleteUserCommand(int Id) : ICommand;
public record UpdateUserCommand(int Id, string Name) : ICommand<User>;
```

**Queries** read state:
```csharp
public record GetUserQuery(int Id) : IQuery<User>;
public record ListUsersQuery(int Page, int PageSize) : IQuery<PagedResult<User>>;
public record SearchUsersQuery(string Term) : IQuery<IEnumerable<User>>;
```

## 🎯 Core Interfaces

```csharp
// Commands
public interface ICommand<TResponse> : IRequest<TResponse> { }
public interface ICommand : IRequest { }

// Queries  
public interface IQuery<TResponse> : IRequest<TResponse> { }

// Handlers
public interface ICommandHandler<TCommand, TResponse> 
    where TCommand : ICommand<TResponse>
{
    Task<TResponse> Handle(TCommand command, CancellationToken cancellationToken);
}

public interface IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    Task<TResponse> Handle(TQuery query, CancellationToken cancellationToken);
}
```

## 💡 Why Separate Commands and Queries?

### 1. **Different Optimization Needs**
```csharp
// Commands: Optimize for consistency
public class CreateOrderHandler : ICommandHandler<CreateOrderCommand, Order>
{
    public async Task<Order> Handle(CreateOrderCommand command)
    {
        using var transaction = await _db.BeginTransactionAsync();
        
        var order = new Order(command.Items);
        await _db.Orders.AddAsync(order);
        await _inventory.ReserveItems(command.Items);
        await _payment.ChargeCard(command.PaymentInfo);
        
        await transaction.CommitAsync();
        return order;
    }
}

// Queries: Optimize for speed
public class GetOrdersHandler : IQueryHandler<GetOrdersQuery, List<OrderDto>>
{
    public async Task<List<OrderDto>> Handle(GetOrdersQuery query)
    {
        // Read from cache, read replica, or denormalized view
        return await _cache.GetOrAddAsync(
            $"orders-{query.UserId}",
            () => _readDb.Orders.Where(o => o.UserId == query.UserId).ToListAsync()
        );
    }
}
```

### 2. **Different Models**
```csharp
// Write model (normalized)
public class Order
{
    public int Id { get; set; }
    public List<OrderItem> Items { get; set; }
    public int CustomerId { get; set; }
}

// Read model (denormalized) 
public class OrderDto
{
    public int Id { get; set; }
    public string CustomerName { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; }
}
```

### 3. **Different Validation**
```csharp
// Command validation - strict business rules
public class CreateProductValidator : IValidator<CreateProductCommand>
{
    public ValidationResult Validate(CreateProductCommand command)
    {
        // Check inventory, pricing rules, etc.
    }
}

// Query validation - just parameter validation
public class GetProductValidator : IValidator<GetProductQuery>  
{
    public ValidationResult Validate(GetProductQuery query)
    {
        // Just check ID is valid
    }
}
```

## 🚀 Patterns and Best Practices

### 1. **Return What Makes Sense**
```csharp
// Return the created entity
public record CreateTodoCommand(string Title) : ICommand<Todo>;

// Return just the ID
public record CreateUserCommand(string Email) : ICommand<int>;

// Return nothing for fire-and-forget
public record SendEmailCommand(string To, string Body) : ICommand;
```

### 2. **Query Flexibility**
```csharp
public record SearchProductsQuery : IQuery<PagedResult<Product>>
{
    public string? Term { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public string? Category { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }
}
```

### 3. **Composite Commands**
```csharp
public record ProcessOrderCommand(
    List<OrderItem> Items,
    PaymentInfo Payment,
    ShippingAddress Shipping
) : ICommand<OrderResult>;

public class ProcessOrderHandler : ICommandHandler<ProcessOrderCommand, OrderResult>
{
    public async Task<OrderResult> Handle(ProcessOrderCommand command)
    {
        // Orchestrate multiple operations
        var order = await _orderService.Create(command.Items);
        var payment = await _paymentService.Charge(command.Payment);
        var shipping = await _shippingService.Schedule(command.Shipping);
        
        return new OrderResult(order, payment, shipping);
    }
}
```

## 🎮 Game Development Example

```csharp
// Commands modify game state
public record MoveCharacterCommand(int PlayerId, Vector3 Position) : ICommand<MoveResult>;
public record AttackCommand(int AttackerId, int TargetId) : ICommand<CombatResult>;
public record UseItemCommand(int PlayerId, int ItemId) : ICommand<UseItemResult>;

// Queries read game state
public record GetPlayerStatsQuery(int PlayerId) : IQuery<PlayerStats>;
public record GetNearbyPlayersQuery(Vector3 Position, float Radius) : IQuery<List<Player>>;
public record GetLeaderboardQuery(int Top = 10) : IQuery<List<LeaderboardEntry>>;

// Handlers
public class MoveCharacterHandler : ICommandHandler<MoveCharacterCommand, MoveResult>
{
    public async Task<MoveResult> Handle(MoveCharacterCommand command)
    {
        // Validate move
        if (!IsValidPosition(command.Position))
            return MoveResult.Invalid();
            
        // Update position
        await _gameState.UpdatePosition(command.PlayerId, command.Position);
        
        // Check for triggers (entered new area, etc)
        var triggers = await _triggerSystem.Check(command.PlayerId, command.Position);
        
        return new MoveResult(command.Position, triggers);
    }
}
```

## 📊 Advanced Patterns

### 1. **Notification Pattern**
```csharp
public record CreateUserCommand(string Email) : ICommand<int>, INotification
{
    public string NotificationEmail => Email;
}

// After handler completes, notification is sent automatically
```

### 2. **Streaming Queries**
```csharp
public record StreamEventsQuery(DateTime Since) : IStreamQuery<Event>;

public class StreamEventsHandler : IStreamQueryHandler<StreamEventsQuery, Event>
{
    public async IAsyncEnumerable<Event> Handle(StreamEventsQuery query)
    {
        await foreach (var evt in _eventStore.GetEvents(query.Since))
        {
            yield return evt;
        }
    }
}
```

### 3. **Batch Commands**
```csharp
public record BatchUpdateCommand<T>(List<T> Items) : ICommand<BatchResult>;

// Process multiple items efficiently
```

## 💾 Command Sourcing

Store commands for audit/replay:

```csharp
public class CommandSourcingBehavior<TCommand, TResponse> : IPipelineBehavior<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    public async Task<TResponse> Handle(TCommand command, RequestHandlerDelegate<TResponse> next)
    {
        // Store command
        await _commandStore.Store(new CommandRecord
        {
            Type = typeof(TCommand).Name,
            Data = JsonSerializer.Serialize(command),
            Timestamp = DateTime.UtcNow,
            UserId = _currentUser.Id
        });
        
        // Execute
        return await next();
    }
}
```

## ✅ Guidelines

**DO:**
- Keep commands/queries simple (POCOs)
- Name them clearly (verb + noun)
- Return appropriate types
- Validate in handlers or behaviors

**DON'T:**
- Put logic in commands/queries
- Make them mutable
- Mix reads and writes
- Create god commands

## 🔧 Testing

```csharp
[Test]
public async Task CreateUser_WithValidEmail_ReturnsUserId()
{
    // Arrange
    var handler = new CreateUserHandler(_mockDb.Object);
    var command = new CreateUserCommand("test@example.com");
    
    // Act
    var userId = await handler.Handle(command, CancellationToken.None);
    
    // Assert
    Assert.Greater(userId, 0);
    _mockDb.Verify(db => db.Users.AddAsync(It.IsAny<User>()), Times.Once);
}
```

Next: [Endpoints](endpoints.md) →