namespace AtcWeb.Components.Compliance;

public enum ChipState
{
    Ok,
    Warning,
    Error,
    NotApplicable,
}

public partial class ComplianceStatusChip : ComponentBase
{
    [Parameter]
    public ChipState State { get; set; } = ChipState.Ok;

    [Parameter]
    public string Label { get; set; } = string.Empty;

    [Parameter]
    public string Tooltip { get; set; } = string.Empty;

    private Color ChipColor => State switch
    {
        ChipState.Ok => Color.Success,
        ChipState.Warning => Color.Warning,
        ChipState.Error => Color.Error,
        _ => Color.Default,
    };

    private string Icon => State switch
    {
        ChipState.Ok => Icons.Material.Filled.CheckCircle,
        ChipState.Warning => Icons.Material.Filled.Warning,
        ChipState.Error => Icons.Material.Filled.Error,
        _ => Icons.Material.Filled.Remove,
    };
}