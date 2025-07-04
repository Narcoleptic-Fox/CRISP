# Crisp.Runtime API Reference

High-performance runtime components with pre-compiled dispatchers and comprehensive pipeline behaviors.

## Dispatchers

The runtime package provides pre-compiled dispatchers that eliminate reflection overhead during request processing.

### PreCompiledCommandDispatcher
**Namespace:** `Crisp.Dispatchers`  
**Access:** Internal (configured automatically)  
**Purpose:** High-performance command dispatcher using pre-compiled pipelines

The dispatcher pre-compiles expression trees at startup for maximum runtime performance:

```csharp
// Automatic registration - no direct usage needed
services.AddCrisp(); // Registers PreCompiledCommandDispatcher internally
```

### PreCompiledQueryDispatcher
**Namespace:** `Crisp.Dispatchers`  
**Access:** Internal (configured automatically)  
**Purpose:** High-performance query dispatcher using pre-compiled pipelines

Similar to command dispatcher, this provides optimized query processing through pre-compilation.

## Pipeline Behaviors

### ValidationBehavior\<TRequest, TResponse\>
**Namespace:** `Crisp.Runtime.Pipeline`  
**Purpose:** Validates requests using registered validators

```csharp
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
```

**Example Registration:**
```csharp
services.AddCrisp(builder => 
{
    builder.AddPipelineBehavior<ValidationBehavior<,>>();
});

// Register validators
services.AddTransient<IValidator<CreateTodoCommand>, CreateTodoCommandValidator>();
```

### LoggingBehavior\<TRequest, TResponse\>
**Namespace:** `Crisp.Runtime.Pipeline`  
**Purpose:** Provides structured logging for all requests

```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
```

**Configuration:**
```csharp
services.AddCrisp(builder => 
{
    builder.ConfigureOptions(opt => 
    {
        opt.Logging.LogRequestDetails = true;
        opt.Logging.LogResponseDetails = false;
        opt.Logging.LogPerformanceMetrics = true;
    });
    builder.AddPipelineBehavior<LoggingBehavior<,>>();
});
```

**Sample Log Output:**
```
[INF] Handling CreateTodoCommand with data: {"Title":"Buy groceries","Description":"Milk, eggs, bread"}
[INF] Handled CreateTodoCommand in 45ms with result: 123
```

### ErrorHandlingBehavior\<TRequest, TResponse\>
**Namespace:** `Crisp.Runtime.Pipeline`  
**Purpose:** Centralized exception handling with correlation IDs

```csharp
public class ErrorHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
```

**Features:**
- Automatic correlation ID generation
- Structured error logging
- Exception type mapping
- Sensitive data protection

**Configuration:**
```csharp
services.AddCrisp(builder => 
{
    builder.ConfigureOptions(opt => 
    {
        opt.ErrorHandling.IncludeStackTrace = false; // Production setting
        opt.ErrorHandling.ExposeInternalErrors = false;
        opt.ErrorHandling.EnableCorrelationIds = true;
    });
    builder.AddPipelineBehavior<ErrorHandlingBehavior<,>>();
});
```

### DataAnnotationValidationBehavior\<TRequest, TResponse\>
**Namespace:** `Crisp.Runtime.Pipeline`  
**Purpose:** Validates requests using data annotations

```csharp
public class DataAnnotationValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
```

**Example:**
```csharp
public record CreateTodoCommand(
    [Required] [MaxLength(100)] string Title,
    [MaxLength(500)] string? Description
) : ICommand<int>;

// Behavior automatically validates based on attributes
services.AddCrisp(builder => 
{
    builder.AddPipelineBehavior<DataAnnotationValidationBehavior<,>>();
});
```

## Caching

### CachingBehavior\<TRequest, TResponse\>
**Namespace:** `Crisp.Runtime.Pipeline`  
**Purpose:** In-memory caching for queries and commands

```csharp
public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
```

**Usage with ICacheable:**
```csharp
public record GetTodoQuery(int Id) : IQuery<Todo>, ICacheable
{
    public string CacheKey => $"todo:{Id}";
    public TimeSpan CacheDuration => TimeSpan.FromMinutes(5);
}

// Registration
services.AddCrisp(builder => 
{
    builder.AddPipelineBehavior<CachingBehavior<,>>();
});
services.AddMemoryCache();
```

