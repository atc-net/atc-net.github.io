namespace AtcWeb.Pages;

public class NewsBase : ComponentBase
{
    protected IEnumerable<int> Years { get; set; } = [];

    protected IEnumerable<NewsItem>? News { get; set; }

    protected int? SelectedYear { get; set; }

    protected string? FilterString { get; set; }

    [Inject]
    protected GitHubRepositoryService RepositoryService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        News = await RepositoryService.GetNews();
        Years = GetYears(News);

        await base.OnInitializedAsync();
    }

    protected IEnumerable<NewsItem> GetFilteredNews()
    {
        if (News is null)
        {
            return [];
        }

        return News.Where(Filter);
    }

    protected static Color GetTimelineColor(NewsItemAction action)
        => action switch
        {
            NewsItemAction.RepositoryNew => Color.Primary,
            NewsItemAction.FeatureNew => Color.Tertiary,
            _ => Color.Default,
        };

    private bool Filter(NewsItem element)
    {
        if (SelectedYear is not null && element.Time.Year != SelectedYear)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(FilterString))
        {
            return true;
        }

        return element.Title.Contains(FilterString, StringComparison.OrdinalIgnoreCase) ||
               element.Body.Contains(FilterString, StringComparison.OrdinalIgnoreCase) ||
               element.RepositoryName.Contains(FilterString, StringComparison.OrdinalIgnoreCase) ||
               element.Action.GetDescription().Contains(FilterString, StringComparison.OrdinalIgnoreCase);
    }

    private static List<int> GetYears(IEnumerable<NewsItem> items)
    {
        var firstYear = items.Min(x => x.Time).Year;
        var years = new List<int>();
        for (var i = firstYear; i <= DateTimeOffset.Now.Year; i++)
        {
            years.Add(i);
        }

        return years;
    }
}