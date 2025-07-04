# Crisp.AspNetCore API Reference

ASP.NET Core integration providing automatic endpoint mapping, middleware, health checks, and security features.

## Service Registration

### AddCrisp Extension
**Namespace:** `Microsoft.Extensions.DependencyInjection`  
**Purpose:** Registers CRISP framework with ASP.NET Core DI container

```csharp
public static IServiceCollection AddCrisp(
    this IServiceCollection services, 
    Action<NetCoreCrispBuilder>? configureBuilder = null)
```

**Basic Usage:**
```csharp
var builder = WebApplication.CreateBuilder(args);

// Register CRISP framework
builder.Services.AddCrisp();

var app = builder.Build();

// Map CRISP endpoints
app.MapCrisp();
```

**Advanced Configuration:**
```csharp
builder.Services.AddCrisp(crisp => 
{
    crisp.RegisterHandlersFromAssemblies(typeof(Program).Assembly);
    crisp.ConfigureOptions(opt => 
    {
        opt.Endpoints.RoutePrefix = "api";
        opt.Pipeline.EnableValidation = true;
        opt.Logging.LogRequestDetails = true;
    });
    crisp.AddPipelineBehavior<ValidationBehavior<,>>();
    crisp.AddPipelineBehavior<LoggingBehavior<,>>();
});
```

## Endpoints

### Automatic Endpoint Mapping

CRISP automatically maps commands and queries to HTTP endpoints based on naming conventions:

**Convention Rules:**
- **Commands:** POST endpoints (creates/updates data)
- **Queries:** GET endpoints (reads data)
- **Route patterns:** Derived from class names

**Examples:**
```csharp
// Maps to: POST /todos
public record CreateTodoCommand(string Title) : ICommand<int>;

// Maps to: GET /todos/{id}
public record GetTodoQuery(int Id) : IQuery<Todo>;

// Maps to: GET /todos
public record GetAllTodosQuery : IQuery<List<Todo>>;

// Maps to: PUT /todos/{id}
public record UpdateTodoCommand(int Id, string Title) : ICommand<Todo>;

// Maps to: DELETE /todos/{id}
public record DeleteTodoCommand(int Id) : ICommand;
```

### MapCrisp Extension
**Namespace:** `Microsoft.AspNetCore.Routing`  
**Purpose:** Maps CRISP commands and queries to HTTP endpoints

```csharp
public static IEndpointRouteBuilder MapCrisp(
    this IEndpointRouteBuilder endpoints, 
    Action<EndpointOptions>? configure = null)
```

**Basic Mapping:**
```csharp
app.MapCrisp();
```

**With Configuration:**
```csharp
app.MapCrisp(options => 
{
    options.RoutePrefix = "api/v1";
    options.RequireAuthorization = true;
    options.EnableOpenApi = true;
});
```

**Manual Endpoint Mapping:**
```csharp
app.MapCrispEndpoint<CreateTodoCommand>("/api/todos", "POST")
   .RequireAuthorization()
   .WithTags("Todos");

app.MapCrispEndpoint<GetTodoQuery>("/api/todos/{id}")
   .AllowAnonymous();
```

### Custom Route Attributes

Override default routing conventions with attributes:

```csharp
[Route("/api/v1/tasks")]
[HttpMethod("POST")]
public record CreateTaskCommand(string Title) : ICommand<int>;

[Route("/api/health/todos")]
public record GetTodoHealthQuery : IQuery<HealthStatus>;
```

### IEndpoint Interface
**Namespace:** `Crisp.Endpoints`  
**Purpose:** Base interface for HTTP endpoints

```csharp
public interface IEndpoint
{
    string Pattern { get; }
    string HttpMethod { get; }
    Type RequestType { get; }
    Type? ResponseType { get; }
    RouteHandlerBuilder Map(IEndpointRouteBuilder app);
}
```

## Middleware

### CrispExceptionMiddleware
**Namespace:** `Crisp.Middleware`  
**Purpose:** Global exception handling for CRISP operations

