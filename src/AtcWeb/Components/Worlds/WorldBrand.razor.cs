namespace AtcWeb.Components.Worlds;

/// <summary>
/// The top-bar logo. Apps use the atc-net-apps logo, everything else the ATC-NET logo.
/// </summary>
public sealed partial class WorldBrand : ComponentBase, IDisposable
{
    private SiteWorld world;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    private string LogoSrc
        => world == SiteWorld.AtcApps
            ? "/images/atc-apps-logo.png"
            : "/images/atc-logo.svg";

    private string AriaLabel
        => world switch
        {
            SiteWorld.AtcApps => "ATC-NET Apps, home",
            SiteWorld.Libraries => "ATC-NET Libraries, home",
            _ => "ATC-NET, home",
        };

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
}