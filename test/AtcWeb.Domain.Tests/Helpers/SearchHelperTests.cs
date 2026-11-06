namespace AtcWeb.Domain.Tests.Helpers;

public sealed class SearchHelperTests
{
    private static readonly IReadOnlyList<SearchHit> All =
    [
        new(SiteWorld.Libraries, "atc-wpf", "atc-wpf", "WPF viewer controls"),
        new(SiteWorld.AtcApps, "atc-soft-log-viewer", "Log Viewer", null),
        new(SiteWorld.AtcApps, "atc-soft-json-viewer", "Json Viewer", null),
        new(SiteWorld.Libraries, "atc-cosmos", "atc-cosmos", "Cosmos DB"),
    ];

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Search_Empty_ReturnsEmpty(string? text)
        => SearchHelper
            .Search(text, All)
            .Should()
            .BeEmpty();

    [Fact]
    public void Search_MatchesBothWorlds_AtcAppsFirst()
        => SearchHelper
            .Search("view", All)
            .Select(x => x.Name)
            .Should()
            .Equal("atc-soft-log-viewer", "atc-soft-json-viewer", "atc-wpf");

    [Fact]
    public void Search_IgnoresCase()
        => SearchHelper
            .Search("COSMOS", All)
            .Should()
            .ContainSingle(x => x.Name == "atc-cosmos");

    [Fact]
    public void Search_CapsPerWorld()
    {
        var many = Enumerable
            .Range(1, 8)
            .Select(i => new SearchHit(SiteWorld.AtcApps, $"p{i}", $"Viewer {i}", null))
            .ToList();

        SearchHelper
            .Search("viewer", many)
            .Should()
            .HaveCount(5);
    }
}