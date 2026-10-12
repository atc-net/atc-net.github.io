namespace AtcWeb.Pages;

public partial class News
{
    private List<AtcRepository>? repositories;

    [Inject]
    private GitHubRepositoryService RepositoryService { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            repositories = await RepositoryService.GetRepositoriesAsync();
            StateHasChanged();
        }
    }

    private IEnumerable<AtcRepository> GetReposByRecency(
        Func<DateTimeOffset, bool> predicate)
    {
        if (repositories is null)
        {
            return [];
        }

        return repositories
            .Where(x => x.BaseData is { Private: false, PushedAt: not null })
            .Where(x => predicate(x.BaseData.PushedAt!.Value))
            .OrderByDescending(x => x.BaseData.PushedAt)
            .ToList();
    }
}