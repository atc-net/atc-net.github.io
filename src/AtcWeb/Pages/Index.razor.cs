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

        featuredRepos = allRepos
            .Where(r => !r.BaseData.Private)
            .OrderByDescending(r => r.BaseData.StargazersCount)
            .ThenBy(r => r.Name, StringComparer.Ordinal)
            .Take(8)
            .ToList();

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