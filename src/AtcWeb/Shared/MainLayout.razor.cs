// ReSharper disable InvertIf
// ReSharper disable SwitchStatementMissingSomeEnumCasesNoDefault
namespace AtcWeb.Shared;

public partial class MainLayout : LayoutComponentBase
{
    private bool drawerOpen;
    private MudThemeProvider mudThemeProviderRef;

    [Inject]
    private StateContainer StateContainer { get; set; }

    private void DrawerToggle()
    {
        drawerOpen = !drawerOpen;
    }

    protected override void OnInitialized()
    {
        drawerOpen = true;

        base.OnInitialized();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            StateContainer.UseDarkMode(await mudThemeProviderRef.GetSystemDarkModeAsync());
            StateHasChanged();
        }

        await base.OnAfterRenderAsync(firstRender);
    }
}