// ReSharper disable StringLiteralTypo
namespace AtcWeb.Domain.Data;

public static class RepositoryMetadata
{
    public static string RecommendedVisualStudioName => "Visual Studio 2026";

    public static string RecommendedLangVersion => "14.0";

    public static IEnumerable<string> RecommendedTargetFramework =>
    [
        "net8.0",
        "net10.0",
        "netstandard2.0",
        "netstandard2.1",
    ];

    private static readonly string[] DefaultResponsibleMembers = ["davidkallesen", "perkops"];

    /// <summary>
    /// Overrides for repositories that have different responsible members than the defaults.
    /// Repositories not listed here will use <see cref="DefaultResponsibleMembers"/>.
    /// </summary>
    private static readonly Dictionary<string, string[]> ResponsibleMemberOverrides = new(StringComparer.Ordinal)
    {
        ["atc-autoformatter"] = ["rickykaare"],
        ["atc-azure-digitaltwin"] = ["perkops"],
        ["atc-azure-messaging"] = ["christianhelle"],
        ["atc-azure-options"] = ["kimlundjohansen"],
        ["atc-cosmos"] = ["rickykaare"],
        ["atc-cosmos-eventstore"] = ["LarsSkovslund"],
        ["atc-data-platform"] = ["davidkallesen", "perkops", "mrmasterplan"],
        ["atc-data-platform-tools"] = ["davidkallesen", "perkops", "mrmasterplan"],
        ["atc-react"] = ["perkops"],
        ["atc-rest-client"] = ["davidkallesen", "perkops", "LarsSkovslund", "egil"],
        ["atc-semantic-kernel"] = ["perkops"],
        ["atc-snippets"] = ["perkops", "lupusbytes"],
        ["atc-test"] = ["rickykaare"],
        ["atc-wpf"] = ["davidkallesen"],
    };

    public static string[] GetResponsibleMembersByName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return ResponsibleMemberOverrides
                .SelectMany(x => x.Value)
                .Concat(DefaultResponsibleMembers)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToArray();
        }

        var members = ResponsibleMemberOverrides.TryGetValue(name, out var overrides)
            ? overrides
            : DefaultResponsibleMembers;

        return members
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray();
    }

    public static bool HasNotRequiredResponsibleMembersByName(string name)
        => GetResponsibleMembersByName(name).Length < 2;

    public static bool IsVisualStudioNameInAcceptedVersion(
        string visualStudioName)
        => !string.IsNullOrEmpty(visualStudioName) &&
           visualStudioName.Equals(RecommendedVisualStudioName, StringComparison.Ordinal);

    public static bool IsTargetFrameworkInLongTimeSupport(
        string targetFramework)
        => !string.IsNullOrEmpty(targetFramework) &&
           RecommendedTargetFramework.Contains(targetFramework, StringComparer.OrdinalIgnoreCase);

    public static bool IsLangVersionInAcceptedVersion(string langVersion)
        => !string.IsNullOrEmpty(langVersion) &&
           langVersion.Contains(RecommendedLangVersion, StringComparison.Ordinal);
}