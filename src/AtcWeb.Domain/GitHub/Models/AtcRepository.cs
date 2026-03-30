// ReSharper disable InvertIf
namespace AtcWeb.Domain.GitHub.Models;

public class AtcRepository
{
    public AtcRepository(GitHubRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);

        BaseData = repository;
        FolderAndFilePaths = [];
        OpenIssues = [];
        Root = new RootMetadata();
        CodingRules = new CodingRulesMetadata();
        Dotnet = new DotnetMetadata();
        Python = new PythonMetadata();
        Wiki = new WikiMetadata();
    }

    public GitHubRepository BaseData { get; }

    public string Name => BaseData.Name;

    public string DotName => Name
        .Replace('-', '.')
        .Replace("atc.rest.api.generator", "atc-api-gen", StringComparison.Ordinal)
        .Replace("atc.coding.rules.updater", "atc-coding-rules-updater", StringComparison.Ordinal);

    public string Description => BaseData.Description;

    public string Url => $"https://github.com/atc-net/{Name}";

    public List<GitHubPath> FolderAndFilePaths { get; set; }

    public RootMetadata Root { get; set; }

    public CodingRulesMetadata CodingRules { get; set; }

    public DotnetMetadata? Dotnet { get; set; }

    public PythonMetadata? Python { get; set; }

    public WikiMetadata Wiki { get; set; }

    public List<GitHubIssue> OpenIssues { get; set; }

    public bool HasRootReadme => Root?.HasReadme ?? false;

    public bool HasCodingRulesEditorConfigRoot
        => CodingRules?.HasRoot ?? false;

    public bool HasCodingRulesEditorConfigSrc
        => CodingRules?.HasSrc ?? false;

    public bool HasCodingRulesEditorConfigTest
        => CodingRules?.HasTest ?? false;

    public bool HasDotnetSolution => Dotnet?.HasSolution ?? false;

    public bool HasDotnetDirectoryBuildPropsRoot
        => Dotnet?.HasDirectoryBuildPropsRoot ?? false;

    public bool HasDotnetDirectoryBuildPropsSrc
        => Dotnet?.HasDirectoryBuildPropsSrc ?? false;

    public bool HasDotnetDirectoryBuildPropsTest
        => Dotnet?.HasDirectoryBuildPropsTest ?? false;

    public bool IsDotnetSolution
        => "C#".Equals(BaseData.Language, StringComparison.Ordinal);

    public bool IsPythonSolution
        => "Python".Equals(BaseData.Language, StringComparison.Ordinal);

    public DateTimeOffset? GetOpenIssuesNewest()
    {
        if (OpenIssues.Count == 0)
        {
            return null;
        }

        return OpenIssues.Max(x => x.CreatedAt);
    }

    public DateTimeOffset? GetOpenIssuesOldest()
    {
        if (OpenIssues.Count == 0)
        {
            return null;
        }

        return OpenIssues.Min(x => x.CreatedAt);
    }

    public LogCategoryType GetOpenIssuesNewestState(
        int monthWarning,
        int monthError)
        => GetOpenIssuesState(
            GetOpenIssuesNewest(),
            monthWarning,
            monthError);

    public LogCategoryType GetOpenIssuesOldestState(
        int monthWarning,
        int monthError)
        => GetOpenIssuesState(
            GetOpenIssuesOldest(),
            monthWarning,
            monthError);

    private static LogCategoryType GetOpenIssuesState(
        DateTimeOffset? date,
        int monthWarning,
        int monthError)
    {
        var logCategoryType = LogCategoryType.Information;
        if (date is not null)
        {
            if (date.Value <= DateTimeOffset.Now.AddMonths(monthWarning * -1))
            {
                logCategoryType = LogCategoryType.Warning;
            }

            if (date.Value <= DateTimeOffset.Now.AddMonths(monthError * -1))
            {
                logCategoryType = LogCategoryType.Error;
            }
        }

        return logCategoryType;
    }
}