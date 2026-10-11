---
title: Anatomy of a vertical slice
description: One feature end to end: Core records, co-located endpoint and service files, IFeature registration, route-id updates and per-entity activity.
nav_order: 2
---

# Anatomy of a vertical slice

`Item` is the worked example. **Copy it and rename throughout** when adding a feature. A slice

## Core side — `CRISP.Core/<Feature>/`

One record per contract, each marked `[ExportTsInterface]`, validators beside the command.

```csharp
// Item.cs — read model
[ExportTsInterface]
public sealed record Item : BaseAuditableModel
{
    public string Name { get; init; } = default!;
    public string? Description { get; init; }
    public string? Owner { get; init; }
    public bool IsActive { get; init; }
    public IReadOnlyCollection<StoredFile> Files { get; init; } = [];
}

// GetItem.cs — lookup by Id is a one-liner
public sealed record GetItem : SingularQuery<Item>;

// GetItems.cs — paged query adds only the feature-specific filters
[ExportTsInterface]
public sealed record GetItems : PagedQuery<Item>
{
    public string? Name { get; init; }
    public string? Owner { get; init; }
    public bool? IsActive { get; init; }
}

// CreateItem.cs — command + its AbstractValidator live in the same file
[ExportTsInterface]
public sealed record CreateItem : CreateCommand
{
    public string Name { get; init; } = default!;
    public string? Description { get; init; }
    public string? Owner { get; init; }
}
public sealed class CreateItemValidator : AbstractValidator<CreateItem>
{
    public CreateItemValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Item name is required.")
                            .MaximumLength(100).WithMessage("Item name must not exceed 100 characters.");
        RuleFor(x => x.Description).MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");
        RuleFor(x => x.Owner).MaximumLength(100).WithMessage("Owner must not exceed 100 characters.");
    }
}
```

`UpdateItem : ModifyCommand` carries the `Id` (from the base) and its validator requires it
(`RuleFor(x => x.Id).NotEmpty()`).

## Server side — `CRISP.Server/Features/<Feature>/`

**Endpoint and its handler service are co-located in one file.** The endpoint is a `static
class` with a `Map(this RouteGroupBuilder)` extension and a `HandleAsync`; the service is an
`internal sealed class` using primary-constructor DI, implementing the matching Core contract.

```csharp
public static class CreateItemEndpoint
{
    public static RouteGroupBuilder Map(this RouteGroupBuilder group)
    {
        group.MapPost("/", HandleAsync)
            .WithName(nameof(CreateItem))
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest);
        return group;
    }

    public static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        [FromRoute] string version,
        [FromBody] CreateItem command,
        [FromServices] ICreateService<CreateItem> service,   // direct injection — no mediator
        CancellationToken cancellationToken)
    {
        var id = await service.Send(command, cancellationToken);
        return Results.Created($"/api/v{version}/{nameof(Item).Pluralize()}/{id}", id);
    }
}

internal sealed class CreateItemService(ApplicationDbContext db, ILogger<CreateItemService> logger)
    : ICreateService<CreateItem>
{
    private readonly DbSet<ItemEntity> _items = db.Set<ItemEntity>();

    public async ValueTask<Guid> Send(CreateItem command, CancellationToken cancellationToken = default)
    {
        // FluentValidation already ran in the endpoint filter; this is *business* validation.
        if (await _items.AnyAsync(i => i.Name == command.Name, cancellationToken))
            throw RejectedException.Conflict($"Item with name '{command.Name}' already exists."); // → 409

        var entity = new ItemEntity { Name = command.Name, Description = command.Description, Owner = command.Owner, IsActive = true };
        _items.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Created item {Name} with ID {Id}.", entity.Name, entity.Id);
        return entity.Id;
    }
}
```

Endpoint conventions seen across the slice:
- Use primary-constructor DI on services; cache the set via `db.Set<T>()`.
- Bind read queries with `[AsParameters]`; command bodies with `[FromBody]`; the service with
  `[FromServices]` (or `[FromKeyedServices(...)]`, see delete below).
- Return `Results.Created/Ok/NoContent`; declare every outcome with `.Produces…` /
  `.ProducesProblem…` so OpenAPI is accurate.

## Feature registration — `IFeature`

Each slice has **one** `IFeature` that registers its services and maps its endpoints. Modules
are auto-discovered by reflection (`FeatureExtensions.AddFeatures` / `MapFeatures`), so a new
feature needs **no central wiring** — implementing `IFeature` is enough.

```csharp
public sealed class ItemFeature : IFeature
{
    public IServiceCollection AddFeature(IServiceCollection services)
    {
        services.AddScoped<ICreateService<CreateItem>, CreateItemService>();
        services.AddScoped<IModifyService<UpdateItem>, UpdateItemService>();
        services.AddScoped<IQueryService<GetItem, Item>, GetItemService>();
        services.AddScoped<IQueryService<GetItems, PagedResponse<Item>>, GetItemsService>();
        services.AddKeyedScoped<IModifyService<DeleteCommand>, DeleteItemService>(nameof(Items)); // keyed: DeleteCommand is shared
        services.AddScoped<IQueryService<GetItemOwnerNames, IEnumerable<AutoComplete>>, GetItemOwnerNamesService>();
        services.AddScoped<IEventHandler<ItemRenamedEvent>, ItemRenamedLogger>();                 // example event handler
        return services;
    }

    public IEndpointRouteBuilder MapFeature(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(nameof(Item).Pluralize()).WithTags(nameof(Item));
        var v1Authorized = group.HasApiVersion(new ApiVersion(1, 0)).RequireAuthorization();
        CreateItemEndpoint.Map(v1Authorized);
        UpdateItemEndpoint.Map(v1Authorized);
        DeleteItemEndpoint.Map(v1Authorized);
        GetItemEndpoint.Map(v1Authorized);
        GetItemsEndpoint.Map(v1Authorized);
        GetItemOwnerNamesEndpoint.Map(v1Authorized);
        return app;
    }
}
```

## The shared `DeleteCommand` (keyed registration)

`DeleteCommand` is reused by every feature, so its handler is registered **keyed** by feature
name and resolved with `[FromKeyedServices(...)]` at the endpoint:

```csharp
// registration
services.AddKeyedScoped<IModifyService<DeleteCommand>, DeleteItemService>(nameof(Items));

// endpoint
public static async Task<IResult> HandleAsync(
    [AsParameters] DeleteCommand command,
    [FromKeyedServices(nameof(Items))] IModifyService<DeleteCommand> service,
    CancellationToken cancellationToken)
{
    await service.Send(command, cancellationToken);
    return Results.NoContent();
}
```

Endpoints are mapped under the versioned group `/api/v{version}/items`, `RequireAuthorization`
by default.

## Updates take the id from the route

`UpdateItemEndpoint` maps `PUT /{id:guid}`. The handler binds `[FromRoute] Guid id` alongside the
body and sends `command with { Id = id }`; `RouteIdBinder` has already put the same value on the
command before validation ran (see validation-and-errors.md), so the body never has to repeat it.

## Per-entity activity

`GetActivityEndpoint.Map<ItemEntity>(group)` adds `GET /{Id}/activity`, the audit rows the
`AuditLogInterceptor` wrote for that entity, newest first. Every auditable feature maps it.
