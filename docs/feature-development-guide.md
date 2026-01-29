# Feature Development Guide

This guide walks you through developing new features in CRISP architecture, from domain modeling to UI implementation.

## Overview

CRISP features are developed as vertical slices that span all three layers:
1. **Core**: Domain contracts, validation, and events
2. **Infrastructure**: Entity mappings and service implementations  
3. **Application**: Endpoints, components, and orchestration

## Step-by-Step Feature Development

### Step 1: Define Domain Contracts (Core Layer)

Start by modeling your domain in the Core layer with immutable contracts.

#### Domain Model

```csharp
// CRISP.Core/Catalog/Product.cs
namespace CRISP.Core.Catalog;

public record Product(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    bool IsActive,
    Guid CategoryId
) : BaseAuditableModel
{
    public ValidationResult Validate()
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Product name is required");
            
        if (Name.Length > 100)
            errors.Add("Product name cannot exceed 100 characters");
            
        if (Price < 0)
            errors.Add("Price cannot be negative");
            
        if (StockQuantity < 0)
            errors.Add("Stock quantity cannot be negative");
            
        return errors.Any() 
            ? ValidationResult.Failure(errors)
            : ValidationResult.Success();
    }
}

public record Category(
    Guid Id,
    string Name,
    string Description
) : BaseAuditableModel
{
    public ValidationResult Validate()
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Category name is required");
            
        if (Name.Length > 50)
            errors.Add("Category name cannot exceed 50 characters");
            
        return errors.Any() 
            ? ValidationResult.Failure(errors)
            : ValidationResult.Success();
    }
}
```

#### Commands

```csharp
// CRISP.Core/Catalog/ProductCommands.cs
namespace CRISP.Core.Catalog;

public record CreateProduct(
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    Guid CategoryId
) : CreateCommand
{
    public Product ToProduct() => new(
        Id: Guid.NewGuid(),
        Name: Name,
        Description: Description,
        Price: Price,
        StockQuantity: StockQuantity,
        IsActive: true,
        CategoryId: CategoryId
    );
}

public record UpdateProduct(
    Guid Id,
    string? Name = null,
    string? Description = null,
    decimal? Price = null,
    int? StockQuantity = null,
    bool? IsActive = null,
    Guid? CategoryId = null
) : ModifyCommand(Id);

public record AdjustProductStock(
    Guid ProductId,
    int Adjustment,
    string Reason
) : ICommand;
```

#### Queries

```csharp
// CRISP.Core/Catalog/ProductQueries.cs
namespace CRISP.Core.Catalog;

public record GetProducts : PagedQuery<Product>
{
    public string? SearchTerm { get; init; }
    public Guid? CategoryId { get; init; }
    public bool? IsActive { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    
    public override string ToQueryString()
    {
        var baseQuery = base.ToQueryString();
        var filters = new List<string>();
        
        if (!string.IsNullOrEmpty(SearchTerm))
            filters.Add($"searchTerm={Uri.EscapeDataString(SearchTerm)}");
            
        if (CategoryId.HasValue)
            filters.Add($"categoryId={CategoryId.Value}");
            
        if (IsActive.HasValue)
            filters.Add($"isActive={IsActive.Value}");
            
        if (MinPrice.HasValue)
            filters.Add($"minPrice={MinPrice.Value}");
            
        if (MaxPrice.HasValue)
            filters.Add($"maxPrice={MaxPrice.Value}");
            
        return filters.Any() 
            ? $"{baseQuery}&{string.Join("&", filters)}"
            : baseQuery;
    }
}

public record GetProductById(Guid Id) : SingularQuery<Product>;
public record GetProductsByCategory(Guid CategoryId) : IQuery<IEnumerable<Product>>;
public record SearchProducts(string SearchTerm) : IQuery<IEnumerable<Product>>;
```

#### Domain Events

