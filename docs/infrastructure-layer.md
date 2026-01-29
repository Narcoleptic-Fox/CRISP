# Infrastructure Layer

The Infrastructure layer implements the service contracts defined in the Core layer using mutable entities and external services. This layer handles data persistence, external API calls, and other infrastructure concerns.

## Overview

The Infrastructure layer bridges the gap between your pure domain contracts (Core) and the external world. It contains mutable Entity Framework entities, service implementations, and configurations needed for data persistence and external integrations.

```
Infrastructure Layer Contents:
├── Common/
│   ├── BaseEntity.cs           # Base mutable entity
│   ├── BaseAuditableEntity.cs  # Mutable auditable entity  
│   └── ISoftDelete.cs          # Soft delete interface
├── Data/
│   ├── Entities/               # EF mutable entities
│   ├── Configurations/         # EF configurations
│   ├── Migrations/             # Database migrations
│   └── ApplicationDbContext.cs # EF context
├── Services/                   # Service implementations
│   ├── [Domain]Services/       # Domain service implementations
│   └── External/               # External service clients
├── Extensions/                 # Service registration
└── Mappings/                   # Entity ↔ Contract mappings
```

## Mutable Entities

### Base Entities

Infrastructure entities are **mutable classes** designed for Entity Framework tracking and persistence:

```csharp
namespace CRISP.Infrastructure.Common;

public abstract class BaseEntity
{
    public Guid Id { get; set; }
}

public abstract class BaseAuditableEntity : BaseEntity
{
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedOn { get; set; } = DateTime.UtcNow;
    public bool IsArchived { get; set; }
}

public interface ISoftDelete
{
    bool IsDeleted { get; set; }
}
```

### Domain Entities

```csharp
namespace CRISP.Infrastructure.Data.Entities;

public class ApplicationUser : IdentityUser<Guid>, ISoftDelete
{
    public string Name { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    
    // Navigation properties
    public virtual ICollection<ApplicationUserRole> UserRoles { get; set; } = [];
}

public class ApplicationRole : IdentityRole<Guid>, ISoftDelete
{
    public string Permission { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    
    // Computed property for permissions
    public IEnumerable<Permissions> PermissionsList
    {
        get => Permission
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => Enum.TryParse(p.Trim(), out Permissions permission) ? permission : Permissions.None)
            .Distinct();

        set => Permission = string.Join(',', value.Select(p => p.ToString()));
    }
}
```

### Key Characteristics:
- **Mutable**: Properties can be set for EF tracking
- **EF Optimized**: Designed for Entity Framework operations
- **Navigation Properties**: Support for EF relationships
- **Soft Delete**: Implementation of soft delete patterns

## Entity Framework Configuration

### DbContext

```csharp
namespace CRISP.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Apply all configurations from assembly
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        
        // Global query filters for soft delete
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                builder.Entity(entityType.ClrType)
                    .HasQueryFilter(GetSoftDeleteFilter(entityType.ClrType));
            }
        }
    }
    
    private static LambdaExpression GetSoftDeleteFilter(Type entityType)
    {
        var parameter = Expression.Parameter(entityType, "e");
        var body = Expression.Equal(
            Expression.Property(parameter, nameof(ISoftDelete.IsDeleted)),
            Expression.Constant(false));
        return Expression.Lambda(body, parameter);
    }
}
```

### Entity Configurations

```csharp
namespace CRISP.Infrastructure.Data.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.Name)
            .HasMaxLength(100)
            .IsRequired();
            
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
            
        builder.Property(u => u.IsDeleted)
            .HasDefaultValue(false);
    }
}

public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.Property(r => r.Permission)
            .HasMaxLength(1000);
            
        builder.Property(r => r.IsDeleted)
            .HasDefaultValue(false);
    }
}
```

## Service Implementations

### Query Service Implementation

