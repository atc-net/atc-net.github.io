namespace AtcWeb.Domain.AtcApi;

public class AtcApiGitHubRepositoryClient
{
    private const string BaseAddress = $"{AtcApiConstants.BaseAddress}/github/repository";
    private readonly HttpClient httpClient;
    private readonly IMemoryCache memoryCache;
    private readonly BrowserCacheService browserCache;
    private static readonly SemaphoreSlim SemaphoreRepositories = new(1, 1);
    private static readonly SemaphoreSlim SemaphorePaths = new(1, 1);
    private static readonly SemaphoreSlim SemaphoreFiles = new(1, 1);
    private static readonly SemaphoreSlim SemaphoreIssues = new(1, 1);

    public AtcApiGitHubRepositoryClient(
        HttpClient httpClient,
        IMemoryCache memoryCache,
        BrowserCacheService browserCache)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(memoryCache);
        ArgumentNullException.ThrowIfNull(browserCache);

        this.httpClient = httpClient;
        this.memoryCache = memoryCache;
        this.browserCache = browserCache;
    }

    public async Task<(bool IsSuccessful, List<GitHubRepository> GitHubRepositories)> GetRepositories(
        CancellationToken cancellationToken = default)
    {
        const string cacheKey = CacheConstants.CacheKeyRepositories;
        if (memoryCache.TryGetValue(cacheKey, out List<GitHubRepository> data))
        {
            return (IsSuccessful: true, data!);
        }

        var browserCached = await browserCache.GetAsync<List<GitHubRepository>>(cacheKey);
        if (browserCached is not null)
        {
            memoryCache.Set(cacheKey, browserCached, CacheConstants.AbsoluteExpirationRelativeToNow);
            return (IsSuccessful: true, browserCached);
        }

        await SemaphoreRepositories.WaitAsync(cancellationToken);

        try
        {
            const string url = $"{BaseAddress}/";

            var responseMessage = await httpClient.GetAsync(url, cancellationToken);
            if (!responseMessage.IsSuccessStatusCode)
            {
                return (IsSuccessful: false, []);
            }

            var content = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<IReadOnlyList<GitHubRepository>>(content, JsonSerializerOptionsFactory.Create());
            if (result is null)
            {
                return (IsSuccessful: false, []);
            }

            var gitHubRepositories = result
                .Where(x => x is { Archived: false, Private: false } &&
                            !x.Name.Equals("atc-dummy", StringComparison.Ordinal) &&
                            !x.Name.Equals("atc-template-dotnet-package", StringComparison.Ordinal))
                .OrderBy(x => x.Name, StringComparer.Ordinal)
                .ToList();

            memoryCache.Set(cacheKey, gitHubRepositories, CacheConstants.AbsoluteExpirationRelativeToNow);
            await browserCache.SetAsync(cacheKey, gitHubRepositories);
            return (IsSuccessful: true, gitHubRepositories);
        }
        catch
        {
            return (IsSuccessful: false, []);
        }
        finally
        {
            SemaphoreRepositories.Release();
        }
    }

    public async Task<(bool IsSuccessful, GitHubRepository? GitHubRepository)> GetRepositoryByName(
        string repositoryName,
        CancellationToken cancellationToken = default)
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

    public async Task<(bool IsSuccessful, List<DotnetNugetPackageMetadataBase> DotnetNugetPackagesMetadata)> GetLatestNugetPackageVersionsUsed(
        CancellationToken cancellationToken = default)
    {
        const string cacheKey = CacheConstants.CacheKeyNugetPackagesUsedByAtcRepositories;
        if (memoryCache.TryGetValue(cacheKey, out List<DotnetNugetPackageMetadataBase> data))
        {
            return (IsSuccessful: true, data!);
        }

        try
        {
            const string url = $"{BaseAddress}/nuget-packages-used";

            var responseMessage = await httpClient.GetAsync(url, cancellationToken);
            if (!responseMessage.IsSuccessStatusCode)
            {
                return (IsSuccessful: false, []);
            }

            var content = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<List<DotnetNugetPackageMetadataBase>>(content, JsonSerializerOptionsFactory.Create());
            if (result is null)
            {
                return (IsSuccessful: false, []);
            }

            memoryCache.Set(cacheKey, result, CacheConstants.AbsoluteExpirationRelativeToNow);
            return (IsSuccessful: true, result);
        }
        catch
        {
            return (IsSuccessful: false, []);
        }
    }

    public async Task<(bool IsSuccessful, List<GitHubPath> GitHubPaths)> GetAllPathsByRepositoryByName(
        string repositoryName,
        CancellationToken cancellationToken = default)
    {
        var url = $"{BaseAddress}/{repositoryName}/paths";
        var cacheKey = $"{CacheConstants.CacheKeyRepositories}_{url}";
        if (memoryCache.TryGetValue(cacheKey, out List<GitHubPath> data))
        {
            return (IsSuccessful: true, data!);
        }

        await SemaphorePaths.WaitAsync(cancellationToken);

        try
        {
            var responseMessage = await httpClient.GetAsync(url, cancellationToken);
            if (!responseMessage.IsSuccessStatusCode)
            {
                return (IsSuccessful: false, []);
            }

            var content = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<List<GitHubPath>>(content, JsonSerializerOptionsFactory.Create());
            if (result is null)
            {
                return (IsSuccessful: false, []);
            }

            memoryCache.Set(cacheKey, result, CacheConstants.AbsoluteExpirationRelativeToNow);
            return (IsSuccessful: true, result);
        }
        catch
        {
            return (IsSuccessful: false, []);
        }
        finally
        {
            SemaphorePaths.Release();
        }
    }

    public async Task<(bool IsSuccessful, string FilePath)> GetFileByRepositoryNameAndFilePath(
        string repositoryName,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        var url = $"{BaseAddress}/{repositoryName}/file?filePath={filePath}";
        var cacheKey = $"{CacheConstants.CacheKeyRepositoryFile}_{url}";
        if (memoryCache.TryGetValue(cacheKey, out string data))
        {
            return (IsSuccessful: true, data!);
        }

        await SemaphoreFiles.WaitAsync(cancellationToken);

        try
        {
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
        finally
        {
            SemaphoreFiles.Release();
        }
    }

    public Task<(bool IsSuccessful, List<GitHubIssue> GitHubIssues)> GetIssuesAllByRepositoryByName(
        string repositoryName,
        CancellationToken cancellationToken = default)
        => GetIssuesByRepositoryByName(repositoryName, "all", cancellationToken);

    public Task<(bool IsSuccessful, List<GitHubIssue> GitHubIssues)> GetIssuesOpenByRepositoryByName(
        string repositoryName,
        CancellationToken cancellationToken = default)
        => GetIssuesByRepositoryByName(repositoryName, "open", cancellationToken);

    public Task<(bool IsSuccessful, List<GitHubIssue> GitHubIssues)> GetIssuesClosedByRepositoryByName(
        string repositoryName,
        CancellationToken cancellationToken = default)
        => GetIssuesByRepositoryByName(repositoryName, "closed", cancellationToken);

    private async Task<(bool IsSuccessful, List<GitHubIssue> GitHubIssues)> GetIssuesByRepositoryByName(
        string repositoryName,
        string state,
        CancellationToken cancellationToken = default)
    {
        var url = $"{BaseAddress}/{repositoryName}/issues/{state}";
        var cacheKey = $"{CacheConstants.CacheKeyIssues}_{url}";
        if (memoryCache.TryGetValue(cacheKey, out List<GitHubIssue> data))
        {
            return (IsSuccessful: true, data!);
        }

        await SemaphoreIssues.WaitAsync(cancellationToken);

        try
        {
            var responseMessage = await httpClient.GetAsync(url, cancellationToken);
            if (!responseMessage.IsSuccessStatusCode)
            {
                return (IsSuccessful: false, []);
            }

            var content = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<List<GitHubIssue>>(content, JsonSerializerOptionsFactory.Create());
            if (result is null)
            {
                return (IsSuccessful: false, []);
            }

            memoryCache.Set(cacheKey, result, CacheConstants.AbsoluteExpirationRelativeToNow);
            return (IsSuccessful: true, result.ToList());
        }
        catch
        {
            return (IsSuccessful: false, []);
        }
        finally
        {
            SemaphoreIssues.Release();
        }
    }
}