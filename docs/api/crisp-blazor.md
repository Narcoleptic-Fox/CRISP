# Crisp.Blazor API Reference

Blazor integration providing state management, reactive dispatchers, and UI-optimized components.

## Service Registration

### AddCrispBlazor Extension
**Namespace:** `Microsoft.Extensions.DependencyInjection`  
**Purpose:** Registers CRISP framework with Blazor-specific features

```csharp
public static IServiceCollection AddCrispBlazor(
    this IServiceCollection services, 
    Action<BlazorCrispBuilder>? configureBuilder = null)
```

**Basic Registration:**
```csharp
// Program.cs (Blazor Server)
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddCrispBlazor();

// Program.cs (Blazor WebAssembly)
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddCrispBlazor();
```

**Advanced Configuration:**
```csharp
builder.Services.AddCrispBlazor(crisp => 
{
    crisp.RegisterHandlersFromAssemblies(typeof(App).Assembly);
    crisp.ConfigureOptions(opt => 
    {
        opt.EnableLoadingState = true;
        opt.EnableStateUpdates = true;
        opt.EnableErrorBoundary = true;
        opt.EnableQueryCaching = true;
        opt.DefaultCacheDuration = TimeSpan.FromMinutes(5);
    });
    crisp.AddPipelineBehavior<LoadingStateBehavior<,>>();
    crisp.AddPipelineBehavior<StateUpdateBehavior<,>>();
});
```

## Dispatchers

### IBlazorCommandDispatcher
**Namespace:** `Crisp.Services`  
**Purpose:** Command dispatcher with UI state management

```csharp
public interface IBlazorCommandDispatcher : ICommandDispatcher
{
    Task<TResponse> SendWithLoadingState<TResponse>(
        ICommand<TResponse> command, 
        string? loadingKey = null, 
        CancellationToken cancellationToken = default);
        
    Task SendWithLoadingState(
        ICommand command, 
        string? loadingKey = null, 
        CancellationToken cancellationToken = default);
}
```

**Usage in Components:**
```razor
@page "/todos"
@inject IBlazorCommandDispatcher Commands
@inject ICrispState State

<button @onclick="CreateTodo" disabled="@State.IsLoading("create-todo")">
    @if (State.IsLoading("create-todo"))
    {
        <span class="spinner"></span> Creating...
    }
    else
    {
        Create Todo
    }
</button>

@code {
    private async Task CreateTodo()
    {
        try
        {
            var command = new CreateTodoCommand("New Todo", "Description");
            var todoId = await Commands.SendWithLoadingState(command, "create-todo");
            
            // UI automatically updates via state management
            NavigationManager.NavigateTo($"/todos/{todoId}");
        }
        catch (Exception ex)
        {
            // Error handling via state management
            var error = State.GetError("create-todo");
        }
    }
}
```

### IBlazorQueryDispatcher
**Namespace:** `Crisp.Services`  
**Purpose:** Query dispatcher with caching and loading state support

```csharp
public interface IBlazorQueryDispatcher : IQueryDispatcher
{
    Task<TResponse> SendWithCache<TResponse>(
        IQuery<TResponse> query, 
        TimeSpan? cacheDuration = null, 
        CancellationToken cancellationToken = default);
        
    Task<TResponse> SendWithLoadingState<TResponse>(
        IQuery<TResponse> query, 
        string? loadingKey = null, 
        CancellationToken cancellationToken = default);
        
    void InvalidateCache<TQuery, TResponse>() where TQuery : IQuery<TResponse>;
}
```

**Cached Queries:**
```razor
@page "/todos/{id:int}"
@inject IBlazorQueryDispatcher Queries
@implements IDisposable

@if (todo != null)
{
    <h2>@todo.Title</h2>
    <p>@todo.Description</p>
    
    <button @onclick="RefreshTodo">Refresh</button>
}

@code {
    [Parameter] public int Id { get; set; }
    private Todo? todo;
    
    protected override async Task OnInitializedAsync()
    {
        // Cached for 5 minutes by default
        todo = await Queries.SendWithCache(new GetTodoQuery(Id));
    }
    
    private async Task RefreshTodo()
    {
        // Clear cache and reload
        Queries.InvalidateCache<GetTodoQuery, Todo>();
        todo = await Queries.Send(new GetTodoQuery(Id));
        StateHasChanged();
    }
}
```

## State Management

### ICrispState
**Namespace:** `Crisp.State`  
**Purpose:** Centralized UI state management for CRISP operations

```csharp
public interface ICrispState
{
    bool IsAnyLoading { get; }
    event EventHandler? StateChanged;
    
    bool IsLoading(string key);
    void SetLoading(string key, bool isLoading);
    Exception? GetError(string key);
    void SetError(string key, Exception? error);
    void ClearErrors();
}
```

