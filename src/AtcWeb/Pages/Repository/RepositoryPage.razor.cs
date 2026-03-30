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
        StateHasChanged();

        // Phase 1: Basic repo info + paths in parallel — renders header immediately
        var taskRepo = RepositoryService.GetRepositoryByNameAsync(RepositoryName);
        var taskPaths = RepositoryService.GetDirectoryMetadataAsync(RepositoryName);
        await Task.WhenAll(taskRepo, taskPaths);

        repository = await taskRepo;
        isLoaded = true;

        if (repository is null)
        {
            return;
        }

        repository.FolderAndFilePaths = await taskPaths;
        StateHasChanged();

        // Phase 2: README — renders README as fast as possible
        await RepositoryService.PopulateReadmeAsync(repository);
        StateHasChanged();

        // Phase 3: Advanced metadata (coding rules, issues, .NET projects)
        // Start wiki fetch in parallel (independent of advanced metadata)
        var taskWiki = RepositoryService.PopulateWikiAsync(repository);
        await RepositoryService.PopulateMetaDataAdvancedAsync(repository);
        StateHasChanged();

        // Phase 4: Wiki content
        await taskWiki;
        StateHasChanged();

        await base.OnParametersSetAsync();
    }
}