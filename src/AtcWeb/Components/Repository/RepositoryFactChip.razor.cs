namespace AtcWeb.Components.Repository;

public partial class RepositoryFactChip : ComponentBase
{
    [Parameter]
    public string Label { get; set; } = string.Empty;

    [Parameter]
    public IReadOnlyList<string> Values { get; set; } = [];
}