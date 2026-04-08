namespace AtcWeb.Pages.Tools;

public partial class CliTools
{
    private NugetCliToolSearchResult? searchResult;

    [Inject]
    private AtcApiNugetClient NugetClient { get; set; } = default!;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        var (isSuccessful, result) = await NugetClient.GetCliTools();
        if (isSuccessful)
        {
            result.Data = result.Data
                .OrderByDescending(t => t.TotalDownloads)
                .ToList();

            searchResult = result;
        }

        await base.OnInitializedAsync();
    }

    private async Task CopyToClipboard(string text)
    {
        await JsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", text);
    }

    private static string FormatDownloads(long downloads)
        => downloads switch
        {
            >= 1_000_000 => $"{downloads / 1_000_000.0:F1}M",
            >= 1_000 => $"{downloads / 1_000.0:F1}K",
            _ => downloads.ToString("N0", CultureInfo.InvariantCulture),
        };
}