# Endpoints in CRISP

CRISP automatically generates HTTP endpoints from your commands and queries. No controllers needed!

## 🎯 Convention Over Configuration

CRISP maps commands and queries to endpoints using sensible defaults:

```csharp
// This command...
public record CreateUserCommand(string Email) : ICommand<User>;

// Automatically becomes...
POST /api/users/create
```

## 📍 Endpoint Conventions

| Pattern          | HTTP Method | Route                         | Example                         |
| ---------------- | ----------- | ----------------------------- | ------------------------------- |
| `Create*Command` | POST        | `/api/{resource}/create`      | POST /api/users/create          |
| `Update*Command` | PUT         | `/api/{resource}/update`      | PUT /api/users/update           |
| `Delete*Command` | DELETE      | `/api/{resource}/delete/{id}` | DELETE /api/users/delete/123    |
| `Get*Query`      | GET         | `/api/{resource}/get`         | GET /api/users/get?id=123       |
| `List*Query`     | GET         | `/api/{resource}/list`        | GET /api/users/list             |
| `Search*Query`   | GET         | `/api/{resource}/search`      | GET /api/users/search?term=john |

## 🔧 Basic Configuration

```csharp
// Program.cs
app.MapCrisp(); // Uses all defaults

// Or customize
app.MapCrisp(options =>
{
    options.WithPrefix("api/v1");
    options.RequireAuthorization();
    options.WithOpenApi();
});
```

## 📝 Customizing Endpoints

### 1. Route Attributes
```csharp
[Route("api/auth/register")]
[HttpPost]
public record RegisterUserCommand(string Email, string Password) : ICommand<AuthResult>;

[Route("api/products/{id:int}")]
[HttpGet]
public record GetProductQuery(int Id) : IQuery<Product>;
```

### 2. HTTP Method Override
```csharp
[HttpPatch] // Use PATCH instead of PUT
public record PartialUpdateCommand(int Id, JsonPatchDocument<User> Patch) : ICommand<User>;
```

### 3. Custom Endpoint Configuration
```csharp
public class CreateUserEndpoint : IEndpoint<CreateUserCommand>
{
    public static void Configure(RouteHandlerBuilder builder)
    {
        builder
            .WithName("CreateUser")
            .WithSummary("Creates a new user")
            .WithDescription("Creates a user with the specified email")
            .Produces<User>(201)
            .ProducesValidationProblem()
            .RequireAuthorization("Admin");
    }
}
```

## 🎨 Response Formatting

CRISP automatically handles different response scenarios:

### Success Responses
```csharp
// Command returns entity → 201 Created
public record CreateTodoCommand(string Title) : ICommand<Todo>;
// POST /api/todos/create → 201 + Location header

// Query returns data → 200 OK  
public record GetTodoQuery(int Id) : IQuery<Todo>;
// GET /api/todos/get?id=1 → 200 + JSON

// Command returns nothing → 204 No Content
public record DeleteTodoCommand(int Id) : ICommand;
// DELETE /api/todos/delete/1 → 204
```

### Error Responses
```csharp
// Validation failures → 400 Bad Request
{
    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    "title": "Validation failed",
    "status": 400,
    "errors": {
        "Email": ["Email is required", "Email is invalid"]
    }
}

// Not found → 404
public class GetUserHandler : IQueryHandler<GetUserQuery, User>
{
    public Task<User> Handle(GetUserQuery query, CancellationToken ct)
    {
        var user = await _db.Users.FindAsync(query.Id);
        if (user == null)
            throw new NotFoundException($"User {query.Id} not found");
        return user;
    }
}
```

## 🔒 Security

### Authorization
```csharp
// Global authorization
app.MapCrisp(o => o.RequireAuthorization());

// Per-command authorization
[Authorize(Roles = "Admin")]
public record DeleteUserCommand(int Id) : ICommand;

// Policy-based
[Authorize(Policy = "CanEditUsers")]
public record UpdateUserCommand(int Id, string Name) : ICommand<User>;
```

### API Keys
```csharp
app.MapCrisp(options =>
{
    options.RequireApiKey(key => key.FromHeader("X-API-Key"));
});
```