```csharp
// CRISP.Core/Catalog/ProductEvents.cs
namespace CRISP.Core.Catalog;

public record ProductCreated(
    Guid ProductId,
    string Name,
    decimal Price,
    Guid CategoryId
) : DomainEvent;

public record ProductUpdated(
    Guid ProductId,
    string Name,
    decimal Price
) : DomainEvent;

public record ProductStockAdjusted(
    Guid ProductId,
    int PreviousQuantity,
    int NewQuantity,
    int Adjustment,
    string Reason
) : DomainEvent;

public record ProductArchived(
    Guid ProductId,
    string Reason
) : DomainEvent;
```

### Step 2: Create Infrastructure Entities (Infrastructure Layer)

#### Entity Framework Entities

```csharp
// CRISP.Infrastructure/Data/Entities/ProductEntity.cs
namespace CRISP.Infrastructure.Data.Entities;

public class ProductEntity : BaseAuditableEntity, ISoftDelete
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    
    // Foreign keys
    public Guid CategoryId { get; set; }
    
    // Navigation properties
    public virtual CategoryEntity Category { get; set; } = null!;
    public virtual ICollection<OrderItemEntity> OrderItems { get; set; } = [];
}

public class CategoryEntity : BaseAuditableEntity, ISoftDelete
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    
    // Navigation properties
    public virtual ICollection<ProductEntity> Products { get; set; } = [];
}
```

#### Entity Configurations

```csharp
// CRISP.Infrastructure/Data/Configurations/ProductConfiguration.cs
namespace CRISP.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<ProductEntity>
{
    public void Configure(EntityTypeBuilder<ProductEntity> builder)
    {
        builder.ToTable("Products");
        
        builder.Property(p => p.Name)
            .HasMaxLength(100)
            .IsRequired();
            
        builder.Property(p => p.Description)
            .HasMaxLength(500);
            
        builder.Property(p => p.Price)
            .HasPrecision(18, 2);
            
        builder.Property(p => p.IsDeleted)
            .HasDefaultValue(false);
            
        builder.Property(p => p.IsActive)
            .HasDefaultValue(true);
        
        // Indexes
        builder.HasIndex(p => p.Name);
        builder.HasIndex(p => p.CategoryId);
        builder.HasIndex(p => p.IsActive);
        builder.HasIndex(p => p.IsDeleted);
        
        // Relationships
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class CategoryConfiguration : IEntityTypeConfiguration<CategoryEntity>
{
    public void Configure(EntityTypeBuilder<CategoryEntity> builder)
    {
        builder.ToTable("Categories");
        
        builder.Property(c => c.Name)
            .HasMaxLength(50)
            .IsRequired();
            
        builder.Property(c => c.Description)
            .HasMaxLength(200);
            
        builder.Property(c => c.IsDeleted)
            .HasDefaultValue(false);
        
        // Indexes
        builder.HasIndex(c => c.Name)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}
```

#### Manual Mapping

```csharp
// CRISP.Infrastructure/Mappings/ProductMappings.cs
namespace CRISP.Infrastructure.Mappings;

public static class ProductMappings
{
    public static Product ToContract(this ProductEntity entity)
    {
        return new Product(
            Id: entity.Id,
            Name: entity.Name,
            Description: entity.Description,
            Price: entity.Price,
            StockQuantity: entity.StockQuantity,
            IsActive: entity.IsActive,
            CategoryId: entity.CategoryId
        );
    }
    
    public static ProductEntity ToEntity(this Product contract)
    {
        return new ProductEntity
        {
            Id = contract.Id,
            Name = contract.Name,
            Description = contract.Description,
            Price = contract.Price,
            StockQuantity = contract.StockQuantity,
            IsActive = contract.IsActive,
            CategoryId = contract.CategoryId,
            CreatedOn = contract.CreatedOn,
            UpdatedOn = contract.UpdatedOn,
            IsArchived = contract.IsArchived
        };
    }
    
    public static void UpdateEntity(this ProductEntity entity, Product contract)
    {
        entity.Name = contract.Name;
        entity.Description = contract.Description;
        entity.Price = contract.Price;
        entity.StockQuantity = contract.StockQuantity;
        entity.IsActive = contract.IsActive;
        entity.CategoryId = contract.CategoryId;
        entity.UpdatedOn = DateTime.UtcNow;
    }
}

public static class CategoryMappings
{
    public static Category ToContract(this CategoryEntity entity)
    {
        return new Category(
            Id: entity.Id,
            Name: entity.Name,
            Description: entity.Description
        );
    }
    
    public static CategoryEntity ToEntity(this Category contract)
    {
        return new CategoryEntity
        {
            Id = contract.Id,
            Name = contract.Name,
            Description = contract.Description,
            CreatedOn = contract.CreatedOn,
            UpdatedOn = contract.UpdatedOn,
            IsArchived = contract.IsArchived
        };
    }
}
```

