namespace AtcWeb.Domain.AtcApi.Models;

public class AtcAppRelease
{
    public string TagName { get; set; } = string.Empty;

    public string? Name { get; set; }

    /// <summary>
    /// Release notes as markdown.
    /// </summary>
    public string? Body { get; set; }

    public string HtmlUrl { get; set; } = string.Empty;

    public DateTimeOffset? PublishedAt { get; set; }

    public List<AtcAppAsset> Assets { get; set; } = [];
}