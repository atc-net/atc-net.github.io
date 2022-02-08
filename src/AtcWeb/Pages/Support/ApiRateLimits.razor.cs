namespace AtcWeb.Pages.Support;

public class ApiRateLimitsBase : ComponentBase
{
    protected GitHubApiRateLimits? RateLimits;

    [Inject]
    protected GitHubRepositoryService RepositoryService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        RateLimits = await RepositoryService.GetRestApiRateLimitsAsync();

        await base.OnInitializedAsync();
    }
}