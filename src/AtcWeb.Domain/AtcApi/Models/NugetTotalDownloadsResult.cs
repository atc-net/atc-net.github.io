namespace AtcWeb.Domain.AtcApi.Models;

public sealed record NugetTotalDownloadsResult(
    long TotalDownloads,
    int PackageCount,
    string FormattedDownloads);