// ReSharper disable InvertIf
// ReSharper disable SwitchStatementMissingSomeEnumCasesNoDefault
namespace AtcWeb.Shared;

public partial class MainLayout : LayoutComponentBase
{
    private bool drawerOpen;
    private NavMenu navMenuRef;
    private MudThemeProvider mudThemeProviderRef;

    [Inject]
    private StateContainer StateContainer { get; set; }

    [Inject]
    protected GitHubRepositoryService RepositoryService { get; set; }

    private void DrawerToggle()
    {
        drawerOpen = !drawerOpen;
    }

    protected override void OnInitialized()
    {
        drawerOpen = true;

        base.OnInitialized();
    }

    protected override async Task OnAfterRenderAsync(
        bool firstRender)
    {
        if (firstRender)
        {
            StateContainer.UseDarkMode(await mudThemeProviderRef.GetSystemPreference());

            navMenuRef.Refresh();

            StateHasChanged();
        }

        await base.OnAfterRenderAsync(firstRender);
    }
}