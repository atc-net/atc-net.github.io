namespace AtcWeb.Components.Repository;

public partial class RepositoryContentToc : ComponentBase, IAsyncDisposable
{
    /// <summary>
    /// A heading counts as "current" once it reaches this far from the top (app bar plus a little air).
    /// </summary>
    private const int ActiveHeadingOffset = 96;

    /// <summary>
    /// After a click the smooth scroll passes other headings; ignore the scroll spy until it has settled.
    /// </summary>
    private static readonly TimeSpan ClickSettleTime = TimeSpan.FromMilliseconds(1000);

    private readonly string navId = $"repo-toc-{Guid.NewGuid():N}";
    private DotNetObjectReference<RepositoryContentToc>? selfReference;
    private string? observedHeadingIds;
    private string? activeId;
    private DateTimeOffset ignoreScrollSpyUntil;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    [Parameter]
    public string Title { get; set; } = "On this page";

    [Parameter]
    public IReadOnlyList<MarkdownHeadingInfo> Headings { get; set; } = [];

    [JSInvokable]
    public void SetActiveHeading(string? id)
    {
        if (DateTimeOffset.UtcNow < ignoreScrollSpyUntil ||
            string.Equals(id, activeId, StringComparison.Ordinal))
        {
            return;
        }

        activeId = id;
        StateHasChanged();
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await JsRuntime.InvokeVoidAsync("atcRepoNav.dispose", navId);
        }
        catch (JSDisconnectedException)
        {
            // The page is going away; nothing to clean up in the browser
        }

        selfReference?.Dispose();
        GC.SuppressFinalize(this);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // (Re)start the scroll spy only when the set of headings changes
        var headingIds = Headings.Select(x => x.Id).ToArray();
        var key = string.Join('|', headingIds);
        if (string.Equals(key, observedHeadingIds, StringComparison.Ordinal))
        {
            return;
        }

        observedHeadingIds = key;
        selfReference ??= DotNetObjectReference.Create(this);
        await JsRuntime.InvokeVoidAsync("atcRepoNav.observe", navId, selfReference, headingIds, ActiveHeadingOffset);
    }

    private async Task ScrollToHeading(string id)
    {
        activeId = id;
        ignoreScrollSpyUntil = DateTimeOffset.UtcNow + ClickSettleTime;
        await JsRuntime.InvokeVoidAsync("atcRepoNav.scrollTo", id);
    }
}