```csharp
namespace CRISP.Infrastructure.Services.Identity;

public class GetUserByEmailService : IQueryService<GetUserByEmail, User>
{
    private readonly ApplicationDbContext _context;
    
    public GetUserByEmailService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async ValueTask<User> Send(GetUserByEmail query, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == query.Email, cancellationToken);
            
        if (entity == null)
            throw new NotFoundException($"User with email '{query.Email}' not found");
            
        return entity.ToContract();
    }
}

public class GetUsersService : IQueryService<GetUsers, PagedResponse<User>>
{
    private readonly ApplicationDbContext _context;
    
    public GetUsersService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async ValueTask<PagedResponse<User>> Send(GetUsers query, CancellationToken cancellationToken = default)
    {
        var queryable = _context.Users.AsQueryable();
        
        // Apply filters
        if (!string.IsNullOrEmpty(query.SearchTerm))
        {
            queryable = queryable.Where(u => 
                u.Name.Contains(query.SearchTerm) || 
                u.Email.Contains(query.SearchTerm));
        }
        
        if (query.IsActive.HasValue)
        {
            queryable = queryable.Where(u => u.LockoutEnd == null == query.IsActive.Value);
        }
        
        // Apply archived filter if not explicitly included
        if (!query.IncludeArchived.GetValueOrDefault())
        {
            queryable = queryable.Where(u => !u.IsDeleted);
        }
        
        // Apply sorting
        queryable = query.GetSortByOrDefault() switch
        {
            nameof(User.Email) => query.GetSortDescendingOrDefault() 
                ? queryable.OrderByDescending(u => u.Email)
                : queryable.OrderBy(u => u.Email),
            nameof(User.Name) => query.GetSortDescendingOrDefault()
                ? queryable.OrderByDescending(u => u.Name)
                : queryable.OrderBy(u => u.Name),
            _ => queryable.OrderBy(u => u.Email)
        };
        
        // Apply pagination
        var totalCount = await queryable.CountAsync(cancellationToken);
        var entities = await queryable
            .Skip(query.GetPageOrDefault() * query.GetPageSizeOrDefault())
            .Take(query.GetPageSizeOrDefault())
            .ToListAsync(cancellationToken);
            
        return new PagedResponse<User>
        {
            Items = entities.Select(e => e.ToContract()).ToList(),
            TotalCount = totalCount,
            Page = query.GetPageOrDefault(),
            PageSize = query.GetPageSizeOrDefault()
        };
    }
}
```

### Command Service Implementation

```csharp
namespace CRISP.Infrastructure.Services.Identity;

public class CreateUserService : ICreateService<CreateUser>
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IDomainEventDispatcher _eventDispatcher;
    
    public CreateUserService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IDomainEventDispatcher eventDispatcher)
    {
        _context = context;
        _userManager = userManager;
        _eventDispatcher = eventDispatcher;
    }
    
    public async ValueTask<Guid> Send(CreateUser command, CancellationToken cancellationToken = default)
    {
        // Core validation
        var user = command.ToUser();
        var validation = user.Validate();
        if (!validation.IsSuccess)
            throw new ValidationException(validation.Errors);
        
        // Application validation (database constraints)
        if (await _context.Users.AnyAsync(u => u.Email == command.Email, cancellationToken))
            throw new ConflictException($"User with email '{command.Email}' already exists");
        
        // Create entity
        var entity = new ApplicationUser
        {
            Id = user.Id,
            Email = command.Email,
            UserName = command.Email,
            Name = command.Name,
            EmailConfirmed = false
        };
        
        // Use UserManager for password handling
        var result = await _userManager.CreateAsync(entity, command.Password);
        if (!result.Succeeded)
            throw new ValidationException(result.Errors.Select(e => e.Description));
        
        // Dispatch domain event
        await _eventDispatcher.Dispatch(new UserCreated(entity.Id, entity.Email!, entity.Name));
        
        return entity.Id;
    }
}

public class UpdateUserService : IModifyService<UpdateUser>
{
    private readonly ApplicationDbContext _context;
    private readonly IDomainEventDispatcher _eventDispatcher;
    
    public UpdateUserService(ApplicationDbContext context, IDomainEventDispatcher eventDispatcher)
    {
        _context = context;
        _eventDispatcher = eventDispatcher;
    }
    
    public async ValueTask Send(UpdateUser command, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == command.Id, cancellationToken);
            
        if (entity == null)
            throw new NotFoundException($"User with ID '{command.Id}' not found");
        
        // Apply updates
        if (command.Email != null) entity.Email = command.Email;
        if (command.Name != null) entity.Name = command.Name;
        
        // Update timestamp
        entity.LockoutEnd = command.IsActive == false ? DateTimeOffset.MaxValue : null;
        
        // Core validation on updated state
        var updatedContract = entity.ToContract();
        var validation = updatedContract.Validate();
        if (!validation.IsSuccess)
            throw new ValidationException(validation.Errors);
        
        await _context.SaveChangesAsync(cancellationToken);
        
        // Dispatch domain event
        await _eventDispatcher.Dispatch(new UserUpdated(entity.Id, entity.Email!, entity.Name));
    }
}
```

## Manual Mapping

### Entity to Contract Mapping

