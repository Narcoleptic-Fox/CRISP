# Pipeline Behavior Registration in CRISP

Pipeline behaviors are the heart of CRISP's cross-cutting concerns. This guide explains how to register behaviors with different scopes and specificity.

## 🎯 Overview

CRISP supports four levels of behavior registration:

1. **Global Behaviors** - Run for all commands and queries
2. **Command-Only Behaviors** - Run only for commands
3. **Query-Only Behaviors** - Run only for queries
4. **Specific Behaviors** - Run only for specific command/query types

## 📚 Core Interfaces

```csharp
// Base pipeline behavior interface
public interface IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken = default);
}

// Command-only behaviors
public interface ICommandPipelineBehavior<TCommand, TResponse> : IPipelineBehavior<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
}

// Query-only behaviors
public interface IQueryPipelineBehavior<TQuery, TResponse> : IPipelineBehavior<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
}
```

## 🔧 Registration Methods

### Global Behaviors

For cross-cutting concerns that apply to everything:

```csharp
// Runs for ALL commands and queries
services.AddPipelineBehavior<LoggingBehavior<,>>();
services.AddPipelineBehavior<ExceptionHandlingBehavior<,>>();
services.AddPipelineBehavior<PerformanceMonitoringBehavior<,>>();
```

**Use for:**
- Logging
- Exception handling
- Performance monitoring
- Request/response validation

### Command-Only Behaviors

For behaviors that should only run on write operations:

```csharp
// Runs for ALL commands, but NOT queries
services.AddCommandBehavior<TransactionBehavior<,>>();
services.AddCommandBehavior<AuditBehavior<,>>();
services.AddCommandBehavior<EventPublishingBehavior<,>>();
```

**Use for:**
- Database transactions
- Audit logging
- Domain event publishing
- State change validation

### Query-Only Behaviors

For behaviors that should only run on read operations:

```csharp
// Runs for ALL queries, but NOT commands
services.AddQueryBehavior<CachingBehavior<,>>();
services.AddQueryBehavior<QueryOptimizationBehavior<,>>();
services.AddQueryBehavior<ReadOnlyValidationBehavior<,>>();
```

**Use for:**
- Result caching
- Query optimization
- Read-only validation
- Response compression

### Specific Behaviors

For behaviors that target individual command/query types:

```csharp
// Runs ONLY for specific commands/queries
services.AddPipelineBehavior<UserAuditBehavior, CreateUserCommand, User>();
services.AddPipelineBehavior<ProductCacheBehavior, GetProductQuery, Product>();
services.AddPipelineBehavior<PaymentRetryBehavior, ProcessPaymentCommand, PaymentResult>();
```

**Use for:**
- Command-specific validation
- Targeted caching strategies
- Special retry policies
- Feature-specific concerns

## 💻 Implementation Examples

### Global Logging Behavior

```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        => _logger = logger;
      public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("Handling {RequestName}", requestName);
        
        var sw = Stopwatch.StartNew();
        var response = await next();
        sw.Stop();
        
        _logger.LogInformation("Handled {RequestName} in {ElapsedMs}ms", 
            requestName, sw.ElapsedMilliseconds);
            
        return response;
    }
}
```

### Command-Only Transaction Behavior

```csharp
public class TransactionBehavior<TCommand, TResponse> : ICommandPipelineBehavior<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    private readonly IDbContext _dbContext;
    
    public TransactionBehavior(IDbContext dbContext)
        => _dbContext = dbContext;
      public async Task<TResponse> Handle(TCommand request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Only runs for commands!
        using var transaction = await _dbContext.BeginTransactionAsync();
        try
        {
            var response = await next();
            await transaction.CommitAsync();
            return response;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
```

### Query-Only Caching Behavior

```csharp
public class CachingBehavior<TQuery, TResponse> : IQueryPipelineBehavior<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    private readonly IMemoryCache _cache;
    
    public CachingBehavior(IMemoryCache cache)
        => _cache = cache;
      public async Task<TResponse> Handle(TQuery request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Only runs for queries!
        var cacheKey = GenerateCacheKey(request);
        
        if (_cache.TryGetValue(cacheKey, out TResponse cachedResponse))
        {
            return cachedResponse;
        }
        
        var response = await next();
        _cache.Set(cacheKey, response, GetCacheDuration(request));
        
        return response;
    }
    
    private string GenerateCacheKey(TQuery request)
        => $"{typeof(TQuery).Name}:{JsonSerializer.Serialize(request)}";
        
    private TimeSpan GetCacheDuration(TQuery request)
        => request is ICacheable cacheable ? cacheable.CacheDuration : TimeSpan.FromMinutes(5);
}
```

### Specific Command Behavior

```csharp
public class CreateUserAuditBehavior : IPipelineBehavior<CreateUserCommand, User>
{
    private readonly IAuditService _auditService;
    
    public CreateUserAuditBehavior(IAuditService auditService)
        => _auditService = auditService;
    
    public async Task<User> Handle(CreateUserCommand request, RequestHandlerDelegate<User> next)
    {
        // Only runs for CreateUserCommand!
        var user = await next();
        
        await _auditService.LogAsync(new AuditEntry
        {
            Action = "UserCreated",
            EntityId = user.Id,
            EntityType = "User",
            Details = $"Created user with email: {user.Email}",
            Timestamp = DateTime.UtcNow
        });
        
        return user;
    }
}
```

