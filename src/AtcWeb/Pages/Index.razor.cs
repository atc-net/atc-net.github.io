namespace AtcWeb.Pages;

public partial class Index
{
    private const int FeaturedAtcAppCount = 3;
    private const int FeaturedRepositoryCount = 9;

    // Curated like the previous home page: well-known repositories across categories.
    private static readonly string[] FeaturedRepositoryNames =
    [
        "atc-kusto",
        "atc-claude-kanban",
        "atc-rest-api-source-generator",
        "atc-agentic-toolkit",
        "atc-dsc-configurations",
        "atc-opc-ua",
        "atc-hosting",
        "atc-test",
        "atc-azure-iot",
        "atc-rest-minimalapi",
        "atc-kepware",
    ];

    private List<AtcAppCardModel>? featuredAtcApps;
    private List<FeaturedRepository>? featuredRepositories;
    private NugetTotalDownloadsResult? nugetStats;
    private int appCount;
    private int libraryCount;
    private int cliToolCount;

    [Inject]
    private GitHubRepositoryService RepositoryService { get; set; } = default!;

    [Inject]
    private AtcApiNugetClient NugetClient { get; set; } = default!;

    [Inject]
    private AtcAppService AtcAppService { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        var taskAtcApps = AtcAppService.GetAtcAppsAsync();
        var taskRepositories = RepositoryService.GetRepositoriesAsync(populateMetaDataBase: false);
        var taskSearch = NugetClient.Search(RepositoryNugetPackageHelper.OwnerSearchQuery);
        var taskCliTools = NugetClient.GetCliTools();
        var taskTotals = NugetClient.GetTotalDownloads();
        await Task.WhenAll(taskAtcApps, taskRepositories, taskSearch, taskCliTools, taskTotals);

        var appCards = (await taskAtcApps)
            .Select(AtcAppCardModel.From)
            .ToList();
        appCount = appCards.Count;
        featuredAtcApps = appCards
            .Where(x => !x.IsComingSoon)
            .OrderByDescending(x => x.UpdatedAt ?? DateTimeOffset.MinValue)
            .Take(FeaturedAtcAppCount)
            .ToList();

        var (totalsOk, totals) = await taskTotals;
        if (totalsOk)
        {
            nugetStats = totals;
        }

        var (_, searchResult) = await taskSearch;
        var (_, cliToolsResult) = await taskCliTools;
        var toolPackageIds = cliToolsResult.Data
            .Select(x => x.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        (libraryCount, cliToolCount) = SiteStatsHelper.CountPackages(
            searchResult.Data.Select(x => x.Id),
            toolPackageIds);

        var repositories = (await taskRepositories)
            .Where(x => !x.BaseData.Private)
            .ToList();

        featuredRepositories = FeaturedRepositoryNames
            .Select(name => repositories.Find(x => x.Name.Equals(name, StringComparison.Ordinal)))
            .OfType<AtcRepository>()
            .Take(FeaturedRepositoryCount)
            .Select(repository =>
            {
                var packages = RepositoryNugetPackageHelper.GetPackagesForRepository(
                    repository.Name,
                    repository.DotName,
                    searchResult.Data,
                    toolPackageIds);
                return new FeaturedRepository(
                    repository.Name,
                    string.IsNullOrWhiteSpace(repository.Description) ? null : repository.Description,
                    repository.BaseData.Language,
                    ShipsFormatter.ForRepository(packages.Count(x => !x.IsTool), packages.Count(x => x.IsTool)),
                    packages.Sum(x => x.TotalDownloads),
                    repository.BaseData.PushedAt);
            })
            .ToList();
    }

    private static string Count(int value)
        => value > 0
            ? value.ToString(CultureInfo.InvariantCulture)
            : "--";

    private sealed record FeaturedRepository(
        string Name,
        string? Description,
        string? Language,
        string? Ships,
        long Downloads,
        DateTimeOffset? UpdatedAt);
}