namespace AtcWeb.Components;

public partial class GitHubContributorItem
{
    [Inject]
    private StateContainer? StateContainer { get; set; }

    [Parameter]
    public string AvatarLink { get; set; } = string.Empty;

    [Parameter]
    public string GithubLink { get; set; } = string.Empty;

    [Parameter]
    public string Name { get; set; } = string.Empty;

    [Parameter]
    public string Title { get; set; } = "Contributor";

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}