namespace AtcWeb.Pages.Manuals;

public class DevOpsPlaybookBase : ComponentBase
{
    protected List<Tuple<string, string>>? Sections { get; set; }

    [Inject]
    protected GitHubRepositoryService RepositoryService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Sections = await RepositoryService.GetDevOpsPlaybook();

        await base.OnInitializedAsync();
    }
}