**Component Integration:**
```razor
@inject ICrispState State
@implements IDisposable

<div class="loading-overlay" style="display: @(State.IsAnyLoading ? "block" : "none")">
    <div class="spinner">Loading...</div>
</div>

<div class="error-messages">
    @foreach (var error in GetCurrentErrors())
    {
        <div class="alert alert-danger">
            @error.Message
            <button @onclick="() => ClearError(error)">×</button>
        </div>
    }
</div>

@code {
    protected override void OnInitialized()
    {
        State.StateChanged += OnStateChanged;
    }
    
    private void OnStateChanged(object? sender, EventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }
    
    private List<Exception> GetCurrentErrors()
    {
        // Get errors from state
        return new List<Exception>();
    }
    
    public void Dispose()
    {
        State.StateChanged -= OnStateChanged;
    }
}
```

### StateContainer
**Namespace:** `Crisp.State`  
**Purpose:** Container for managing component-specific state

```csharp
public class StateContainer
{
    public event EventHandler? StateChanged;
    
    public void SetProperty<T>(string key, T value);
    public T? GetProperty<T>(string key);
    public void ClearProperty(string key);
    public void NotifyStateChanged();
}
```

**Usage:**
```razor
@inject StateContainer StateContainer
@implements IDisposable

<input @bind="searchTerm" @oninput="OnSearchChanged" />

<ul>
    @foreach (var todo in filteredTodos)
    {
        <li>@todo.Title</li>
    }
</ul>

@code {
    private string searchTerm = "";
    private List<Todo> allTodos = new();
    private List<Todo> filteredTodos = new();
    
    protected override void OnInitialized()
    {
        StateContainer.StateChanged += OnStateChanged;
        
        // Restore previous search term
        searchTerm = StateContainer.GetProperty<string>("searchTerm") ?? "";
    }
    
    private void OnSearchChanged(ChangeEventArgs e)
    {
        searchTerm = e.Value?.ToString() ?? "";
        
        // Persist search term
        StateContainer.SetProperty("searchTerm", searchTerm);
        
        // Filter todos
        filteredTodos = allTodos
            .Where(t => t.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .ToList();
            
        StateContainer.NotifyStateChanged();
    }
    
    public void Dispose()
    {
        StateContainer.StateChanged -= OnStateChanged;
    }
}
```

## Notifications

### INotificationService
**Namespace:** `Crisp.Notifications`  
**Purpose:** Service for displaying notifications in Blazor applications

```csharp
public interface INotificationService
{
    event EventHandler<NotificationEventArgs>? NotificationRequested;
    
    void ShowSuccess(string message, string? title = null);
    void ShowError(string message, string? title = null);
    void ShowInfo(string message, string? title = null);
    void ShowWarning(string message, string? title = null);
}
```

**Service Registration:**
```csharp
builder.Services.AddCrispBlazor(crisp => 
{
    crisp.AddNotificationService();
});
```

**Notification Component:**
```razor
<!-- NotificationComponent.razor -->
@inject INotificationService NotificationService
@implements IDisposable

<div class="notification-container">
    @foreach (var notification in notifications)
    {
        <div class="notification notification-@notification.Type.ToString().ToLower()" 
             style="animation: slideIn 0.3s ease-out">
            @if (!string.IsNullOrEmpty(notification.Title))
            {
                <h4>@notification.Title</h4>
            }
            <p>@notification.Message</p>
            <button @onclick="() => RemoveNotification(notification)">×</button>
        </div>
    }
</div>

@code {
    private List<NotificationItem> notifications = new();
    
    protected override void OnInitialized()
    {
        NotificationService.NotificationRequested += OnNotificationRequested;
    }
    
    private void OnNotificationRequested(object? sender, NotificationEventArgs e)
    {
        var notification = new NotificationItem
        {
            Type = e.Type,
            Title = e.Title,
            Message = e.Message,
            Timestamp = DateTime.Now
        };
        
        notifications.Add(notification);
        InvokeAsync(StateHasChanged);
        
        // Auto-remove after 5 seconds
        Task.Delay(5000).ContinueWith(_ => 
        {
            RemoveNotification(notification);
        });
    }
    
    private void RemoveNotification(NotificationItem notification)
    {
        notifications.Remove(notification);
        InvokeAsync(StateHasChanged);
    }
    
    public void Dispose()
    {
        NotificationService.NotificationRequested -= OnNotificationRequested;
    }
}
```

**Usage in Components:**
```razor
@inject INotificationService Notifications
@inject IBlazorCommandDispatcher Commands

<button @onclick="CreateTodo">Create Todo</button>

@code {
    private async Task CreateTodo()
    {
        try
        {
            var command = new CreateTodoCommand("New Todo");
            var todoId = await Commands.Send(command);
            
            Notifications.ShowSuccess($"Todo created successfully! ID: {todoId}", "Success");
        }
        catch (ValidationException ex)
        {
            Notifications.ShowError(string.Join(", ", ex.ValidationResult.Errors.Select(e => e.ErrorMessage)), "Validation Error");
        }
        catch (Exception ex)
        {
            Notifications.ShowError("An unexpected error occurred. Please try again.", "Error");
        }
    }
}
```