```csharp
namespace CRISP.Infrastructure.Mappings;

public static class UserMappings
{
    public static User ToContract(this ApplicationUser entity)
    {
        return new User(
            Id: entity.Id,
            Email: entity.Email ?? string.Empty,
            Name: entity.Name,
            IsActive: entity.LockoutEnd == null
        );
    }
    
    public static ApplicationUser ToEntity(this User contract)
    {
        return new ApplicationUser
        {
            Id = contract.Id,
            Email = contract.Email,
            UserName = contract.Email,
            Name = contract.Name,
            LockoutEnd = contract.IsActive ? null : DateTimeOffset.MaxValue
        };
    }
    
    public static void UpdateEntity(this ApplicationUser entity, User contract)
    {
        entity.Email = contract.Email;
        entity.UserName = contract.Email;
        entity.Name = contract.Name;
        entity.LockoutEnd = contract.IsActive ? null : DateTimeOffset.MaxValue;
    }
}

public static class RoleMappings
{
    public static Role ToContract(this ApplicationRole entity)
    {
        return new Role(
            Id: entity.Id,
            Name: entity.Name ?? string.Empty,
            Permissions: entity.PermissionsList.ToList()
        );
    }
    
    public static ApplicationRole ToEntity(this Role contract)
    {
        return new ApplicationRole
        {
            Id = contract.Id,
            Name = contract.Name,
            NormalizedName = contract.Name.ToUpperInvariant(),
            PermissionsList = contract.Permissions
        };
    }
}
```

## Service Registration

### Extension Methods

```csharp
namespace CRISP.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        
        // Identity
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.SignIn.RequireConfirmedAccount = true;
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();
        
        // Query Services
        services.AddScoped<IQueryService<GetUserByEmail, User>, GetUserByEmailService>();
        services.AddScoped<IQueryService<SingularQuery<User>, User>, GetUserService>();
        services.AddScoped<IQueryService<GetUsers, PagedResponse<User>>, GetUsersService>();
        
        // Command Services
        services.AddScoped<ICreateService<CreateUser>, CreateUserService>();
        services.AddScoped<IModifyService<UpdateUser>, UpdateUserService>();
        services.AddKeyedScoped<IModifyService<DeleteCommand>, DeleteUserService>(nameof(User));
        
        // External Services
        services.AddHttpClient<IEmailService, EmailService>();
        
        return services;
    }
}
```

## Domain Event Handling

### Event Dispatcher

```csharp
namespace CRISP.Infrastructure.Services;

public interface IDomainEventDispatcher
{
    Task Dispatch<TEvent>(TEvent domainEvent) where TEvent : IDomainEvent;
}

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DomainEventDispatcher> _logger;
    
    public DomainEventDispatcher(IServiceProvider serviceProvider, ILogger<DomainEventDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    
    public async Task Dispatch<TEvent>(TEvent domainEvent) where TEvent : IDomainEvent
    {
        var handlers = _serviceProvider.GetServices<IDomainEventHandler<TEvent>>();
        
        foreach (var handler in handlers)
        {
            try
            {
                await handler.Handle(domainEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling domain event {EventType}", typeof(TEvent).Name);
                throw;
            }
        }
    }
}
```

## Testing Infrastructure

### Integration Tests

```csharp
[Test]
public async Task GetUserByEmailService_Should_Return_User_When_Exists()
{
    // Arrange
    using var context = CreateInMemoryContext();
    var user = new ApplicationUser 
    { 
        Email = "test@example.com", 
        Name = "Test User" 
    };
    context.Users.Add(user);
    await context.SaveChangesAsync();
    
    var service = new GetUserByEmailService(context);
    var query = new GetUserByEmail("test@example.com");
    
    // Act
    var result = await service.Send(query);
    
    // Assert
    Assert.Equal("test@example.com", result.Email);
    Assert.Equal("Test User", result.Name);
}

[Test]
public async Task CreateUserService_Should_Validate_Unique_Email()
{
    // Arrange
    using var context = CreateInMemoryContext();
    var existingUser = new ApplicationUser { Email = "test@example.com" };
    context.Users.Add(existingUser);
    await context.SaveChangesAsync();
    
    var service = new CreateUserService(context, Mock.Object, Mock.Object);
    var command = new CreateUser("test@example.com", "New User", "password");
    
    // Act & Assert
    await Assert.ThrowsAsync<ConflictException>(() => service.Send(command));
}
```

## Best Practices

### DO:
- ✅ Use mutable entities for EF operations
- ✅ Implement proper entity configurations
- ✅ Apply both Core and Application validation
- ✅ Use manual mapping for explicit control
- ✅ Handle domain events in infrastructure
- ✅ Implement proper error handling
- ✅ Use global query filters for soft delete

### DON'T:
- ❌ Put business logic in infrastructure services
- ❌ Reference Application layer from Infrastructure
- ❌ Use automatic mapping tools (prefer manual)
- ❌ Ignore validation results from Core
- ❌ Let EF entities leak to other layers

## Next Steps

Continue with the [Application Layer](application-layer.md) to understand how Core contracts and Infrastructure services are orchestrated in features.