### Step 3: Implement Services (Infrastructure Layer)

#### Query Services

```csharp
// CRISP.Infrastructure/Services/Catalog/GetProductsService.cs
namespace CRISP.Infrastructure.Services.Catalog;

public class GetProductsService : IQueryService<GetProducts, PagedResponse<Product>>
{
    private readonly ApplicationDbContext _context;
    
    public GetProductsService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async ValueTask<PagedResponse<Product>> Send(GetProducts query, CancellationToken cancellationToken = default)
    {
        var queryable = _context.Products
            .Include(p => p.Category)
            .AsQueryable();
        
        // Apply filters
        if (!string.IsNullOrEmpty(query.SearchTerm))
        {
            queryable = queryable.Where(p => 
                p.Name.Contains(query.SearchTerm) || 
                p.Description.Contains(query.SearchTerm));
        }
        
        if (query.CategoryId.HasValue)
        {
            queryable = queryable.Where(p => p.CategoryId == query.CategoryId.Value);
        }
        
        if (query.IsActive.HasValue)
        {
            queryable = queryable.Where(p => p.IsActive == query.IsActive.Value);
        }
        
        if (query.MinPrice.HasValue)
        {
            queryable = queryable.Where(p => p.Price >= query.MinPrice.Value);
        }
        
        if (query.MaxPrice.HasValue)
        {
            queryable = queryable.Where(p => p.Price <= query.MaxPrice.Value);
        }
        
        // Apply archived filter
        if (!query.IncludeArchived.GetValueOrDefault())
        {
            queryable = queryable.Where(p => !p.IsDeleted);
        }
        
        // Apply sorting
        queryable = query.GetSortByOrDefault() switch
        {
            nameof(Product.Name) => query.GetSortDescendingOrDefault() 
                ? queryable.OrderByDescending(p => p.Name)
                : queryable.OrderBy(p => p.Name),
            nameof(Product.Price) => query.GetSortDescendingOrDefault()
                ? queryable.OrderByDescending(p => p.Price)
                : queryable.OrderBy(p => p.Price),
            nameof(Product.CreatedOn) => query.GetSortDescendingOrDefault()
                ? queryable.OrderByDescending(p => p.CreatedOn)
                : queryable.OrderBy(p => p.CreatedOn),
            _ => queryable.OrderBy(p => p.Name)
        };
        
        // Apply pagination
        var totalCount = await queryable.CountAsync(cancellationToken);
        var entities = await queryable
            .Skip(query.GetPageOrDefault() * query.GetPageSizeOrDefault())
            .Take(query.GetPageSizeOrDefault())
            .ToListAsync(cancellationToken);
            
        return new PagedResponse<Product>
        {
            Items = entities.Select(e => e.ToContract()).ToList(),
            TotalCount = totalCount,
            Page = query.GetPageOrDefault(),
            PageSize = query.GetPageSizeOrDefault()
        };
    }
}
```

