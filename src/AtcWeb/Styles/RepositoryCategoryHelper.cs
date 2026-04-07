namespace AtcWeb.Styles;

public static class RepositoryCategoryHelper
{
    public static string GetCategory(string? name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return "Core & Libraries";
        }

        if (name.Contains("iot", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("digitaltwin", StringComparison.OrdinalIgnoreCase))
        {
            return "IoT";
        }

        if (name.Contains("azure", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("cosmos", StringComparison.OrdinalIgnoreCase))
        {
            return "Azure";
        }

        if (name.Contains("rest", StringComparison.OrdinalIgnoreCase))
        {
            return "REST & API";
        }

        if (name.Contains("semantic-kernel", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("agentic", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("claude", StringComparison.OrdinalIgnoreCase))
        {
            return "AI & Agents";
        }

        if (name.Contains("coding-rules", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("analyzer", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("source-gen", StringComparison.OrdinalIgnoreCase))
        {
            return "Code Quality";
        }

        if (name.Contains("wpf", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("xaml", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("blazor", StringComparison.OrdinalIgnoreCase))
        {
            return "UI & Desktop";
        }

        if (name.Contains("test", StringComparison.OrdinalIgnoreCase))
        {
            return "Testing";
        }

        return "Core & Libraries";
    }

    public static string GetCardCssClass(string? name)
        => GetCategory(name) switch
        {
            "IoT" => "repo-card-iot",
            "Azure" => "repo-card-azure",
            "REST & API" => "repo-card-rest",
            "AI & Agents" => "repo-card-ai",
            "Code Quality" => "repo-card-tools",
            _ => "repo-card-core",
        };

    public static string GetIcon(string category)
        => category switch
        {
            "IoT" => Icons.Material.Filled.Sensors,
            "Azure" => Icons.Material.Filled.Cloud,
            "REST & API" => Icons.Material.Filled.Api,
            "AI & Agents" => Icons.Material.Filled.Psychology,
            "Code Quality" => Icons.Material.Filled.Rule,
            "UI & Desktop" => Icons.Material.Filled.DesktopWindows,
            "Testing" => Icons.Material.Filled.Science,
            "Core & Libraries" => Icons.Material.Filled.Hub,
            _ => Icons.Material.Filled.Folder,
        };

    public static int GetSortOrder(string category)
        => category switch
        {
            "Core & Libraries" => 0,
            "Azure" => 1,
            "IoT" => 2,
            "REST & API" => 3,
            "AI & Agents" => 4,
            "Code Quality" => 5,
            "UI & Desktop" => 6,
            "Testing" => 7,
            _ => 99,
        };
}