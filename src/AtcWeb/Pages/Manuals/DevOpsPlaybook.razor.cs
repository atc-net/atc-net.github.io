namespace AtcWeb.Pages.Manuals;

public class DevOpsPlaybookBase : ComponentBase
{
    protected DocumentMetadata? DocumentMetadata { get; set; }

    [Inject]
    protected GitHubRepositoryService RepositoryService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        DocumentMetadata = await RepositoryService.GetDevOpsPlaybook();

        await base.OnInitializedAsync();
    }
}