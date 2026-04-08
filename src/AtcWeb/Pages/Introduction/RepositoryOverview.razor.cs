namespace AtcWeb.Pages.Introduction;

public class RepositoryOverviewBase : ComponentBase
{
    protected List<AtcRepository>? Repositories;

    [Parameter]
    [SupplyParameterFromQuery(Name = "category")]
    public string? CategoryFilter { get; set; }

    [Inject]
    protected GitHubRepositoryService RepositoryService { get; set; }

    [Inject]
    protected NavigationManager NavigationManager { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Repositories = await RepositoryService.GetRepositoriesAsync(populateMetaDataBase: false);

        await base.OnInitializedAsync();
    }

    protected IEnumerable<AtcRepository> GetFilteredRepositories()
    {
        if (Repositories is null)
        {
            return [];
        }

        var repos = Repositories
            .Where(x => !x.BaseData.Private);

        if (!string.IsNullOrEmpty(CategoryFilter))
        {
            var categories = CategoryFilter
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            repos = repos.Where(x =>
                categories.Any(c =>
                    RepositoryCategoryHelper.GetCategory(x.Name)
                        .Equals(c, StringComparison.OrdinalIgnoreCase)));
        }

        return repos.OrderBy(x => x.Name, StringComparer.Ordinal);
    }

    protected void ClearFilter()
    {
        NavigationManager.NavigateTo("/introduction/repository-overview");
    }
}