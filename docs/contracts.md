# Core Contracts (CQRS Primitives)

All live in `CRISP.Core/Common`. These are the real interface names — use them, do not
invent `ICreateEndpoint<T>` or similar. Feature records inherit from these bases.

## Commands (writes)

```csharp
// Marker interfaces
public interface ICommand;                       // command with no response
public interface ICommand<out TResponse>;        // command that returns data

// Service contracts (the handler side)
public interface ICommandService<TCommand> where TCommand : ICommand
{
    ValueTask Send(TCommand command, CancellationToken cancellationToken = default);
}
public interface ICommandService<TCommand, TResponse> where TCommand : ICommand<TResponse>
{
    ValueTask<TResponse> Send(TCommand command, CancellationToken cancellationToken = default);
}

// Base command records features inherit from
public abstract record CreateCommand : ICommand<Guid>;          // create → returns new Id
public abstract record ModifyCommand : ICommand                 // update/delete/archive against an existing Id
{
    public ModifyCommand() { }
    public ModifyCommand(Guid id) => Id = id;
    public Guid Id { get; set; }
}
public sealed record DeleteCommand(Guid Id) : ModifyCommand(Id);
public sealed record ArchiveCommand(Guid Id, string ArchiveReason) : ModifyCommand(Id);

// Convenience handler aliases used in feature registration
public interface ICreateService<TCreate> : ICommandService<TCreate, Guid> where TCreate : CreateCommand;
public interface IModifyService<TModify> : ICommandService<TModify> where TModify : ModifyCommand;
```

## Queries (reads)

```csharp
public interface IQuery<TResponse> where TResponse : class;

public abstract record SingularQuery<TResponse> : IQuery<TResponse>      // lookup by Id
    where TResponse : BaseModel
{
    public SingularQuery() { }
    public SingularQuery(Guid id) => Id = id;
    public Guid Id { get; init; }
}

public abstract record PagedQuery<TResponse> : IQuery<PagedResponse<TResponse>>   // paged list
    where TResponse : BaseModel
{
    public int? Page { get; set; } = Query.Page;             // defaults from Constants.Query
    public int? PageSize { get; set; } = Query.PageSize;     // 0 / 100
    public string? SortBy { get; set; } = Query.SortBy;
    public bool? SortDescending { get; set; } = Query.SortDescending;
    public IEnumerable<Guid>? Ids { get; set; }
    public int GetPageOrDefault() => Page ?? Query.Page;
    public int GetPageSizeOrDefault() => PageSize ?? Query.PageSize;
    public virtual string ToQueryString() { /* builds ?page=…&pageSize=… */ }
}

public interface IQueryService<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
    where TResponse : class
{
    ValueTask<TResponse> Send(TQuery query, CancellationToken cancellationToken = default);
}

public sealed record PagedResponse<TModel> where TModel : BaseModel
{
    public IEnumerable<TModel> Items { get; init; } = [];
    public int TotalCount { get; init; }
}
```

Not every read is `BaseModel`-typed. A query may return any reference type — e.g. the
autocomplete endpoint is `IQuery<IEnumerable<AutoComplete>>` (see `GetItemOwnerNames`).

## Models

```csharp
[ExportTsInterface]
public abstract record BaseModel { public Guid Id { get; init; } }

[ExportTsInterface]
public abstract record BaseAuditableModel : BaseModel
{
    public DateTime CreatedOn { get; init; } = DateTime.UtcNow;
    public string CreatedBy { get; init; } = default!;
    public DateTime UpdatedOn { get; init; } = DateTime.UtcNow;
    public string UpdatedBy { get; init; } = default!;
}
```

`AutoComplete` (`{ Guid Id; string Name }`) is the shared shape for free-text suggestion
endpoints. When the suggestion has no backing entity (free-text owner names), `Id` is
`Guid.Empty` and the client keys by `Name`.

## Query defaults (`Constants.Query`)

| Setting | Default |
| --- | --- |
| Page | `0` |
| Page size | `100` |
| Sort by | `Id` |
| Sort descending | `false` |

