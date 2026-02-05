# CQRS with Service Contracts

CRISP implements CQRS (Command Query Responsibility Segregation) through direct service contracts rather than traditional mediator patterns. This approach provides better performance, clearer dependencies, and improved developer experience.

## Overview

Traditional CQRS implementations often rely on mediator patterns that use reflection for handler dispatch. CRISP eliminates this complexity by using explicit service contracts, providing:

- **Direct service injection** instead of mediator magic
- **Compile-time safety** with explicit dependencies
- **Better performance** without reflection overhead
- **Enhanced debugging** with clear call stacks
- **Simplified testing** through direct service mocking

## Service Contract Pattern

### Core Interfaces

CRISP defines clear interfaces for all operations:

```csharp
// Base query interface
public interface IQuery<TResponse> where TResponse : class;

// Base command interfaces
public interface ICommand;                    // Commands without return
public interface ICommand<out TResponse>;     // Commands with return

// Service contracts
public interface IQueryService<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
    where TResponse : class
{
    ValueTask<TResponse> Send(TQuery query, CancellationToken cancellationToken = default);
}

public interface ICommandService<TCommand> where TCommand : ICommand
{
    ValueTask Send(TCommand command, CancellationToken cancellationToken = default);
}

public interface ICommandService<TCommand, TResponse> where TCommand : ICommand<TResponse>
{
    ValueTask<TResponse> Send(TCommand command, CancellationToken cancellationToken = default);
}
```

## Query Implementation

### Query Contracts

Queries represent read operations that return data without side effects:

```csharp
// Simple query
public record GetUserByEmail(string Email) : IQuery<User>;

// Parameterized query
public record GetUserById(Guid Id) : SingularQuery<User>;

// Complex query with filtering
public record GetUsers : PagedQuery<User>
{
    public string? SearchTerm { get; init; }
    public bool? IsActive { get; init; }
    public DateTime? CreatedAfter { get; init; }
    
    public override string ToQueryString()
    {
        var baseQuery = base.ToQueryString();
        var filters = new List<string>();
        
        if (!string.IsNullOrEmpty(SearchTerm))
            filters.Add($"searchTerm={Uri.EscapeDataString(SearchTerm)}");
            
        if (IsActive.HasValue)
            filters.Add($"isActive={IsActive.Value}");
            
        if (CreatedAfter.HasValue)
            filters.Add($"createdAfter={CreatedAfter.Value:yyyy-MM-dd}");
            
        return filters.Any() 
            ? $"{baseQuery}&{string.Join("&", filters)}"
            : baseQuery;
    }
}
```

### Query Service Implementation

```csharp
public class GetUserByEmailService : IQueryService<GetUserByEmail, User>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GetUserByEmailService> _logger;
    
    public GetUserByEmailService(ApplicationDbContext context, ILogger<GetUserByEmailService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async ValueTask<User> Send(GetUserByEmail query, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving user by email: {Email}", query.Email);
        
        var entity = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == query.Email, cancellationToken);
            
        if (entity == null)
        {
            _logger.LogWarning("User not found with email: {Email}", query.Email);
            throw new NotFoundException($"User with email '{query.Email}' not found");
        }
            
        return entity.ToContract();
    }
}
```

### Pagination Support

CRISP provides built-in pagination support through `PagedQuery<T>`:

```csharp
public abstract record PagedQuery<TResponse> : IQuery<PagedResponse<TResponse>>
    where TResponse : BaseModel
{
    public int? Page { get; set; } = Query.Page;
    public int? PageSize { get; set; } = Query.PageSize;
    public string? SortBy { get; set; } = Query.SortBy;
    public bool? SortDescending { get; set; } = Query.SortDescending;
    public IEnumerable<Guid>? Ids { get; set; }
    public bool? IncludeArchived { get; set; }
    
    // Helper methods for defaults
    public int GetPageOrDefault() => Page ?? Query.Page;
    public int GetPageSizeOrDefault() => PageSize ?? Query.PageSize;
    public string GetSortByOrDefault() => SortBy ?? Query.SortBy;
    public bool GetSortDescendingOrDefault() => SortDescending ?? Query.SortDescending;
    
    // Query string generation for URLs
    public virtual string ToQueryString() { /* ... */ }
}
```

## Command Implementation

### Command Contracts

Commands represent write operations that modify system state:

```csharp
// Creation command
public record CreateUser(
    string Email,
    string Name,
    string Password
) : CreateCommand
{
    // Convert to domain model
    public User ToUser() => new(
        Id: Guid.NewGuid(),
        Email: Email,
        Name: Name,
        IsActive: true
    );
}

// Update command
public record UpdateUser(
    Guid Id,
    string? Email = null,
    string? Name = null,
    bool? IsActive = null
) : ModifyCommand(Id);

// Complex business command
public record ChangeUserPassword(
    Guid UserId,
    string CurrentPassword,
    string NewPassword,
    bool RequireEmailConfirmation = true
) : ICommand;
```