### NotificationEventArgs
**Namespace:** `Crisp.Notifications`  
**Purpose:** Event arguments for notification requests

```csharp
public class NotificationEventArgs : EventArgs
{
    public NotificationType Type { get; init; }
    public string Message { get; init; } = "";
    public string? Title { get; init; }
}
```

### NotificationType
**Namespace:** `Crisp.Notifications`  
**Purpose:** Enumeration of notification types

```csharp
public enum NotificationType
{
    Success,
    Error,
    Info,
    Warning
}
```

## Pipeline Behaviors

### LoadingStateBehavior\<TRequest, TResponse\>
**Namespace:** `Crisp.Pipeline`  
**Purpose:** Automatically manages loading states for UI operations

```csharp
public class LoadingStateBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
```

**Features:**
- Automatic loading state management
- Component refresh on state changes
- Error state handling
- Configurable loading keys

**Configuration:**
```csharp
services.AddCrispBlazor(crisp => 
{
    crisp.ConfigureOptions(opt => 
    {
        opt.EnableLoadingState = true;
        opt.AutoGenerateLoadingKeys = true;
        opt.MinimumLoadingDuration = TimeSpan.FromMilliseconds(300);
    });
    crisp.AddPipelineBehavior<LoadingStateBehavior<,>>();
});
```

### StateUpdateBehavior\<TRequest, TResponse\>
**Namespace:** `Crisp.Pipeline`  
**Purpose:** Triggers UI updates when operations complete

```csharp
public class StateUpdateBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
```

**Features:**
- Automatic component re-rendering
- Selective update targeting
- Batch update optimization
- Error boundary integration

## Configuration

### CrispBlazorOptions
**Namespace:** `Crisp`  
**Purpose:** Configuration options for Blazor integration

```csharp
public class CrispBlazorOptions : CrispOptions
{
    public bool EnableLoadingState { get; set; } = true;
    public bool EnableStateUpdates { get; set; } = true;
    public bool EnableErrorBoundary { get; set; } = true;
    public bool EnableQueryCaching { get; set; } = false;
    public TimeSpan DefaultCacheDuration { get; set; } = TimeSpan.FromMinutes(5);
    public bool AutoGenerateLoadingKeys { get; set; } = true;
    public TimeSpan MinimumLoadingDuration { get; set; } = TimeSpan.FromMilliseconds(200);
    public int MaxCachedQueries { get; set; } = 100;
    public bool EnableNotifications { get; set; } = false;
}
```

**Complete Configuration:**
```csharp
builder.Services.AddCrispBlazor(crisp => 
{
    crisp.ConfigureOptions(opt => 
    {
        // UI State Management
        opt.EnableLoadingState = true;
        opt.EnableStateUpdates = true;
        opt.AutoGenerateLoadingKeys = true;
        opt.MinimumLoadingDuration = TimeSpan.FromMilliseconds(300);
        
        // Query Caching
        opt.EnableQueryCaching = true;
        opt.DefaultCacheDuration = TimeSpan.FromMinutes(10);
        opt.MaxCachedQueries = 50;
        
        // Error Handling
        opt.EnableErrorBoundary = true;
        opt.EnableNotifications = true;
        
        // Pipeline
        opt.Pipeline.EnableValidation = true;
        opt.Pipeline.EnableLogging = false; // Disable in browser
    });
    
    // Register handlers
    crisp.RegisterHandlersFromAssemblies(typeof(App).Assembly);
    
    // Add Blazor-specific behaviors
    crisp.AddPipelineBehavior<LoadingStateBehavior<,>>();
    crisp.AddPipelineBehavior<StateUpdateBehavior<,>>();
    crisp.AddPipelineBehavior<ValidationBehavior<,>>();
});
```

## Error Boundaries

### CrispErrorBoundary Component
**Purpose:** Catches and displays CRISP-related errors

```razor
<!-- CrispErrorBoundary.razor -->
@inherits ErrorBoundaryBase

<div class="error-boundary">
    @if (CurrentException != null)
    {
        <div class="error-content">
            <h3>Something went wrong</h3>
            
            @if (CurrentException is ValidationException validationEx)
            {
                <div class="validation-errors">
                    <h4>Validation Errors:</h4>
                    <ul>
                        @foreach (var error in validationEx.ValidationResult.Errors)
                        {
                            <li>@error.PropertyName: @error.ErrorMessage</li>
                        }
                    </ul>
                </div>
            }
            else if (CurrentException is NotFoundException notFoundEx)
            {
                <p>The requested @notFoundEx.ResourceType was not found.</p>
            }
            else
            {
                <p>An unexpected error occurred. Please try again.</p>
            }
            
            <button @onclick="Recover">Try Again</button>
        </div>
    }
    else
    {
        @ChildContent
    }
</div>

@code {
    [Parameter] public RenderFragment? ChildContent { get; set; }
    
    private void Recover()
    {
        CurrentException = null;
        StateHasChanged();
    }
}
```

