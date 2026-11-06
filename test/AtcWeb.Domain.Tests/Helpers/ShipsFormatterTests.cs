namespace AtcWeb.Domain.Tests.Helpers;

public sealed class ShipsFormatterTests
{
    [Theory]
    [InlineData(3, 1, "3 libraries · 1 CLI tool")]
    [InlineData(1, 0, "1 library")]
    [InlineData(0, 1, "1 CLI tool")]
    [InlineData(12, 0, "12 libraries")]
    [InlineData(2, 2, "2 libraries · 2 CLI tools")]
    [InlineData(0, 0, null)]
    public void ForRepository(
        int libraries,
        int tools,
        string? expected)
        => ShipsFormatter
            .ForRepository(libraries, tools)
            .Should()
            .Be(expected);

    [Theory]
    [InlineData(4, "4 downloads")]
    [InlineData(1, "1 download")]
    [InlineData(0, null)]
    public void ForAtcApp(
        int downloads,
        string? expected)
        => ShipsFormatter
            .ForAtcApp(downloads)
            .Should()
            .Be(expected);
}