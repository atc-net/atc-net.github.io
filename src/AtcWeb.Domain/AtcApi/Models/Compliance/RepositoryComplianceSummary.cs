namespace AtcWeb.Domain.AtcApi.Models.Compliance;

public sealed class RepositoryComplianceSummary
{
    public string Name { get; init; } = string.Empty;

    public string? Language { get; init; }

    public string? Description { get; init; }

    public string? Homepage { get; init; }

    public string? LicenseKey { get; init; }

    public string? DefaultBranch { get; init; }

    public List<string> Topics { get; init; } = [];

    public int StargazersCount { get; init; }

    public int ForksCount { get; init; }

    public int OpenIssuesCount { get; init; }

    public DateTimeOffset? PushedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? OldestOpenIssueAt { get; init; }

    public DateTimeOffset? NewestOpenIssueAt { get; init; }

    public RepositoryComplianceSignals Signals { get; init; } = new();

    public RepositoryComplianceDetail Detail { get; init; } = new();
}