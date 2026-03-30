namespace AtcWeb.Pages;

public partial class Index
{
    private List<AtcRepository>? featuredRepos;
    private int repositoryCount;
    private int contributorCount;

    [Inject]
    private GitHubRepositoryService RepositoryService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var reposTask = RepositoryService.GetRepositoriesAsync();
        var contributorsTask = RepositoryService.GetContributorsAsync();

        await Task.WhenAll(reposTask, contributorsTask);

        var allRepos = await reposTask;
        var contributors = await contributorsTask;

        repositoryCount = allRepos.Count(r => !r.BaseData.Private);
        contributorCount = contributors.Count;

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
}