#### Command Services

```csharp
// CRISP.Infrastructure/Services/Catalog/CreateProductService.cs
namespace CRISP.Infrastructure.Services.Catalog;

public class CreateProductService : ICreateService<CreateProduct>
{
    private readonly ApplicationDbContext _context;
    private readonly IDomainEventDispatcher _eventDispatcher;
    
    public CreateProductService(ApplicationDbContext context, IDomainEventDispatcher eventDispatcher)
    {
        _context = context;
        _eventDispatcher = eventDispatcher;
    }
    
    public async ValueTask<Guid> Send(CreateProduct command, CancellationToken cancellationToken = default)
    {
        // Core validation
        var product = command.ToProduct();
        var validation = product.Validate();
        if (!validation.IsSuccess)
            throw new ValidationException(validation.Errors);
        
        // Application validation
        if (await _context.Products.AnyAsync(p => p.Name == command.Name, cancellationToken))
            throw new ConflictException($"Product with name '{command.Name}' already exists");
            
        if (!await _context.Categories.AnyAsync(c => c.Id == command.CategoryId, cancellationToken))
            throw new NotFoundException($"Category with ID '{command.CategoryId}' not found");
        
        // Create entity
        var entity = product.ToEntity();
        _context.Products.Add(entity);
        
        await _context.SaveChangesAsync(cancellationToken);
        
        // Dispatch domain event
        await _eventDispatcher.Dispatch(new ProductCreated(
            entity.Id, 
            entity.Name, 
            entity.Price, 
            entity.CategoryId));
        
        return entity.Id;
    }
}

public class AdjustProductStockService : ICommandService<AdjustProductStock>
{
    private readonly ApplicationDbContext _context;
    private readonly IDomainEventDispatcher _eventDispatcher;
    
    public AdjustProductStockService(ApplicationDbContext context, IDomainEventDispatcher eventDispatcher)
    {
        _context = context;
        _eventDispatcher = eventDispatcher;
    }
    
    public async ValueTask Send(AdjustProductStock command, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == command.ProductId, cancellationToken);
            
        if (entity == null)
            throw new NotFoundException($"Product with ID '{command.ProductId}' not found");
        
        var previousQuantity = entity.StockQuantity;
        var newQuantity = previousQuantity + command.Adjustment;
        
        if (newQuantity < 0)
            throw new ValidationException(new[] { "Insufficient stock for adjustment" });
        
        entity.StockQuantity = newQuantity;
        entity.UpdatedOn = DateTime.UtcNow;
        
        await _context.SaveChangesAsync(cancellationToken);
        
        // Dispatch domain event
        await _eventDispatcher.Dispatch(new ProductStockAdjusted(
            command.ProductId,
            previousQuantity,
            newQuantity,
            command.Adjustment,
            command.Reason));
    }
}
```

### Step 4: Create Feature (Application Layer)

#### Feature Registration

```csharp
// CRISP.Server/Features/Catalog/CatalogFeature.cs
namespace CRISP.Server.Features.Catalog;

public class CatalogFeature : IFeature
{
    public IServiceCollection AddFeature(IServiceCollection services)
    {
        // Query services
        services.AddScoped<IQueryService<GetProducts, PagedResponse<Product>>, GetProductsService>();
        services.AddScoped<IQueryService<SingularQuery<Product>, Product>, GetProductService>();
        services.AddScoped<IQueryService<GetProductsByCategory, IEnumerable<Product>>, GetProductsByCategoryService>();
        
        // Command services
        services.AddScoped<ICreateService<CreateProduct>, CreateProductService>();
        services.AddScoped<IModifyService<UpdateProduct>, UpdateProductService>();
        services.AddScoped<ICommandService<AdjustProductStock>, AdjustProductStockService>();
        services.AddKeyedScoped<IModifyService<DeleteCommand>, DeleteProductService>(nameof(Product));
        
        return services;
    }

    public IEndpointRouteBuilder MapFeature(IEndpointRouteBuilder app)
    {
        var catalogGroup = app.MapGroup("/api/catalog").WithTags("Catalog");
        
        // Product endpoints
        var productsGroup = catalogGroup.MapGroup("/products").WithTags("Products");
        ProductEndpoints.MapEndpoints(productsGroup);
        
        // Category endpoints
        var categoriesGroup = catalogGroup.MapGroup("/categories").WithTags("Categories");
        CategoryEndpoints.MapEndpoints(categoriesGroup);
        
        return app;
    }
}
```

