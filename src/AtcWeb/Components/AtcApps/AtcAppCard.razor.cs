namespace AtcWeb.Components.AtcApps;

public partial class AtcAppCard : ComponentBase
{
    [Parameter]
    [EditorRequired]
    public AtcAppCardModel Model { get; set; } = default!;

    private string DetailsHref
        => $"/apps/{Model.Name}";

    private string GitHubHref
        => $"https://github.com/atc-net-apps/{Model.Name}";

    // A direct file download must not be handled by the SPA router.
    private Dictionary<string, object> DownloadAttributes
        => Model.IsDirectDownload
            ? new Dictionary<string, object>(StringComparer.Ordinal) { ["download"] = string.Empty }
            : new Dictionary<string, object>(StringComparer.Ordinal);
}