namespace AtcWeb.Pages.Support;

public class RepositoryComplianceOverviewBase : ComponentBase
{
    [Inject]
    protected AtcApiGitHubRepositoryClient Client { get; set; } = default!;

    protected List<RepositoryComplianceSummary>? Summaries { get; private set; }

    protected ComplianceFilterState FilterState { get; set; } = new();

    protected string GroupBy { get; set; } = "None";

    protected IReadOnlyList<RepositoryComplianceSummary> GetFiltered()
    {
        if (Summaries is null)
        {
            return [];
        }

        return ComplianceFilterEngine
            .Apply(Summaries, FilterState)
            .Where(s =>
                string.IsNullOrEmpty(FilterState.Category) ||
                string.Equals(
                    RepositoryCategoryHelper.GetCategory(s.Name),
                    FilterState.Category,
                    StringComparison.Ordinal))
            .ToList();
    }

    protected IReadOnlyList<string> GetCategoryNames()
    {
        if (Summaries is null)
        {
            return [];
        }

        return Summaries
            .Select(s => RepositoryCategoryHelper.GetCategory(s.Name))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToList();
    }

    protected override async Task OnInitializedAsync()
    {
        var (isSuccessful, summaries) = await Client.GetComplianceSummary();
        Summaries = isSuccessful ? summaries : [];
        await base.OnInitializedAsync();
    }

    protected void OnFilterChanged(ComplianceFilterState state)
    {
        FilterState = state;
        StateHasChanged();
    }

    protected void OnGroupByChanged(string value)
    {
        GroupBy = value;
        StateHasChanged();
    }
}