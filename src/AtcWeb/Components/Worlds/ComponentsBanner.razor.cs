namespace AtcWeb.Components.Worlds;

public sealed partial class ComponentsBanner : ComponentBase, IDisposable
{
    private static readonly TimeSpan CommandInterval = TimeSpan.FromSeconds(4);

    private Timer? timer;
    private int current;

    /// <summary>
    /// Gets or sets the packages the terminal cycles through (most popular first).
    /// </summary>
    [Parameter]
    public IReadOnlyList<RepositoryNugetPackage> Commands { get; set; } = [];

    [Parameter]
    public int LibraryCount { get; set; }

    [Parameter]
    public int CliToolCount { get; set; }

    [Parameter]
    public string? Downloads { get; set; }

    public void Dispose()
        => timer?.Dispose();

    protected override void OnParametersSet()
    {
        if (Commands.Count > 1 && timer is null)
        {
            timer = new Timer(OnTick, state: null, CommandInterval, CommandInterval);
        }
    }

    private void OnTick(object? state)
    {
        _ = InvokeAsync(Next);
    }

    private void Next()
    {
        current = (current + 1) % Commands.Count;
        StateHasChanged();
    }
}