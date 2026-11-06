namespace AtcWeb.Domain.Tests.AtcApi;

public sealed class AtcApiAppClientTests : IDisposable
{
    private const string LogViewerJson =
        """
        [{"name":"atc-soft-log-viewer","displayName":"Log Viewer","owner":"atc-net-apps",
          "htmlUrl":"https://github.com/atc-net-apps/atc-soft-log-viewer","defaultBranch":"main",
          "topics":[],"screenshots":[],"socialImageUrl":"https://opengraph.githubassets.com/1/atc-net-apps/atc-soft-log-viewer",
          "latestRelease":{"tagName":"v1.1.0","htmlUrl":"https://github.com/atc-net-apps/atc-soft-log-viewer/releases/tag/v1.1.0","assets":[
            {"fileName":"AtcSoft.Log.Viewer.Installer.msi","label":"Installer","fileType":"msi","isSecondary":false,"size":1000,"downloadCount":3,
             "downloadUrl":"https://github.com/atc-net-apps/atc-soft-log-viewer/releases/download/v1.1.0/AtcSoft.Log.Viewer.Installer.msi"}]}}]
        """;

    private readonly StubHttpMessageHandler handler = new();
    private readonly MemoryCache memoryCache = new(new MemoryCacheOptions());
    private readonly HttpClient httpClient;

    public AtcApiAppClientTests()
        => httpClient = new HttpClient(handler, disposeHandler: false);

    public void Dispose()
    {
        httpClient.Dispose();
        handler.Dispose();
        memoryCache.Dispose();
    }

    [Fact]
    public async Task GetAtcApps_DeserializesAtcApps()
    {
        handler.Add("/github/apps", LogViewerJson);

        var (isSuccessful, apps) = await CreateSut().GetAtcApps(TestContext.Current.CancellationToken);

        isSuccessful.Should().BeTrue();
        apps[0].DisplayName.Should().Be("Log Viewer");
        apps[0].LatestRelease!.Assets[0].Label.Should().Be("Installer");
        apps[0].LatestRelease!.Assets[0].Size.Should().Be(1000);
    }

    [Fact]
    public async Task GetAtcApps_ServerError_ReturnsNotSuccessful()
    {
        handler.Add("/github/apps", "boom", HttpStatusCode.InternalServerError);

        var (isSuccessful, apps) = await CreateSut().GetAtcApps(TestContext.Current.CancellationToken);

        isSuccessful.Should().BeFalse();
        apps.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAtcAppByName_UnknownName_ReturnsNotSuccessful()
    {
        handler.Add("/github/apps", LogViewerJson);

        var (isSuccessful, app) = await CreateSut().GetAtcAppByName("nope", TestContext.Current.CancellationToken);

        isSuccessful.Should().BeFalse();
        app.Should().BeNull();
    }

    [Fact]
    public async Task GetReadme_ReturnsText()
    {
        handler.Add("/github/apps/atc-soft-log-viewer/readme", "# Log Viewer");

        var (isSuccessful, readme) = await CreateSut().GetReadme("atc-soft-log-viewer", TestContext.Current.CancellationToken);

        isSuccessful.Should().BeTrue();
        readme.Should().Be("# Log Viewer");
    }

    [Fact]
    public async Task GetBuiltWith_DeserializesReferences()
    {
        handler.Add("/github/apps/atc-soft-log-viewer/built-with", """[{"packageId":"Atc.Wpf","repositoryName":"atc-wpf"}]""");

        var (isSuccessful, references) = await CreateSut().GetBuiltWith("atc-soft-log-viewer", TestContext.Current.CancellationToken);

        isSuccessful.Should().BeTrue();
        references.Should().ContainSingle(x => x.PackageId == "Atc.Wpf" && x.RepositoryName == "atc-wpf");
    }

    [Fact]
    public async Task GetWiki_DeserializesPages()
    {
        handler.Add(
            "/github/apps/atc-soft-video-surveillance/wiki",
            """{"repositoryName":"atc-soft-video-surveillance","rawSidebar":"x","pages":[{"title":"Install","slug":"Install-Server","groupName":"Installation","rawContent":"# Install"}]}""");

        var (isSuccessful, wiki) = await CreateSut().GetWiki("atc-soft-video-surveillance", TestContext.Current.CancellationToken);

        isSuccessful.Should().BeTrue();
        wiki.HasWiki.Should().BeTrue();
        wiki.Pages[0].Slug.Should().Be("Install-Server");
    }

    [Fact]
    public async Task GetWiki_NoWiki_ReturnsEmptyWiki()
    {
        var (isSuccessful, wiki) = await CreateSut().GetWiki("atc-soft-log-viewer", TestContext.Current.CancellationToken);

        isSuccessful.Should().BeFalse();
        wiki.HasWiki.Should().BeFalse();
    }

    private AtcApiAppClient CreateSut()
        => new(
            httpClient,
            memoryCache,
            new BrowserCacheService(Substitute.For<IJSRuntime>()));
}