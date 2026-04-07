namespace AtcWeb.Helpers;

public static class MarkdownHeadingHelper
{
    private static readonly Regex EmojiRegex = new(
        @"[\p{So}\p{Cs}]+\s*",
        RegexOptions.Compiled,
        TimeSpan.FromSeconds(5));

    [SuppressMessage("Performance", "MA0023:Add RegexOptions.ExplicitCapture", Justification = "Capture groups are used in heading ID replacements.")]
    private static readonly Regex HeadingIdRegex = new(
        @"<h(2)(\s[^>]*)?\s*id=""[^""]*""([^>]*)>",
        RegexOptions.Compiled,
        TimeSpan.FromSeconds(5));

    [SuppressMessage("Performance", "MA0023:Add RegexOptions.ExplicitCapture", Justification = "Capture groups are used in heading ID injection.")]
    private static readonly Regex HeadingWithoutIdRegex = new(
        @"<h(2)(\s[^>]*)?>",
        RegexOptions.Compiled,
        TimeSpan.FromSeconds(5));

    private static readonly Regex TocHeadingRegex = new(
        @"^#{1,3}\s+[\p{So}\p{Cs}\s]*table\s+of\s+contents?\s*$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled,
        TimeSpan.FromSeconds(5));

    private static readonly Regex TocListItemRegex = new(
        @"^\s*[-*]\s*\[.*\]\(#",
        RegexOptions.Compiled,
        TimeSpan.FromSeconds(5));

    public static List<MarkdownHeadingInfo> ExtractHeadings(
        string markdown,
        MarkdownPipeline pipeline,
        string idPrefix)
    {
        var headings = new List<MarkdownHeadingInfo>();

        if (string.IsNullOrWhiteSpace(markdown))
        {
            return headings;
        }

        var document = Markdown.Parse(markdown, pipeline);
        var usedIds = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var block in document.Descendants<HeadingBlock>())
        {
            if (block.Level is not 2)
            {
                continue;
            }

            var title = ExtractInlineText(block.Inline);
            if (string.IsNullOrWhiteSpace(title))
            {
                continue;
            }

            var slug = GenerateSlug(title, idPrefix);
            slug = EnsureUniqueId(slug, usedIds);

            headings.Add(new MarkdownHeadingInfo
            {
                Level = block.Level,
                Title = title,
                Id = slug,
            });
        }

