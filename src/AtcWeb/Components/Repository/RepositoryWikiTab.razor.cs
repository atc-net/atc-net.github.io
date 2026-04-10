namespace AtcWeb.Components.Repository;

public partial class RepositoryWikiTab : ComponentBase
{
    private static readonly MarkdownPipeline MarkdownPipelineInstance = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .UseEmojiAndSmiley()
        .Build();

    private static readonly Regex WikiLinkPattern = new(
        @"\[\[(?<display>[^|\]]+?)(?:\|(?<slug>[^\]]+))?\]\]",
        RegexOptions.Compiled,
        TimeSpan.FromSeconds(1));

    private static readonly Regex GroupHeadingPattern = new(
        @"^##\s+(?<title>.+)$",
        RegexOptions.Compiled | RegexOptions.Multiline,
        TimeSpan.FromSeconds(1));

    private WikiPage? selectedPage;
    private Dictionary<string, string>? sidebarTitleMap;
    private Dictionary<string, string>? sidebarGroupTitleMap;

    [Parameter]
    public AtcRepository? Repository { get; set; }

    protected override void OnParametersSet()
    {
        if (Repository?.Wiki.HasWiki == true)
        {
            BuildSidebarTitleMaps();

            if (selectedPage is null && Repository.Wiki.Pages.Count > 0)
            {
                selectedPage = Repository.Wiki.Pages[0];
            }
        }
    }

    private void SelectPage(WikiPage page)
    {
        selectedPage = page;
        StateHasChanged();
    }

    private static string? GetWikiPageTitle(WikiPage page) =>
        MarkdownHeadingHelper.ExtractFirstH1Title(page.RawContent, MarkdownPipelineInstance)
        ?? page.Title;

    private string GetNavTitle(WikiPage page)
    {
        if (sidebarTitleMap is not null &&
            sidebarTitleMap.TryGetValue(page.Slug, out var emojiTitle))
        {
            return emojiTitle;
        }

        return page.Title;
    }

    private string GetGroupTitle(string groupName)
    {
        if (sidebarGroupTitleMap is not null)
        {
            // Try exact match first
            if (sidebarGroupTitleMap.TryGetValue(groupName, out var emojiGroupTitle))
            {
                return emojiGroupTitle;
            }

            // GroupName from API may have garbled emoji remnants — strip and retry
            var stripped = StripLeadingEmoji(groupName);
            if (sidebarGroupTitleMap.TryGetValue(stripped, out emojiGroupTitle))
            {
                return emojiGroupTitle;
            }
        }

        return groupName;
    }

    private List<KeyValuePair<string, List<WikiPage>>> GetGroupedPages()
    {
        if (Repository?.Wiki.Pages is null)
        {
            return [];
        }

        return Repository.Wiki.Pages
            .GroupBy(p => p.GroupName ?? string.Empty, StringComparer.Ordinal)
            .Select(g => new KeyValuePair<string, List<WikiPage>>(g.Key, g.ToList()))
            .ToList();
    }

    /// <summary>
    /// Parses the wiki sidebar markdown to build mappings from slug/groupName
    /// to the emoji-prefixed display title.
    /// </summary>
    private void BuildSidebarTitleMaps()
    {
        sidebarTitleMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        sidebarGroupTitleMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var sidebar = Repository?.Wiki.RawSidebar;
        if (string.IsNullOrEmpty(sidebar))
        {
            return;
        }

        // Extract group headings: "## 📖 Getting Started" → map "Getting Started" → "📖 Getting Started"
        foreach (Match match in GroupHeadingPattern.Matches(sidebar))
        {
            var fullTitle = match.Groups["title"].Value.Trim();

            // Strip emoji prefix to get the plain group name for lookup
            var plainName = StripLeadingEmoji(fullTitle);
            if (!string.IsNullOrEmpty(plainName) &&
                !plainName.Equals(fullTitle, StringComparison.Ordinal))
            {
                sidebarGroupTitleMap[plainName] = fullTitle;
            }
        }

        // Extract wiki links: "🚀 [[Getting Started with Basic]]" or "💼 [[Display|Slug]]"
        foreach (var line in sidebar.Split('\n'))
        {
            var trimmed = line.TrimStart('-', ' ', '*');
            var linkMatch = WikiLinkPattern.Match(trimmed);
            if (!linkMatch.Success)
            {
                continue;
            }

            var display = linkMatch.Groups["display"].Value.Trim();
            var slug = linkMatch.Groups["slug"].Success
                ? linkMatch.Groups["slug"].Value.Trim()
                : display.Replace(' ', '-');

            // The text before the [[ is the emoji prefix
            var prefixEnd = trimmed.IndexOf("[[", StringComparison.Ordinal);
            var prefix = prefixEnd > 0
                ? trimmed[..prefixEnd].Trim()
                : string.Empty;

            var navTitle = string.IsNullOrEmpty(prefix)
                ? display
                : $"{prefix} {display}";

            sidebarTitleMap[slug] = navTitle;
        }
    }

    private static string StripLeadingEmoji(string text)
    {
        // Skip leading characters until we hit a letter or digit
        var i = 0;
        while (i < text.Length && !char.IsLetterOrDigit(text[i]))
        {
            // Handle surrogate pairs (emojis are often 2 chars)
            if (char.IsHighSurrogate(text[i]) && i + 1 < text.Length)
            {
                i += 2;
            }
            else
            {
                i++;
            }
        }

        return text[i..].TrimStart();
    }
}