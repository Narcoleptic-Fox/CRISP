# Models vs Entities

CRISP keeps **two types per concept** and maps explicitly between them — intentional, not
redundant:

- **Core model** (`Item`): immutable `record`, infrastructure-free, exported to TS, crosses
  the API boundary.
- **Server entity** (`ItemEntity`): mutable `class`, EF-mapped, carries persistence-only
  state (soft-delete, archival, stored-file value objects).

```csharp
public abstract class BaseEntity { public Guid Id { get; set; } }

public abstract class BaseAuditableEntity : BaseEntity, IAchievable, ISoftDelete
{
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime UpdatedOn { get; set; } = DateTime.UtcNow;
    public string UpdatedBy { get; set; } = string.Empty;
    public bool IsArchived { get; set; }
    public string? ArchivingReason { get; set; }
    public bool IsDeleted { get; set; }
}

public class ItemEntity : BaseAuditableEntity, IHasStoredFiles
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? Owner { get; set; }
    public bool IsActive { get; set; }
    public List<StoredFileObject> Files { get; set; } = [];

    // Single-entity mapping (includes files)
    public Item ToItem() => new() { Id = Id, Name = Name, /* … */ Files = [.. Files.Select(f => f.ToStoredFile())] };
}

public static class ItemExtensions
{
    extension(IQueryable<ItemEntity> entities)
    {
        // Queryable projection for list queries (files intentionally omitted — load per-item when needed)
        public IQueryable<Item> ProjectToItem() => entities.Select(e => new Item { Id = e.Id, Name = e.Name, /* … */ });
    }
}
```

**Mapping is manual and explicit — no AutoMapper.** Use `ToItem()` for a single hydrated
entity and `ProjectToItem()` for `IQueryable` list projections so EF translates them to SQL.

## Interceptors own audit + soft-delete

Handlers **never** set `CreatedBy` / `UpdatedOn` / `IsDeleted` / write `SystemMessage` rows by
hand. Two `SaveChangesInterceptor`s do it automatically:

- **`AuditLogInterceptor`** — on `SavingChanges`, stamps `CreatedOn/By` + `UpdatedOn/By` from
  the current user (`IHttpContextAccessor`, falling back to `"System"`), and writes a
  `SystemMessageEntity` audit-log row describing the Added/Modified/Deleted/Archived/Restored
  change (with per-property diffs for modifications).
- **`SoftDeleteInterceptor`** — converts an EF `Remove` into a soft delete (sets `IsDeleted`).
  A global query filter then hides soft-deleted rows from normal queries.

So a delete handler just calls `_items.Remove(entity)` + `SaveChangesAsync`; the row is
flagged, not physically removed, and disappears from subsequent reads.

## EF configuration

Each entity has an `IEntityTypeConfiguration<T>` (here via a `BaseAuditableConfiguration<T>`
base) under `Data/Configurations/`. The Item config shows the conventions: table name from
`Pluralize()`, `IsRequired`/`HasMaxLength`, a **unique** index on `Name`, a plain index on
`Owner`, and an **owned collection** for attached files in a separate `ItemFiles` table.

```csharp
public class ItemConfiguration : BaseAuditableConfiguration<ItemEntity>
{
    public override void Configure(EntityTypeBuilder<ItemEntity> builder)
    {
        base.Configure(builder);
        builder.ToTable(nameof(Item).Pluralize());
        builder.Property(i => i.Name).IsRequired().HasMaxLength(100);
        builder.Property(i => i.Description).HasMaxLength(1000);
        builder.Property(i => i.Owner).HasMaxLength(100);
        builder.Property(i => i.IsActive).HasDefaultValue(true);
        builder.HasIndex(i => i.Name).IsUnique();
        builder.HasIndex(i => i.Owner);

        builder.OwnsMany(i => i.Files, fileBuilder =>
        {
            fileBuilder.ToTable("ItemFiles");
            fileBuilder.WithOwner().HasForeignKey("ItemId");
            fileBuilder.HasKey(f => f.Id);
            fileBuilder.Property(f => f.Id).ValueGeneratedNever();
            fileBuilder.Property(f => f.ObjectKey)
                .IsRequired().HasMaxLength(1024)
                .HasColumnType("varchar(1024)").HasCharSet("latin1");   // charset matters for index length on MySQL
            fileBuilder.HasIndex("ItemId", nameof(StoredFileObject.ObjectKey)).IsUnique();
        });
    }
}
```

> MySQL index-length trap: long indexed string columns may need an explicit `latin1` charset
> (as on `ObjectKey`) to stay under the index byte limit.

## Paged query handler

```csharp
internal sealed class GetItemsService(ApplicationDbContext db) : IQueryService<GetItems, PagedResponse<Item>>
{
    private readonly DbSet<ItemEntity> _items = db.Set<ItemEntity>();

    public async ValueTask<PagedResponse<Item>> Send(GetItems query, CancellationToken ct = default)
    {
        var entities = from e in _items.AsNoTracking().SortBy(query.SortBy, query.SortDescending)
                       where query.Ids == null || !query.Ids.Any() || query.Ids.Contains(e.Id)
                       where query.Name == null || e.Name.Contains(query.Name)
                       where query.Owner == null || (e.Owner != null && e.Owner.Contains(query.Owner))
                       where query.IsActive == null || e.IsActive == query.IsActive
                       select e;
        var totalCount = await entities.CountAsync(ct);
        var items = await entities.ProjectToItem().ToPaged(query.Page, query.PageSize).ToListAsync(ct);
        return new PagedResponse<Item> { Items = items, TotalCount = totalCount };
    }
}
```

`SortBy` and `ToPaged` are shared `IQueryable` extensions in `Core/Extensions`. List queries
use `AsNoTracking()`.

Source: `src/CRISP.Server/Data/Entities/ItemEntity.cs`, `src/CRISP.Server/Data/Configurations/ItemConfiguration.cs`.
