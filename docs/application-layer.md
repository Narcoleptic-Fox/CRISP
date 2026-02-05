# Application Layer

The Application layer orchestrates the interaction between Core contracts and Infrastructure services, providing feature-based organization and manual mapping between immutable contracts and mutable entities.

## Overview

The Application layer serves as the composition root and orchestration layer of CRISP architecture. It's where features come together, mapping between Core contracts and Infrastructure entities while handling user interactions through endpoints and UI components.

```
Application Layer Contents:
├── Server/                        # Server-side application
│   ├── Features/                  # Feature implementations
│   │   └── [Domain]/
│   │       ├── Endpoints/         # API endpoints
│   │       ├── Mappers/           # Manual mapping logic
│   │       └── Validators/        # Application validation
│   ├── Components/                # Blazor Server components
│   ├── Program.cs                 # Composition root
│   └── Middleware/                # Custom middleware
└── Client/                        # Client-side application
    ├── Features/                  # Client feature implementations
    │   └── [Domain]/
    │       ├── Components/        # Blazor components
    │       ├── Services/          # Client services
    │       └── Models/            # View models
    ├── Common/                    # Shared client logic
    ├── Layout/                    # Layout components
    └── Program.cs                 # Client composition root
```

## Feature Organization

### Feature Structure

Features in CRISP are organized as vertical slices that contain all the logic needed for a specific capability:

```csharp
namespace CRISP.Server.Features.Identity;

public class IdentityFeature : IFeature
{
    public IServiceCollection AddFeature(IServiceCollection services)
    {
        // Register all services for this feature
        services.AddScoped<IQueryService<GetUserByEmail, User>, GetUserByEmailService>();
        services.AddScoped<IQueryService<GetUsers, PagedResponse<User>>, GetUsersService>();
        services.AddScoped<ICreateService<CreateUser>, CreateUserService>();
        services.AddScoped<IModifyService<UpdateUser>, UpdateUserService>();
        
        return services;
    }

    public IEndpointRouteBuilder MapFeature(IEndpointRouteBuilder app)
    {
        var identityGroup = app.MapGroup("/api/identity").WithTags("Identity");
        
        // User endpoints
        var usersGroup = identityGroup.MapGroup("/users").WithTags("Users");
        UserEndpoints.MapEndpoints(usersGroup);
        
        // Role endpoints  
        var rolesGroup = identityGroup.MapGroup("/roles").WithTags("Roles");
        RoleEndpoints.MapEndpoints(rolesGroup);
        
        return app;
    }
}
```

### Endpoint Implementation

```csharp
namespace CRISP.Server.Features.Identity.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", GetUsersEndpoint.Handle);
        app.MapGet("/{id:guid}", GetUserByIdEndpoint.Handle);
        app.MapGet("/by-email/{email}", GetUserByEmailEndpoint.Handle);
        app.MapPost("/", CreateUserEndpoint.Handle);
        app.MapPut("/{id:guid}", UpdateUserEndpoint.Handle);
        app.MapDelete("/{id:guid}", DeleteUserEndpoint.Handle);
        
        return app;
    }
}

public static class GetUsersEndpoint
{
    public static async Task<PagedResponse<User>> Handle(
        [AsParameters] GetUsers query,
        IQueryService<GetUsers, PagedResponse<User>> service)
    {
        return await service.Send(query);
    }
}

public static class GetUserByEmailEndpoint
{
    public static async Task<IResult> Handle(
        string email,
        IQueryService<GetUserByEmail, User> service)
    {
        try
        {
            var user = await service.Send(new GetUserByEmail(email));
            return Results.Ok(user);
        }
        catch (NotFoundException)
        {
            return Results.NotFound($"User with email '{email}' not found");
        }
    }
}

public static class CreateUserEndpoint
{
    public static async Task<IResult> Handle(
        CreateUser command,
        ICreateService<CreateUser> service,
        LinkGenerator linkGenerator,
        HttpContext context)
    {
        try
        {
            var userId = await service.Send(command);
            
            var location = linkGenerator.GetUriByName(
                context, 
                nameof(GetUserByIdEndpoint), 
                new { id = userId });
                
            return Results.Created(location, new { Id = userId });
        }
        catch (ValidationException ex)
        {
            return Results.BadRequest(new { Errors = ex.Errors });
        }
        catch (ConflictException ex)
        {
            return Results.Conflict(new { Error = ex.Message });
        }
    }
}

public static class UpdateUserEndpoint
{
    public static async Task<IResult> Handle(
        Guid id,
        UpdateUser command,
        IModifyService<UpdateUser> service)
    {
        try
        {
            // Ensure ID consistency
            var commandWithId = command with { Id = id };
            await service.Send(commandWithId);
            
            return Results.NoContent();
        }
        catch (NotFoundException)
        {
            return Results.NotFound();
        }
        catch (ValidationException ex)
        {
            return Results.BadRequest(new { Errors = ex.Errors });
        }
    }
}
```

