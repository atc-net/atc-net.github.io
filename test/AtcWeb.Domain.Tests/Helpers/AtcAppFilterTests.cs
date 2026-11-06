namespace AtcWeb.Domain.Tests.Helpers;

public sealed class AtcAppFilterTests
{
    private static readonly AtcApp LogViewer = new()
    {
        Name = "atc-soft-log-viewer",
        DisplayName = "Log Viewer",
        Description = "Read log4net, NLog and Serilog files.",
    };

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Matches_EmptyFilter_MatchesAll(string? filter)
        => AtcAppFilter
            .Matches(LogViewer, filter)
            .Should()
            .BeTrue();

    [Theory]
    [InlineData("log")]
    [InlineData("VIEWER")]
    [InlineData("atc-soft-log")]
    [InlineData("serilog")]
    public void Matches_NameOrDescription_IgnoringCase(string filter)
        => AtcAppFilter
            .Matches(LogViewer, filter)
            .Should()
            .BeTrue();

    [Fact]
    public void Matches_Unrelated_ReturnsFalse()
        => AtcAppFilter
            .Matches(LogViewer, "camera")
            .Should()
            .BeFalse();
}