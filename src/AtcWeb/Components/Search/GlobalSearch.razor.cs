namespace AtcWeb.Components.Search;

public sealed partial class GlobalSearch : ComponentBase, IDisposable
{
    private List<SearchHit>? all;
    private SiteWorld world;

    [Inject]
    private AtcAppService AtcAppService { get; set; } = default!;

    [Inject]
    private GitHubRepositoryService RepositoryService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    /// <summary>
    /// Gets or sets a placeholder that replaces the world-dependent one.
    /// </summary>
    [Parameter]
    public string? PlaceholderText { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the box is the large one on the home page.
    /// </summary>
    [Parameter]
    public bool Large { get; set; }

    private string Placeholder
        => PlaceholderText ?? world switch
        {
            SiteWorld.AtcApps => "Search apps…",
            SiteWorld.Libraries => "Search libraries…",
            _ => "Search apps & libraries…",
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

    private async Task<IEnumerable<SearchHit>> SearchAsync(
        string? text,
        CancellationToken cancellationToken)
    {
        all ??= await LoadAll();
        return SearchHelper.Search(text, all);
    }

    // Both lists are cached by their clients, so this only costs a request the first time.
    private async Task<List<SearchHit>> LoadAll()
    {
        var taskAtcApps = AtcAppService.GetAtcAppsAsync();
        var taskRepositories = RepositoryService.GetRepositoriesAsync(populateMetaDataBase: false);
        await Task.WhenAll(taskAtcApps, taskRepositories);

        var apps = (await taskAtcApps)
            .Select(x => new SearchHit(SiteWorld.AtcApps, x.Name, x.DisplayName, x.Description));
        var components = (await taskRepositories)
            .Where(x => !x.BaseData.Private)
            .Select(x => new SearchHit(SiteWorld.Libraries, x.Name, x.Name, x.Description));

        return apps
            .Concat(components)
            .ToList();
    }

    private void OnSelected(SearchHit? hit)
    {
        if (hit is not null)
        {
            NavigationManager.NavigateTo(hit.Href);
        }
    }
}