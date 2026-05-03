namespace AtcWeb.Domain.AtcApi.Models.Compliance;

public sealed class AnalyzerPackageRef
{
    public string PackageId { get; init; } = string.Empty;

    public string Version { get; init; } = string.Empty;

    public bool IsLatest { get; init; }
}