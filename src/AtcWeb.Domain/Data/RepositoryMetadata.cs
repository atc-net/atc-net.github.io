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