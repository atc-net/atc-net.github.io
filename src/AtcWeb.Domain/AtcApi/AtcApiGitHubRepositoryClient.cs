namespace AtcWeb.Domain.AtcApi;

public class AtcApiGitHubRepositoryClient
{
    private const string BaseAddress = "https://atc-api.azurewebsites.net/github/repository";
    private readonly IMemoryCache memoryCache;

    public AtcApiGitHubRepositoryClient(IMemoryCache memoryCache)
    {
        ArgumentNullException.ThrowIfNull(memoryCache);

        this.memoryCache = memoryCache;
    }

    public async Task<(bool IsSuccessful, List<GitHubRepository> GitHubRepositories)> GetRepositories(CancellationToken cancellationToken = default)
    {
        const string cacheKey = CacheConstants.CacheKeyRepositories;
        if (memoryCache.TryGetValue(cacheKey, out List<GitHubRepository> data))
        {
            return (IsSuccessful: true, data!);
        }

        try
        {
            const string url = $"{BaseAddress}/";
            using var httpClient = new HttpClient();
            var responseMessage = await httpClient.GetAsync(url, cancellationToken);
            if (!responseMessage.IsSuccessStatusCode)
            {
                return (IsSuccessful: false, new List<GitHubRepository>());
            }

            var content = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<IReadOnlyList<GitHubRepository>>(content, JsonSerializerOptionsFactory.Create());
            if (result is null)
            {
                return (IsSuccessful: false, new List<GitHubRepository>());
            }

            var gitHubRepositories = result
                .Where(x => !x.Name.Equals("atc-dummy", StringComparison.Ordinal) &&
                            !x.Name.Equals("atc-template-dotnet-package", StringComparison.Ordinal))
                .OrderBy(x => x.Name, StringComparer.Ordinal)
                .ToList();

            memoryCache.Set(cacheKey, gitHubRepositories, CacheConstants.AbsoluteExpirationRelativeToNow);
            return (IsSuccessful: true, gitHubRepositories);
        }
        catch
        {
            return (IsSuccessful: false, new List<GitHubRepository>());
        }
    }

    public async Task<(bool IsSuccessful, GitHubRepository? GitHubRepository)> GetRepositoryByName(string repositoryName, CancellationToken cancellationToken = default)
    {
        var (isSuccessful, repositories) = await GetRepositories(cancellationToken);
        if (!isSuccessful)
        {
            return (IsSuccessful: false, null);
        }

        var repository = repositories.SingleOrDefault(x => x.Name.Equals(repositoryName, StringComparison.OrdinalIgnoreCase));

        return repository is null
            ? (IsSuccessful: false, gitHubRepository: null)
            : (IsSuccessful: true, gitHubRepository: repository);
    }

    public async Task<(bool IsSuccessful, List<GitHubRepositoryContributor> GitHubRepositoryContributors)> GetContributors(CancellationToken cancellationToken = default)
    {
        const string cacheKey = CacheConstants.CacheKeyContributors;
        if (memoryCache.TryGetValue(cacheKey, out List<GitHubRepositoryContributor> data))
        {
            return (IsSuccessful: true, data!);
        }

        try
        {
            var bag = new ConcurrentBag<GitHubRepositoryContributor>();

            var (isSuccessful, gitHubRepositories) = await GetRepositories(cancellationToken);
            if (isSuccessful)
            {
                var tasks = gitHubRepositories
                    .Select(async gitHubRepository =>
                    {
                        var (isSuccessfulContributors, contributors) = await GetContributorsByRepositoryByName(gitHubRepository.Name, cancellationToken);
                        if (isSuccessfulContributors)
                        {
                            foreach (var contributor in contributors
                                         .Where(gitHubContributor =>
                                             bag.FirstOrDefault(x => x.Id.Equals(gitHubContributor.Id)) is null &&
                                             !gitHubContributor.Login.Equals("ATCBot", StringComparison.Ordinal)))
                            {
                                bag.Add(contributor);
                            }
                        }
                    });

                await TaskHelper.WhenAll(tasks);
            }

            memoryCache.Set(cacheKey, bag.ToList(), CacheConstants.AbsoluteExpirationRelativeToNow);
            return (IsSuccessful: true, bag.ToList());
        }
        catch
        {
            return (IsSuccessful: false, new List<GitHubRepositoryContributor>());
        }
    }

    public async Task<(bool IsSuccessful, List<GitHubRepositoryContributor> GitHubRepositoryContributors)> GetContributorsByRepositoryByName(string repositoryName, CancellationToken cancellationToken = default)
    {
        var url = $"{BaseAddress}/contributors/{repositoryName}";
        var cacheKey = $"{CacheConstants.CacheKeyRepositories}_{url}";
        if (memoryCache.TryGetValue(cacheKey, out List<GitHubRepositoryContributor> data))
        {
            return (IsSuccessful: true, data!);
        }

        try
        {
            using var httpClient = new HttpClient();
            var responseMessage = await httpClient.GetAsync(url, cancellationToken);
            if (!responseMessage.IsSuccessStatusCode)
            {
                return (IsSuccessful: false, new List<GitHubRepositoryContributor>());
            }

            var content = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<List<GitHubRepositoryContributor>>(content, JsonSerializerOptionsFactory.Create());
            if (result is null)
            {
                return (IsSuccessful: false, new List<GitHubRepositoryContributor>());
            }

            memoryCache.Set(cacheKey, result, CacheConstants.AbsoluteExpirationRelativeToNow);
            return (IsSuccessful: true, result.ToList());
        }
        catch
        {
            return (IsSuccessful: false, new List<GitHubRepositoryContributor>());
        }
    }

