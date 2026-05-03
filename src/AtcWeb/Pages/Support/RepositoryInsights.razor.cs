namespace AtcWeb.Pages.Support;

public class RepositoryInsightsBase : ComponentBase
{
    [Inject]
    protected AtcApiGitHubRepositoryClient Client { get; set; } = default!;

    protected List<RepositoryComplianceSummary>? Summaries { get; private set; }

    protected override async Task OnInitializedAsync()
    {
        var (isSuccessful, summaries) = await Client.GetComplianceSummary();
        Summaries = isSuccessful ? summaries : [];
        await base.OnInitializedAsync();
    }
}