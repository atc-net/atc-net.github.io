namespace AtcWeb.Domain.Compliance;

public sealed class ComplianceFilterState
{
    public string? SearchText { get; set; }

    public string? Language { get; set; }

    public string? Category { get; set; }

    public HealthStatus? Health { get; set; }

    public List<string> FailingSignals { get; set; } = [];
}