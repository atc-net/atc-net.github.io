namespace AtcWeb.Components.Repository;

public partial class CopyableSnippet : ComponentBase
{
    private bool copied;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    [Parameter]
    public string Code { get; set; } = string.Empty;

    [Parameter]
    public string Label { get; set; } = string.Empty;

    [Parameter]
    public string Icon { get; set; } = Icons.Material.Filled.Terminal;

    private async Task CopyToClipboard()
    {
        await JsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", Code);
        copied = true;
        StateHasChanged();
        await Task.Delay(2000);
        copied = false;
        StateHasChanged();
    }
}