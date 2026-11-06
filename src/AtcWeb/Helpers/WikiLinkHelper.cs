namespace AtcWeb.Helpers;

public static class WikiLinkHelper
{
    private static readonly Regex RelativeLink = new(
        @"<a href=""(?<slug>[^""/#:]+)(?<anchor>#[^""]*)?""",
        RegexOptions.ExplicitCapture,
        TimeSpan.FromSeconds(1));

    /// <summary>
    /// Wiki pages link to each other by slug, e.g. <c>[Install CameraWall](Install-CameraWall)</c>.
    /// Points those links at the GitHub wiki page; all other links are left as they are.
    /// </summary>
    /// <param name="html">The rendered (and sanitized) wiki page.</param>
    /// <param name="owner">The repository owner.</param>
    /// <param name="repositoryName">The repository name.</param>
    /// <param name="pageSlugs">The slugs of the wiki's pages.</param>
    /// <returns>The html with page links rewritten.</returns>
    public static string RewritePageLinks(
        string html,
        string owner,
        string repositoryName,
        IReadOnlyCollection<string> pageSlugs)
    {
        ArgumentNullException.ThrowIfNull(html);
        ArgumentNullException.ThrowIfNull(pageSlugs);

        var slugs = new HashSet<string>(pageSlugs, StringComparer.OrdinalIgnoreCase);

        return RelativeLink.Replace(html, match =>
        {
            var slug = match.Groups["slug"].Value;
            return slugs.Contains(slug)
                ? $@"<a target=""_blank"" href=""https://github.com/{owner}/{repositoryName}/wiki/{slug}{match.Groups["anchor"].Value}"""
                : match.Value;
        });
    }
}