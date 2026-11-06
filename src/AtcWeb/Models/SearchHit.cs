namespace AtcWeb.Models;

/// <summary>
/// One searchable item: an app or a component.
/// </summary>
public sealed record SearchHit(
    SiteWorld World,
    string Name,
    string Title,
    string? Subtitle)
{
    public string Href
        => World == SiteWorld.AtcApps
            ? $"/apps/{Name}"
            : $"/libraries/{Name}";
}