namespace AtcWeb.Components;

public partial class DocsPage : ComponentBase
{
    private Queue<DocsSectionLink> bufferedSections = new();
    private Dictionary<DocsPageSection, MudPageContentSection> sectionMapper = new();
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
        if (firstRender && contentNavigation is not null)
        {
            await contentNavigation.ScrollToSection(new Uri(NavigationManager.Uri));
        }
    }

    public string GetParentTitle(DocsPageSection section)
    {
        ArgumentNullException.ThrowIfNull(section);

        if (section.ParentSection is null ||
            !sectionMapper.TryGetValue(section.ParentSection, out var value))
        {
            return string.Empty;
        }

        return value.Title;
    }

    internal void ClearSections()
    {
        bufferedSections = new Queue<DocsSectionLink>();
        sectionMapper = new Dictionary<DocsPageSection, MudPageContentSection>();
        navigationKey++;
        StateHasChanged();
    }

    internal void AddSection(
        DocsSectionLink sectionLinkInfo,
        DocsPageSection section)
    {
        bufferedSections.Enqueue(sectionLinkInfo);

        if (contentNavigation is null)
        {
            return;
        }

        while (bufferedSections.Count > 0)
        {
            bufferedSections.Dequeue();

            if (contentNavigation.Sections.FirstOrDefault(x => x.Id == sectionLinkInfo.Id) != default)
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
                sectionLinkInfo.Title,
                sectionLinkInfo.Id,
                section.Level,
                parentInfo);

            sectionMapper.Add(section, info);
            contentNavigation.AddSection(info, forceUpdate: true);
        }
    }
}