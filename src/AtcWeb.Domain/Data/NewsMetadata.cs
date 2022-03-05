namespace AtcWeb.Domain.Data;

public static class NewsMetadata
{
    private static readonly List<NewsItem> News = new ()
    {
        Create(2022, 3, 3, NewsItemAction.FeatureNew, "atc-net.github.io", "News (this)", string.Empty),
        Create(2022, 2, 26, NewsItemAction.FeatureNew, "atc-net.github.io", "Manuals -> DevOps Playbook", string.Empty),
        Create(2022, 1, 31, NewsItemAction.FeatureNew, "atc-net.github.io", "Utilizing Atc.Blazor.ColorThemePreference", string.Empty),
        Create(2022, 2, 23, NewsItemAction.FeatureNew, "atc-docs", "DevOps Playbook", string.Empty),
        Create(2019, 12, 10, NewsItemAction.Organization, "atc-net", "Was born", string.Empty),
    };

    private static NewsItem Create(
        int year,
        int month,
        int day,
        NewsItemAction newsItemAction,
        string repositoryName,
        string title,
        string body)
        => new (
            new DateTimeOffset(year, month, day, 0, 0, 0, TimeSpan.Zero),
            newsItemAction,
            repositoryName,
            title,
            body);

    public static IEnumerable<NewsItem> GetNews()
        => News
            .OrderByDescending(x => x.Time)
            .ToArray();
}