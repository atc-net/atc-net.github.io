namespace AtcWeb.Domain.AtcApi.Models;

public sealed class NugetCliToolSearchResult
{
    public int TotalHits { get; set; }

    public List<NugetCliTool> Data { get; set; } = [];
}