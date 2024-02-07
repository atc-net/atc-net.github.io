// ReSharper disable StringLiteralTypo
namespace AtcWeb.Domain.Data;

public static class RepositoryMetadata
{
    public static string RecommendedVisualStudioName => "Visual Studio 2022";

    public static string RecommendedLangVersion => "10.0";

    public static IEnumerable<string> RecommendedTargetFramework => new[]
    {
        "net6.0",
        "net7.0",
        "netstandard2.0",
        "netstandard2.1",
    };

    private static readonly List<Tuple<string, string>> ResponsibleMembers = new()
    {
        Tuple.Create("atc", "davidkallesen"),
        Tuple.Create("atc", "perkops"),
        Tuple.Create("atc-api", "davidkallesen"),
        Tuple.Create("atc-api", "perkops"),
        Tuple.Create("atc-autoformatter", "rickykaare"),
        Tuple.Create("atc-azure-digitaltwin", "perkops"),
        Tuple.Create("atc-azure-messaging", "christianhelle"),
        Tuple.Create("atc-azure-options", "kimlundjohansen"),
        Tuple.Create("atc-blazor", "davidkallesen"),
        Tuple.Create("atc-blazor", "perkops"),
        Tuple.Create("atc-coding-rules", "davidkallesen"),
        Tuple.Create("atc-coding-rules", "perkops"),
        Tuple.Create("atc-coding-rules-updater", "davidkallesen"),
        Tuple.Create("atc-coding-rules-updater", "perkops"),
        Tuple.Create("atc-cosmos", "rickykaare"),
        Tuple.Create("atc-cosmos-eventstore", "LarsSkovslund"),
        Tuple.Create("atc-cosmos-sql-api-repository", "davidkallesen"),
        Tuple.Create("atc-cosmos-sql-api-repository", "perkops"),
        Tuple.Create("atc-dataplatform", "mrmasterplan"),
        Tuple.Create("atc-dataplatform", "LauJohansson"),
        Tuple.Create("atc-dataplatform-tools", "mrmasterplan"),
        Tuple.Create("atc-dataplatform-tools", "LauJohansson"),
        Tuple.Create("atc-docs", "davidkallesen"),
        Tuple.Create("atc-docs", "perkops"),
        Tuple.Create("atc-hosting", "davidkallesen"),
        Tuple.Create("atc-hosting", "perkops"),
        Tuple.Create("atc-installer", "davidkallesen"),
        Tuple.Create("atc-installer", "perkops"),
        Tuple.Create("atc-kepware", "davidkallesen"),
        Tuple.Create("atc-kepware", "perkops"),
        Tuple.Create("atc-opc-ua", "davidkallesen"),
        Tuple.Create("atc-opc-ua", "perkops"),
        Tuple.Create("atc-net.github.io", "davidkallesen"),
        Tuple.Create("atc-net.github.io", "perkops"),
        Tuple.Create("atc-network", "davidkallesen"),
        Tuple.Create("atc-network", "perkops"),
        Tuple.Create("atc-react", "MogensFogh"),
        Tuple.Create("atc-rest-api-generator", "davidkallesen"),
        Tuple.Create("atc-rest-api-generator", "perkops"),
        Tuple.Create("atc-rest-client", "davidkallesen"),
        Tuple.Create("atc-rest-client", "perkops"),
        Tuple.Create("atc-rest-client", "LarsSkovslund"),
        Tuple.Create("atc-rest-client", "egil"),
        Tuple.Create("atc-rest-minimalapi", "davidkallesen"),
        Tuple.Create("atc-rest-minimalapi", "perkops"),
        Tuple.Create("atc-snippets", "perkops"),
        Tuple.Create("atc-snippets", "Mikael O. Hansen"),
        Tuple.Create("atc-snippets", "lupusbytes"),
        Tuple.Create("atc-test", "rickykaare"),
        Tuple.Create("atc-winget-configurations", "davidkallesen"),
        Tuple.Create("atc-winget-configurations", "perkops"),
        Tuple.Create("atc-wpf", "davidkallesen"),
    };

    public static string[] GetResponsibleMembersByName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return ResponsibleMembers
                .Select(x => x.Item2)
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToArray();
        }

        return ResponsibleMembers
            .Where(x => x.Item1.Equals(name, StringComparison.Ordinal))
            .Select(x => x.Item2)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray();
    }

    public static bool HasNotRequiredResponsibleMembersByName(string name)
    {
        return GetResponsibleMembersByName(name).Length < 2;
    }

    public static bool IsVisualStudioNameInAcceptedVersion(string visualStudioName)
        => !string.IsNullOrEmpty(visualStudioName) &&
           visualStudioName.Equals(RecommendedVisualStudioName, StringComparison.Ordinal);

    public static bool IsTargetFrameworkInLongTimeSupport(string targetFramework)
        => !string.IsNullOrEmpty(targetFramework) &&
           RecommendedTargetFramework.Contains(targetFramework, StringComparer.OrdinalIgnoreCase);

    public static bool IsLangVersionInAcceptedVersion(string langVersion)
        => !string.IsNullOrEmpty(langVersion) &&
           langVersion.Contains(RecommendedLangVersion, StringComparison.Ordinal);
}