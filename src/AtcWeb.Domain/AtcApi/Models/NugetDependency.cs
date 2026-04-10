namespace AtcWeb.Domain.AtcApi.Models;

public record NugetDependency(
    string Id,
    string Range,
    string DisplayRange);