#### Endpoints

```csharp
// CRISP.Server/Features/Catalog/Endpoints/ProductEndpoints.cs
namespace CRISP.Server.Features.Catalog.Endpoints;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", GetProductsEndpoint.Handle)
            .WithName(nameof(GetProductsEndpoint))
            .WithSummary("Get products with filtering and pagination");
            
        app.MapGet("/{id:guid}", GetProductByIdEndpoint.Handle)
            .WithName(nameof(GetProductByIdEndpoint))
            .WithSummary("Get product by ID");
            
        app.MapPost("/", CreateProductEndpoint.Handle)
            .WithName(nameof(CreateProductEndpoint))
            .WithSummary("Create a new product");
            
        app.MapPut("/{id:guid}", UpdateProductEndpoint.Handle)
            .WithName(nameof(UpdateProductEndpoint))
            .WithSummary("Update an existing product");
            
        app.MapPost("/{id:guid}/adjust-stock", AdjustProductStockEndpoint.Handle)
            .WithName(nameof(AdjustProductStockEndpoint))
            .WithSummary("Adjust product stock quantity");
            
        app.MapDelete("/{id:guid}", DeleteProductEndpoint.Handle)
            .WithName(nameof(DeleteProductEndpoint))
            .WithSummary("Delete a product");
        
        return app;
    }
}

public static class GetProductsEndpoint
{
    public static async Task<PagedResponse<Product>> Handle(
        [AsParameters] GetProducts query,
        IQueryService<GetProducts, PagedResponse<Product>> service)
    {
        return await service.Send(query);
    }
}

public static class CreateProductEndpoint
{
    public static async Task<IResult> Handle(
        CreateProduct command,
        ICreateService<CreateProduct> service,
        LinkGenerator linkGenerator,
        HttpContext context)
    {
        try
        {
            var productId = await service.Send(command);
            
            var location = linkGenerator.GetUriByName(
                context, 
                nameof(GetProductByIdEndpoint), 
                new { id = productId });
                
            return Results.Created(location, new { Id = productId });
        }
        catch (ValidationException ex)
        {
            return Results.BadRequest(new { Errors = ex.Errors });
        }
        catch (ConflictException ex)
        {
            return Results.Conflict(new { Error = ex.Message });
        }
        catch (NotFoundException ex)
        {
            return Results.BadRequest(new { Error = ex.Message });
        }
    }
}

public static class AdjustProductStockEndpoint
{
    public static async Task<IResult> Handle(
        Guid id,
        AdjustStockRequest request,
        ICommandService<AdjustProductStock> service)
    {
        try
        {
            var command = new AdjustProductStock(id, request.Adjustment, request.Reason);
            await service.Send(command);
            
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

public record AdjustStockRequest(int Adjustment, string Reason);
```

### Step 5: Add Client Components (Application Layer)

#### Product List Component

