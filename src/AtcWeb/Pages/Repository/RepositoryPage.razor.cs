namespace AtcWeb.Pages.Repository;

public partial class RepositoryPage : ComponentBase
{
    private AtcRepository? repository;
    private bool isLoaded;

    [Parameter]
    public string RepositoryName { get; set; } = string.Empty;

    [Inject]
    private GitHubRepositoryService RepositoryService { get; set; } = default!;

    protected override async Task OnParametersSetAsync()
    {
        isLoaded = false;
        repository = await RepositoryService.GetRepositoryByNameAsync(
            RepositoryName,
            populateMetaDataBase: true,
            populateMetaDataAdvanced: true);
        isLoaded = true;
        await base.OnParametersSetAsync();
    }
}