namespace AtcWeb.Components.Repository;

public partial class RepositoryDetailSidebar : ComponentBase
{
    [Parameter]
    public AtcRepository? Repository { get; set; }

    private string GetInstallCommand()
    {
        if (Repository?.NugetInfo is null)
        {
            return string.Empty;
        }

        var packageId = Repository.NugetInfo.PackageId;
        var version = Repository.NugetInfo.Version;

        return Repository.NugetInfo.IsTool
            ? $"dotnet tool install -g {packageId} --version {version}"
            : $"dotnet add package {packageId} --version {version}";
    }

    private string GetProjectFileSnippet()
    {
        if (Repository?.NugetInfo is null)
        {
            return string.Empty;
        }

        return $"""<PackageReference Include="{Repository.NugetInfo.PackageId}" Version="{Repository.NugetInfo.Version}" />""";
    }

    private List<string> GetMergedTags()
    {
        var tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (Repository?.BaseData.Topics is not null)
        {
            foreach (var topic in Repository.BaseData.Topics)
            {
                tags.Add(topic);
            }
        }

        if (Repository?.NugetInfo?.Tags is not null)
        {
            foreach (var tag in Repository.NugetInfo.Tags)
            {
                tags.Add(tag);
            }
        }

        return tags.OrderBy(t => t, StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static string FormatDate(string? dateString)
    {
        if (string.IsNullOrEmpty(dateString))
        {
            return "Unknown";
        }

        return DateTimeOffset.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date.ToString("MMM d, yyyy", CultureInfo.InvariantCulture)
            : dateString;
    }
}