namespace AtcWeb.Components.Repository;

public partial class RepositoryDetailHeader : ComponentBase
{
    [Parameter]
    public AtcRepository? Repository { get; set; }

    private List<StatTile> GetStatTiles()
    {
        var tiles = new List<StatTile>();
        if (Repository is null)
        {
            return tiles;
        }

        if (Repository.NugetPackages.Count > 0)
        {
            var downloadsTitle = Repository.NugetPackages.Count > 1
                ? $"Total across {Repository.NugetPackages.Count} packages"
                : null;

            tiles.Add(new StatTile(
                "Downloads",
                Repository.NugetTotalDownloads.ToString("N0", CultureInfo.CurrentCulture),
                Icons.Material.Filled.CloudDownload,
                Href: null,
                Title: downloadsTitle));
        }

        tiles.Add(new StatTile(
            "Stars",
            Repository.BaseData.StargazersCount.ToString(CultureInfo.CurrentCulture),
            Icons.Material.Filled.Star,
            $"{Repository.Url}/stargazers"));

        tiles.Add(new StatTile(
            "Issues",
            Repository.BaseData.OpenIssuesCount.ToString(CultureInfo.CurrentCulture),
            Icons.Material.Filled.BugReport,
            $"{Repository.Url}/issues"));

        if (TryFormatDate(Repository.NugetInfo?.Published, out var published))
        {
            tiles.Add(new StatTile(
                "Latest release",
                published,
                Icons.Material.Filled.CalendarMonth,
                Href: null));
        }

        return tiles;
    }

    private List<string> GetFrameworkLabels()
        => Repository?.NugetInfo?.TargetFrameworks
               .Select(x => x.Label)
               .Where(x => !string.IsNullOrWhiteSpace(x))
               .Distinct(StringComparer.Ordinal)
               .ToList()
           ?? [];

    private List<string> GetMergedTags()
    {
        var tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (Repository?.BaseData.Topics is not null)
        {
            foreach (var topic in Repository.BaseData.Topics)
            {
                tags.Add(topic);
            }
        }

        if (Repository?.NugetInfo?.Tags is not null)
        {
            foreach (var tag in Repository.NugetInfo.Tags)
            {
                tags.Add(tag);
            }
        }

        return tags.OrderBy(t => t, StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static bool TryFormatDate(
        string? dateString,
        out string formatted)
    {
        if (!string.IsNullOrEmpty(dateString) &&
            DateTimeOffset.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            formatted = date.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);
            return true;
        }

        formatted = string.Empty;
        return false;
    }

    private sealed record StatTile(
        string Label,
        string Value,
        string Icon,
        string? Href,
        string? Title = null);
}