namespace AtcWeb.Domain.AtcApi.Models.Compliance;

public sealed class WorkflowsStatus
{
    public List<string> Actions { get; init; } = [];

    public List<string> DotnetVersions { get; init; } = [];

    public bool CheckoutIsLatest { get; set; }

    public bool SetupDotnetIsLatest { get; set; }

    public bool HasJavaSetup { get; set; }

    public bool DotnetVersionIsLatest { get; set; }
}