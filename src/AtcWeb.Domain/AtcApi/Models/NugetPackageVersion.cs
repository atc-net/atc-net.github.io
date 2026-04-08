namespace AtcWeb.Domain.AtcApi.Models;

public sealed record NugetPackageVersion(
    string Version,
    long Downloads);