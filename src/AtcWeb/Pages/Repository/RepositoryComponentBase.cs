namespace AtcWeb.Pages.Repository;

public class RepositoryComponentBase : ComponentBase
{
    private readonly string repositoryName;
    protected AtcRepository? repository;

    [Inject]
    protected GitHubRepositoryService RepositoryService { get; set; }

    protected RepositoryComponentBase(string repositoryName)
    {
        this.repositoryName = repositoryName;
    }

    protected override async Task OnInitializedAsync()
    {
        repository = await RepositoryService.GetRepositoryByNameAsync(repositoryName, populateMetaDataBase: true, populateMetaDataAdvanced: true);
        await base.OnInitializedAsync();
    }
}