```csharp
@page "/products"
@using CRISP.Core.Catalog
@inject IQueryService<GetProducts, PagedResponse<Product>> GetProductsService
@inject NavigationManager Navigation

<PageTitle>Products</PageTitle>

<MudContainer MaxWidth="MaxWidth.Large" Class="mt-4">
    <MudText Typo="Typo.h4" Class="mb-4">Product Catalog</MudText>
    
    <MudCard>
        <MudCardContent>
            <!-- Filters -->
            <MudGrid Class="mb-4">
                <MudItem xs="12" md="4">
                    <MudTextField @bind-Value="searchTerm" 
                                  Label="Search products" 
                                  Adornment="Adornment.End"
                                  AdornmentIcon="@Icons.Material.Filled.Search"
                                  OnAdornmentClick="SearchProducts" />
                </MudItem>
                <MudItem xs="12" md="2">
                    <MudSelect @bind-Value="categoryFilter" Label="Category">
                        <MudSelectItem Value="@((Guid?)null)">All Categories</MudSelectItem>
                        @foreach (var category in categories)
                        {
                            <MudSelectItem Value="category.Id">@category.Name</MudSelectItem>
                        }
                    </MudSelect>
                </MudItem>
                <MudItem xs="12" md="2">
                    <MudNumericField @bind-Value="minPrice" Label="Min Price" Format="C" />
                </MudItem>
                <MudItem xs="12" md="2">
                    <MudNumericField @bind-Value="maxPrice" Label="Max Price" Format="C" />
                </MudItem>
                <MudItem xs="12" md="2">
                    <MudButton Variant="Variant.Filled" 
                               Color="Color.Primary"
                               StartIcon="@Icons.Material.Filled.Add"
                               OnClick="CreateProduct">
                        Add Product
                    </MudButton>
                </MudItem>
            </MudGrid>
            
            <!-- Products table -->
            <MudTable Items="products" 
                      Loading="loading" 
                      ServerData="LoadProductsAsync"
                      @ref="table">
                <HeaderContent>
                    <MudTh><MudTableSortLabel SortLabel="@nameof(Product.Name)" T="Product">Name</MudTableSortLabel></MudTh>
                    <MudTh>Category</MudTh>
                    <MudTh><MudTableSortLabel SortLabel="@nameof(Product.Price)" T="Product">Price</MudTableSortLabel></MudTh>
                    <MudTh>Stock</MudTh>
                    <MudTh>Status</MudTh>
                    <MudTh>Actions</MudTh>
                </HeaderContent>
                <RowTemplate>
                    <MudTd DataLabel="Name">
                        <div>
                            <MudText Typo="Typo.body1">@context.Name</MudText>
                            <MudText Typo="Typo.caption" Color="Color.Tertiary">@context.Description</MudText>
                        </div>
                    </MudTd>
                    <MudTd DataLabel="Category">@GetCategoryName(context.CategoryId)</MudTd>
                    <MudTd DataLabel="Price">@context.Price.ToString("C")</MudTd>
                    <MudTd DataLabel="Stock">
                        <MudChip Color="@GetStockColor(context.StockQuantity)" Size="Size.Small">
                            @context.StockQuantity
                        </MudChip>
                    </MudTd>
                    <MudTd DataLabel="Status">
                        <MudChip Color="@(context.IsActive ? Color.Success : Color.Default)" Size="Size.Small">
                            @(context.IsActive ? "Active" : "Inactive")
                        </MudChip>
                    </MudTd>
                    <MudTd DataLabel="Actions">
                        <MudIconButton Icon="@Icons.Material.Filled.Edit" 
                                       Size="Size.Small"
                                       OnClick="@(() => EditProduct(context.Id))" />
                        <MudIconButton Icon="@Icons.Material.Filled.Inventory" 
                                       Size="Size.Small"
                                       Color="Color.Info"
                                       OnClick="@(() => AdjustStock(context.Id))" />
                        <MudIconButton Icon="@Icons.Material.Filled.Delete" 
                                       Size="Size.Small"
                                       Color="Color.Error"
                                       OnClick="@(() => DeleteProduct(context.Id))" />
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
    private MudTable<Product> table = null!;
    private IList<Product> products = new List<Product>();
    private IList<Category> categories = new List<Category>();
    private bool loading;
    private string searchTerm = string.Empty;
    private Guid? categoryFilter;
    private decimal? minPrice;
    private decimal? maxPrice;
    
    protected override async Task OnInitializedAsync()
    {
        // Load categories for filter
        // categories = await GetCategoriesService.Send(new GetCategories());
    }
    
    private async Task<TableData<Product>> LoadProductsAsync(TableState state, CancellationToken cancellationToken)
    {
        loading = true;
        
        var query = new GetProducts
        {
            Page = state.Page,
            PageSize = state.PageSize,
            SortBy = state.SortLabel,
            SortDescending = state.SortDirection == SortDirection.Descending,
            SearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm,
            CategoryId = categoryFilter,
            MinPrice = minPrice,
            MaxPrice = maxPrice
        };
        
        try
        {
            var result = await GetProductsService.Send(query, cancellationToken);
            products = result.Items;
            
            return new TableData<Product>
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
    
    private async Task SearchProducts()
    {
        await table.ReloadServerData();
    }
    
    private string GetCategoryName(Guid categoryId)
    {
        return categories.FirstOrDefault(c => c.Id == categoryId)?.Name ?? "Unknown";
    }
    
    private Color GetStockColor(int stock)
    {
        return stock switch
        {
            0 => Color.Error,
            <= 10 => Color.Warning,
            _ => Color.Success
        };
    }
    
    private void CreateProduct() => Navigation.NavigateTo("/products/create");
    private void EditProduct(Guid id) => Navigation.NavigateTo($"/products/{id}/edit");
    private void AdjustStock(Guid id) => Navigation.NavigateTo($"/products/{id}/adjust-stock");
    private async Task DeleteProduct(Guid id) { /* Implement deletion */ }
}
```

