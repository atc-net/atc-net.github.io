namespace AtcWeb.Domain.AtcApi.Models;

public sealed class NugetPackageSearchResult
{
    public int TotalHits { get; set; }

    public List<NugetPackageSearchItem> Data { get; set; } = [];
}