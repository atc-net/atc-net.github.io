namespace AtcWeb.Components.Repository;

public partial class RepositoryCodingRulesLocalSuppressRules
{
    [Inject]
    private StateContainer? StateContainer { get; set; }

    [Parameter]
    public List<KeyValueItem> LocalSuppressRules { get; set; } = new List<KeyValueItem>();
}