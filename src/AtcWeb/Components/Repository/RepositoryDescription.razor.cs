namespace AtcWeb.Components.Repository;

public partial class RepositoryDescription
{
    [Parameter]
    public AtcRepository? Repository { get; set; }

    private string IconSrc
        => Repository is null
            ? string.Empty
            : ImageHelper.GetProgramIconPathForLanguage(Repository.BaseData.Language);
}