## 🔄 Pipeline Execution Order

The pipeline resolver executes behaviors in this order:

```
1. Global Behaviors (in registration order)
2. Command/Query-specific Behaviors (in registration order)
3. Type-specific Behaviors (in registration order)
4. Handler
```

### Example: CreateUserCommand Flow

```
CreateUserCommand → 
  LoggingBehavior (global) →
  ExceptionHandlingBehavior (global) →
  TransactionBehavior (command-only) →
  AuditBehavior (command-only) →
  CreateUserAuditBehavior (specific) →
  CreateUserHandler
```

### Example: GetProductQuery Flow

```
GetProductQuery →
  LoggingBehavior (global) →
  ExceptionHandlingBehavior (global) →
  CachingBehavior (query-only) →
  GetProductHandler
```

## 📝 Registration Patterns

### Complete Setup Example

```csharp
// Program.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddCrisp(options =>
    {
        crisp.RegisterHandlersFromAssemblies(typeof(Program).Assembly);
    });
    
    // Global behaviors - always first
    services.AddPipelineBehavior<LoggingBehavior<,>>(ServiceLifetime.Singleton);
    services.AddPipelineBehavior<ExceptionHandlingBehavior<,>>(ServiceLifetime.Singleton);
    
    // Command-only behaviors
    services.AddCommandBehavior<TransactionBehavior<,>>(ServiceLifetime.Scoped);
    services.AddCommandBehavior<AuditBehavior<,>>(ServiceLifetime.Scoped);
    services.AddCommandBehavior<ValidationBehavior<,>>(ServiceLifetime.Scoped);
    
    // Query-only behaviors
    services.AddQueryBehavior<CachingBehavior<,>>(ServiceLifetime.Singleton);
    services.AddQueryBehavior<QueryTimeoutBehavior<,>>(ServiceLifetime.Scoped);
    
    // Specific behaviors
    services.AddPipelineBehavior<CreateUserAuditBehavior, CreateUserCommand, User>();
    services.AddPipelineBehavior<ExpensiveQueryCacheBehavior, GetSalesReportQuery, SalesReport>();
}
```

### Service Lifetime Guidelines

- **Singleton**: Stateless behaviors (logging, caching)
- **Scoped**: Database/transaction behaviors
- **Transient**: Rarely used (avoid for performance)

## 🎮 Domain-Specific Examples

### Game Development

```csharp
// Anti-cheat for all player commands
services.AddCommandBehavior<AntiCheatBehavior<,>>();

// Rate limiting for specific actions
services.AddPipelineBehavior<ActionRateLimitBehavior, PlayerActionCommand, ActionResult>();

// Cache frequently accessed game state
services.AddQueryBehavior<GameStateCachingBehavior<,>>();
```

### Financial Application

```csharp
// Audit all monetary commands
services.AddCommandBehavior<FinancialAuditBehavior<,>>();

// Special validation for payments
services.AddPipelineBehavior<PaymentValidationBehavior, ProcessPaymentCommand, PaymentResult>();

// Encrypt sensitive query results
services.AddQueryBehavior<SensitiveDataEncryptionBehavior<,>>();
```

## ⚡ Performance Considerations

1. **Behavior Order Matters**: Place fastest behaviors first
2. **Avoid Duplicate Work**: Don't validate in multiple behaviors
3. **Cache Wisely**: Not all queries benefit from caching
4. **Measure Impact**: Use the built-in performance monitoring

```csharp
// Good: Fast checks first
services.AddPipelineBehavior<QuickValidationBehavior<,>>();
services.AddPipelineBehavior<ExpensiveDatabaseBehavior<,>>();

// Bad: Slow operations first
services.AddPipelineBehavior<DatabaseLookupBehavior<,>>();
services.AddPipelineBehavior<SimpleNullCheckBehavior<,>>();
```

## 🚫 Common Pitfalls

1. **Don't Mix Concerns**: Keep behaviors focused on one thing
2. **Avoid State**: Behaviors should be stateless when possible
3. **Don't Over-Register**: Not everything needs a behavior
4. **Watch Dependencies**: Heavy DI in behaviors impacts performance

## ✅ Best Practices

1. **Name Clearly**: `XxxBehavior` where Xxx describes what it does
2. **Log Wisely**: Don't log sensitive data in behaviors
3. **Fail Fast**: Validate early in the pipeline
4. **Document Behaviors**: Explain why each behavior exists
5. **Test Behaviors**: Unit test behaviors independently

## 🎯 Decision Guide

| If you need to...             | Use...                |
| ----------------------------- | --------------------- |
| Log all operations            | Global behavior       |
| Wrap commands in transactions | Command-only behavior |
| Cache query results           | Query-only behavior   |
| Audit specific operations     | Specific behavior     |
| Validate all inputs           | Global behavior       |
| Retry failed payments         | Specific behavior     |
| Track performance             | Global behavior       |
| Publish domain events         | Command-only behavior |

Remember: Start simple with global behaviors, add specificity as needed!