# Validation & Error Handling

## Validation — FluentValidation only

- Validators are **FluentValidation** `AbstractValidator<T>`, defined in Core beside the
  command. There is **no** parallel domain `Validate() → ValidationResult` method.
- Use `BaseValidator<T>` (extends `AbstractValidator<T>`) only when a caller needs
  **property-level** validation via its `ValidateValue` helper (e.g. live form fields).
- Register from both assemblies at startup:

```csharp
builder.Services.AddValidatorsFromAssemblies([typeof(CreateCommand).Assembly, typeof(Program).Assembly]);
```

- Validation runs as an **endpoint filter applied at the route-group level** in
  `FeatureExtensions.MapFeatures` via `.WithValidation()`. The filter resolves `IValidator<T>`
  for each endpoint argument and returns `Results.ValidationProblem(errors)` (400 with
  per-property messages) on failure.
- **Handlers do not call validators.** By the time `Send` runs, shape/length rules have
  passed; the handler enforces only business rules that need the database (uniqueness,
  existence).

```csharp
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

## Error handling — throw, don't build responses

Handlers **throw**; a single `GlobalExceptionHandler` (`IExceptionHandler`) maps exceptions to
RFC 7807 `ProblemDetails`. Stack traces are exposed only in Development.

| Throw this | Becomes | `detail` returned outside Development? |
| --- | --- | --- |
| `RejectedException.BadRequest(msg)` | 400 Bad Request | **yes** — the message is written for the caller |
| `RejectedException.Forbidden(msg)` | 403 Forbidden | **yes** |
| `RejectedException.NotFound(msg)` | 404 Not Found | **yes** |
| `RejectedException.Conflict(msg)` | 409 Conflict | **yes** |
| `ArgumentException` | 400 | no |
| `UnauthorizedAccessException` | 403 | no |
| `KeyNotFoundException` | 404 | no |
| `InvalidOperationException` | 409 | no |
| `OperationCanceledException` | 499 Client Closed Request | no |
| anything else | 500 Internal Server Error | no |

`RejectedException` (`CRISP.Server.Common`) is the **one** exception type a handler throws on
purpose, and the only one whose message reaches the client in deployed environments. It is sealed,
has the four factories above, and has no hierarchy — do not subclass it per feature and do not
introduce `NotFoundException` / `ConflictException` types.

Why not just throw `KeyNotFoundException`? EF Core and LINQ throw `InvalidOperationException` and
friends with text that names entity types, key values and internals, and the handler cannot tell a
service's rejection from one of those by type alone. So framework exceptions still map to the
status they imply, but their message is withheld outside Development. A rejection the user should
read has to say so by being a `RejectedException`.

```csharp
var entity = await _items.FirstOrDefaultAsync(i => i.Id == command.Id, ct);
if (entity is null)
    throw RejectedException.NotFound($"Item with ID '{command.Id}' not found.");          // → 404

if (await _items.AnyAsync(i => i.Id != command.Id && i.Name == command.Name, ct))
    throw RejectedException.Conflict($"Item with name '{command.Name}' already exists."); // → 409
```

## Route ids are bound before validation

Minimal APIs bind the body and the route separately, so a command sent to `PUT /Items/{id}`
would arrive with `Id` empty and fail validation unless the caller repeated the id in the body.
`RouteIdBinder` (called by both `ValidationEndpointFilter` overloads) copies any `Guid` property
whose name matches a route value onto the bound argument **before** validation runs, and the route
wins over a disagreeing body value. Handlers still write `command with { Id = id }` so the
authority is visible at the call site.