**Features:**
- Automatic exception-to-HTTP status mapping
- Structured error responses
- Correlation ID injection
- Sensitive data protection

**Configuration:**
```csharp
// Add to pipeline (recommended after routing)
app.UseRouting();
app.UseCrispExceptionHandler();
app.MapCrisp();
```

**Exception Mapping:**
```csharp
// Automatic HTTP status mapping
throw new NotFoundException("Todo", "123");        // → 404 Not Found
throw new ValidationException(validationResult);   // → 400 Bad Request
throw new UnauthorizedException();                  // → 401 Unauthorized
throw new CrispException("Custom error");          // → 500 Internal Server Error
```

**Response Format:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Resource Not Found",
  "status": 404,
  "detail": "Todo with ID '123' was not found.",
  "correlationId": "550e8400-e29b-41d4-a716-446655440000",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### UseCrisp Extension
**Namespace:** `Microsoft.AspNetCore.Builder`  
**Purpose:** Configures CRISP middleware pipeline

```csharp
public static IApplicationBuilder UseCrisp(
    this IApplicationBuilder app, 
    Action<CrispAppOptions>? configure = null)
```

**Complete Setup:**
```csharp
app.UseCrisp(options => 
{
    options.EnableExceptionHandler = true;
    options.EnableCorrelationIds = true;
    options.EnableRequestLogging = true;
});
```

## Health Checks

### CrispFrameworkHealthCheck
**Namespace:** `Crisp.HealthChecks`  
**Purpose:** Monitors CRISP framework health

```csharp
public class CrispFrameworkHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default);
}
```

**Registration:**
```csharp
builder.Services.AddHealthChecks()
    .AddCrispFramework()
    .AddCrispDependencies()
    .AddCrispPerformance();
```

### CrispDependencyHealthCheck
**Namespace:** `Crisp.HealthChecks`  
**Purpose:** Verifies all handler dependencies are registered

**Checks:**
- Handler registrations are valid
- Dependencies can be resolved
- No circular dependencies exist

### CrispPerformanceHealthCheck
**Namespace:** `Crisp.HealthChecks`  
**Purpose:** Monitors framework performance metrics

**Metrics:**
- Handler execution times
- Pipeline behavior overhead
- Memory usage patterns
- Compilation performance

**Configuration:**
```csharp
builder.Services.AddHealthChecks()
    .AddCrispPerformance(options => 
    {
        options.MaxAcceptableLatency = TimeSpan.FromMilliseconds(100);
        options.MaxMemoryUsage = 50; // MB
        options.SampleRequests = true;
    });
```

**Endpoint Setup:**
```csharp
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/crisp", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("crisp")
});
```

## Security

### AuthorizationBehavior\<TRequest, TResponse\>
**Namespace:** `Crisp.Security`  
**Purpose:** Enforces authorization policies for commands and queries

```csharp
public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
```

**Usage with Attributes:**
```csharp
[CrispAuthorize]
public record CreateTodoCommand(string Title) : ICommand<int>;

[CrispRequireRole("Admin")]
public record DeleteTodoCommand(int Id) : ICommand;

[CrispRequireClaim("permission", "todos:read")]
public record GetTodoQuery(int Id) : IQuery<Todo>;
```

**Policy-Based Authorization:**
```csharp
[CrispAuthorize("RequireOwnership")]
public record UpdateTodoCommand(int Id, string Title) : ICommand<Todo>;

// Register policy
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireOwnership", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("todo_owner", context.Resource?.ToString())));
});
```

### InputSanitizationBehavior\<TRequest, TResponse\>
**Namespace:** `Crisp.Security`  
**Purpose:** Sanitizes user inputs to prevent XSS and injection attacks

```csharp
public class InputSanitizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
```

