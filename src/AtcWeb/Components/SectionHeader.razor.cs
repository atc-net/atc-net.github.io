namespace AtcWeb.Components;

public partial class SectionHeader
{
    [CascadingParameter]
    private DocsPage? DocsPage { get; set; }

    [CascadingParameter]
    private SectionSubGroups? SubGroup { get; set; }

    [CascadingParameter]
    private DocsPageSection? Section { get; set; }

    protected string Classname =>
        new CssBuilder("docs-section-header")
            .AddClass(Class)
            .Build();

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public Typo TitleType { get; set; } = Typo.h4;

    [Parameter]
    public string? TitleLink { get; set; }

    [Parameter]
    public Color TitleColor { get; set; } = Color.Default;

    [Parameter]
    public RenderFragment? SubTitle { get; set; }

    [Parameter]
    public RenderFragment? Description { get; set; }

    public DocsSectionLink? SectionInfo { get; set; }

    public ElementReference SectionReference { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (DocsPage is null || Section is null || string.IsNullOrWhiteSpace(Title))
        {
            return;
        }

        Section.HeaderTitle = Title;

        var parentTitle = Section.ParentSection?.HeaderTitle ?? string.Empty;
        if (!string.IsNullOrEmpty(parentTitle))
        {
            parentTitle += '-';
        }

        var id = (parentTitle + Title).Replace(" ", "-", StringComparison.Ordinal).ToLowerInvariant();

        SectionInfo = new DocsSectionLink
        {
            Id = id,
            Title = Title,
        };
    }

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);
        if (firstRender &&
            DocsPage is not null &&
            !string.IsNullOrWhiteSpace(Title) &&
            SectionInfo is not null &&
            Section is not null)
        {
            DocsPage.AddSection(SectionInfo, Section);
        }
    }

    private string GetSectionId()
        => SectionInfo?.Id is null
            ? Guid.NewGuid().ToString()
            : SectionInfo.Id;
}