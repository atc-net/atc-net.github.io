namespace AtcWeb.Styles;

public static class RepositoryCategoryHelper
{
    public static string GetCategory(string? name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return "Core & Libraries";
        }

        if (name.Contains("kepware", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("opc", StringComparison.OrdinalIgnoreCase))
        {
            return "Industrial";
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

        if (name.Contains("dsc-config", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("templates", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("aspire", StringComparison.OrdinalIgnoreCase))
        {
            return "Developer Tools";
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
            name.Contains("blazor", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("logviewer", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("installer", StringComparison.OrdinalIgnoreCase))
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
            "Industrial" => "repo-card-industrial",
            "IoT" => "repo-card-iot",
            "Azure" => "repo-card-azure",
            "REST & API" => "repo-card-rest",
            "AI & Agents" => "repo-card-ai",
            "Code Quality" => "repo-card-tools",
            "Developer Tools" => "repo-card-devtools",
            _ => "repo-card-core",
        };

    public static string GetIcon(string category)
        => category switch
        {
            "Industrial" => Icons.Material.Filled.Factory,
            "IoT" => Icons.Material.Filled.Sensors,
            "Azure" => Icons.Material.Filled.Cloud,
            "REST & API" => Icons.Material.Filled.Api,
            "AI & Agents" => Icons.Material.Filled.Psychology,
            "Code Quality" => Icons.Material.Filled.Rule,
            "Developer Tools" => Icons.Material.Filled.Construction,
            "UI & Desktop" => Icons.Material.Filled.DesktopWindows,
            "Testing" => Icons.Material.Filled.Science,
            "Core & Libraries" => Icons.Material.Filled.Hub,
            _ => Icons.Material.Filled.Folder,
        };

    public static int GetSortOrder(string category)
        => category switch
        {
            "AI & Agents" => 0,
            "Azure" => 1,
            "Code Quality" => 2,
            "Core & Libraries" => 3,
            "Developer Tools" => 4,
            "Industrial" => 5,
            "IoT" => 6,
            "REST & API" => 7,
            "Testing" => 8,
            "UI & Desktop" => 9,
            _ => 99,
        };
}