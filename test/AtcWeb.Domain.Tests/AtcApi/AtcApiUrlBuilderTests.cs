namespace AtcWeb.Domain.Tests.AtcApi;

public sealed class AtcApiUrlBuilderTests
{
    [Fact]
    public void ResolveAtcAppImageUrl_Relative_UsesExternalProxy()
        => AtcApiUrlBuilder
            .ResolveAtcAppImageUrl(NewAtcApp(), "docs/main.png")
            .Should()
            .Be($"{AtcApiConstants.BaseAddress}/github/raw/atc-net-apps/atc-soft-log-viewer?branch=main&filePath=docs%2Fmain.png");

    [Fact]
    public void ResolveAtcAppImageUrl_Absolute_Unchanged()
        => AtcApiUrlBuilder
            .ResolveAtcAppImageUrl(NewAtcApp(), "https://github.com/user-attachments/assets/abc")
            .Should()
            .Be("https://github.com/user-attachments/assets/abc");

    [Fact]
    public void IsExternalProxyOwner_AtcAppsOrg_IsAllowed()
        => AtcApiUrlBuilder
            .IsExternalProxyOwner("atc-net-apps")
            .Should()
            .BeTrue();

    private static AtcApp NewAtcApp()
        => new()
        {
            Name = "atc-soft-log-viewer",
            Owner = "atc-net-apps",
            DefaultBranch = "main",
        };
}