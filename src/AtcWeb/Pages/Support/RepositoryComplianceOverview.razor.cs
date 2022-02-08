namespace AtcWeb.Pages.Support;

public class RepositoryComplianceOverviewBase : ComponentBase
{
    protected List<AtcRepository>? Repositories;

    [Inject]
    protected GitHubRepositoryService RepositoryService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Repositories = await RepositoryService.GetRepositoriesAsync(populateMetaDataBase: true, populateMetaDataAdvanced: true);

        await base.OnInitializedAsync();
    }
}