        return headings;
    }

    [SuppressMessage("Performance", "MA0023:Add RegexOptions.ExplicitCapture", Justification = "Capture groups are used in heading ID replacements.")]
    public static string InjectHeadingIds(
        string html,
        List<MarkdownHeadingInfo> headings)
    {
        ArgumentNullException.ThrowIfNull(headings);

        if (string.IsNullOrWhiteSpace(html) || headings.Count == 0)
        {
            return html;
        }

        var headingIndex = 0;

        // First pass: replace existing id attributes on h2/h3 tags
        var result = HeadingIdRegex.Replace(html, match =>
        {
            if (headingIndex >= headings.Count)
            {
                return match.Value;
            }

            var level = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            var heading = headings[headingIndex];

            if (level != heading.Level)
            {
                return match.Value;
            }

            headingIndex++;
            var before = match.Groups[2].Value;
            var after = match.Groups[3].Value;
            return $"<h{level}{before} id=\"{heading.Id}\"{after}>";
        });

        // Second pass: if some headings were not matched (missing id), inject ids
        if (headingIndex < headings.Count)
        {
            var remainingIndex = headingIndex;
            result = HeadingWithoutIdRegex.Replace(result, match =>
            {
                if (remainingIndex >= headings.Count)
                {
                    return match.Value;
                }

                // Skip tags that already have an id (they were handled in pass 1)
                if (match.Value.Contains("id=\"", StringComparison.Ordinal))
                {
                    return match.Value;
                }

                var level = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                var heading = headings[remainingIndex];

                if (level != heading.Level)
                {
                    return match.Value;
                }

                remainingIndex++;
                var attrs = match.Groups[2].Value;
                return $"<h{level}{attrs} id=\"{heading.Id}\">";
            });
        }

        return result;
    }

    public static string StripInlineToc(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return markdown;
        }

        var lines = markdown.Split('\n');
        var result = new StringBuilder(markdown.Length);
        var i = 0;

        while (i < lines.Length)
        {
            var trimmedLine = lines[i].TrimEnd('\r');

            if (TocHeadingRegex.IsMatch(trimmedLine))
            {
                // Found a TOC heading; skip it and consume following blank/list lines
                i++;

                // Skip blank lines between heading and list
                while (i < lines.Length && string.IsNullOrWhiteSpace(lines[i].TrimEnd('\r')))
                {
                    i++;
                }

                // Skip list items that are anchor links
                while (i < lines.Length)
                {
                    var listLine = lines[i].TrimEnd('\r');
                    if (TocListItemRegex.IsMatch(listLine))
                    {
                        i++;
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(listLine) &&
                        i + 1 < lines.Length &&
                        TocListItemRegex.IsMatch(lines[i + 1].TrimEnd('\r')))
                    {
                        // Blank line between list items
                        i++;
                        continue;
                    }

                    break;
                }

                continue;
            }

            result.Append(trimmedLine);
            result.Append('\n');
            i++;
        }

        return result.ToString();
    }

    public static string? ExtractFirstH1Title(
        string markdown,
        MarkdownPipeline pipeline)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return null;
        }

        var document = Markdown.Parse(markdown, pipeline);
        var h1 = document.Descendants<HeadingBlock>().FirstOrDefault(b => b.Level == 1);
        if (h1 is null)
        {
            return null;
        }

        return ExtractInlineText(h1.Inline);
    }

    private static string ExtractInlineText(ContainerInline? inlines)
    {
        if (inlines is null)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        foreach (var inline in inlines)
        {
            switch (inline)
            {
                case LiteralInline literal:
                    sb.Append(literal.Content);
                    break;
                case CodeInline code:
                    sb.Append(code.Content);
                    break;
                case EmphasisInline emphasis:
                    sb.Append(ExtractInlineText(emphasis));
                    break;
                case LinkInline link:
                    sb.Append(ExtractInlineText(link));
                    break;
                default:
                    // For other inline types, try to get any text content
                    if (inline is ContainerInline container)
                    {
                        sb.Append(ExtractInlineText(container));
                    }

                    break;
            }
        }

        return sb.ToString();
    }

    private static string GenerateSlug(
        string title,
        string idPrefix)
    {
        // Strip emoji from the slug but not from the display title
        var slug = EmojiRegex.Replace(title, string.Empty);

        slug = slug
            .ToLowerInvariant()
            .Replace(' ', '-')
            .Replace(".", string.Empty, StringComparison.Ordinal)
            .Replace(",", string.Empty, StringComparison.Ordinal)
            .Replace(":", string.Empty, StringComparison.Ordinal)
            .Replace("(", string.Empty, StringComparison.Ordinal)
            .Replace(")", string.Empty, StringComparison.Ordinal)
            .Replace("'", string.Empty, StringComparison.Ordinal)
            .Replace("\"", string.Empty, StringComparison.Ordinal)
            .Replace("?", string.Empty, StringComparison.Ordinal)
            .Replace("!", string.Empty, StringComparison.Ordinal)
            .Replace("/", string.Empty, StringComparison.Ordinal);

        // Collapse multiple hyphens
        while (slug.Contains("--", StringComparison.Ordinal))
        {
            slug = slug.Replace("--", "-", StringComparison.Ordinal);
        }

        slug = slug.Trim('-');

        return string.IsNullOrEmpty(idPrefix)
            ? slug
            : $"{idPrefix}-{slug}";
    }

    private static string EnsureUniqueId(
        string id,
        Dictionary<string, int> usedIds)
    {
        if (!usedIds.TryGetValue(id, out var count))
        {
            usedIds[id] = 1;
            return id;
        }

        count++;
        usedIds[id] = count;
        var uniqueId = $"{id}-{count}";

        // Ensure the suffixed version is also tracked
        usedIds[uniqueId] = 1;
        return uniqueId;
    }
}