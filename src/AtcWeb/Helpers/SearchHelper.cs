namespace AtcWeb.Helpers;

public static class SearchHelper
{
    /// <summary>
    /// Case-insensitive search over name, title and subtitle. Apps come before libraries.
    /// </summary>
    /// <param name="text">The search text.</param>
    /// <param name="all">Everything that can be found.</param>
    /// <param name="maxPerWorld">The maximum number of hits per world.</param>
    /// <returns>The matching hits.</returns>
    public static IReadOnlyList<SearchHit> Search(
        string? text,
        IReadOnlyList<SearchHit> all,
        int maxPerWorld = 5)
    {
        ArgumentNullException.ThrowIfNull(all);

        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        var term = text.Trim();
        var matches = all
            .Where(x => x.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                        x.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                        (x.Subtitle?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false))
            .ToList();

        return matches
            .Where(x => x.World == SiteWorld.AtcApps)
            .Take(maxPerWorld)
            .Concat(matches.Where(x => x.World != SiteWorld.AtcApps).Take(maxPerWorld))
            .ToList();
    }
}