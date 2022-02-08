namespace AtcWeb.Pages.Introduction;

public class RepositoryOverviewBase : ComponentBase
{
    protected List<AtcRepository>? Repositories;

    [Inject]
    protected GitHubRepositoryService RepositoryService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Repositories = await RepositoryService.GetRepositoriesAsync(populateMetaDataBase: true, populateMetaDataAdvanced: false);

        await base.OnInitializedAsync();
    }
}