**Usage:**
```razor
<CrispErrorBoundary>
    <TodoList />
</CrispErrorBoundary>
```

## Integration Examples

### Complete Todo Component
```razor
@page "/todos"
@inject IBlazorCommandDispatcher Commands
@inject IBlazorQueryDispatcher Queries
@inject ICrispState State
@inject INotificationService Notifications
@implements IDisposable

<PageTitle>Todos</PageTitle>

<h1>Todo List</h1>

<div class="todo-form">
    <input @bind="newTodoTitle" placeholder="Enter todo title..." 
           disabled="@State.IsLoading("create-todo")" />
    <button @onclick="CreateTodo" disabled="@State.IsLoading("create-todo")">
        @if (State.IsLoading("create-todo"))
        {
            <span class="spinner"></span>
        }
        Add Todo
    </button>
</div>

<div class="todo-list">
    @if (State.IsLoading("load-todos"))
    {
        <div class="loading">Loading todos...</div>
    }
    else if (todos?.Any() == true)
    {
        @foreach (var todo in todos)
        {
            <div class="todo-item @(todo.IsCompleted ? "completed" : "")">
                <input type="checkbox" @bind="todo.IsCompleted" 
                       @onchange="() => UpdateTodo(todo)" />
                <span>@todo.Title</span>
                <button @onclick="() => DeleteTodo(todo.Id)" 
                        disabled="@State.IsLoading($"delete-{todo.Id}")">
                    @if (State.IsLoading($"delete-{todo.Id}"))
                    {
                        <span class="spinner"></span>
                    }
                    else
                    {
                        Delete
                    }
                </button>
            </div>
        }
    }
    else
    {
        <p>No todos found. Add one above!</p>
    }
</div>

@code {
    private List<Todo>? todos;
    private string newTodoTitle = "";
    
    protected override async Task OnInitializedAsync()
    {
        State.StateChanged += OnStateChanged;
        await LoadTodos();
    }
    
    private async Task LoadTodos()
    {
        try
        {
            todos = await Queries.SendWithLoadingState(new GetAllTodosQuery(), "load-todos");
        }
        catch (Exception ex)
        {
            Notifications.ShowError("Failed to load todos. Please try again.", "Error");
        }
    }
    
    private async Task CreateTodo()
    {
        if (string.IsNullOrWhiteSpace(newTodoTitle))
        {
            Notifications.ShowWarning("Please enter a todo title.", "Validation");
            return;
        }
        
        try
        {
            var command = new CreateTodoCommand(newTodoTitle.Trim());
            var todoId = await Commands.SendWithLoadingState(command, "create-todo");
            
            newTodoTitle = "";
            await LoadTodos(); // Refresh list
            
            Notifications.ShowSuccess("Todo created successfully!", "Success");
        }
        catch (ValidationException ex)
        {
            var errors = string.Join(", ", ex.ValidationResult.Errors.Select(e => e.ErrorMessage));
            Notifications.ShowError(errors, "Validation Error");
        }
        catch (Exception ex)
        {
            Notifications.ShowError("Failed to create todo. Please try again.", "Error");
        }
    }
    
    private async Task UpdateTodo(Todo todo)
    {
        try
        {
            var command = new UpdateTodoCommand(todo.Id, todo.Title, todo.IsCompleted);
            await Commands.Send(command);
            
            Notifications.ShowSuccess("Todo updated!", "Success");
        }
        catch (Exception ex)
        {
            Notifications.ShowError("Failed to update todo.", "Error");
            await LoadTodos(); // Revert changes
        }
    }
    
    private async Task DeleteTodo(int todoId)
    {
        try
        {
            var command = new DeleteTodoCommand(todoId);
            await Commands.SendWithLoadingState(command, $"delete-{todoId}");
            
            await LoadTodos(); // Refresh list
            
            Notifications.ShowSuccess("Todo deleted!", "Success");
        }
        catch (Exception ex)
        {
            Notifications.ShowError("Failed to delete todo.", "Error");
        }
    }
    
    private void OnStateChanged(object? sender, EventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }
    
    public void Dispose()
    {
        State.StateChanged -= OnStateChanged;
    }
}
```

This comprehensive API reference covers all major components of the Blazor integration, providing developers with the tools needed to build reactive, state-managed Blazor applications using the CRISP framework.