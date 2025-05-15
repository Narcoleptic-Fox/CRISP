---
applyTo: "**/*.razor,**/*.cshtml"
---
# Razor Components Guidelines

## Component Structure

- Keep components focused on a single responsibility.
- Break large components into smaller, reusable ones.
- Use layout components for shared UI structures.
- Separate business logic from UI concerns where possible.
- Consider partial classes for complex components to separate markup from logic.
- Group Parameters into a `#region`
- Group Injection into a `#region`

## Naming Conventions

- **Components**: Use PascalCase for component names (e.g., `Counter.razor`, `ProductList.razor`).
- **Parameters**: Use PascalCase for component parameters (e.g., `[Parameter] public int ProductId { get; set; }`).
- **Events**: Use PascalCase with "On" prefix for event callbacks (e.g., `[Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }`).
- **CSS Classes**: Use snake_case for CSS class names (e.g., `<div class="product_card">`).
- **Injection**: Use PascalCase for component injections (e.g, `[Inject] private int ProductId { get; set; } = default!`)

## Component Parameters

- Use the `[Parameter]` attribute for public properties that accept input.
- Use the `[CascadingParameter]` attribute for values passed down from ancestor components.
- Use `[Parameter] public EventCallback<T>` for component events.
- Consider using `[EditorRequired]` for required parameters.
- Add XML documentation to parameters for better IntelliSense support.

```csharp
/// <summary>
/// The unique identifier for the product.
/// </summary>
[Parameter, EditorRequired]
public int ProductId { get; set; }

/// <summary>
/// Called when the product is selected.
/// </summary>
[Parameter]
public EventCallback<int> OnSelected { get; set; }
```

## Lifecycle Methods

- Use `OnInitialized` or `OnInitializedAsync` for one-time initialization work.
- Use `OnParametersSet` or `OnParametersSetAsync` when parameter values change.
- Use `OnAfterRender` or `OnAfterRenderAsync` for interop with JavaScript.
- Avoid expensive operations in `OnParametersSet` that could degrade performance.
- Use the `firstRender` parameter in `OnAfterRender` to run code only on the first render.

```csharp
protected override async Task OnInitializedAsync()
{
    Products = await ProductService.GetProductsAsync();
}

protected override void OnParametersSet()
{
    // React to parameter changes
    if (OldValue != CurrentValue)
    {
        // Update component state
    }
}
```

## Event Handling

- Use lambda expressions for simple event handlers.
- Use method references for complex event handlers.
- Consider using `@bind` and `@bind-*` for two-way binding.
- Prefer `EventCallback<T>` over regular `Action<T>` delegates.

```razor
<button @onclick="() => Count++">Increment</button>
<button @onclick="HandleClick">Submit</button>

@code {
    private void HandleClick(MouseEventArgs e)
    {
        // Handle click event
    }
}
```

## Rendering

- Use `@if` for conditional rendering.
- Use `@foreach` for iteration.
- Use `RenderFragment` and `RenderFragment<T>` for template parameters.
- Avoid rendering large collections without virtualization.
- Use `@key` directive when rendering lists to maintain component state.

```razor
@if (IsVisible)
{
    <div>Visible content</div>
}

<ul>
    @foreach (var item in Items)
    {
        <li @key="item.Id">@item.Name</li>
    }
</ul>
```

## Forms and Validation

- Use `EditForm` and `DataAnnotationValidator` for form handling.
- Use data annotations for validation rules.
- Use form components like `InputText`, `InputSelect` for form fields.
- Create custom `InputBase<T>` components for specialized inputs.
- Use `ValidationSummary` and `ValidationMessage` to display validation errors.

```razor
<EditForm Model="@model" OnValidSubmit="HandleValidSubmit">
    <DataAnnotationsValidator />
    <ValidationSummary />
    
    <InputText @bind-Value="model.Name" />
    <ValidationMessage For="@(() => model.Name)" />
    
    <button type="submit">Submit</button>
</EditForm>
```

## State Management

- Use component state for local UI state.
- Use cascading parameters for shared state across a component tree.
- Consider using a state container pattern for application-wide state.
- Use services registered with DI for shared data and operations.
- Avoid static state except for application constants.

## Code-Behind and Partial Classes

- Use code-behind (`Component.razor.cs`) for complex components to separate UI from logic.
- Ensure code-behind files are properly namespaced and inherit from `ComponentBase`.
- Use partial classes with the same name as the component.

```csharp
// ProductList.razor.cs
using Microsoft.AspNetCore.Components;

namespace MyApp.Components;

public partial class ProductList : ComponentBase
{
    [Inject]
    private IProductService ProductService { get; set; } = default!;
    
    private List<Product> Products { get; set; } = new();
    
    protected override async Task OnInitializedAsync()
    {
        Products = await ProductService.GetProductsAsync();
    }
}
```

## Component Libraries

- Create reusable component libraries for shared UI elements.
- Use `@namespace` directive to control component namespaces.
- Consider using CSS isolation for component-specific styles.
- Use `@typeparam` for generic components.

## JavaScript Interop

- Use `IJSRuntime` for JavaScript interop.
- Create JavaScript modules for complex interop scenarios.
- Consider using `IJSObjectReference` for module-based interop.
- Dispose of JS references in `IAsyncDisposable.DisposeAsync`.

```razor
@inject IJSRuntime JS

<div @ref="elementRef">Content</div>

@code {
    private ElementReference elementRef;
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JS.InvokeVoidAsync("setupElement", elementRef);
        }
    }
}
```

## Performance Optimization

- Use `@key` directive to preserve component instances during rendering.
- Implement `ShouldRender()` to prevent unnecessary rendering.
- Use virtualization (`Virtualize` component) for large lists.
- Avoid creating new delegates in render fragments.
- Use lazy loading for complex components with `@DynamicComponent`.

```razor
@inherits ComponentBase

<div class="my-component">@ChildContent</div>

@code {
    private string? _previousValue;
    
    [Parameter]
    public string? Value { get; set; }
    
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
    
    protected override bool ShouldRender()
    {
        if (_previousValue != Value)
        {
            _previousValue = Value;
            return true;
        }
        
        return false;
    }
}
```

## Testing

- Use bUnit for component testing.
- Test component rendering, parameter binding, and event handling.
- Mock dependencies using Moq or NSubstitute.
- Test component lifecycle methods.

## Resources

- [Blazor Documentation](https://docs.microsoft.com/en-us/aspnet/core/blazor/)
- [ASP.NET Core Razor components](https://docs.microsoft.com/en-us/aspnet/core/mvc/views/razor)
- [Blazor University](https://blazor-university.com/)
- [bUnit Documentation](https://bunit.dev/)
