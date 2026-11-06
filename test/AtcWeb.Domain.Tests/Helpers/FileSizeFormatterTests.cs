namespace AtcWeb.Domain.Tests.Helpers;

public sealed class FileSizeFormatterTests
{
    [Theory]
    [InlineData(151736384, "145 MB")]
    [InlineData(19751106, "19 MB")]
    [InlineData(2114, "2 KB")]
    [InlineData(512, "512 B")]
    [InlineData(0, "0 B")]
    public void Format_ReturnsReadableSize(
        long bytes,
        string expected)
        => FileSizeFormatter
            .Format(bytes)
            .Should()
            .Be(expected);
}