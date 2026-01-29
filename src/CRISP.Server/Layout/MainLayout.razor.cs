using CRISP.Server.Theme;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace CRISP.Server.Layout;

public partial class MainLayout
{
    private bool _isTransitioning = false;
    private bool _drawerOpen = true;
    private bool _isDarkMode = false;
    private AuthenticationState? _authState;
    private MudTheme _theme = new ServerTheme();
    private ErrorBoundary _errorBoundary = default!;

    [Inject]
    private ILogger<MainLayout> Logger { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IRenderContext RenderContext { get; set; } = default!;

    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    protected override void OnInitialized()
    {
        AuthenticationStateProvider.AuthenticationStateChanged += AuthStateChanged;
        NavigationManager.LocationChanged += OnLocationChanged;
    }

    protected override void OnParametersSet()
    {
        _errorBoundary?.Recover();
    }

    private async void AuthStateChanged(Task<AuthenticationState> task)
    {
        Logger.LogTrace("AuthenticationState changed");
        _authState = await task;
        await InvokeAsync(StateHasChanged);
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        if (RenderContext.IsPreRendering(AssignedRenderMode))
        {
            _isTransitioning = true;
            InvokeAsync(async () =>
            {
                StateHasChanged();
                await Task.Delay(100);
                _isTransitioning = false;
                StateHasChanged();
            });
        }
    }

    private void ToggleDrawer()
    {
        _drawerOpen = !_drawerOpen;
    }

    private void BreakPointChanged(Breakpoint breakpoint)
    {
        Logger.LogTrace("Breakpoint changed to {Breakpoint}", breakpoint);
        _drawerOpen = breakpoint > Breakpoint.Sm;
    }
}
