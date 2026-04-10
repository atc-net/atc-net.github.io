namespace AtcWeb.Domain.AtcApi.Models;

public record NugetVersionEntry(
    string Version,
    string? Published,
    bool IsPrerelease,
    long Downloads);