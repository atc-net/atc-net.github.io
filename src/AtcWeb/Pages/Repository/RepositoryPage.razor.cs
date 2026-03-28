namespace AtcWeb.Pages.Repository;

public partial class RepositoryPage : ComponentBase
{
    private AtcRepository? repository;
    private bool isLoaded;

    [Parameter]
    public string RepositoryName { get; set; } = string.Empty;

    [Inject]
    private GitHubRepositoryService RepositoryService { get; set; } = default!;

    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "Progressive loading phases.")]
    protected override async Task OnParametersSetAsync()
    {
        isLoaded = false;
        repository = null;

        // Phase 1: Basic repo info — renders header + description immediately
        repository = await RepositoryService.GetRepositoryByNameAsync(RepositoryName);
        isLoaded = true;

        if (repository is null)
        {
            return;
        }

        StateHasChanged();

        // Phase 2: Paths + README — renders README as fast as possible
        // Start responsible members fetch in parallel (slow — fetches all contributors)
        var taskMembers = RepositoryService.PopulateResponsibleMembersAsync(repository);
        await RepositoryService.PopulatePathsAndReadmeAsync(repository);
        StateHasChanged();

        // Phase 3: Workflows + badges — renders badge table
        await RepositoryService.PopulateWorkflowAndBadgesAsync(repository);
        StateHasChanged();

        // Phase 4: Responsible members — renders member cards (may already be done)
        await taskMembers;
        StateHasChanged();

        // Phase 5: Advanced metadata (coding rules, issues, .NET projects)
        // Start wiki fetch in parallel (independent of advanced metadata)
        var taskWiki = RepositoryService.PopulateWikiAsync(repository);
        await RepositoryService.PopulateMetaDataAdvancedAsync(repository);
        StateHasChanged();

        // Phase 6: Wiki content
        await taskWiki;
        StateHasChanged();

        await base.OnParametersSetAsync();
    }
}