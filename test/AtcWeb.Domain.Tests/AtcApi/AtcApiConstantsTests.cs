namespace AtcWeb.Domain.Tests.AtcApi;

public sealed class AtcApiConstantsTests
{
    [Fact]
    public void Configure_Null_KeepsProduction()
    {
        AtcApiConstants.Configure(null);

        AtcApiConstants.BaseAddress
            .Should()
            .Be(AtcApiConstants.ProductionBaseAddress);
    }

    [Fact]
    public void Configure_TrimsTrailingSlash()
    {
        try
        {
            AtcApiConstants.Configure("https://localhost:51612/");

            AtcApiConstants.BaseAddress
                .Should()
                .Be("https://localhost:51612");
        }
        finally
        {
            AtcApiConstants.Configure(AtcApiConstants.ProductionBaseAddress);
        }
    }
}