// ReSharper disable InconsistentNaming
// ReSharper disable ReturnTypeCanBeEnumerable.Local
namespace AtcWeb.Pages;

public class NewsBase : ComponentBase
{
    protected IEnumerable<int> Years { get; set; }

    protected IEnumerable<AtcRepository>? Repositories { get; set; }

    protected IEnumerable<NewsItem>? News { get; set; }

    protected int? SelectedYear { get; set; }

    protected string? SelectedRepositoryName { get; set; }

    protected string? FilterString { get; set; }

    [Inject]
    protected GitHubRepositoryService RepositoryService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Repositories = await RepositoryService.GetRepositoriesAsync();
        News = await RepositoryService.GetNews();
        Years = GetYears(News);

        await base.OnInitializedAsync();
    }

    protected bool FilterFunc(NewsItem element)
        => Filter(element);

    private bool Filter(
        NewsItem element)
    {
        var isYear = !(SelectedYear is not null &&
                       element.Time.Year != SelectedYear);

        var isRepo = !(SelectedRepositoryName is not null &&
                       !element.RepositoryName.Equals(SelectedRepositoryName, StringComparison.Ordinal));

        var isFilter = string.IsNullOrWhiteSpace(FilterString) ||
                       element.Title.Contains(FilterString, StringComparison.OrdinalIgnoreCase) ||
                       element.Body.Contains(FilterString, StringComparison.OrdinalIgnoreCase) ||
                       element.Action.GetDescription().Contains(FilterString, StringComparison.OrdinalIgnoreCase);

        return isYear &&
               isRepo &&
               isFilter;
    }

    private static List<int> GetYears(
        IEnumerable<NewsItem> items)
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