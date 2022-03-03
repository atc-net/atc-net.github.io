// ReSharper disable InconsistentNaming
namespace AtcWeb.Pages;

public class NewsBase : ComponentBase
{
    protected IEnumerable<NewsItem>? News { get; set; }

    protected string filter = string.Empty;

    [Inject]
    protected GitHubRepositoryService RepositoryService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        News = await RepositoryService.GetNews();

        await base.OnInitializedAsync();
    }

    protected bool FilterFunc(NewsItem element) => Filter(element, filter);

    private static bool Filter(NewsItem element, string filterString)
        => string.IsNullOrWhiteSpace(filterString) ||
           element.Title.Contains(filterString, StringComparison.OrdinalIgnoreCase) ||
           element.Body.Contains(filterString, StringComparison.OrdinalIgnoreCase) ||
           element.Time.ToString("dd-MM-yyy", GlobalizationConstants.EnglishCultureInfo).Contains(filterString, StringComparison.Ordinal) ||
           element.Action.GetDescription().Contains(filterString, StringComparison.OrdinalIgnoreCase);
}