namespace AtcWeb.Shared;

public partial class NavMenu
{
    private string? section;
    private string? componentLink;
    private string searchText = string.Empty;

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

    private Dictionary<string, List<AtcRepository>> GetGroupedRepositories()
    {
        if (repositories is null)
        {
            return new Dictionary<string, List<AtcRepository>>(StringComparer.Ordinal);
        }

        var filtered = repositories
            .Where(x => !x.BaseData.Private)
            .Where(x => string.IsNullOrWhiteSpace(searchText) ||
                        x.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.Name, StringComparer.Ordinal);

        var groups = new Dictionary<string, List<AtcRepository>>(StringComparer.Ordinal);

        foreach (var repo in filtered)
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

    private bool IsGroupExpanded(string groupName)
    {
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            return true;
        }

        var currentUri = NavigationManager.Uri;
        if (!currentUri.Contains("/repository/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var repoName = currentUri.Split("/repository/", StringSplitOptions.None).LastOrDefault() ?? string.Empty;
        return string.Equals(RepositoryCategoryHelper.GetCategory(repoName), groupName, StringComparison.Ordinal);
    }
}