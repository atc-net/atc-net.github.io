namespace AtcWeb.Pages.AtcApps;

public partial class AtcAppsPage : ComponentBase
{
    private List<AtcAppCardModel>? cards;
    private List<string> topics = [];
    private string? selectedTopic;
    private List<AtcAppCardModel> bannerApps = [];
    private int readyCount;
    private int comingSoonCount;

    [Inject]
    private AtcAppService AtcAppService { get; set; } = default!;

    // Released apps first, then "coming soon"; each group by most recent update.
    private IEnumerable<AtcAppCardModel> VisibleCards
        => (cards ?? [])
            .Where(x => selectedTopic is null || x.Topics.Contains(selectedTopic, StringComparer.OrdinalIgnoreCase))
            .OrderBy(x => x.IsComingSoon)
            .ThenByDescending(x => x.UpdatedAt ?? DateTimeOffset.MinValue);

    protected override async Task OnInitializedAsync()
    {
        var apps = await AtcAppService.GetAtcAppsAsync();
        cards = apps
            .Select(AtcAppCardModel.From)
            .ToList();
        (readyCount, comingSoonCount) = SiteStatsHelper.CountAtcApps(cards);
        bannerApps = cards
            .Where(x => !x.IsComingSoon)
            .OrderByDescending(x => x.UpdatedAt ?? DateTimeOffset.MinValue)
            .ToList();
        topics = apps
            .SelectMany(x => x.Topics)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private string FilterClass(string? topic)
        => string.Equals(topic, selectedTopic, StringComparison.OrdinalIgnoreCase)
            ? "world-filter-chip world-filter-chip--active"
            : "world-filter-chip";
}