## Manual Mapping

### Mapping Philosophy

CRISP uses manual mapping to maintain explicit control over the transformation between Core contracts and Infrastructure entities. This approach provides:

- **Explicit control** over field mappings
- **Compile-time safety** for mapping logic
- **Testable** mapping operations
- **Performance** without reflection overhead

### Application-Level Mapping

```csharp
namespace CRISP.Server.Features.Identity.Mappers;

public static class UserMapper
{
    // Request/Response mapping
    public static CreateUser ToCommand(this CreateUserRequest request) =>
        new(request.Email, request.Name, request.Password);
    
    public static UpdateUser ToCommand(this UpdateUserRequest request, Guid id) =>
        new(id, request.Email, request.Name, request.IsActive);
    
    public static UserResponse ToResponse(this User user) =>
        new(user.Id, user.Email, user.Name, user.IsActive, user.CreatedOn);
    
    // Collection mapping
    public static PagedResponse<UserResponse> ToResponse(this PagedResponse<User> pagedUsers) =>
        new()
        {
            Items = pagedUsers.Items.Select(u => u.ToResponse()).ToList(),
            TotalCount = pagedUsers.TotalCount,
            Page = pagedUsers.Page,
            PageSize = pagedUsers.PageSize
        };
    
    // Query parameter mapping
    public static GetUsers ToQuery(this GetUsersRequest request) =>
        new()
        {
            Page = request.Page,
            PageSize = request.PageSize,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending,
            SearchTerm = request.SearchTerm,
            IsActive = request.IsActive,
            IncludeArchived = request.IncludeArchived
        };
}

// Request/Response DTOs
public record CreateUserRequest(string Email, string Name, string Password);
public record UpdateUserRequest(string? Email, string? Name, bool? IsActive);
public record UserResponse(Guid Id, string Email, string Name, bool IsActive, DateTime CreatedOn);

public record GetUsersRequest
{
    public int? Page { get; init; }
    public int? PageSize { get; init; }
    public string? SortBy { get; init; }
    public bool? SortDescending { get; init; }
    public string? SearchTerm { get; init; }
    public bool? IsActive { get; init; }
    public bool? IncludeArchived { get; init; }
}
```

## Application Validation

### Validation Layer

Application validation handles concerns that require infrastructure dependencies:

```csharp
namespace CRISP.Server.Features.Identity.Validators;

public class CreateUserApplicationValidator
{
    private readonly ApplicationDbContext _context;
    
    public CreateUserApplicationValidator(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<ValidationResult> ValidateAsync(CreateUser command, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();
        
        // Core validation first
        var coreUser = command.ToUser();
        var coreValidation = coreUser.Validate();
        if (!coreValidation.IsSuccess)
        {
            errors.AddRange(coreValidation.Errors);
        }
        
        // Application-specific validation
        if (await _context.Users.AnyAsync(u => u.Email == command.Email, cancellationToken))
        {
            errors.Add($"User with email '{command.Email}' already exists");
        }
        
        // Password complexity (could also be in Core)
        if (command.Password.Length < 8)
        {
            errors.Add("Password must be at least 8 characters long");
        }
        
        return errors.Any() 
            ? ValidationResult.Failure(errors)
            : ValidationResult.Success();
    }
}

public class UpdateUserApplicationValidator
{
    private readonly ApplicationDbContext _context;
    
    public UpdateUserApplicationValidator(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<ValidationResult> ValidateAsync(UpdateUser command, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();
        
        // Check if user exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == command.Id, cancellationToken);
            
        if (existingUser == null)
        {
            errors.Add($"User with ID '{command.Id}' not found");
            return ValidationResult.Failure(errors);
        }
        
        // Email uniqueness validation
        if (command.Email != null && command.Email != existingUser.Email)
        {
            if (await _context.Users.AnyAsync(u => u.Email == command.Email && u.Id != command.Id, cancellationToken))
            {
                errors.Add($"User with email '{command.Email}' already exists");
            }
        }
        
        // Create updated domain model for core validation
        var updatedUser = new User(
            Id: existingUser.Id,
            Email: command.Email ?? existingUser.Email!,
            Name: command.Name ?? existingUser.Name,
            IsActive: command.IsActive ?? (existingUser.LockoutEnd == null)
        );
        
        var coreValidation = updatedUser.Validate();
        if (!coreValidation.IsSuccess)
        {
            errors.AddRange(coreValidation.Errors);
        }
        
        return errors.Any() 
            ? ValidationResult.Failure(errors)
            : ValidationResult.Success();
    }
}
```

