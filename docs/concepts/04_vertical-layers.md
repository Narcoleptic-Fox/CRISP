# Vertical Layers in CRISP

Organize by feature, not by file type. Each feature is self-contained and independent.

## 🎯 What Are Vertical Layers?

Traditional architectures organize by technical concerns:
```
src/
├── Controllers/
├── Services/
├── Repositories/
├── Models/
└── Validators/
```

CRISP organizes by **features**:
```
src/
├── Features/
│   ├── Users/
│   ├── Projects/
│   ├── Billing/
│   └── Reports/
└── Shared/
```

Each feature contains **everything** it needs.

## 📁 Feature Structure

```
Features/
└── Projects/
    ├── Commands/
    │   ├── CreateProject.cs
    │   ├── UpdateProject.cs
    │   └── DeleteProject.cs
    ├── Queries/
    │   ├── GetProject.cs
    │   └── ListProjects.cs
    ├── Models/
    │   ├── Project.cs
    │   └── ProjectStatus.cs
    ├── Validators/
    │   └── ProjectValidator.cs
    ├── Events/
    │   └── ProjectCreated.cs
    └── ProjectModule.cs    // Wire-up
```

## 🔧 Module Registration

Each feature registers itself:

```csharp
public class ProjectModule : IModule
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Register handlers
        services.AddScoped<ICommandHandler<CreateProjectCommand, Project>, CreateProjectHandler>();
        services.AddScoped<IQueryHandler<GetProjectQuery, Project>, GetProjectHandler>();
        
        // Register validators
        services.AddScoped<IValidator<CreateProjectCommand>, CreateProjectValidator>();
        
        // Register feature-specific services
        services.AddScoped<IProjectNotificationService, ProjectNotificationService>();
    }
    
    public void ConfigureEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/projects")
            .WithTags("Projects");
            
        group.MapPost("/", CreateProject);
        group.MapGet("/{id}", GetProject);
        group.MapPut("/{id}", UpdateProject);
    }
}
```

## 💡 Benefits

### 1. **Feature Independence**
```csharp
// Moving a feature? Just move the folder!
Features/Projects/ → Microservice/Projects/
```

### 2. **Team Scalability**
```
Team A → Features/Billing/
Team B → Features/Projects/
Team C → Features/Reports/
// No merge conflicts!
```

### 3. **Clear Dependencies**
```csharp
// Projects feature needs users? Make it explicit:
public class CreateProjectHandler
{
    private readonly IUserQuery _userQuery; // From Users feature
    
    public async Task<Project> Handle(CreateProjectCommand command)
    {
        var user = await _userQuery.GetUser(command.UserId);
        // Create project...
    }
}
```

## 🎮 Real Example: Game Features

```
Features/
├── Players/
│   ├── Commands/
│   │   ├── CreatePlayer.cs
│   │   ├── MovePlayer.cs
│   │   └── LevelUp.cs
│   ├── Queries/
│   │   └── GetPlayerStats.cs
│   └── Models/
│       └── Player.cs
├── Combat/
│   ├── Commands/
│   │   ├── Attack.cs
│   │   └── Defend.cs
│   ├── Models/
│   │   └── CombatResult.cs
│   └── CombatModule.cs
└── Inventory/
    ├── Commands/
    │   ├── AddItem.cs
    │   └── UseItem.cs
    └── Models/
        └── Item.cs
```

## 🔄 Cross-Feature Communication

### Option 1: Direct Query (Simple)
```csharp
// In Billing feature
public class CreateInvoiceHandler
{
    private readonly IQueryDispatcher _queries;
    
    public async Task<Invoice> Handle(CreateInvoiceCommand command)
    {
        // Query the Projects feature
        var project = await _queries.Send(new GetProjectQuery(command.ProjectId));
        
        // Create invoice...
    }
}
```

### Option 2: Events (Decoupled)
```csharp
// Projects feature publishes
public class CreateProjectHandler
{
    private readonly IEventBus _events;
    
    public async Task<Project> Handle(CreateProjectCommand command)
    {
        var project = CreateProject(command);
        
        // Notify other features
        await _events.Publish(new ProjectCreatedEvent(project));
        
        return project;
    }
}

// Billing feature subscribes
public class ProjectEventHandler : IEventHandler<ProjectCreatedEvent>
{
    public Task Handle(ProjectCreatedEvent e)
    {
        // React to project creation
    }
}
```

### Option 3: Shared Interfaces
```csharp
// In Shared/
public interface IProjectQuery
{
    Task<ProjectSummary> GetProjectSummary(int projectId);
}

// Projects feature implements
public class ProjectQuery : IProjectQuery { }

// Other features consume
public class ReportGenerator
{
    private readonly IProjectQuery _projects;
}
```

## 📊 Organizing Shared Code

```
src/
├── Features/
│   └── [Feature folders]
├── Shared/
│   ├── Models/       // Shared DTOs
│   ├── Interfaces/   // Cross-feature contracts
│   ├── Behaviors/    // Common pipeline behaviors
│   └── Extensions/   // Helper methods
└── Infrastructure/
    ├── Database/
    ├── Email/
    └── Storage/
```

## 🚀 Scaling Strategies

### Small App (1-10 features)
```
TodoApp/
└── Features/
    ├── Todos/
    └── Users/
```

### Medium App (10-50 features)
```
CRM/
└── Features/
    ├── Core/
    │   ├── Users/
    │   └── Auth/
    ├── Sales/
    │   ├── Leads/
    │   └── Opportunities/
    └── Support/
        ├── Tickets/
        └── Knowledge/
```

### Large App (50+ features)
```
Enterprise/
├── Modules/           // Bounded contexts
│   ├── Sales/
│   │   └── Features/
│   ├── Inventory/
│   │   └── Features/
│   └── Accounting/
│       └── Features/
└── Shared/
```

## ✅ Best Practices

1. **Keep Features Small** - If a feature gets too big, split it
2. **Minimize Cross-Feature Calls** - Features should be independent
3. **Use Events for Side Effects** - Don't create tight coupling
4. **Shared Models Are OK** - But keep them minimal
5. **Test Features in Isolation** - Each feature should be testable alone

## 🎯 When to Split Features

Split when:
- Different teams work on them
- Different deployment cycles
- Different scaling needs  
- Different business domains

Keep together when:
- Tight business coupling
- Always change together
- Share complex state

## Example: E-commerce Layers

```csharp
// Features/Catalog/
public record Product(int Id, string Name, decimal Price);
public record GetProductQuery(int Id) : IQuery<Product>;

// Features/Cart/
public record AddToCartCommand(int ProductId, int Quantity) : ICommand<CartItem>;

// Features/Orders/  
public record CheckoutCommand(int CartId) : ICommand<Order>;

// Each feature is independent but they work together!
```

Next: [State Machines](state-machines.md) →