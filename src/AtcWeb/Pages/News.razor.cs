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

    private IEnumerable<AtcRepository> GetReposByRecency(Func<DateTimeOffset, bool> predicate)
    {
        if (repositories is null)
        {
            return [];
        }

        return repositories
            .Where(x => !x.BaseData.Private && x.BaseData.PushedAt.HasValue)
            .Where(x => predicate(x.BaseData.PushedAt!.Value))
            .OrderByDescending(x => x.BaseData.PushedAt)
            .ToList();
    }

    private static string FormatRelativeTime(DateTimeOffset? pushed)
    {
        if (pushed is null)
        {
            return string.Empty;
        }

        var diff = DateTimeOffset.UtcNow - pushed.Value;

        return diff.TotalHours switch
        {
            < 1 => "just now",
            < 24 => $"{(int)diff.TotalHours}h ago",
            < 48 => "yesterday",
            _ when diff.TotalDays < 7 => $"{(int)diff.TotalDays}d ago",
            _ when diff.TotalDays < 30 => $"{(int)(diff.TotalDays / 7)}w ago",
            _ when diff.TotalDays < 365 => $"{(int)(diff.TotalDays / 30)}mo ago",
            _ => $"{(int)(diff.TotalDays / 365)}y ago",
        };
    }
}