## 📊 OpenAPI/Swagger Integration

CRISP automatically generates OpenAPI documentation:

```csharp
// Automatic documentation from command/query properties
public record CreateProductCommand(
    [property: JsonPropertyName("name")]
    [property: Description("Product name (max 100 chars)")]
    string Name,
    
    [property: JsonPropertyName("price")]
    [property: Description("Price in USD")]
    [property: Range(0.01, 999999.99)]
    decimal Price
) : ICommand<Product>;
```

Generated OpenAPI:
```yaml
/api/products/create:
  post:
    summary: Create Product
    requestBody:
      content:
        application/json:
          schema:
            type: object
            properties:
              name:
                type: string
                description: Product name (max 100 chars)
                maxLength: 100
              price:
                type: number
                description: Price in USD
                minimum: 0.01
                maximum: 999999.99
```

## 🎮 Advanced Patterns

### 1. File Uploads
```csharp
[Consumes("multipart/form-data")]
public record UploadAvatarCommand(IFormFile File, int UserId) : ICommand<string>;
// POST /api/users/upload-avatar
```

### 2. Streaming Responses
```csharp
public record StreamEventsQuery(DateTime Since) : IStreamQuery<Event>;

// Endpoint automatically uses Server-Sent Events
// GET /api/events/stream?since=2024-01-01
```

### 3. Batch Operations
```csharp
public record BatchCreateCommand<T>(List<T> Items) : ICommand<BatchResult<T>>;

// POST /api/users/batch-create
// Body: { "items": [{...}, {...}] }
```

### 4. Versioning
```csharp
// V1
[ApiVersion("1.0")]
public record GetUserQuery(int Id) : IQuery<UserV1>;

// V2 with more fields
[ApiVersion("2.0")]
public record GetUserQueryV2(int Id) : IQuery<UserV2>;

// Routes:
// GET /api/v1/users/get?id=1
// GET /api/v2/users/get?id=1
```

## 🛠️ Custom Endpoint Handlers

For complex scenarios, implement custom endpoint logic:

```csharp
public class FileDownloadEndpoint : IEndpoint<DownloadFileQuery>
{
    public static async Task<IResult> Handle(
        DownloadFileQuery query,
        IQueryDispatcher dispatcher,
        CancellationToken ct)
    {
        var file = await dispatcher.Send(query, ct);
        return Results.File(file.Content, file.MimeType, file.Name);
    }
}
```

## 📱 Content Negotiation

CRISP supports multiple response formats:

```csharp
// Accept: application/json → JSON response
// Accept: application/xml → XML response  
// Accept: text/csv → CSV response

public class CsvFormatter : IResponseFormatter
{
    public bool CanFormat(Type type) => type.IsAssignableTo(typeof(IEnumerable));
    
    public Task FormatAsync(HttpContext context, object response)
    {
        // Convert to CSV and write to response
    }
}
```

## 🔍 Endpoint Discovery

List all CRISP endpoints:

```csharp
if (app.Environment.IsDevelopment())
{
    app.MapGet("/api/endpoints", (IEndpointDiscovery discovery) =>
        discovery.GetAllEndpoints());
}
```

Output:
```json
[
  {
    "path": "/api/users/create",
    "method": "POST",
    "command": "CreateUserCommand",
    "authorization": ["Admin"]
  }
]
```

## ✨ Best Practices

1. **Use conventions** - Let CRISP generate routes
2. **Keep endpoints thin** - Logic belongs in handlers
3. **Leverage attributes** - For OpenAPI documentation
4. **Standard responses** - Use Problem Details for errors
5. **Version carefully** - Only when breaking changes

## 🎯 Why No Controllers?

Controllers in traditional MVC:
- Mix HTTP concerns with business logic
- Require manual route configuration
- Lead to fat controllers
- Create unnecessary abstraction

CRISP endpoints:
- HTTP handling is automatic
- Business logic stays in handlers
- Routes follow conventions
- Direct command → handler flow

Next: [Flow Diagrams](05-flow-diagrams.md) →