## Error Handling

### Global Exception Handling

```csharp
namespace CRISP.Server.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    
    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = exception switch
        {
            NotFoundException notFound => new
            {
                StatusCode = 404,
                Error = "Not Found",
                Message = notFound.Message
            },
            ValidationException validation => new
            {
                StatusCode = 400,
                Error = "Validation Failed",
                Errors = validation.Errors
            },
            ConflictException conflict => new
            {
                StatusCode = 409,
                Error = "Conflict",
                Message = conflict.Message
            },
            UnauthorizedAccessException => new
            {
                StatusCode = 401,
                Error = "Unauthorized",
                Message = "Access denied"
            },
            _ => new
            {
                StatusCode = 500,
                Error = "Internal Server Error",
                Message = "An unexpected error occurred"
            }
        };
        
        context.Response.StatusCode = response.StatusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
```

## Client-Side Implementation

### Blazor Components

```csharp
@page "/users"
@using CRISP.Core.Identity
@inject IQueryService<GetUsers, PagedResponse<User>> GetUsersService
@inject NavigationManager Navigation

<PageTitle>Users</PageTitle>

<MudContainer MaxWidth="MaxWidth.Large" Class="mt-4">
    <MudText Typo="Typo.h4" Class="mb-4">Users</MudText>
    
    <MudCard>
        <MudCardContent>
            <!-- Search and filters -->
            <MudGrid Class="mb-4">
                <MudItem xs="12" md="6">
                    <MudTextField @bind-Value="searchTerm" 
                                  Label="Search users" 
                                  Adornment="Adornment.End"
                                  AdornmentIcon="@Icons.Material.Filled.Search"
                                  OnAdornmentClick="SearchUsers" />
                </MudItem>
                <MudItem xs="12" md="3">
                    <MudSelect @bind-Value="isActiveFilter" Label="Status">
                        <MudSelectItem Value="@((bool?)null)">All</MudSelectItem>
                        <MudSelectItem Value="true">Active</MudSelectItem>
                        <MudSelectItem Value="false">Inactive</MudSelectItem>
                    </MudSelect>
                </MudItem>
                <MudItem xs="12" md="3">
                    <MudButton Variant="Variant.Filled" 
                               Color="Color.Primary"
                               StartIcon="@Icons.Material.Filled.Add"
                               OnClick="CreateUser">
                        Create User
                    </MudButton>
                </MudItem>
            </MudGrid>
            
            <!-- Users table -->
            <MudTable Items="users" 
                      Loading="loading" 
                      ServerData="LoadUsersAsync"
                      @ref="table">
                <HeaderContent>
                    <MudTh>Email</MudTh>
                    <MudTh>Name</MudTh>
                    <MudTh>Status</MudTh>
                    <MudTh>Created</MudTh>
                    <MudTh>Actions</MudTh>
                </HeaderContent>
                <RowTemplate>
                    <MudTd DataLabel="Email">@context.Email</MudTd>
                    <MudTd DataLabel="Name">@context.Name</MudTd>
                    <MudTd DataLabel="Status">
                        <MudChip Color="@(context.IsActive ? Color.Success : Color.Default)"
                                 Size="Size.Small">
                            @(context.IsActive ? "Active" : "Inactive")
                        </MudChip>
                    </MudTd>
                    <MudTd DataLabel="Created">@context.CreatedOn.ToString("MMM dd, yyyy")</MudTd>
                    <MudTd DataLabel="Actions">
                        <MudIconButton Icon="@Icons.Material.Filled.Edit" 
                                       Size="Size.Small"
                                       OnClick="@(() => EditUser(context.Id))" />
                        <MudIconButton Icon="@Icons.Material.Filled.Delete" 
                                       Size="Size.Small"
                                       Color="Color.Error"
                                       OnClick="@(() => DeleteUser(context.Id))" />
                    </MudTd>
                </RowTemplate>
                <PagerContent>
                    <MudTablePager />
                </PagerContent>
            </MudTable>
        </MudCardContent>
    </MudCard>
</MudContainer>

@code {
    private MudTable<User> table = null!;
    private IList<User> users = new List<User>();
    private bool loading;
    private string searchTerm = string.Empty;
    private bool? isActiveFilter;
    
    private async Task<TableData<User>> LoadUsersAsync(TableState state, CancellationToken cancellationToken)
    {
        loading = true;
        
        var query = new GetUsers
        {
            Page = state.Page,
            PageSize = state.PageSize,
            SortBy = state.SortLabel,
            SortDescending = state.SortDirection == SortDirection.Descending,
            SearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm,
            IsActive = isActiveFilter
        };
        
        try
        {
            var result = await GetUsersService.Send(query, cancellationToken);
            users = result.Items;
            
            return new TableData<User>
            {
                Items = result.Items,
                TotalItems = result.TotalCount
            };
        }
        finally
        {
            loading = false;
        }
    }
    
    private async Task SearchUsers()
    {
        await table.ReloadServerData();
    }
    
    private void CreateUser()
    {
        Navigation.NavigateTo("/users/create");
    }
    
    private void EditUser(Guid userId)
    {
        Navigation.NavigateTo($"/users/{userId}/edit");
    }
    
    private async Task DeleteUser(Guid userId)
    {
        // Show confirmation dialog and handle deletion
    }
}
```

