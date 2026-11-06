namespace AtcWeb.Domain.Tests.Models;

public sealed class AtcAppCardModelTests
{
    [Fact]
    public void AtcAppCardModel_NoRelease_IsComingSoon()
    {
        var model = AtcAppCardModel.From(NewAtcApp("atc-soft-json-viewer", "Json Viewer"));

        model.IsComingSoon.Should().BeTrue();
        model.Version.Should().BeNull();
    }

    [Fact]
    public void AtcAppCardModel_SinglePrimaryAsset_DirectDownload()
    {
        var app = NewAtcApp("atc-soft-log-viewer", "Log Viewer");
        app.LatestRelease = NewRelease(
            ("AtcSoft.Log.Viewer.Installer.msi", false),
            ("AtcSoft.Log.Viewer.Installer.msi.sha256", true));

        var model = AtcAppCardModel.From(app);

        model.IsComingSoon.Should().BeFalse();
        model.DownloadCount.Should().Be(1);
        model.IsDirectDownload.Should().BeTrue();
        model.DownloadHref.Should().Be("https://example.test/AtcSoft.Log.Viewer.Installer.msi");
        model.Version.Should().Be("v1.0.0");
    }

    [Fact]
    public void AtcAppCardModel_SeveralPrimaryAssets_LinksToAtcAppPage()
    {
        var app = NewAtcApp("atc-soft-video-surveillance", "Video Surveillance");
        app.LatestRelease = NewRelease(("Server.msi", false), ("Client.msi", false));

        var model = AtcAppCardModel.From(app);

        model.IsDirectDownload.Should().BeFalse();
        model.DownloadCount.Should().Be(2);
        model.DownloadHref.Should().Be("/apps/atc-soft-video-surveillance#download");
    }

    [Fact]
    public void AtcAppCardModel_NoScreenshots_UsesSocialImage()
    {
        var model = AtcAppCardModel.From(NewAtcApp("atc-soft-log-viewer", "Log Viewer"));

        model.HeroImageUrl.Should().Be("https://opengraph.githubassets.com/1/atc-net-apps/atc-soft-log-viewer");
        model.HasScreenshot.Should().BeFalse();
    }

    [Fact]
    public void AtcAppCardModel_WithScreenshot_HasScreenshot()
    {
        var app = NewAtcApp("atc-soft-log-viewer", "Log Viewer");
        app.Screenshots = ["https://example.test/main.png"];

        var model = AtcAppCardModel.From(app);

        model.HasScreenshot.Should().BeTrue();
        model.HeroImageUrl.Should().Be("https://example.test/main.png");
    }

    private static AtcApp NewAtcApp(
        string name,
        string displayName)
        => new()
        {
            Name = name,
            DisplayName = displayName,
            Owner = "atc-net-apps",
            DefaultBranch = "main",
            HtmlUrl = $"https://github.com/atc-net-apps/{name}",
            SocialImageUrl = $"https://opengraph.githubassets.com/1/atc-net-apps/{name}",
        };

    private static AtcAppRelease NewRelease(
        params (string FileName, bool IsSecondary)[] assets)
        => new()
        {
            TagName = "v1.0.0",
            Assets = assets
                .Select(x => new AtcAppAsset
                {
                    FileName = x.FileName,
                    IsSecondary = x.IsSecondary,
                    DownloadUrl = $"https://example.test/{x.FileName}",
                })
                .ToList(),
        };
}