### DistributedCachingBehavior\<TRequest, TResponse\>
**Namespace:** `Crisp.Runtime.Pipeline`  
**Purpose:** Distributed caching for scalable applications

```csharp
public class DistributedCachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
```

**Configuration:**
```csharp
services.AddStackExchangeRedisCache(options => 
{
    options.Configuration = "localhost:6379";
});

services.AddCrisp(builder => 
{
    builder.ConfigureOptions(opt => 
    {
        opt.Caching.DefaultDuration = TimeSpan.FromMinutes(10);
        opt.Caching.EnableDistributedCaching = true;
    });
    builder.AddPipelineBehavior<DistributedCachingBehavior<,>>();
});
```

### ICacheable Interface
**Namespace:** `Crisp.Runtime.Pipeline`  
**Purpose:** Marker interface for cacheable requests

```csharp
public interface ICacheable
{
    string CacheKey { get; }
    TimeSpan CacheDuration { get; }
}
```

### CacheInvalidationService
**Namespace:** `Crisp.Runtime.Pipeline`  
**Purpose:** Manages cache invalidation across distributed systems

```csharp
public interface ICacheInvalidationService
{
    Task InvalidateAsync(string cacheKey);
    Task InvalidateByPatternAsync(string pattern);
    Task InvalidateByTagAsync(string tag);
}
```

**Example:**
```csharp
public class UpdateTodoHandler : ICommandHandler<UpdateTodoCommand, Todo>
{
    private readonly ICacheInvalidationService _cacheInvalidation;
    
    public async Task<Todo> Handle(UpdateTodoCommand command, CancellationToken cancellationToken = default)
    {
        var todo = await _repository.UpdateAsync(command.Todo);
        
        // Invalidate related caches
        await _cacheInvalidation.InvalidateAsync($"todo:{command.Todo.Id}");
        await _cacheInvalidation.InvalidateByPatternAsync("todos:*");
        
        return todo;
    }
}
```

## Resilience

### RetryBehavior\<TRequest, TResponse\>
**Namespace:** `Crisp.Runtime.Pipeline`  
**Purpose:** Automatic retry with exponential backoff

```csharp
public class RetryBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
```

**Usage with IRetryable:**
```csharp
public record GetExternalDataQuery(string Url) : IQuery<string>, IRetryable
{
    public int MaxRetries => 3;
    public TimeSpan BaseDelay => TimeSpan.FromSeconds(1);
    public bool ShouldRetry(Exception exception) => exception is HttpRequestException;
}

// Configuration
services.AddCrisp(builder => 
{
    builder.ConfigureOptions(opt => 
    {
        opt.Resilience.DefaultMaxRetries = 3;
        opt.Resilience.DefaultBaseDelay = TimeSpan.FromMilliseconds(500);
        opt.Resilience.MaxJitter = TimeSpan.FromMilliseconds(100);
    });
    builder.AddPipelineBehavior<RetryBehavior<,>>();
});
```

### CircuitBreakerBehavior\<TRequest, TResponse\>
**Namespace:** `Crisp.Runtime.Pipeline`  
**Purpose:** Circuit breaker pattern for fault tolerance

```csharp
public class CircuitBreakerBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
```

**Configuration:**
```csharp
services.AddCrisp(builder => 
{
    builder.ConfigureOptions(opt => 
    {
        opt.Resilience.CircuitBreakerFailureThreshold = 5;
        opt.Resilience.CircuitBreakerTimeout = TimeSpan.FromMinutes(1);
        opt.Resilience.CircuitBreakerMinimumThroughput = 10;
    });
    builder.AddPipelineBehavior<CircuitBreakerBehavior<,>>();
});
```

**Circuit Breaker States:**
- **Closed:** Normal operation, failures tracked
- **Open:** All requests fail fast, no external calls
- **Half-Open:** Limited requests allowed to test recovery

### IRetryable Interface
**Namespace:** `Crisp.Runtime.Pipeline`  
**Purpose:** Configuration interface for retry behavior