### Client Services

```csharp
namespace CRISP.Client.Features.Identity.Services;

public class UserClientService
{
    private readonly HttpClient _httpClient;
    
    public UserClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<PagedResponse<User>> GetUsersAsync(GetUsers query, CancellationToken cancellationToken = default)
    {
        var queryString = query.ToQueryString();
        var response = await _httpClient.GetAsync($"/api/identity/users{queryString}", cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<PagedResponse<User>>(json, JsonOptions.Default)!;
    }
    
    public async Task<User> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"/api/identity/users/by-email/{Uri.EscapeDataString(email)}", cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<User>(json, JsonOptions.Default)!;
    }
    
    public async Task<Guid> CreateUserAsync(CreateUser command, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(command, JsonOptions.Default);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync("/api/identity/users", content, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<CreateUserResponse>(responseJson, JsonOptions.Default)!;
        return result.Id;
    }
}

public record CreateUserResponse(Guid Id);
```

## Testing Application Layer

### Endpoint Testing

```csharp
[Test]
public async Task GetUsers_Should_Return_Paged_Results()
{
    // Arrange
    var mockService = new Mock<IQueryService<GetUsers, PagedResponse<User>>>();
    var expectedUsers = new PagedResponse<User>
    {
        Items = new List<User> 
        { 
            new(Guid.NewGuid(), "user1@example.com", "User 1", true),
            new(Guid.NewGuid(), "user2@example.com", "User 2", true)
        },
        TotalCount = 2,
        Page = 0,
        PageSize = 10
    };
    
    mockService.Setup(s => s.Send(It.IsAny<GetUsers>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(expectedUsers);
    
    // Act
    var result = await GetUsersEndpoint.Handle(new GetUsers(), mockService.Object);
    
    // Assert
    Assert.Equal(2, result.Items.Count);
    Assert.Equal(2, result.TotalCount);
}

[Test]
public async Task CreateUser_Should_Return_Created_Result_With_Location()
{
    // Arrange
    var mockService = new Mock<ICreateService<CreateUser>>();
    var expectedUserId = Guid.NewGuid();
    var command = new CreateUser("test@example.com", "Test User", "password123");
    
    mockService.Setup(s => s.Send(command, It.IsAny<CancellationToken>()))
        .ReturnsAsync(expectedUserId);
    
    // Act
    var result = await CreateUserEndpoint.Handle(command, mockService.Object, null!, null!);
    
    // Assert
    var createdResult = Assert.IsType<Created<object>>(result);
    Assert.Equal(expectedUserId, ((dynamic)createdResult.Value!).Id);
}
```

### Integration Testing

```csharp
public class UserEndpointsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    
    public UserEndpointsIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }
    
    [Test]
    public async Task GetUsers_Should_Return_Success()
    {
        // Act
        var response = await _client.GetAsync("/api/identity/users");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResponse<User>>(content);
        
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
    }
    
    [Test]
    public async Task CreateUser_Should_Create_And_Return_Location()
    {
        // Arrange
        var command = new CreateUser("newuser@example.com", "New User", "password123");
        var json = JsonSerializer.Serialize(command);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        // Act
        var response = await _client.PostAsync("/api/identity/users", content);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
    }
}
```

## Best Practices

### DO:
- ✅ Organize features as vertical slices
- ✅ Use manual mapping for explicit control
- ✅ Implement both Core and Application validation
- ✅ Handle errors consistently across endpoints
- ✅ Use proper HTTP status codes
- ✅ Implement proper cancellation token support
- ✅ Separate client and server concerns

### DON'T:
- ❌ Put business logic in endpoints
- ❌ Use automatic mapping tools
- ❌ Ignore validation results
- ❌ Return infrastructure entities from endpoints
- ❌ Mix server and client responsibilities
- ❌ Create god components/services

## Next Steps

Continue with the [Feature Development Guide](feature-development-guide.md) to learn how to build new features using the CRISP pattern.