## Testing Your Feature

### Unit Tests

```csharp
// Test Core validation
[Test]
public void Product_Validate_Should_Require_Name()
{
    var product = new Product(Guid.NewGuid(), "", "Description", 10.0m, 5, true, Guid.NewGuid());
    var result = product.Validate();
    
    Assert.False(result.IsSuccess);
    Assert.Contains("Product name is required", result.Errors);
}

// Test service implementation
[Test]
public async Task CreateProductService_Should_Create_Product_Successfully()
{
    using var context = CreateInMemoryContext();
    var service = new CreateProductService(context, Mock.Of<IDomainEventDispatcher>());
    
    var command = new CreateProduct("Test Product", "Description", 10.0m, 5, Guid.NewGuid());
    var productId = await service.Send(command);
    
    var product = await context.Products.FindAsync(productId);
    Assert.NotNull(product);
    Assert.Equal("Test Product", product.Name);
}
```

### Integration Tests

```csharp
[Test]
public async Task GetProducts_Should_Return_Filtered_Results()
{
    var response = await _client.GetAsync("/api/catalog/products?searchTerm=test");
    
    response.EnsureSuccessStatusCode();
    var content = await response.Content.ReadAsStringAsync();
    var result = JsonSerializer.Deserialize<PagedResponse<Product>>(content);
    
    Assert.NotNull(result);
    Assert.All(result.Items, p => Assert.Contains("test", p.Name, StringComparison.OrdinalIgnoreCase));
}
```

## Best Practices Summary

1. **Start with Core contracts** - Model your domain first
2. **Keep validation in domain models** - Business rules belong in Core
3. **Use manual mapping** - Explicit control over transformations
4. **Implement proper error handling** - Consistent error responses
5. **Test at all layers** - Unit tests for Core, integration tests for features
6. **Follow naming conventions** - Clear, descriptive names for all artifacts
7. **Use ValueTask for performance** - Optimal async operations
8. **Handle cancellation tokens** - Proper async cancellation support

## Next Steps

- Review [Migration Guide](migration-guide.md) for migrating existing applications
- Explore [Examples](examples/) for more complex scenarios
- Check [ADRs](adrs/) for architectural decisions and rationale