```csharp
public interface IRetryable
{
    int MaxRetries { get; }
    TimeSpan BaseDelay { get; }
    bool ShouldRetry(Exception exception);
}
```

## Event Publishing

### ChannelEventPublisher
**Namespace:** `Crisp.Runtime.Events`  
**Purpose:** Channel-based event publisher for async processing

```csharp
public class ChannelEventPublisher : IEventPublisher, IDisposable
{
    public Task Publish<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent;
    public void Dispose();
}
```

**Features:**
- Async event processing using .NET Channels
- Bounded/unbounded queue options
- Automatic backpressure handling
- Graceful shutdown with timeout

**Configuration:**
```csharp
services.AddCrisp(builder => 
{
    builder.ConfigureOptions(opt => 
    {
        opt.Events.ChannelCapacity = 1000;
        opt.Events.EnableParallelProcessing = true;
        opt.Events.MaxConcurrency = Environment.ProcessorCount;
    });
});

// Events are automatically published by the framework
public class CreateTodoHandler : ICommandHandler<CreateTodoCommand, int>
{
    private readonly IEventPublisher _eventPublisher;
    
    public async Task<int> Handle(CreateTodoCommand command, CancellationToken cancellationToken = default)
    {
        var todo = await _repository.AddAsync(new Todo(command.Title));
        
        // Publish domain event
        await _eventPublisher.Publish(new TodoCreatedEvent(todo.Id, todo.Title, DateTime.UtcNow));
        
        return todo.Id;
    }
}
```

## Pipeline Compilation

### ICompiledPipeline\<TResponse\>
**Namespace:** `Crisp.Runtime.Pipeline`  
**Access:** Internal  
**Purpose:** Pre-compiled pipeline execution for maximum performance

The compilation system converts reflection-based handler discovery into compiled expression trees at startup:

```csharp
// Compilation happens automatically during startup
var compilationTime = builder.CompilationTime; // Measure compilation performance

// Runtime execution uses pre-compiled delegates (zero reflection)
var result = await dispatcher.Send(command); // Uses compiled pipeline
```

**Performance Benefits:**
- **Zero reflection overhead** during request processing
- **Pre-validated handler chains** catch configuration errors at startup
- **Optimized memory allocation** through expression tree compilation
- **Type safety** enforced at compile time

### PipelineExecutor
**Namespace:** `Crisp.Runtime.Pipeline`  
**Access:** Internal  
**Purpose:** Static execution methods for compiled pipelines

The executor provides optimized execution paths for different request types while maintaining the same public API.

## Configuration Options

### PipelineOptions
**Purpose:** Configure pipeline behavior execution

```csharp
public class PipelineOptions
{
    public bool EnableValidation { get; set; } = true;
    public bool EnableLogging { get; set; } = true;
    public bool EnableErrorHandling { get; set; } = true;
    public bool EnableCaching { get; set; } = false;
    public bool EnableRetry { get; set; } = false;
    public bool EnableCircuitBreaker { get; set; } = false;
    public int MaxPipelineDepth { get; set; } = 10;
}
```

### ResilienceOptions
**Purpose:** Configure resilience patterns

```csharp
public class ResilienceOptions
{
    public int DefaultMaxRetries { get; set; } = 3;
    public TimeSpan DefaultBaseDelay { get; set; } = TimeSpan.FromMilliseconds(500);
    public TimeSpan MaxJitter { get; set; } = TimeSpan.FromMilliseconds(100);
    public int CircuitBreakerFailureThreshold { get; set; } = 5;
    public TimeSpan CircuitBreakerTimeout { get; set; } = TimeSpan.FromMinutes(1);
    public int CircuitBreakerMinimumThroughput { get; set; } = 10;
}
```

### CachingOptions
**Purpose:** Configure caching behavior

```csharp
public class CachingOptions
{
    public TimeSpan DefaultDuration { get; set; } = TimeSpan.FromMinutes(5);
    public bool EnableDistributedCaching { get; set; } = false;
    public bool EnableMemoryCache { get; set; } = true;
    public int MaxCacheSize { get; set; } = 1000;
    public string KeyPrefix { get; set; } = "crisp:";
}
```