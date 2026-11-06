namespace AtcWeb.Domain.Tests.Helpers;

public sealed class WikiLinkHelperTests
{
    private static readonly string[] Slugs = ["Home", "Install-CameraWall", "Settings-Reference"];

    [Fact]
    public void Rewrite_LinkToWikiPage_PointsAtTheWiki()
        => WikiLinkHelper
            .RewritePageLinks("<p><a href=\"Install-CameraWall\">Install CameraWall</a></p>", "atc-net-apps", "atc-soft-video-surveillance", Slugs)
            .Should()
            .Be("<p><a target=\"_blank\" href=\"https://github.com/atc-net-apps/atc-soft-video-surveillance/wiki/Install-CameraWall\">Install CameraWall</a></p>");

    [Fact]
    public void Rewrite_KeepsAnchorAndIgnoresCase()
        => WikiLinkHelper
            .RewritePageLinks("<a href=\"settings-reference#ports\">x</a>", "atc-net-apps", "repo", Slugs)
            .Should()
            .Be("<a target=\"_blank\" href=\"https://github.com/atc-net-apps/repo/wiki/settings-reference#ports\">x</a>");

    [Theory]
    [InlineData("<a href=\"src/Program.cs\">x</a>")]
    [InlineData("<a href=\"https://example.test\">x</a>")]
    [InlineData("<a href=\"#ports\">x</a>")]
    [InlineData("<a href=\"Unknown-Page\">x</a>")]
    public void Rewrite_OtherLinks_AreUntouched(string html)
        => WikiLinkHelper
            .RewritePageLinks(html, "atc-net-apps", "repo", Slugs)
            .Should()
            .Be(html);
}