namespace AtcWeb.Components;

public partial class DocsPage : ComponentBase
{
    private Queue<(DocsSectionLink Link, DocsPageSection Section)> bufferedSections = new();
    private Dictionary<DocsPageSection, MudPageContentSection> sectionMapper = new();
    private Dictionary<string, List<MarkdownHeadingInfo>> pendingHeadingSections = new(StringComparer.Ordinal);
    private List<MudPageContentSection>? pendingRebuild;
    private MudPageContentNavigation? contentNavigation;
    private int navigationKey;

    [Inject]
    private NavigationManager NavigationManager { get; set; }

    [Parameter]
    public MaxWidth MaxWidth { get; set; } = MaxWidth.Medium;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private bool contentDrawerOpen = true;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (contentNavigation is not null)
        {
            if (pendingRebuild is not null)
            {
                var sections = pendingRebuild;
                pendingRebuild = null;

                foreach (var section in sections)
                {
                    contentNavigation.AddSection(section, forceUpdate: true);
                }
            }

            DrainBufferedSections();
        }

        if (firstRender && contentNavigation is not null)
        {
            await contentNavigation.ScrollToSection(new Uri(NavigationManager.Uri));
        }
    }

    internal void ClearSections()
    {
        bufferedSections = new Queue<(DocsSectionLink, DocsPageSection)>();
        sectionMapper = new Dictionary<DocsPageSection, MudPageContentSection>();
        pendingHeadingSections = new Dictionary<string, List<MarkdownHeadingInfo>>(StringComparer.Ordinal);
        pendingRebuild = null;
        navigationKey++;
        StateHasChanged();
    }

    internal void AddSection(
        DocsSectionLink sectionLinkInfo,
        DocsPageSection section)
    {
        bufferedSections.Enqueue((sectionLinkInfo, section));

        if (contentNavigation is not null)
        {
            DrainBufferedSections();
        }
    }

    internal void AddMarkdownHeadingSections(
        string parentSectionId,
        List<MarkdownHeadingInfo> headings)
    {
        // Store headings to be inserted right after their parent section
        // during DrainBufferedSections, ensuring correct ordering.
        pendingHeadingSections[parentSectionId] = headings;

        // If the parent is already registered, apply immediately
        if (contentNavigation is not null)
        {
            ApplyPendingHeadings(parentSectionId);
        }
    }

    private void ApplyPendingHeadings(string parentSectionId)
    {
        if (contentNavigation is null ||
            !pendingHeadingSections.TryGetValue(parentSectionId, out var headings))
        {
            return;
        }

        pendingHeadingSections.Remove(parentSectionId);

        var existingSections = contentNavigation.Sections.ToList();
        var parentIndex = existingSections.FindIndex(s => s.Id == parentSectionId);
        if (parentIndex < 0)
        {
            return;
        }

        var parentSection = existingSections[parentIndex];

        // Build the heading sections to insert
        var newSections = headings
            .Where(h => existingSections.All(s => s.Id != h.Id))
            .Select(h => new MudPageContentSection(h.Title, h.Id, 1, parentSection))
            .ToList();

        if (newSections.Count == 0)
        {
            return;
        }

        // Increment key to destroy and recreate MudPageContentNavigation,
        // then re-add all sections in the correct order on next render.
        var rebuilt = new List<MudPageContentSection>(existingSections.Count + newSections.Count);
        rebuilt.AddRange(existingSections.Take(parentIndex + 1));
        rebuilt.AddRange(newSections);
        rebuilt.AddRange(existingSections.Skip(parentIndex + 1));

        pendingRebuild = rebuilt;
        navigationKey++;
        StateHasChanged();
    }

    private void DrainBufferedSections()
    {
        while (bufferedSections.Count > 0)
        {
            var (link, section) = bufferedSections.Dequeue();

            if (contentNavigation!.Sections.FirstOrDefault(x => x.Id == link.Id) != default)
            {
                continue;
            }

            MudPageContentSection? parentInfo = null;
            if (section.ParentSection is not null &&
                sectionMapper.TryGetValue(section.ParentSection, out var value))
            {
                parentInfo = value;
            }

            var info = new MudPageContentSection(
                link.Title,
                link.Id,
                section.Level,
                parentInfo);

            sectionMapper.Add(section, info);
            contentNavigation.AddSection(info, forceUpdate: true);

            // If this section has pending markdown headings, insert them right after
            if (pendingHeadingSections.ContainsKey(link.Id))
            {
                ApplyPendingHeadings(link.Id);
            }
        }
    }
}