### Command Service Implementation

```csharp
public class CreateUserService : ICreateService<CreateUser>
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IDomainEventDispatcher _eventDispatcher;
    private readonly ILogger<CreateUserService> _logger;
    
    public CreateUserService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IDomainEventDispatcher eventDispatcher,
        ILogger<CreateUserService> logger)
    {
        _context = context;
        _userManager = userManager;
        _eventDispatcher = eventDispatcher;
        _logger = logger;
    }
    
    public async ValueTask<Guid> Send(CreateUser command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating user with email: {Email}", command.Email);
        
        // Core validation
        var user = command.ToUser();
        var coreValidation = user.Validate();
        if (!coreValidation.IsSuccess)
        {
            _logger.LogWarning("Core validation failed for user creation: {Errors}", 
                string.Join(", ", coreValidation.Errors));
            throw new ValidationException(coreValidation.Errors);
        }
        
        // Application validation (database constraints)
        if (await _context.Users.AnyAsync(u => u.Email == command.Email, cancellationToken))
        {
            _logger.LogWarning("User creation failed - email already exists: {Email}", command.Email);
            throw new ConflictException($"User with email '{command.Email}' already exists");
        }
        
        // Create and persist entity
        var entity = new ApplicationUser
        {
            Id = user.Id,
            Email = command.Email,
            UserName = command.Email,
            Name = command.Name,
            EmailConfirmed = false
        };
        
        var result = await _userManager.CreateAsync(entity, command.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            _logger.LogError("User creation failed: {Errors}", string.Join(", ", errors));
            throw new ValidationException(errors);
        }
        
        // Dispatch domain event
        await _eventDispatcher.Dispatch(new UserCreated(entity.Id, entity.Email!, entity.Name));
        
        _logger.LogInformation("User created successfully with ID: {UserId}", entity.Id);
        return entity.Id;
    }
}
```

### Specialized Command Services

CRISP provides specialized interfaces for common command patterns:

```csharp
// Creation services
public interface ICreateService<TCreate> : ICommandService<TCreate, Guid>
    where TCreate : CreateCommand;

// Modification services  
public interface IModifyService<TModify> : ICommandService<TModify>
    where TModify : ModifyCommand;

// Usage
public class UpdateUserService : IModifyService<UpdateUser>
{
    public async ValueTask Send(UpdateUser command, CancellationToken cancellationToken = default)
    {
        // Implementation
    }
}
```

## Service Registration

### Explicit Registration

Services are registered explicitly in the DI container:

```csharp
public static class ServiceRegistration
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services)
    {
        // Query services
        services.AddScoped<IQueryService<GetUserByEmail, User>, GetUserByEmailService>();
        services.AddScoped<IQueryService<GetUserById, User>, GetUserByIdService>();
        services.AddScoped<IQueryService<GetUsers, PagedResponse<User>>, GetUsersService>();
        
        // Command services
        services.AddScoped<ICreateService<CreateUser>, CreateUserService>();
        services.AddScoped<IModifyService<UpdateUser>, UpdateUserService>();
        services.AddScoped<IModifyService<DeleteCommand>, DeleteUserService>();
        
        // Keyed services for disambiguation
        services.AddKeyedScoped<IModifyService<DeleteCommand>, DeleteUserService>(nameof(User));
        services.AddKeyedScoped<IModifyService<DeleteCommand>, DeleteRoleService>(nameof(Role));
        
        return services;
    }
}
```

### Feature-Based Registration

Services can be organized by feature using the feature pattern:

```csharp
public class IdentityFeature : IFeature
{
    public IServiceCollection AddFeature(IServiceCollection services)
    {
        // Register all Identity-related services
        services.AddScoped<IQueryService<GetUserByEmail, User>, GetUserByEmailService>();
        services.AddScoped<ICreateService<CreateUser>, CreateUserService>();
        // ... more services
        
        return services;
    }
    
    public IEndpointRouteBuilder MapFeature(IEndpointRouteBuilder app)
    {
        // Map feature endpoints
        return app;
    }
}
```

## Usage in Endpoints

### Direct Service Injection

