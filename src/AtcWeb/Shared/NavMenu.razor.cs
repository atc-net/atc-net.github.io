namespace AtcWeb.Shared;

public partial class NavMenu
{
    private string? section;
    private string? componentLink;

    protected List<AtcRepository>? repositories;

    [Inject]
    private NavigationManager NavigationManager { get; set; }

    [Inject]
    protected GitHubRepositoryService RepositoryService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        repositories = await RepositoryService.GetRepositoriesAsync();

        Refresh();
        await base.OnInitializedAsync();
    }

    public void Refresh()
    {
        section = NavigationManager.GetSection();
        componentLink = NavigationManager.GetComponentLink();
        StateHasChanged();
    }

    public bool IsSubGroupExpanded(AtcComponent? item)
        => item is not null &&
           item.GroupItems.Elements.Any(i => i.Link == componentLink);
}