    public async Task<(bool IsSuccessful, List<DotnetNugetPackageMetadataBase> DotnetNugetPackagesMetadata)> GetLatestNugetPackageVersionsUsed(CancellationToken cancellationToken = default)
    {
        const string cacheKey = CacheConstants.CacheKeyNugetPackagesUsedByAtcRepositories;
        if (memoryCache.TryGetValue(cacheKey, out List<DotnetNugetPackageMetadataBase> data))
        {
            return (IsSuccessful: true, data!);
        }

        try
        {
            const string url = $"{BaseAddress}/nuget-packages-used";
            using var httpClient = new HttpClient();
            var responseMessage = await httpClient.GetAsync(url, cancellationToken);
            if (!responseMessage.IsSuccessStatusCode)
            {
                return (IsSuccessful: false, new List<DotnetNugetPackageMetadataBase>());
            }

            var content = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<List<DotnetNugetPackageMetadataBase>>(content, JsonSerializerOptionsFactory.Create());
            if (result is null)
            {
                return (IsSuccessful: false, new List<DotnetNugetPackageMetadataBase>());
            }

            memoryCache.Set(cacheKey, result, CacheConstants.AbsoluteExpirationRelativeToNow);
            return (IsSuccessful: true, result);
        }
        catch
        {
            return (IsSuccessful: false, new List<DotnetNugetPackageMetadataBase>());
        }
    }

    public async Task<(bool IsSuccessful, List<GitHubPath> GitHubPaths)> GetAllPathsByRepositoryByName(string repositoryName, CancellationToken cancellationToken = default)
    {
        var url = $"{BaseAddress}/{repositoryName}/paths";
        var cacheKey = $"{CacheConstants.CacheKeyRepositories}_{url}";
        if (memoryCache.TryGetValue(cacheKey, out List<GitHubPath> data))
        {
            return (IsSuccessful: true, data!);
        }

        try
        {
            using var httpClient = new HttpClient();
            var responseMessage = await httpClient.GetAsync(url, cancellationToken);
            if (!responseMessage.IsSuccessStatusCode)
            {
                return (IsSuccessful: false, new List<GitHubPath>());
            }

            var content = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<List<GitHubPath>>(content, JsonSerializerOptionsFactory.Create());
            if (result is null)
            {
                return (IsSuccessful: false, new List<GitHubPath>());
            }

            memoryCache.Set(cacheKey, result, CacheConstants.AbsoluteExpirationRelativeToNow);
            return (IsSuccessful: true, result);
        }
        catch
        {
            return (IsSuccessful: false, new List<GitHubPath>());
        }
    }

    public async Task<(bool IsSuccessful, string FilePath)> GetFileByRepositoryNameAndFilePath(string repositoryName, string filePath, CancellationToken cancellationToken = default)
    {
        var url = $"{BaseAddress}/{repositoryName}/file?filePath={filePath}";
        var cacheKey = $"{CacheConstants.CacheKeyRepositoryFile}_{url}";
        if (memoryCache.TryGetValue(cacheKey, out string data))
        {
            return (IsSuccessful: true, data!);
        }

        try
        {
            using var httpClient = new HttpClient();
            var responseMessage = await httpClient.GetAsync(url, cancellationToken);
            if (!responseMessage.IsSuccessStatusCode)
            {
                return (IsSuccessful: false, string.Empty);
            }

            var result = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrEmpty(result))
            {
                return (IsSuccessful: false, string.Empty);
            }

            memoryCache.Set(cacheKey, result, CacheConstants.AbsoluteExpirationRelativeToNow);
            return (IsSuccessful: true, result);
        }
        catch
        {
            return (IsSuccessful: false, string.Empty);
        }
    }

    public Task<(bool IsSuccessful, List<GitHubIssue> GitHubIssues)> GetIssuesAllByRepositoryByName(string repositoryName, CancellationToken cancellationToken = default)
        => GetIssuesByRepositoryByName(repositoryName, "all", cancellationToken);

    public Task<(bool IsSuccessful, List<GitHubIssue> GitHubIssues)> GetIssuesOpenByRepositoryByName(string repositoryName, CancellationToken cancellationToken = default)
        => GetIssuesByRepositoryByName(repositoryName, "open", cancellationToken);

    public Task<(bool IsSuccessful, List<GitHubIssue> GitHubIssues)> GetIssuesClosedByRepositoryByName(string repositoryName, CancellationToken cancellationToken = default)
        => GetIssuesByRepositoryByName(repositoryName, "closed", cancellationToken);

    private async Task<(bool IsSuccessful, List<GitHubIssue> GitHubIssues)> GetIssuesByRepositoryByName(string repositoryName, string state, CancellationToken cancellationToken = default)
    {
        var url = $"{BaseAddress}/{repositoryName}/issues/{state}";
        var cacheKey = $"{CacheConstants.CacheKeyIssues}_{url}";
        if (memoryCache.TryGetValue(cacheKey, out List<GitHubIssue> data))
        {
            return (IsSuccessful: true, data!);
        }

        try
        {
            using var httpClient = new HttpClient();
            var responseMessage = await httpClient.GetAsync(url, cancellationToken);
            if (!responseMessage.IsSuccessStatusCode)
            {
                return (IsSuccessful: false, new List<GitHubIssue>());
            }

            var content = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<List<GitHubIssue>>(content, JsonSerializerOptionsFactory.Create());
            if (result is null)
            {
                return (IsSuccessful: false, new List<GitHubIssue>());
            }

            memoryCache.Set(cacheKey, result, CacheConstants.AbsoluteExpirationRelativeToNow);
            return (IsSuccessful: true, result.ToList());
        }
        catch
        {
            return (IsSuccessful: false, new List<GitHubIssue>());
        }
    }
}