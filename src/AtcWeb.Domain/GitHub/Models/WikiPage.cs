namespace AtcWeb.Domain.GitHub.Models;

public class WikiPage
{
    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? GroupName { get; set; }

    public string RawContent { get; set; } = string.Empty;

    public bool IsLoaded => !string.IsNullOrEmpty(RawContent);
}