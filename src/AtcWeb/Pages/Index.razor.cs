namespace AtcWeb.Pages;

public partial class Index
{
    private List<AtcRepository>? featuredRepos;
    private int repositoryCount;
    private NugetTotalDownloadsResult? nugetStats;
    private Dictionary<string, long> downloadsByRepository = new(StringComparer.OrdinalIgnoreCase);

    [Inject]
    private GitHubRepositoryService RepositoryService { get; set; }

    [Inject]
    private AtcApiNugetClient NugetClient { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var taskRepos = RepositoryService.GetRepositoriesAsync();
        var taskNuget = NugetClient.GetTotalDownloads();
        var taskDownloads = RepositoryService.GetNugetDownloadsByRepositoryAsync();

        await Task.WhenAll(taskRepos, taskNuget, taskDownloads);

        downloadsByRepository = await taskDownloads;

        var allRepos = await taskRepos;
        var (isSuccessful, result) = await taskNuget;
        if (isSuccessful)
        {
            nugetStats = result;
        }

        repositoryCount = allRepos.Count(r => !r.BaseData.Private);

        // Curate featured repos: pick well-known packages across categories
        var featuredNames = new[]
        {
            "atc-kusto", "atc-claude-kanban", "atc-rest-api-source-generator",
            "atc-agentic-toolkit", "atc-dsc-configurations", "atc-opc-ua",
            "atc-hosting", "atc-test", "atc-azure-iot", "atc-rest-minimalapi",
            "atc-kepware",
        };

        var publicRepos = allRepos
            .Where(r => !r.BaseData.Private)
            .ToList();

        featuredRepos = featuredNames
            .Select(name => publicRepos.Find(r => r.Name.Equals(name, StringComparison.Ordinal)))
            .Where(r => r is not null)
            .Cast<AtcRepository>()
            .ToList();

        // Fill remaining slots if some names weren't found
        if (featuredRepos.Count < 8)
        {
            var existing = featuredRepos.Select(r => r.Name).ToHashSet(StringComparer.Ordinal);
            featuredRepos.AddRange(publicRepos
                .Where(r => !existing.Contains(r.Name))
                .OrderBy(r => r.Name, StringComparer.Ordinal)
                .Take(8 - featuredRepos.Count));
        }

        await base.OnInitializedAsync();
    }

    private long GetDownloads(AtcRepository repository)
        => downloadsByRepository.GetValueOrDefault(repository.Name);

    private static bool HasNonMitLicense(AtcRepository repository)
        => !string.IsNullOrEmpty(repository.BaseData.License?.Key) &&
           !"mit".Equals(repository.BaseData.License.Key, StringComparison.OrdinalIgnoreCase);
}