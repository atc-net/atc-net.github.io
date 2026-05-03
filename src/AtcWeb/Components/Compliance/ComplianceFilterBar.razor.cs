namespace AtcWeb.Components.Compliance;

public partial class ComplianceFilterBar : ComponentBase
{
    [Parameter]
    public ComplianceFilterState State { get; set; } = new();

    [Parameter]
    public EventCallback<ComplianceFilterState> StateChanged { get; set; }

    [Parameter]
    public IReadOnlyList<string> Categories { get; set; } = [];

    [Parameter]
    public string GroupBy { get; set; } = "None";

    [Parameter]
    public EventCallback<string> GroupByChanged { get; set; }

    private Task OnSearchChanged(string value)
        => Update(s => s.SearchText = value);

    private Task OnLanguageChanged(string value)
        => Update(s => s.Language = value);

    private Task OnCategoryChanged(string value)
        => Update(s => s.Category = value);

    private Task OnHealthChanged(HealthStatus? value)
        => Update(s => s.Health = value);

    private Task OnGroupByChanged(string value)
        => GroupByChanged.InvokeAsync(value);

    private Task Update(Action<ComplianceFilterState> mutate)
    {
        mutate(State);
        return StateChanged.InvokeAsync(State);
    }
}