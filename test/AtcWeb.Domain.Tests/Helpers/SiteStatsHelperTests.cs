namespace AtcWeb.Domain.Tests.Helpers;

public sealed class SiteStatsHelperTests
{
    [Fact]
    public void CountPackages_SplitsLibrariesAndCliTools()
    {
        var (libraries, cliTools) = SiteStatsHelper.CountPackages(
            ["Atc", "Atc.Rest", "atc-coding-rules-updater"],
            ["atc-coding-rules-updater"]);

        libraries.Should().Be(2);
        cliTools.Should().Be(1);
    }

    [Fact]
    public void CountPackages_MatchesCliToolIdsIgnoringCase()
    {
        var (libraries, cliTools) = SiteStatsHelper.CountPackages(
            ["Atc", "Atc-Coding-Rules-Updater"],
            ["atc-coding-rules-updater"]);

        libraries.Should().Be(1);
        cliTools.Should().Be(1);
    }

    [Fact]
    public void CountPackages_IgnoresCliToolsMissingFromThePackages()
    {
        var (libraries, cliTools) = SiteStatsHelper.CountPackages(
            ["Atc"],
            ["some-other-tool"]);

        libraries.Should().Be(1);
        cliTools.Should().Be(0);
    }

    [Fact]
    public void CountAtcApps_SplitsReadyAndComingSoon()
    {
        var released = new AtcApp
        {
            Name = "atc-soft-log-viewer",
            DisplayName = "Log Viewer",
            LatestRelease = new AtcAppRelease { TagName = "v1.0.0" },
        };
        var unreleased = new AtcApp
        {
            Name = "atc-soft-json-viewer",
            DisplayName = "Json Viewer",
        };

        var (ready, comingSoon) = SiteStatsHelper.CountAtcApps(
            new[] { released, unreleased, unreleased }.Select(AtcAppCardModel.From));

        ready.Should().Be(1);
        comingSoon.Should().Be(2);
    }
}