namespace AtcWeb.Domain.GitHub.Models;

public class WikiMetadata
{
    public string RawSidebar { get; set; } = string.Empty;

    public List<WikiPage> Pages { get; set; } = [];

    public bool HasWiki => Pages.Count > 0;
}