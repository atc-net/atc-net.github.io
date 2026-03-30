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
            "atc", "atc-rest-api-generator", "atc-cosmos", "atc-azure-messaging",
            "atc-coding-rules", "atc-semantic-kernel", "atc-blazor", "atc-hosting",
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

    private static string GetRepoCategoryClass(string repoName)
    {
        if (repoName.Contains("azure", StringComparison.OrdinalIgnoreCase))
        {
            return "repo-card-azure";
        }

        if (repoName.Contains("rest", StringComparison.OrdinalIgnoreCase))
        {
            return "repo-card-rest";
        }

        if (repoName.Contains("semantic-kernel", StringComparison.OrdinalIgnoreCase) ||
            repoName.Contains("agentic", StringComparison.OrdinalIgnoreCase))
        {
            return "repo-card-ai";
        }

        if (repoName.Contains("coding-rules", StringComparison.OrdinalIgnoreCase) ||
            repoName.Contains("analyzer", StringComparison.OrdinalIgnoreCase) ||
            repoName.Contains("source-gen", StringComparison.OrdinalIgnoreCase))
        {
            return "repo-card-tools";
        }

        return "repo-card-core";
    }
}