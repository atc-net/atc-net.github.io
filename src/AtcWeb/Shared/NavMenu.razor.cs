namespace AtcWeb.Shared;

public partial class NavMenu
{
    private readonly Dictionary<string, bool> groupExpandedState = new(StringComparer.Ordinal);
    private string? section;
    private string? componentLink;
    private string searchText = string.Empty;

    private Dictionary<string, List<AtcRepository>>? allGroups;

    protected List<AtcRepository>? repositories;

    [Inject]
    private NavigationManager NavigationManager { get; set; }

    [Inject]
    protected GitHubRepositoryService RepositoryService { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        section = NavigationManager.GetSection();
        componentLink = NavigationManager.GetComponentLink();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            repositories = await RepositoryService.GetRepositoriesAsync();
            allGroups = BuildAllGroups();

            // Yield to let the browser process any queued user interactions
            // before the heavy DOM update from rendering all repository groups.
            await Task.Yield();

            StateHasChanged();
        }
    }

    public bool IsSubGroupExpanded(AtcComponent? item)
        => item is not null &&
           item.GroupItems.Elements.Any(i => i.Link == componentLink);

    private Dictionary<string, List<AtcRepository>> BuildAllGroups()
    {
        if (repositories is null)
        {
            return new Dictionary<string, List<AtcRepository>>(StringComparer.Ordinal);
        }

        var groups = new Dictionary<string, List<AtcRepository>>(StringComparer.Ordinal);

        foreach (var repo in repositories
            .Where(x => !x.BaseData.Private)
            .OrderBy(x => x.Name, StringComparer.Ordinal))
        {
            var category = RepositoryCategoryHelper.GetCategory(repo.Name);
            if (!groups.TryGetValue(category, out var list))
            {
                list = [];
                groups[category] = list;
            }

            list.Add(repo);
        }

        return groups
            .OrderBy(g => RepositoryCategoryHelper.GetSortOrder(g.Key))
            .ThenBy(g => g.Key, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Value, StringComparer.Ordinal);
    }

    private bool GroupMatchesSearch(List<AtcRepository> repos)
        => string.IsNullOrWhiteSpace(searchText) ||
           repos.Exists(x => x.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase));

    private List<AtcRepository> GetFilteredRepos(List<AtcRepository> repos)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return repos;
        }

        return repos
            .Where(x => x.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private void OnGroupExpandedChanged(
        string groupName,
        bool expanded)
    {
        groupExpandedState[groupName] = expanded;
    }

    private bool IsGroupExpanded(string groupName)
    {
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            return true;
        }

        if (groupExpandedState.TryGetValue(groupName, out var explicitState))
        {
            return explicitState;
        }

        var currentUri = NavigationManager.Uri;
        if (!currentUri.Contains("/repository/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var repoName = currentUri.Split("/repository/").LastOrDefault() ?? string.Empty;
        return string.Equals(RepositoryCategoryHelper.GetCategory(repoName), groupName, StringComparison.Ordinal);
    }
}