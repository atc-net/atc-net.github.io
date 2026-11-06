namespace AtcWeb.Domain.Tests.State;

public sealed class SiteWorldHelperTests
{
    [Theory]
    [InlineData("apps", SiteWorld.AtcApps)]
    [InlineData("apps/atc-soft-log-viewer", SiteWorld.AtcApps)]
    [InlineData("libraries", SiteWorld.Libraries)]
    [InlineData("libraries/atc-cosmos", SiteWorld.Libraries)]
    [InlineData("components", SiteWorld.Libraries)]
    [InlineData("components/atc-cosmos?tab=wiki", SiteWorld.Libraries)]
    [InlineData("components?category=IoT", SiteWorld.Libraries)]
    [InlineData("repository/atc-cosmos", SiteWorld.Libraries)]
    [InlineData("", SiteWorld.None)]
    [InlineData("news", SiteWorld.None)]
    [InlineData("appsx", SiteWorld.None)]
    public void FromRelativePath(
        string path,
        SiteWorld expected)
        => SiteWorldHelper
            .FromRelativePath(path)
            .Should()
            .Be(expected);
}