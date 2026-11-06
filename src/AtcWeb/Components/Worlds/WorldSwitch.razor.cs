namespace AtcWeb.Components.Worlds;

public sealed partial class WorldSwitch : ComponentBase, IDisposable
{
    private SiteWorld world;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    public void Dispose()
        => NavigationManager.LocationChanged -= OnLocationChanged;

    protected override void OnInitialized()
    {
        world = CurrentWorld();
        NavigationManager.LocationChanged += OnLocationChanged;
    }

    private SiteWorld CurrentWorld()
        => SiteWorldHelper.FromRelativePath(NavigationManager.ToBaseRelativePath(NavigationManager.Uri));

    private void OnLocationChanged(
        object? sender,
        Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs e)
    {
        world = CurrentWorld();
        _ = InvokeAsync(StateHasChanged);
    }

    private string SegmentClass(SiteWorld segment)
        => segment != world
            ? "world-switch__seg"
            : segment == SiteWorld.AtcApps
                ? "world-switch__seg world-switch__seg--active-apps"
                : "world-switch__seg world-switch__seg--active-components";
}