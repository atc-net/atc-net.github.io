namespace AtcWeb.Domain.AtcApi.Models;

public sealed class NugetDependencyGroup
{
    public string TargetFramework { get; set; } = string.Empty;

    public string FrameworkLabel { get; set; } = string.Empty;

    public List<NugetDependency> Dependencies { get; set; } = [];
}