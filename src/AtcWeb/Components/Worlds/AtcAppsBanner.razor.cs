namespace AtcWeb.Components.Worlds;

public sealed partial class AtcAppsBanner : ComponentBase, IDisposable
{
    private static readonly TimeSpan AppInterval = TimeSpan.FromSeconds(4);

    private Timer? timer;
    private int current;

    /// <summary>
    /// Gets or sets the released apps the installer window cycles through.
    /// </summary>
    [Parameter]
    public IReadOnlyList<AtcAppCardModel> Apps { get; set; } = [];

    [Parameter]
    public int ReadyCount { get; set; }

    [Parameter]
    public int ComingSoonCount { get; set; }

    public void Dispose()
        => timer?.Dispose();

    protected override void OnParametersSet()
    {
        if (current >= Apps.Count)
        {
            current = 0;
        }

        if (Apps.Count > 1 && timer is null)
        {
            timer = new Timer(OnTick, state: null, AppInterval, AppInterval);
        }
    }

    private void OnTick(object? state)
    {
        _ = InvokeAsync(Next);
    }

    private void Next()
    {
        current = (current + 1) % Apps.Count;
        StateHasChanged();
    }
}