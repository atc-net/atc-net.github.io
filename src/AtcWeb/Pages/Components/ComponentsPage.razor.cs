namespace AtcWeb.Pages.Components;

public partial class ComponentsPage : ComponentBase
{
    private const int BannerCommandCount = 5;

    private List<ComponentCardModel>? cards;
    private List<(string Name, int Count)> categories = [];
    private HashSet<string> selectedCategories = new(StringComparer.OrdinalIgnoreCase);
    private List<RepositoryNugetPackage> bannerCommands = [];
    private string? totalDownloads;
    private int libraryCount;
    private int cliToolCount;
    private ComponentKind kind = ComponentKind.All;
    private ComponentSort sort = ComponentSort.MostDownloaded;

    private enum ComponentKind
    {
        All,
        Packages,
        CliTools,
    }

    private enum ComponentSort
    {
        MostDownloaded,
        RecentlyUpdated,
        Name,
    }

    /// <summary>
    /// Gets or sets the comma-separated categories to preselect, e.g. ?category=IoT,Industrial.
    /// </summary>
    [Parameter]
    [SupplyParameterFromQuery(Name = "category")]
    public string? CategoryFilter { get; set; }

    [Inject]
    private GitHubRepositoryService RepositoryService { get; set; } = default!;

    [Inject]
    private AtcApiNugetClient NugetClient { get; set; } = default!;

    private IEnumerable<ComponentCardModel> VisibleCards
    {
        get
        {
            var visible = (cards ?? [])
                .Where(x => selectedCategories.Count == 0 || selectedCategories.Contains(x.Category))
                .Where(x => kind switch
                {
                    ComponentKind.Packages => x.HasLibrary,
                    ComponentKind.CliTools => x.HasCliTool,
                    _ => true,
                });

            return sort switch
            {
                ComponentSort.RecentlyUpdated => visible.OrderByDescending(x => x.UpdatedAt ?? DateTimeOffset.MinValue),
                ComponentSort.Name => visible.OrderBy(x => x.Name, StringComparer.Ordinal),
                _ => visible.OrderByDescending(x => x.Downloads).ThenBy(x => x.Name, StringComparer.Ordinal),
            };
        }
    }

    protected override void OnParametersSet()
        => selectedCategories = (CategoryFilter ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    protected override async Task OnInitializedAsync()
    {
        var taskRepositories = RepositoryService.GetRepositoriesAsync(populateMetaDataBase: false);
        var taskSearch = NugetClient.Search(RepositoryNugetPackageHelper.OwnerSearchQuery);
        var taskCliTools = NugetClient.GetCliTools();
        var taskTotals = NugetClient.GetTotalDownloads();
        await Task.WhenAll(taskRepositories, taskSearch, taskCliTools, taskTotals);

        var (_, searchResult) = await taskSearch;
        var (_, cliToolsResult) = await taskCliTools;
        var (totalsOk, totals) = await taskTotals;
        if (totalsOk && totals is not null)
        {
            totalDownloads = totals.FormattedDownloads;
        }

        var toolPackageIds = cliToolsResult.Data
            .Select(x => x.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        (libraryCount, cliToolCount) = SiteStatsHelper.CountPackages(
            searchResult.Data.Select(x => x.Id),
            toolPackageIds);

        var allPackages = new List<RepositoryNugetPackage>();
        cards = [];
        foreach (var repository in (await taskRepositories).Where(x => !x.BaseData.Private))
        {
            var packages = RepositoryNugetPackageHelper.GetPackagesForRepository(
                repository.Name,
                repository.DotName,
                searchResult.Data,
                toolPackageIds);
            allPackages.AddRange(packages.Where(x => !x.IsTool));
            cards.Add(ComponentCardModel.From(repository, packages));
        }

        categories = cards
            .GroupBy(x => x.Category, StringComparer.Ordinal)
            .OrderBy(x => RepositoryCategoryHelper.GetSortOrder(x.Key))
            .ThenBy(x => x.Key, StringComparer.Ordinal)
            .Select(x => (x.Key, x.Count()))
            .ToList();

        bannerCommands = allPackages
            .OrderByDescending(x => x.TotalDownloads)
            .Take(BannerCommandCount)
            .ToList();
    }

    // A repository can ship both a library and a CLI tool: show the install command that matches the filter.
    private (string? Command, bool IsTool) InstallFor(ComponentCardModel card)
        => kind switch
        {
            ComponentKind.Packages => (card.LibraryInstallCommand, false),
            ComponentKind.CliTools => (card.ToolInstallCommand, true),
            _ => (card.InstallCommand, card.IsCliTool),
        };

    private void ClearCategories()
        => selectedCategories.Clear();

    private void SelectCategory(string category)
        => selectedCategories = new HashSet<string>([category], StringComparer.OrdinalIgnoreCase);

    private string KindClass(ComponentKind value)
        => value == kind
            ? "world-filter-chip world-filter-chip--active"
            : "world-filter-chip";
}