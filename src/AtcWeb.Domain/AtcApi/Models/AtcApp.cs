namespace AtcWeb.Domain.AtcApi.Models;

public class AtcApp
{
    public string Name { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Owner { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string HtmlUrl { get; set; } = string.Empty;

    public string DefaultBranch { get; set; } = "main";

    public DateTimeOffset? PushedAt { get; set; }

    public List<string> Topics { get; set; } = [];

    /// <summary>
    /// Screenshot references from the README: absolute URLs or repository-relative paths.
    /// </summary>
    public List<string> Screenshots { get; set; } = [];

    public string SocialImageUrl { get; set; } = string.Empty;

    public string? DocsSiteUrl { get; set; }

    public AtcAppRelease? LatestRelease { get; set; }

    public override string ToString()
        => $"{nameof(Name)}: {Name}, {nameof(LatestRelease)}: {LatestRelease?.TagName}";
}