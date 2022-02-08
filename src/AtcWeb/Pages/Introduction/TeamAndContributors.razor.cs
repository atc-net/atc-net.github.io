namespace AtcWeb.Pages.Introduction;

public class TeamAndContributorsBase : ComponentBase
{
    protected List<GitHubRepositoryContributor>? Contributors { get; set; }

    [Inject]
    protected GitHubRepositoryService RepositoryService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Contributors = await RepositoryService.GetContributorsAsync();

        await base.OnInitializedAsync();
    }
}