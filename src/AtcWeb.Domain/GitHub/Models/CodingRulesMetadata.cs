namespace AtcWeb.Domain.GitHub.Models;

public class CodingRulesMetadata
{
    public static Version LatestVersionRoot => new(1, 0, 7);

    public static Version LatestVersionSrc => new(1, 0, 5);

    public static Version LatestVersionTest => new(1, 0, 7);

    public string RawEditorConfigRoot { get; set; } = string.Empty;

    public string RawEditorConfigSrc { get; set; } = string.Empty;

    public string RawEditorConfigTest { get; set; } = string.Empty;

    public bool HasRoot => !string.IsNullOrEmpty(RawEditorConfigRoot);

    public bool HasSrc => !string.IsNullOrEmpty(RawEditorConfigSrc);

    public bool HasTest => !string.IsNullOrEmpty(RawEditorConfigTest);

    public bool IsLatestVersionRoot => IsLatestVersion(LatestVersionRoot, RawEditorConfigRoot);

    public bool IsLatestVersionSrc => IsLatestVersion(LatestVersionSrc, RawEditorConfigSrc);

    public bool IsLatestVersionTest => IsLatestVersion(LatestVersionTest, RawEditorConfigTest);

    public Version GetCurrentVersionRoot() => GetCurrentVersion(RawEditorConfigRoot);

    public Version GetCurrentVersionSrc() => GetCurrentVersion(RawEditorConfigSrc);

    public Version GetCurrentVersionTest() => GetCurrentVersion(RawEditorConfigTest);

    public List<KeyValueItem> GetLocalSuppressRulesRoot() => GetLocalSuppressRules(RawEditorConfigRoot);

    public List<KeyValueItem> GetLocalSuppressRulesSrc() => GetLocalSuppressRules(RawEditorConfigSrc);

    public List<KeyValueItem> GetLocalSuppressRulesTest() => GetLocalSuppressRules(RawEditorConfigTest);

    private static Version GetCurrentVersion(string rawText)
    {
        if (string.IsNullOrEmpty(rawText))
        {
            return new Version();
        }

        var lines = rawText.Split(Environment.NewLine);
        foreach (var line in lines)
        {
            if (!line.StartsWith("# Version: ", StringComparison.Ordinal))
            {
                continue;
            }

            var s = line.Replace("# Version: ", string.Empty, StringComparison.Ordinal).Trim();
            if (Version.TryParse(s, out var version))
            {
                return version;
            }
        }

        return new Version();
    }

    private static bool IsLatestVersion(Version latestVersion, string rawText)
    {
        if (string.IsNullOrEmpty(rawText))
        {
            return false;
        }

        var version = GetCurrentVersion(rawText);
        return version == latestVersion || version.GreaterThan(latestVersion);
    }

    private static List<KeyValueItem> GetLocalSuppressRules(string rawText)
    {
        var list = new List<KeyValueItem>();
        if (string.IsNullOrEmpty(rawText))
        {
            return list;
        }

        var lines = rawText.Split(Environment.NewLine);
        var isInCustomCodeAnalyzersRules = false;
        foreach (var line in lines)
        {
            if (line.StartsWith("# Custom - Code Analyzers Rules", StringComparison.Ordinal))
            {
                isInCustomCodeAnalyzersRules = true;
                continue;
            }

            if (!isInCustomCodeAnalyzersRules || !line.StartsWith("dotnet_diagnostic.", StringComparison.Ordinal))
            {
                continue;
            }

            var s = line.Replace("dotnet_diagnostic.", string.Empty, StringComparison.Ordinal);
            var ruleId = s.Substring(0, s.IndexOf('.', StringComparison.Ordinal));
            var comment = string.Empty;
            var sa = s.Split('#', StringSplitOptions.RemoveEmptyEntries);
            if (sa.Length == 2)
            {
                comment = sa[1].Trim();
            }

            list.Add(new KeyValueItem(ruleId, comment));
        }

        return list
            .OrderBy(x => x.Key, StringComparer.Ordinal)
            .ToList();
    }
}