**Configuration:**
```csharp
services.AddCrisp(builder => 
{
    builder.ConfigureOptions(opt => 
    {
        opt.Security.EnableInputSanitization = true;
        opt.Security.SanitizeHtml = true;
        opt.Security.RemoveDangerousChars = true;
        opt.Security.MaxInputLength = 10000;
    });
    builder.AddPipelineBehavior<InputSanitizationBehavior<,>>();
});
```

**Custom Sanitization:**
```csharp
[SanitizeInput(SanitizationMode.StripHtml)]
public record CreatePostCommand(
    [SanitizeInput(SanitizationMode.None)] string Title,
    [SanitizeInput(SanitizationMode.Encode)] string Content
) : ICommand<int>;
```

### RateLimitingBehavior\<TRequest, TResponse\>
**Namespace:** `Crisp.Security`  
**Purpose:** Implements rate limiting with multiple algorithms

```csharp
public class RateLimitingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
```

**Rate Limiting Algorithms:**
- **Token Bucket:** Smooth rate limiting with burst capacity
- **Sliding Window:** Time-based rate limiting
- **Fixed Window:** Simple interval-based limiting

**Configuration:**
```csharp
services.AddCrisp(builder => 
{
    builder.ConfigureOptions(opt => 
    {
        opt.Security.RateLimit.Algorithm = RateLimitAlgorithm.TokenBucket;
        opt.Security.RateLimit.RequestsPerMinute = 60;
        opt.Security.RateLimit.BurstCapacity = 10;
        opt.Security.RateLimit.ClientIdentification = ClientIdStrategy.IpAddress;
    });
    builder.AddPipelineBehavior<RateLimitingBehavior<,>>();
});
```

**Per-Request Rate Limiting:**
```csharp
[RateLimit(RequestsPerMinute = 10)]
public record CreateTodoCommand(string Title) : ICommand<int>;

[RateLimit(RequestsPerMinute = 100, ClientStrategy = ClientIdStrategy.UserId)]
public record GetTodoQuery(int Id) : IQuery<Todo>;
```

### SecurityAuditService
**Namespace:** `Crisp.Security`  
**Purpose:** Comprehensive security vulnerability scanning

```csharp
public interface ISecurityAuditService
{
    Task<SecurityAuditResult> AuditAsync<TRequest>(TRequest request);
    Task<SecurityAuditResult> ScanForVulnerabilitiesAsync(object input);
}
```

**Features:**
- SQL injection detection
- XSS vulnerability scanning
- Insecure deserialization checks
- Weak cryptography detection
- Sensitive data exposure analysis

**Configuration:**
```csharp
services.AddCrisp(builder => 
{
    builder.ConfigureOptions(opt => 
    {
        opt.Security.EnableSecurityAudit = true;
        opt.Security.AuditLevel = SecurityAuditLevel.Strict;
        opt.Security.BlockSuspiciousRequests = true;
    });
});

// Manual auditing
public class TodoHandler : ICommandHandler<CreateTodoCommand, int>
{
    private readonly ISecurityAuditService _securityAudit;
    
    public async Task<int> Handle(CreateTodoCommand command, CancellationToken cancellationToken = default)
    {
        var auditResult = await _securityAudit.AuditAsync(command);
        if (auditResult.HasVulnerabilities)
        {
            throw new SecurityException($"Security vulnerabilities detected: {auditResult.Summary}");
        }
        
        // Process command...
    }
}
```

## Extensions

### SwaggerExtensions
**Namespace:** `Crisp.Extensions`  
**Purpose:** OpenAPI/Swagger integration for CRISP endpoints

```csharp
public static class SwaggerExtensions
{
    public static IServiceCollection AddCrispSwagger(this IServiceCollection services, Action<SwaggerGenOptions>? configure = null);
    public static IApplicationBuilder UseCrispSwagger(this IApplicationBuilder app);
}
```

**Usage:**
```csharp
builder.Services.AddCrispSwagger(options => 
{
    options.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Todo API", 
        Version = "v1",
        Description = "A simple API built with CRISP framework"
    });
});

app.UseCrispSwagger();
app.UseSwaggerUI();
```

