namespace AtcWeb.Domain.GitHub.Models;

public sealed record RepositoryNugetPackage(
    string PackageId,
    string Version,
    string Description,
    long TotalDownloads,
    bool IsTool);