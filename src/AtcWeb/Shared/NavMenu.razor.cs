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
            var category = CategorizeRepository(repo.Name);
            if (!groups.TryGetValue(category, out var list))
            {
                list = [];
                groups[category] = list;
            }

            list.Add(repo);
        }

        return groups
            .OrderBy(g => GetGroupSortOrder(g.Key))
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
        return string.Equals(CategorizeRepository(repoName), groupName, StringComparison.Ordinal);
    }

    private static string CategorizeRepository(string name)
    {
        if (name.Contains("azure", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("cosmos", StringComparison.OrdinalIgnoreCase))
        {
            return "Azure";
        }

        if (name.Contains("rest", StringComparison.OrdinalIgnoreCase))
        {
            return "REST & API";
        }

        if (name.Contains("semantic-kernel", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("agentic", StringComparison.OrdinalIgnoreCase))
        {
            return "AI & Agents";
        }

        if (name.Contains("coding-rules", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("analyzer", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("source-gen", StringComparison.OrdinalIgnoreCase))
        {
            return "Code Quality";
        }

        if (name.Contains("wpf", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("xaml", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("blazor", StringComparison.OrdinalIgnoreCase))
        {
            return "UI & Desktop";
        }

        if (name.Contains("test", StringComparison.OrdinalIgnoreCase))
        {
            return "Testing";
        }

        return "Core & Libraries";
    }

    private static string GetGroupIcon(string groupName)
        => groupName switch
        {
            "Azure" => Icons.Material.Filled.Cloud,
            "REST & API" => Icons.Material.Filled.Api,
            "AI & Agents" => Icons.Material.Filled.Psychology,
            "Code Quality" => Icons.Material.Filled.Rule,
            "UI & Desktop" => Icons.Material.Filled.DesktopWindows,
            "Testing" => Icons.Material.Filled.Science,
            "Core & Libraries" => Icons.Material.Filled.Hub,
            _ => Icons.Material.Filled.Folder,
        };

    private static int GetGroupSortOrder(string groupName)
        => groupName switch
        {
            "Core & Libraries" => 0,
            "Azure" => 1,
            "REST & API" => 2,
            "AI & Agents" => 3,
            "Code Quality" => 4,
            "UI & Desktop" => 5,
            "Testing" => 6,
            _ => 99,
        };
}