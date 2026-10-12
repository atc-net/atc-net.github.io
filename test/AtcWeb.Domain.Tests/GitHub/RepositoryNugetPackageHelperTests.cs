namespace AtcWeb.Domain.Tests.GitHub;

public sealed class RepositoryNugetPackageHelperTests
{
    private static readonly HashSet<string> ToolPackageIds = new(StringComparer.OrdinalIgnoreCase)
    {
        "atc-claude-kanban",
        "atc-rest-api-generator",
        "atc-coding-rules-updater",
        "atc-rest-api-gen",
        "atc-kusto",
    };

    private static readonly List<NugetPackageSearchItem> SearchItems =
    [
        Item("Atc", "https://github.com/atc-net/atc", 1317565),
        Item("Atc.Rest", "https://github.com/atc-net/atc", 639426),
        Item("Atc.XUnit", "https://github.com/atc-net/atc", 120000),
        Item("Atc.Rest.ApiGenerator", "https://github.com/atc-net/atc-rest-api-generator", 268524),
        Item("atc-rest-api-generator", "https://github.com/atc-net/atc-rest-api-generator", 28303),
        Item("Atc.Rest.Api.SourceGenerator", "https://github.com/atc-net/atc-rest-api-source-generator", 15646),
        Item("atc-rest-api-gen", "https://github.com/atc-net/atc-rest-api-source-generator", 8616),
        Item("Atc.Rest.Api.Client.Testing", "https://github.com/atc-net/atc-rest-api-source-generator", 280),
        Item("Atc.Kusto", "https://github.com/atc-net/atc-kusto", 9821),
        Item("atc-kusto", "https://github.com/atc-net/atc-kusto", 2140),
        Item("atc-claude-kanban", "https://github.com/atc-net/atc-claude-kanban", 12333),
        Item("atc-coding-rules-updater", "https://github.com/atc-net/atc-coding-rules-updater", 50000),
        Item("Atc.Azure.Messaging", "https://github.com/atc-net/atc-azure-messaging/blob/main/README.md", 1000),
        Item("Atc.Other", "https://github.com/someone-else/atc-kusto", 5),
        Item("Atc.NoUrl", null, 5),
    ];

    [Theory]
    [InlineData("https://github.com/atc-net/atc-kusto", "atc-kusto")]
    [InlineData("https://github.com/atc-net/atc-kusto/", "atc-kusto")]
    [InlineData("https://github.com/atc-net/atc-kusto.git", "atc-kusto")]
    [InlineData("https://GitHub.com/ATC-NET/Atc-Kusto", "Atc-Kusto")]
    [InlineData("https://www.github.com/atc-net/atc-kusto", "atc-kusto")]
    [InlineData("https://github.com/atc-net/atc-azure-messaging/blob/main/README.md", "atc-azure-messaging")]
    [InlineData("https://github.com/someone-else/atc-kusto", null)]
    [InlineData("https://example.com/atc-net/atc-kusto", null)]
    [InlineData("https://github.com/atc-net", null)]
    [InlineData("not a url", null)]
    [InlineData(null, null)]
    [SuppressMessage("Design", "CA1054:URI-like parameters should not be strings", Justification = "Test input mirrors the raw NuGet string.")]
    public void GetRepositoryNameFromProjectUrl(
        string? projectUrl,
        string? expected)
        => RepositoryNugetPackageHelper.GetRepositoryNameFromProjectUrl(projectUrl).Should().Be(expected);

    [Fact]
    public void SourceGenerator_HasThreePackages_WithGeneratorAsPrimary()
    {
        var packages = Get("atc-rest-api-source-generator", "atc.rest.api.source.generator");

        packages.Select(x => x.PackageId).Should().Equal(
            "Atc.Rest.Api.SourceGenerator",
            "Atc.Rest.Api.Client.Testing",
            "atc-rest-api-gen");
        packages.Single(x => x.PackageId == "atc-rest-api-gen").IsTool.Should().BeTrue();
    }

    [Fact]
    public void Kusto_PrefersLibraryMatchingTheRepositoryName_OverTheTool()
    {
        var packages = Get("atc-kusto", "atc.kusto");

        packages.Select(x => x.PackageId).Should().Equal("Atc.Kusto", "atc-kusto");
        packages[0].IsTool.Should().BeFalse();
        packages[1].IsTool.Should().BeTrue();
    }

    [Fact]
    public void Atc_PicksPackageMatchingTheRepositoryName_ThenMostDownloaded()
    {
        var packages = Get("atc", "atc");

        packages.Select(x => x.PackageId).Should().Equal("Atc", "Atc.Rest", "Atc.XUnit");
    }

    [Fact]
    public void RestApiGenerator_FallsBackToMostDownloadedLibrary_WhenDotNameMatchesNothing()
    {
        // DotName for atc-rest-api-generator is rewritten to "atc-api-gen", which is not a package
        var packages = Get("atc-rest-api-generator", "atc-api-gen");

        packages.Select(x => x.PackageId).Should().Equal("Atc.Rest.ApiGenerator", "atc-rest-api-generator");
    }

    [Fact]
    public void ToolOnlyRepository_UsesTheToolAsPrimary()
    {
        var packages = Get("atc-coding-rules-updater", "atc-coding-rules-updater");

        packages.Should().ContainSingle();
        packages[0].IsTool.Should().BeTrue();
    }

    [Fact]
    public void ProjectUrlPointingAtAFile_StillMatchesTheRepository()
        => Get("atc-azure-messaging", "atc.azure.messaging")
            .Select(x => x.PackageId)
            .Should().Equal("Atc.Azure.Messaging");

    [Fact]
    public void GetTotalDownloadsByRepository_SumsAllPackagesPerRepository()
    {
        var downloads = RepositoryNugetPackageHelper.GetTotalDownloadsByRepository(SearchItems);

        downloads["atc-rest-api-source-generator"].Should().Be(15646 + 8616 + 280);
        downloads["ATC-KUSTO"].Should().Be(9821 + 2140);
        downloads.Should().NotContainKey("atc-net.github.io");
    }

    [Fact]
    public void RepositoryWithoutPackages_ReturnsEmpty()
        => Get("atc-net.github.io", "atc.net.github.io").Should().BeEmpty();

    private static List<RepositoryNugetPackage> Get(
        string repositoryName,
        string repositoryDotName)
        => RepositoryNugetPackageHelper.GetPackagesForRepository(
            repositoryName,
            repositoryDotName,
            SearchItems,
            ToolPackageIds);

    private static NugetPackageSearchItem Item(
        string id,
        string? projectUrl,
        long totalDownloads)
        => new()
        {
            Id = id,
            Version = "1.0.0",
            ProjectUrl = projectUrl,
            TotalDownloads = totalDownloads,
        };
}