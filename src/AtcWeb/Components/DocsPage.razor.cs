namespace AtcWeb.Components;

public partial class DocsPage : ComponentBase
{
    private Queue<(DocsSectionLink Link, DocsPageSection Section)> bufferedSections = new();
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
        if (contentNavigation is not null)
        {
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
        }
    }
}