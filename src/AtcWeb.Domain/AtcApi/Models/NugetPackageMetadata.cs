namespace AtcWeb.Domain.AtcApi.Models;

public sealed class NugetPackageMetadata
{
    public string PackageId { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public long TotalDownloads { get; set; }

    public string? IconUrl { get; set; }

    public string? Published { get; set; }

    public bool IsTool { get; set; }

    public List<string> Tags { get; set; } = [];

    public List<NugetTargetFramework> TargetFrameworks { get; set; } = [];

    public List<NugetDependencyGroup> DependencyGroups { get; set; } = [];
}