namespace AtcWeb.Domain.Tests.AtcApi;

public class AtcApiGitHubRepositoryClientTests
{
    [Theory, AutoNSubstituteData]
    public async Task GetRepositories(
        [Frozen] IMemoryCache memoryCache,
        [Frozen] BrowserCacheService browserCacheService)
    {
        // Arrange
        using var httpClient = new HttpClient();
        var client = new AtcApiGitHubRepositoryClient(httpClient, memoryCache, browserCacheService);

        // Act
        var (isSuccessful, gitHubRepositories) = await client.GetRepositories(
            TestContext.Current.CancellationToken);

        // Assert
        Assert.True(isSuccessful);

        gitHubRepositories
            .Should()
            .NotBeEmpty()
            .And
            .HaveCountGreaterThan(1);
    }

    [Theory, AutoNSubstituteData]
    public async Task GetRepositoryByName(
        [Frozen] IMemoryCache memoryCache,
        [Frozen] BrowserCacheService browserCacheService)
    {
        using var httpClient = new HttpClient();
        var client = new AtcApiGitHubRepositoryClient(httpClient, memoryCache, browserCacheService);

        // Act
        var (isSuccessful, gitHubRepository) = await client.GetRepositoryByName(
            "atc",
            TestContext.Current.CancellationToken);

        // Assert
        Assert.True(isSuccessful);

        gitHubRepository
            .Should()
            .NotBeNull();
    }

    [Theory, AutoNSubstituteData]
    public async Task GetAllPathsByRepository(
        [Frozen] IMemoryCache memoryCache,
        [Frozen] BrowserCacheService browserCacheService)
    {
        // Arrange
        using var httpClient = new HttpClient();
        var client = new AtcApiGitHubRepositoryClient(httpClient, memoryCache, browserCacheService);

        // Act
        var (isSuccessful, gitHubPaths) = await client.GetAllPathsByRepositoryByName(
            "atc",
            TestContext.Current.CancellationToken);

        // Assert
        Assert.True(isSuccessful);

        gitHubPaths
            .Should()
            .NotBeEmpty()
            .And
            .HaveCountGreaterThan(1);
    }

    [Theory, AutoNSubstituteData]
    public async Task GetFileByRepositoryNameAndFilePath(
        [Frozen] IMemoryCache memoryCache,
        [Frozen] BrowserCacheService browserCacheService)
    {
        // Arrange
        using var httpClient = new HttpClient();
        var client = new AtcApiGitHubRepositoryClient(httpClient, memoryCache, browserCacheService);

        // Act
        var (isSuccessful, gitHubRaw) = await client.GetFileByRepositoryNameAndFilePath(
            "atc",
            "README.md",
            TestContext.Current.CancellationToken);

        // Assert
        Assert.True(isSuccessful);

        gitHubRaw
            .Should()
            .NotBeEmpty();
    }

    [Theory, AutoNSubstituteData]
    public async Task GetIssuesAllByRepositoryByName(
        [Frozen] IMemoryCache memoryCache,
        [Frozen] BrowserCacheService browserCacheService)
    {
        // Arrange
        using var httpClient = new HttpClient();
        var client = new AtcApiGitHubRepositoryClient(httpClient, memoryCache, browserCacheService);

        // Act
        var (isSuccessful, gitHubIssues) = await client.GetIssuesAllByRepositoryByName(
            "atc",
            TestContext.Current.CancellationToken);

        // Assert
        Assert.True(isSuccessful);

        gitHubIssues
            .Should()
            .NotBeEmpty()
            .And
            .HaveCountGreaterThan(1);
    }

    [Theory, AutoNSubstituteData]
    public async Task GetIssuesOpenByRepositoryByName(
        [Frozen] IMemoryCache memoryCache,
        [Frozen] BrowserCacheService browserCacheService)
    {
        // Arrange
        using var httpClient = new HttpClient();
        var client = new AtcApiGitHubRepositoryClient(httpClient, memoryCache, browserCacheService);

        // Act
        var (isSuccessful, gitHubIssues) = await client.GetIssuesOpenByRepositoryByName(
            "atc",
            TestContext.Current.CancellationToken);

        // Assert
        Assert.True(isSuccessful);

        gitHubIssues
            .Should()
            .NotBeEmpty()
            .And
            .HaveCountGreaterThan(1);
    }

    [Theory, AutoNSubstituteData]
    public async Task GetIssuesClosedByRepositoryByName(
        [Frozen] IMemoryCache memoryCache,
        [Frozen] BrowserCacheService browserCacheService)
    {
        // Arrange
        using var httpClient = new HttpClient();
        var client = new AtcApiGitHubRepositoryClient(httpClient, memoryCache, browserCacheService);

        // Act
        var (isSuccessful, gitHubIssues) = await client.GetIssuesClosedByRepositoryByName(
            "atc",
            TestContext.Current.CancellationToken);

        // Assert
        Assert.True(isSuccessful);

        gitHubIssues
            .Should()
            .NotBeEmpty()
            .And
            .HaveCountGreaterThan(1);
    }
}