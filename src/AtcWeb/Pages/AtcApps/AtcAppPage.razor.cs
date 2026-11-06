namespace AtcWeb.Pages.AtcApps;

public partial class AtcAppPage : ComponentBase
{
    private static readonly HashSet<string> WindowsFileTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "msi",
        "exe",
        "msix",
        "appx",
    };

    private AtcApp? app;
    private bool isLoaded;
    private string? readme;
    private WikiMetadata wiki = new();
    private List<AtcAppRelease>? releases;
    private SortedDictionary<string, List<string>> builtWith = new(StringComparer.Ordinal);
    private List<MarkdownHeadingInfo>? readmeHeadings;
    private int activeTab;
    private bool scrollToDownload;
    private AtcAppDownloadBox? downloadBox;
    private int loadVersion;

    [Parameter]
    public string AtcAppName { get; set; } = string.Empty;

    [Inject]
    private AtcAppService AtcAppService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    private static string[] WindowsFact { get; } = ["Windows"];

    private static string[] PriceFact { get; } = ["Free, open source"];

    private string[] UpdatedFact
        => [app?.PushedAt?.ToString("MMM d, yyyy", CultureInfo.InvariantCulture) ?? string.Empty];

    private string LatestReleaseText
        => app?.LatestRelease?.PublishedAt?.ToString("MMM d, yyyy", CultureInfo.InvariantCulture) ?? "None yet";

    private string VersionsText
        => releases?.Count.ToString(CultureInfo.InvariantCulture) ?? "–";

    private string GuidesText
        => wiki.HasWiki
            ? $"{wiki.Pages.Count} pages"
            : "README";

    private string IssuesHref
        => $"{app?.HtmlUrl}/issues";

    private bool IsWindows
        => app?.LatestRelease?.Assets.Exists(x => !x.IsSecondary && WindowsFileTypes.Contains(x.FileType)) == true;

    protected override async Task OnParametersSetAsync()
    {
        // Navigating to another app while this one is still loading starts a new load;
        // results that arrive for an older load are dropped, so two apps never mix.
        var version = ++loadVersion;
        isLoaded = false;
        app = null;
        readme = null;
        wiki = new WikiMetadata();
        releases = null;
        builtWith = new SortedDictionary<string, List<string>>(StringComparer.Ordinal);
        readmeHeadings = null;
        activeTab = 0;

        var loaded = await AtcAppService.GetAtcAppAsync(AtcAppName);
        if (version != loadVersion)
        {
            return;
        }

        app = loaded;
        isLoaded = true;
        if (app is null)
        {
            return;
        }

        scrollToDownload = NavigationManager.Uri.EndsWith("#download", StringComparison.OrdinalIgnoreCase);
        StateHasChanged();

        var taskReadme = AtcAppService.GetReadmeAsync(app.Name);
        var taskReleases = AtcAppService.GetReleasesAsync(app.Name);
        var taskBuiltWith = AtcAppService.GetBuiltWithAsync(app.Name);
        var taskWiki = AtcAppService.GetWikiAsync(app.Name);
        await Task.WhenAll(taskReadme, taskReleases, taskWiki);
        if (version != loadVersion)
        {
            return;
        }

        readme = await taskReadme;
        releases = await taskReleases;
        wiki = await taskWiki;
        StateHasChanged();

        // "Built with" walks the app's project files, so it can take a while the first time.
        var references = await taskBuiltWith;
        if (version != loadVersion)
        {
            return;
        }

        foreach (var reference in references)
        {
            if (!builtWith.TryGetValue(reference.RepositoryName, out var packages))
            {
                packages = [];
                builtWith[reference.RepositoryName] = packages;
            }

            packages.Add(reference.PackageId);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (scrollToDownload && downloadBox is not null)
        {
            scrollToDownload = false;
            await downloadBox.Element.FocusAsync(preventScroll: false);
        }
    }

    private void OnReadmeHeadingsExtracted(List<MarkdownHeadingInfo> headings)
    {
        readmeHeadings = headings.Count > 0 ? headings : null;
        StateHasChanged();
    }

    // Tabs: Overview, [Guides], What's new, ...
    private void ShowWhatsNew()
        => activeTab = wiki.HasWiki ? 2 : 1;
}