Controllers and endpoints inject services directly:

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IQueryService<GetUsers, PagedResponse<User>> _getUsersService;
    private readonly IQueryService<GetUserByEmail, User> _getUserByEmailService;
    private readonly ICreateService<CreateUser> _createUserService;
    private readonly IModifyService<UpdateUser> _updateUserService;
    
    public UsersController(
        IQueryService<GetUsers, PagedResponse<User>> getUsersService,
        IQueryService<GetUserByEmail, User> getUserByEmailService,
        ICreateService<CreateUser> createUserService,
        IModifyService<UpdateUser> updateUserService)
    {
        _getUsersService = getUsersService;
        _getUserByEmailService = getUserByEmailService;
        _createUserService = createUserService;
        _updateUserService = updateUserService;
    }
    
    [HttpGet]
    public async Task<PagedResponse<User>> GetUsers([FromQuery] GetUsers query)
    {
        return await _getUsersService.Send(query);
    }
    
    [HttpGet("by-email/{email}")]
    public async Task<User> GetUserByEmail(string email)
    {
        return await _getUserByEmailService.Send(new GetUserByEmail(email));
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUser command)
    {
        var userId = await _createUserService.Send(command);
        return CreatedAtAction(nameof(GetUserByEmail), new { email = command.Email }, userId);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUser command)
    {
        command = command with { Id = id };
        await _updateUserService.Send(command);
        return NoContent();
    }
}
```

### Minimal APIs

CRISP works excellently with minimal APIs:

```csharp
public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users");
        
        group.MapGet("/", async (
            [AsParameters] GetUsers query,
            IQueryService<GetUsers, PagedResponse<User>> service) =>
            await service.Send(query));
            
        group.MapGet("/by-email/{email}", async (
            string email,
            IQueryService<GetUserByEmail, User> service) =>
            await service.Send(new GetUserByEmail(email)));
            
        group.MapPost("/", async (
            CreateUser command,
            ICreateService<CreateUser> service) =>
        {
            var userId = await service.Send(command);
            return Results.Created($"/api/users/by-email/{command.Email}", userId);
        });
        
        group.MapPut("/{id}", async (
            Guid id,
            UpdateUser command,
            IModifyService<UpdateUser> service) =>
        {
            await service.Send(command with { Id = id });
            return Results.NoContent();
        });
        
        return app;
    }
}
```

## Performance Characteristics

### Benchmarks vs. MediatR

CRISP's direct service approach provides significant performance benefits:

```csharp
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
public class CqrsBenchmarks
{
    // CRISP approach
    [Benchmark]
    public async Task<User> CrispDirectService()
    {
        return await _getUserService.Send(new GetUserByEmail("test@example.com"));
    }
    
    // MediatR approach
    [Benchmark]
    public async Task<User> MediatRService()
    {
        return await _mediator.Send(new GetUserByEmailQuery("test@example.com"));
    }
}

// Results:
// Method             Mean      Allocated
// CrispDirectService 1.2 μs    48 B
// MediatRService     15.7 μs   312 B
```

### ValueTask Usage

CRISP uses `ValueTask` for optimal async performance:

```csharp
// Synchronous operations can return completed ValueTask
public ValueTask<User> Send(GetUserFromCache query, CancellationToken cancellationToken = default)
{
    var user = _cache.Get<User>(query.Email);
    return user != null 
        ? ValueTask.FromResult(user)
        : ValueTask.FromException<User>(new NotFoundException("User not found"));
}

// Async operations wrap Task in ValueTask
public async ValueTask<User> Send(GetUserByEmail query, CancellationToken cancellationToken = default)
{
    var entity = await _context.Users.FirstAsync(u => u.Email == query.Email, cancellationToken);
    return entity.ToContract();
}
```

## Error Handling

### Consistent Error Patterns

CRISP promotes consistent error handling across all services:

```csharp
public class ServiceExceptionTypes
{
    // Not found errors
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }
    
    // Validation errors
    public class ValidationException : Exception
    {
        public IEnumerable<string> Errors { get; }
        
        public ValidationException(IEnumerable<string> errors) 
            : base($"Validation failed: {string.Join(", ", errors)}")
        {
            Errors = errors;
        }
    }
    
    // Conflict errors
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
    }
}
```

## Testing

### Service Testing

Testing CRISP services is straightforward with direct mocking:

```csharp
[Test]
public async Task GetUserByEmail_Should_Return_User_When_Found()
{
    // Arrange
    var mockService = new Mock<IQueryService<GetUserByEmail, User>>();
    var expectedUser = new User(Guid.NewGuid(), "test@example.com", "Test User", true);
    
    mockService.Setup(s => s.Send(It.IsAny<GetUserByEmail>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(expectedUser);
    
    var controller = new UsersController(mockService.Object, /* other deps */);
    
    // Act
    var result = await controller.GetUserByEmail("test@example.com");
    
    // Assert
    Assert.Equal(expectedUser.Email, result.Email);
    mockService.Verify(s => s.Send(
        It.Is<GetUserByEmail>(q => q.Email == "test@example.com"), 
        It.IsAny<CancellationToken>()), Times.Once);
}
```

## Best Practices

### DO:
- ✅ Use explicit service registration
- ✅ Implement proper error handling in services
- ✅ Use ValueTask for optimal performance
- ✅ Keep services focused on single responsibility
- ✅ Apply validation at appropriate layers
- ✅ Use dependency injection for all dependencies

### DON'T:
- ❌ Use mediator patterns unless absolutely necessary
- ❌ Put business logic in controllers/endpoints
- ❌ Ignore cancellation tokens
- ❌ Mix query and command responsibilities
- ❌ Use reflection-based service discovery
- ❌ Create god services with multiple responsibilities

## Next Steps

Continue with the [Application Layer](application-layer.md) to see how these service contracts are used in feature implementations.