### ValidationExtensions
**Namespace:** `Crisp.Extensions`  
**Purpose:** Enhanced validation integration

```csharp
public static class ValidationExtensions
{
    public static IServiceCollection AddCrispValidation(this IServiceCollection services);
    public static NetCoreCrispBuilder AddFluentValidation(this NetCoreCrispBuilder builder, params Assembly[] assemblies);
}
```

**Usage:**
```csharp
builder.Services
    .AddCrispValidation()
    .AddCrisp(crisp => 
    {
        crisp.AddFluentValidation(typeof(Program).Assembly);
        crisp.AddPipelineBehavior<ValidationBehavior<,>>();
    });
```

### HealthCheckExtensions
**Namespace:** `Crisp.Extensions`  
**Purpose:** Simplified health check registration

```csharp
public static class HealthCheckExtensions
{
    public static IServiceCollection AddCrispHealthChecks(this IServiceCollection services, Action<CrispHealthCheckOptions>? configure = null);
}
```

**Usage:**
```csharp
builder.Services.AddCrispHealthChecks(options => 
{
    options.EnableFrameworkCheck = true;
    options.EnableDependencyCheck = true;
    options.EnablePerformanceCheck = true;
    options.CheckInterval = TimeSpan.FromSeconds(30);
});
```

### SecurityExtensions
**Namespace:** `Crisp.Extensions`  
**Purpose:** Security feature configuration

```csharp
public static class SecurityExtensions
{
    public static IServiceCollection AddCrispSecurity(this IServiceCollection services, Action<SecurityOptions>? configure = null);
}
```

**Complete Security Setup:**
```csharp
builder.Services.AddCrispSecurity(options => 
{
    options.EnableAuthorization = true;
    options.EnableInputSanitization = true;
    options.EnableRateLimiting = true;
    options.EnableSecurityAudit = true;
});

builder.Services.AddCrisp(crisp => 
{
    crisp.AddPipelineBehavior<AuthorizationBehavior<,>>();
    crisp.AddPipelineBehavior<InputSanitizationBehavior<,>>();
    crisp.AddPipelineBehavior<RateLimitingBehavior<,>>();
});
```

## Configuration

### EndpointOptions
**Purpose:** Configure endpoint mapping behavior

```csharp
public class EndpointOptions
{
    public string RoutePrefix { get; set; } = "";
    public bool RequireAuthorization { get; set; } = false;
    public bool EnableOpenApi { get; set; } = true;
    public bool UseConventionRouting { get; set; } = true;
    public bool EnableVersioning { get; set; } = false;
    public Dictionary<string, object> DefaultRouteValues { get; set; } = new();
}
```

### SecurityOptions
**Purpose:** Configure security features

```csharp
public class SecurityOptions
{
    public bool EnableAuthorization { get; set; } = false;
    public bool EnableInputSanitization { get; set; } = false;
    public bool EnableRateLimiting { get; set; } = false;
    public bool EnableSecurityAudit { get; set; } = false;
    public RateLimitOptions RateLimit { get; set; } = new();
    public InputSanitizationOptions Sanitization { get; set; } = new();
    public SecurityAuditOptions Audit { get; set; } = new();
}
```

## Problem Details

### ProblemDetailsFactory
**Namespace:** `Crisp.Serialization`  
**Purpose:** Creates RFC 7807 compliant error responses

```csharp
public static class ProblemDetailsFactory
{
    public static ProblemDetails CreateProblemDetails(Exception exception, string correlationId);
    public static ValidationProblemDetails CreateValidationProblemDetails(ValidationException exception, string correlationId);
}
```

**Custom Problem Details:**
```csharp
public class CustomExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problemDetails = ProblemDetailsFactory.CreateProblemDetails(exception, httpContext.TraceIdentifier);
        
        // Customize based on exception type
        problemDetails.Extensions["timestamp"] = DateTime.UtcNow;
        problemDetails.Extensions["machine"] = Environment.MachineName;
        
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}
```