namespace AtcWeb.Helpers;

public static class DisplayFormatHelper
{
    /// <summary>
    /// Compact download count, e.g. 9.8K or 1.3M.
    /// </summary>
    public static string FormatDownloads(long downloads)
        => downloads switch
        {
            >= 1_000_000 => $"{downloads / 1_000_000.0:F1}M",
            >= 1_000 => $"{downloads / 1_000.0:F1}K",
            _ => downloads.ToString("N0", CultureInfo.InvariantCulture),
        };

    /// <summary>
    /// Short relative time, e.g. "3d ago" or "2mo ago".
    /// </summary>
    public static string FormatRelativeTime(DateTimeOffset? timestamp)
    {
        if (timestamp is null)
        {
            return string.Empty;
        }

        var diff = DateTimeOffset.UtcNow - timestamp.Value;

        return diff.TotalHours switch
        {
            < 1 => "just now",
            < 24 => $"{(int)diff.TotalHours}h ago",
            < 48 => "yesterday",
            _ when diff.TotalDays < 7 => $"{(int)diff.TotalDays}d ago",
            _ when diff.TotalDays < 30 => $"{(int)(diff.TotalDays / 7)}w ago",
            _ when diff.TotalDays < 365 => $"{(int)(diff.TotalDays / 30)}mo ago",
            _ => $"{(int)(diff.TotalDays / 365)}y ago",
        };
    }
}