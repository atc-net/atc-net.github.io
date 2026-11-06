namespace AtcWeb.Components.AtcApps;

public partial class AtcAppDownloadBox : ComponentBase
{
    [Parameter]
    [EditorRequired]
    public AtcAppRelease Release { get; set; } = default!;

    [Parameter]
    public EventCallback OnWhatsNew { get; set; }

    /// <summary>
    /// Gets the box element, so the page can bring it into view for /apps/{name}#download.
    /// </summary>
    public ElementReference Element { get; private set; }

    private static Dictionary<string, object> DownloadAttributes(
        AtcAppAsset asset)
        => new(StringComparer.Ordinal)
        {
            ["download"] = string.Empty,
            ["aria-label"] = $"Download {asset.Label} (.{asset.FileType})",
        };

    private IEnumerable<AtcAppAsset> PrimaryAssets
        => Release.Assets.Where(x => !x.IsSecondary);
}