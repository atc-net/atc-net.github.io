namespace AtcWeb.Domain.AtcApi.Models.Compliance;

public sealed class RepositoryComplianceSignals
{
    public bool HasGoodReadme { get; set; }

    public bool LicenseIsMit { get; set; }

    public bool HomepageIsAtcWeb { get; set; }

    public EditorConfigStatus EditorConfigStatus { get; init; } = new();

    public bool UpdaterPresent { get; set; }

    public bool UpdaterTargetIsLatest { get; set; }

    public string? UpdaterProjectTarget { get; set; }

    public bool GlobalLangVersionIsLatest { get; set; }

    public string? GlobalLangVersion { get; set; }

    public bool GlobalTargetFrameworkIsLatest { get; set; }

    public string? GlobalTargetFramework { get; set; }

    public XunitV3Status XunitV3Status { get; set; } = XunitV3Status.NotApplicable;

    public WorkflowsStatus WorkflowsStatus { get; init; } = new();

    public bool ReleasePleasePresent { get; set; }
}