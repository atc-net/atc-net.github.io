namespace AtcWeb.Domain.Nuget.Models;

public class NugetSearchResultDataPackageMetadata
{
    public string Id { get; set; }

    public string Version { get; set; }

    public string Description { get; set; }

    public override string ToString()
        => $"{nameof(Id)}: {Id}, {nameof(Version)}: {Version}, {nameof(Description)}: {Description}";
}