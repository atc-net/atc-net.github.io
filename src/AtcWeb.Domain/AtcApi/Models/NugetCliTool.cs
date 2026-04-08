namespace AtcWeb.Domain.AtcApi.Models;

public sealed class NugetCliTool
{
    public string Id { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? ProjectUrl { get; set; }

    public string? IconUrl { get; set; }

    public long TotalDownloads { get; set; }

    public List<string> Tags { get; set; } = [];

    public List<NugetPackageVersion> Versions { get; set; } = [];
}