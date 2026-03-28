namespace AtcWeb.Domain.GitHub;

public static class GitHubWikiHelper
{
    private const string WikiRawBaseUrl = "https://raw.githubusercontent.com/wiki/atc-net/";

    public static async Task<WikiMetadata> LoadWiki(
        IMemoryCache memoryCache,
        string repositoryName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(memoryCache);

        var metadata = new WikiMetadata();

        var sidebar = await FetchWikiFile(memoryCache, repositoryName, "_Sidebar.md", cancellationToken);
        if (string.IsNullOrEmpty(sidebar))
        {
            return metadata;
        }

        metadata.RawSidebar = sidebar;
        metadata.Pages = ParseSidebar(sidebar);

        var tasks = metadata.Pages.Select(async page =>
        {
            var fileName = $"{page.Slug}.md";
            var content = await FetchWikiFile(memoryCache, repositoryName, fileName, cancellationToken);
            if (!string.IsNullOrEmpty(content))
            {
                page.RawContent = content;
            }
        });

        await Task.WhenAll(tasks);

        return metadata;
    }

    [SuppressMessage("Performance", "MA0023:Add RegexOptions.ExplicitCapture", Justification = "Capture groups are used.")]
    internal static List<WikiPage> ParseSidebar(string rawSidebar)
    {
        var pages = new List<WikiPage>();
        string? currentGroup = null;

        foreach (var line in rawSidebar.Split('\n'))
        {
            var trimmed = line.Trim();

            // Track H2 group headings (e.g. "## 📖 Getting Started")
            if (trimmed.StartsWith("## ", StringComparison.Ordinal))
            {
                currentGroup = StripEmoji(trimmed[3..]).Trim();
                continue;
            }

            // Parse wiki links: [[Display|slug]] or [[Page Name]]
            var matches = Regex.Matches(trimmed, @"\[\[([^\]]+)\]\]", RegexOptions.None, TimeSpan.FromSeconds(5));
            foreach (Match match in matches)
            {
                var inner = match.Groups[1].Value;

                // Skip the Home link
                if ("Home".Equals(inner, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string title;
                string slug;

                var pipeIndex = inner.IndexOf('|', StringComparison.Ordinal);
                if (pipeIndex >= 0)
                {
                    title = inner[..pipeIndex].Trim();
                    slug = inner[(pipeIndex + 1)..].Trim();
                }
                else
                {
                    title = inner.Trim();
                    slug = title.Replace(' ', '-');
                }

                pages.Add(new WikiPage
                {
                    Title = title,
                    Slug = slug,
                    GroupName = currentGroup,
                });
            }
        }

        return pages;
    }

    private static async Task<string> FetchWikiFile(
        IMemoryCache memoryCache,
        string repositoryName,
        string fileName,
        CancellationToken cancellationToken)
    {
        var url = $"{WikiRawBaseUrl}{repositoryName}/{fileName}";
        var cacheKey = $"{CacheConstants.CacheKeyWiki}_{url}";

        if (memoryCache.TryGetValue(cacheKey, out string? cached) && cached is not null)
        {
            return cached;
        }

        try
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return string.Empty;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            memoryCache.Set(cacheKey, content, CacheConstants.AbsoluteExpirationRelativeToNow);
            return content;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string StripEmoji(string text)
        => Regex.Replace(text, @"[\p{So}\p{Cs}]+\s*", string.Empty, RegexOptions.None, TimeSpan.FromSeconds(5)).Trim();
}