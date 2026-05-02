# Phase 2 — Compliance Overview Redesign

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the existing `/support/repository-compliance-overview` page (one slow alphabetical section per repo) with a hybrid layout: KPI strip → sticky filter bar → sortable/filterable `MudDataGrid` dashboard with expandable row detail → cards grid below. Driven by one call to the new atc-api endpoint from Phase 1.

**Architecture:** Add a new `GetComplianceSummary()` method to `AtcApiGitHubRepositoryClient` reusing the existing two-tier cache pattern. Build a small set of focused components in `src/AtcWeb/Components/Compliance/`. Filter/sort/group state lives in the page-level component; data is fetched once and held in memory.

**Tech Stack:** Blazor WebAssembly, MudBlazor (`MudDataGrid`, `MudChip`, `MudSelect`, `MudTextField`), C# 14, .NET 10, existing `_modern.scss` palette.

**Working repo:** `D:\Code\atc-net\atc-net.github.io` on branch `feature/compliance-summary` (already created).
**Spec:** `docs/superpowers/specs/2026-05-02-repository-compliance-and-insights-design.md`
**Prerequisite:** Phase 1 deployed and `https://atc-api.example.com/github/repository/compliance-summary` returning data.

---

## File structure

**Create:**
- `src/AtcWeb.Domain/AtcApi/Models/Compliance/RepositoryComplianceSummary.cs` (+ 6 sibling DTO files)
- `src/AtcWeb.Domain/Compliance/ComplianceHealth.cs` — enum + helper for green/amber/red derivation
- `src/AtcWeb.Domain/Compliance/ComplianceFilterState.cs` — filter state record
- `src/AtcWeb.Domain/Compliance/ComplianceFilterEngine.cs` — applies filters to the in-memory list
- `src/AtcWeb/Components/Compliance/ComplianceStatusChip.razor` (+ `.razor.cs`)
- `src/AtcWeb/Components/Compliance/ComplianceKpiStrip.razor` (+ `.razor.cs`)
- `src/AtcWeb/Components/Compliance/ComplianceFilterBar.razor` (+ `.razor.cs`)
- `src/AtcWeb/Components/Compliance/ComplianceDashboardTable.razor` (+ `.razor.cs`)
- `src/AtcWeb/Components/Compliance/ComplianceDashboardRowDetail.razor` (+ `.razor.cs`)
- `src/AtcWeb/Components/Compliance/ComplianceCardsGrid.razor` (+ `.razor.cs`)
- `test/AtcWeb.Domain.Tests/Compliance/ComplianceFilterEngineTests.cs`
- `test/AtcWeb.Domain.Tests/Compliance/ComplianceHealthTests.cs`

**Modify:**
- `src/AtcWeb.Domain/AtcApi/AtcApiGitHubRepositoryClient.cs` — add `GetComplianceSummary()`
- `src/AtcWeb.Domain/GitHub/CacheConstants.cs` — add `CacheKeyComplianceSummary`
- `src/AtcWeb/Pages/Support/RepositoryComplianceOverview.razor`
- `src/AtcWeb/Pages/Support/RepositoryComplianceOverview.razor.cs`
- `src/AtcWeb/Styles/_modern.scss` — add three CSS variables for status colors and the dashboard table styles
- `src/AtcWeb/wwwroot/css/AtcWeb.css` + `AtcWeb.min.css` — recompile from SCSS at the end

---

## Task 1 — Domain DTOs mirroring atc-api schemas

**Files:**
- Create: `src/AtcWeb.Domain/AtcApi/Models/Compliance/RepositoryComplianceSummary.cs`
- Create: `src/AtcWeb.Domain/AtcApi/Models/Compliance/RepositoryComplianceSignals.cs`
- Create: `src/AtcWeb.Domain/AtcApi/Models/Compliance/EditorConfigStatus.cs`
- Create: `src/AtcWeb.Domain/AtcApi/Models/Compliance/WorkflowsStatus.cs`
- Create: `src/AtcWeb.Domain/AtcApi/Models/Compliance/RepositoryComplianceDetail.cs`
- Create: `src/AtcWeb.Domain/AtcApi/Models/Compliance/AnalyzerPackageRef.cs`
- Create: `src/AtcWeb.Domain/AtcApi/Models/Compliance/XunitV3Status.cs`

These mirror the atc-api types one-to-one. Property names are PascalCase; JSON serializer is configured for camelCase elsewhere in this project (`JsonSerializerOptionsFactory`).

- [ ] **Step 1: XunitV3Status enum**

```csharp
namespace AtcWeb.Domain.AtcApi.Models.Compliance;

public enum XunitV3Status
{
    Yes,
    No,
    NotApplicable,
}
```

- [ ] **Step 2: EditorConfigStatus**

```csharp
namespace AtcWeb.Domain.AtcApi.Models.Compliance;

public sealed class EditorConfigStatus
{
    public bool RootPresent { get; init; }
    public bool RootIsLatest { get; init; }
    public string? RootVersion { get; init; }
    public bool SrcPresent { get; init; }
    public bool SrcIsLatest { get; init; }
    public string? SrcVersion { get; init; }
    public bool TestPresent { get; init; }
    public bool TestIsLatest { get; init; }
    public string? TestVersion { get; init; }
}
```

- [ ] **Step 3: WorkflowsStatus**

```csharp
namespace AtcWeb.Domain.AtcApi.Models.Compliance;

public sealed class WorkflowsStatus
{
    public List<string> Actions { get; init; } = [];
    public List<string> DotnetVersions { get; init; } = [];
    public bool CheckoutIsLatest { get; init; }
    public bool SetupDotnetIsLatest { get; init; }
    public bool HasJavaSetup { get; init; }
    public bool DotnetVersionIsLatest { get; init; }
}
```

- [ ] **Step 4: AnalyzerPackageRef**

```csharp
namespace AtcWeb.Domain.AtcApi.Models.Compliance;

public sealed class AnalyzerPackageRef
{
    public string PackageId { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public bool IsLatest { get; init; }
}
```

- [ ] **Step 5: RepositoryComplianceDetail**

```csharp
namespace AtcWeb.Domain.AtcApi.Models.Compliance;

public sealed class RepositoryComplianceDetail
{
    public List<string> SrcFrameworks { get; init; } = [];
    public List<string> TestFrameworks { get; init; } = [];
    public List<string> SampleFrameworks { get; init; } = [];
    public List<AnalyzerPackageRef> AnalyzerPackages { get; init; } = [];
    public List<string> SuppressedRulesRoot { get; init; } = [];
    public List<string> SuppressedRulesSrc { get; init; } = [];
    public List<string> SuppressedRulesTest { get; init; } = [];
}
```

- [ ] **Step 6: RepositoryComplianceSignals**

```csharp
namespace AtcWeb.Domain.AtcApi.Models.Compliance;

public sealed class RepositoryComplianceSignals
{
    public bool HasGoodReadme { get; init; }
    public bool LicenseIsMit { get; init; }
    public bool HomepageIsAtcWeb { get; init; }
    public EditorConfigStatus EditorConfigStatus { get; init; } = new();
    public bool UpdaterPresent { get; init; }
    public bool UpdaterTargetIsLatest { get; init; }
    public string? UpdaterProjectTarget { get; init; }
    public bool GlobalLangVersionIsLatest { get; init; }
    public string? GlobalLangVersion { get; init; }
    public bool GlobalTargetFrameworkIsLatest { get; init; }
    public string? GlobalTargetFramework { get; init; }
    public XunitV3Status XunitV3Status { get; init; } = XunitV3Status.NotApplicable;
    public WorkflowsStatus WorkflowsStatus { get; init; } = new();
    public bool ReleasePleasePresent { get; init; }
}
```

- [ ] **Step 7: RepositoryComplianceSummary**

```csharp
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
```

- [ ] **Step 8: Build**

Run: `dotnet build src/AtcWeb.Domain/AtcWeb.Domain.csproj`
Expected: success.

- [ ] **Step 9: Commit**

```powershell
git add src/AtcWeb.Domain/AtcApi/Models/Compliance/
git commit -m "feat(compliance): add RepositoryComplianceSummary domain DTOs"
```

---

## Task 2 — Cache key + client method

**Files:**
- Modify: `src/AtcWeb.Domain/GitHub/CacheConstants.cs`
- Modify: `src/AtcWeb.Domain/AtcApi/AtcApiGitHubRepositoryClient.cs`

- [ ] **Step 1: Read existing CacheConstants**

Run: `Read src/AtcWeb.Domain/GitHub/CacheConstants.cs`.

- [ ] **Step 2: Add the cache key**

Inside the `CacheConstants` static class, add:

```csharp
public const string CacheKeyComplianceSummary = "atc-web.compliance-summary";
```

- [ ] **Step 3: Add client method**

In `src/AtcWeb.Domain/AtcApi/AtcApiGitHubRepositoryClient.cs`, add a new field and method following the existing pattern (memory cache → browser cache → HTTP):

```csharp
private static readonly SemaphoreSlim SemaphoreCompliance = new(1, 1);

public async Task<(bool IsSuccessful, List<RepositoryComplianceSummary> Summaries)> GetComplianceSummary(
    CancellationToken cancellationToken = default)
{
    const string cacheKey = CacheConstants.CacheKeyComplianceSummary;
    if (memoryCache.TryGetValue(cacheKey, out List<RepositoryComplianceSummary> data))
    {
        return (IsSuccessful: true, data!);
    }

    var browserCached = await browserCache.GetAsync<List<RepositoryComplianceSummary>>(cacheKey);
    if (browserCached is not null)
    {
        memoryCache.Set(cacheKey, browserCached, CacheConstants.AbsoluteExpirationRelativeToNow);
        return (IsSuccessful: true, browserCached);
    }

    await SemaphoreCompliance.WaitAsync(cancellationToken);
    try
    {
        const string url = $"{BaseAddress}/compliance-summary";
        var response = await httpClient.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return (IsSuccessful: false, []);
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<List<RepositoryComplianceSummary>>(
            content,
            JsonSerializerOptionsFactory.Create());

        if (result is null)
        {
            return (IsSuccessful: false, []);
        }

        memoryCache.Set(cacheKey, result, CacheConstants.AbsoluteExpirationRelativeToNow);
        await browserCache.SetAsync(cacheKey, result);
        return (IsSuccessful: true, result);
    }
    catch
    {
        return (IsSuccessful: false, []);
    }
    finally
    {
        SemaphoreCompliance.Release();
    }
}
```

Add the using `using AtcWeb.Domain.AtcApi.Models.Compliance;` at the top of the file (or rely on `GlobalUsings.cs` and add it there if other Atc projects import models that way — verify by build).

- [ ] **Step 4: Build**

Run: `dotnet build src/AtcWeb.Domain/AtcWeb.Domain.csproj`
Expected: success.

- [ ] **Step 5: Commit**

```powershell
git add src/AtcWeb.Domain/GitHub/CacheConstants.cs src/AtcWeb.Domain/AtcApi/AtcApiGitHubRepositoryClient.cs
git commit -m "feat(compliance): add GetComplianceSummary client method with two-tier cache"
```

---

## Task 3 — ComplianceHealth helper (TDD)

**Files:**
- Create: `src/AtcWeb.Domain/Compliance/ComplianceHealth.cs`
- Test: `test/AtcWeb.Domain.Tests/Compliance/ComplianceHealthTests.cs`

`ComplianceHealth` derives an `Ok | Warning | Error` aggregate per summary, used for health filtering and KPI math.

- [ ] **Step 1: Adjust DTO mutability for tests**

In `RepositoryComplianceSignals.cs`, change `init` → `set` on the boolean properties (keep object properties as `init`). In `WorkflowsStatus.cs`, do the same. Rebuild.

```csharp
// RepositoryComplianceSignals.cs — change all bool init to set:
public bool HasGoodReadme { get; set; }
public bool LicenseIsMit { get; set; }
public bool HomepageIsAtcWeb { get; set; }
public bool UpdaterPresent { get; set; }
public bool UpdaterTargetIsLatest { get; set; }
public bool GlobalLangVersionIsLatest { get; set; }
public bool GlobalTargetFrameworkIsLatest { get; set; }
public bool ReleasePleasePresent { get; set; }
public XunitV3Status XunitV3Status { get; set; } = XunitV3Status.NotApplicable;
```

```csharp
// WorkflowsStatus.cs — bools become set:
public bool CheckoutIsLatest { get; set; }
public bool SetupDotnetIsLatest { get; set; }
public bool HasJavaSetup { get; set; }
public bool DotnetVersionIsLatest { get; set; }
```

JSON deserialization works fine with `set`.

- [ ] **Step 2: Write the failing tests**

```csharp
namespace AtcWeb.Domain.Tests.Compliance;

using AtcWeb.Domain.AtcApi.Models.Compliance;
using AtcWeb.Domain.Compliance;
using FluentAssertions;
using Xunit;

public class ComplianceHealthTests
{
    [Fact]
    public void Compute_ReturnsOk_WhenAllGreen()
    {
        ComplianceHealth.Compute(AllGreenSummary()).Should().Be(HealthStatus.Ok);
    }

    [Fact]
    public void Compute_ReturnsError_WhenLicenseNotMit()
    {
        var s = AllGreenSummary();
        s.Signals.LicenseIsMit = false;
        ComplianceHealth.Compute(s).Should().Be(HealthStatus.Error);
    }

    [Fact]
    public void Compute_ReturnsError_WhenJavaSetupPresent()
    {
        var s = AllGreenSummary();
        s.Signals.WorkflowsStatus.HasJavaSetup = true;
        ComplianceHealth.Compute(s).Should().Be(HealthStatus.Error);
    }

    [Fact]
    public void Compute_ReturnsWarning_WhenReadmeMissingButNoErrors()
    {
        var s = AllGreenSummary();
        s.Signals.HasGoodReadme = false;
        ComplianceHealth.Compute(s).Should().Be(HealthStatus.Warning);
    }

    private static RepositoryComplianceSummary AllGreenSummary() => new()
    {
        Name = "atc-test",
        Language = "C#",
        LicenseKey = "mit",
        Signals = new RepositoryComplianceSignals
        {
            HasGoodReadme = true,
            LicenseIsMit = true,
            HomepageIsAtcWeb = true,
            UpdaterPresent = true,
            UpdaterTargetIsLatest = true,
            GlobalLangVersionIsLatest = true,
            GlobalTargetFrameworkIsLatest = true,
            XunitV3Status = XunitV3Status.Yes,
            ReleasePleasePresent = true,
            EditorConfigStatus = new EditorConfigStatus
            {
                RootPresent = true, RootIsLatest = true,
                SrcPresent = true, SrcIsLatest = true,
                TestPresent = true, TestIsLatest = true,
            },
            WorkflowsStatus = new WorkflowsStatus
            {
                CheckoutIsLatest = true,
                SetupDotnetIsLatest = true,
                DotnetVersionIsLatest = true,
                HasJavaSetup = false,
            },
        },
    };
}
```

- [ ] **Step 3: Run tests to confirm they fail**

Run: `dotnet test --filter FullyQualifiedName~ComplianceHealthTests`
Expected: compile error (`ComplianceHealth` not found).

- [ ] **Step 4: Implement ComplianceHealth**

```csharp
namespace AtcWeb.Domain.Compliance;

using AtcWeb.Domain.AtcApi.Models.Compliance;

public enum HealthStatus
{
    Ok,
    Warning,
    Error,
}

public static class ComplianceHealth
{
    public static HealthStatus Compute(RepositoryComplianceSummary summary)
    {
        var s = summary.Signals;
        var isDotnet = string.Equals(summary.Language, "C#", StringComparison.Ordinal);

        // Errors — broken or wrong-license repos.
        if (!s.LicenseIsMit) return HealthStatus.Error;
        if (s.WorkflowsStatus.HasJavaSetup) return HealthStatus.Error;
        if (isDotnet && !s.GlobalTargetFrameworkIsLatest && !string.IsNullOrEmpty(s.GlobalTargetFramework))
        {
            // Repo declares a TFM that isn't latest → error
            return HealthStatus.Error;
        }

        // Warnings — drift from latest rule set.
        if (!s.HasGoodReadme) return HealthStatus.Warning;
        if (!s.HomepageIsAtcWeb) return HealthStatus.Warning;
        if (isDotnet)
        {
            if (!s.UpdaterPresent || !s.UpdaterTargetIsLatest) return HealthStatus.Warning;
            if (!s.EditorConfigStatus.RootIsLatest) return HealthStatus.Warning;
            if (!s.EditorConfigStatus.SrcIsLatest) return HealthStatus.Warning;
            if (!s.EditorConfigStatus.TestIsLatest) return HealthStatus.Warning;
            if (!s.GlobalLangVersionIsLatest) return HealthStatus.Warning;
            if (s.XunitV3Status == XunitV3Status.No) return HealthStatus.Warning;
            if (!s.WorkflowsStatus.CheckoutIsLatest) return HealthStatus.Warning;
            if (!s.WorkflowsStatus.SetupDotnetIsLatest) return HealthStatus.Warning;
            if (!s.WorkflowsStatus.DotnetVersionIsLatest) return HealthStatus.Warning;
            if (!s.ReleasePleasePresent) return HealthStatus.Warning;
        }

        return HealthStatus.Ok;
    }
}
```

- [ ] **Step 5: Run tests to confirm they pass**

Run: `dotnet test --filter FullyQualifiedName~ComplianceHealthTests`
Expected: 4 passed.

- [ ] **Step 6: Commit**

```powershell
git add src/AtcWeb.Domain/AtcApi/Models/Compliance/ src/AtcWeb.Domain/Compliance/ComplianceHealth.cs test/AtcWeb.Domain.Tests/Compliance/ComplianceHealthTests.cs
git commit -m "feat(compliance): add ComplianceHealth helper and adjust DTO mutability"
```

---

## Task 4 — Filter state + engine (TDD)

**Files:**
- Create: `src/AtcWeb.Domain/Compliance/ComplianceFilterState.cs`
- Create: `src/AtcWeb.Domain/Compliance/ComplianceFilterEngine.cs`
- Test: `test/AtcWeb.Domain.Tests/Compliance/ComplianceFilterEngineTests.cs`

- [ ] **Step 1: Write tests**

```csharp
namespace AtcWeb.Domain.Tests.Compliance;

using AtcWeb.Domain.AtcApi.Models.Compliance;
using AtcWeb.Domain.Compliance;
using FluentAssertions;
using Xunit;

public class ComplianceFilterEngineTests
{
    [Fact]
    public void Apply_ReturnsAll_WhenStateIsEmpty()
    {
        var data = new[] { Make("atc-rest", "C#"), Make("atc-foo", "Python") };
        ComplianceFilterEngine.Apply(data, new ComplianceFilterState()).Should().HaveCount(2);
    }

    [Fact]
    public void Apply_FiltersByLanguage()
    {
        var data = new[] { Make("a", "C#"), Make("b", "Python") };
        var state = new ComplianceFilterState { Language = "C#" };
        ComplianceFilterEngine.Apply(data, state).Should().ContainSingle(s => s.Name == "a");
    }

    [Fact]
    public void Apply_FiltersBySearchText_OnNameAndDescription()
    {
        var data = new[]
        {
            Make("atc-rest-api-generator", "C#", description: "Generates REST APIs"),
            Make("atc-iot", "C#", description: "IoT helpers"),
        };
        var state = new ComplianceFilterState { SearchText = "rest" };
        ComplianceFilterEngine.Apply(data, state).Should().ContainSingle(s => s.Name.StartsWith("atc-rest", StringComparison.Ordinal));
    }

    [Fact]
    public void Apply_FiltersByHealth()
    {
        var ok = Make("ok", "C#"); ok.Signals.LicenseIsMit = true;
        var bad = Make("bad", "C#"); bad.Signals.LicenseIsMit = false;
        var state = new ComplianceFilterState { Health = HealthStatus.Error };
        ComplianceFilterEngine.Apply(new[] { ok, bad }, state).Should().ContainSingle(s => s.Name == "bad");
    }

    private static RepositoryComplianceSummary Make(string name, string language, string? description = null) =>
        new()
        {
            Name = name,
            Language = language,
            LicenseKey = "mit",
            Description = description,
            Signals = new RepositoryComplianceSignals
            {
                LicenseIsMit = true,
                EditorConfigStatus = new EditorConfigStatus(),
                WorkflowsStatus = new WorkflowsStatus(),
            },
        };
}
```

- [ ] **Step 2: Run tests to confirm they fail**

Run: `dotnet test --filter FullyQualifiedName~ComplianceFilterEngineTests`
Expected: compile error.

- [ ] **Step 3: Implement state + engine**

`ComplianceFilterState.cs`:

```csharp
namespace AtcWeb.Domain.Compliance;

public sealed class ComplianceFilterState
{
    public string? SearchText { get; set; }
    public string? Language { get; set; }      // "C#", "Python", null = all
    public string? Category { get; set; }      // category name from RepositoryCategoryHelper, null = all
    public HealthStatus? Health { get; set; }  // null = all
    public List<string> FailingSignals { get; set; } = []; // signal keys, e.g. "TfmBehind"
}
```

`ComplianceFilterEngine.cs`:

```csharp
namespace AtcWeb.Domain.Compliance;

using AtcWeb.Domain.AtcApi.Models.Compliance;

public static class ComplianceFilterEngine
{
    public static IReadOnlyList<RepositoryComplianceSummary> Apply(
        IEnumerable<RepositoryComplianceSummary> source,
        ComplianceFilterState state)
    {
        var query = source.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(state.SearchText))
        {
            var needle = state.SearchText.Trim();
            query = query.Where(s =>
                s.Name.Contains(needle, StringComparison.OrdinalIgnoreCase) ||
                (s.Description ?? string.Empty).Contains(needle, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(state.Language))
        {
            query = query.Where(s => string.Equals(s.Language, state.Language, StringComparison.Ordinal));
        }

        if (state.Health is { } targetHealth)
        {
            query = query.Where(s => ComplianceHealth.Compute(s) == targetHealth);
        }

        if (state.FailingSignals.Count > 0)
        {
            query = query.Where(s => state.FailingSignals.All(key => SignalIsFailing(s, key)));
        }

        return query.OrderBy(s => s.Name, StringComparer.Ordinal).ToList();
    }

    private static bool SignalIsFailing(RepositoryComplianceSummary s, string key) => key switch
    {
        "ReadmeMissing" => !s.Signals.HasGoodReadme,
        "LicenseWrong" => !s.Signals.LicenseIsMit,
        "HomepageWrong" => !s.Signals.HomepageIsAtcWeb,
        "EditorConfigBehind" => !s.Signals.EditorConfigStatus.RootIsLatest
                             || !s.Signals.EditorConfigStatus.SrcIsLatest
                             || !s.Signals.EditorConfigStatus.TestIsLatest,
        "UpdaterBehind" => !s.Signals.UpdaterPresent || !s.Signals.UpdaterTargetIsLatest,
        "LangVersionBehind" => !s.Signals.GlobalLangVersionIsLatest,
        "TfmBehind" => !s.Signals.GlobalTargetFrameworkIsLatest,
        "XunitNotV3" => s.Signals.XunitV3Status == XunitV3Status.No,
        "WorkflowsBehind" => !s.Signals.WorkflowsStatus.CheckoutIsLatest
                          || !s.Signals.WorkflowsStatus.SetupDotnetIsLatest
                          || !s.Signals.WorkflowsStatus.DotnetVersionIsLatest
                          || s.Signals.WorkflowsStatus.HasJavaSetup,
        "NoReleasePlease" => !s.Signals.ReleasePleasePresent,
        _ => false,
    };
}
```

Note: category filter is applied at the page level (it depends on `RepositoryCategoryHelper` which lives in the AtcWeb project, not Domain).

- [ ] **Step 4: Run tests to confirm they pass**

Run: `dotnet test --filter FullyQualifiedName~ComplianceFilterEngineTests`
Expected: 4 passed.

- [ ] **Step 5: Commit**

```powershell
git add src/AtcWeb.Domain/Compliance/ test/AtcWeb.Domain.Tests/Compliance/ComplianceFilterEngineTests.cs
git commit -m "feat(compliance): add ComplianceFilterState and ComplianceFilterEngine"
```

---

## Task 5 — ComplianceStatusChip (atomic UI primitive)

**Files:**
- Create: `src/AtcWeb/Components/Compliance/ComplianceStatusChip.razor`
- Create: `src/AtcWeb/Components/Compliance/ComplianceStatusChip.razor.cs`

- [ ] **Step 1: Razor markup**

`ComplianceStatusChip.razor`:

```razor
@using MudBlazor

<MudTooltip Text="@Tooltip" Placement="Placement.Top">
    <MudChip T="string"
             Icon="@Icon"
             Size="Size.Small"
             Variant="Variant.Filled"
             Color="@Color"
             Class="atc-compliance-chip">
        @Label
    </MudChip>
</MudTooltip>
```

- [ ] **Step 2: Code-behind**

`ComplianceStatusChip.razor.cs`:

```csharp
namespace AtcWeb.Components.Compliance;

using MudBlazor;

public enum ChipState
{
    Ok,
    Warning,
    Error,
    NotApplicable,
}

public partial class ComplianceStatusChip
{
    [Parameter] public ChipState State { get; set; } = ChipState.Ok;
    [Parameter] public string Label { get; set; } = string.Empty;
    [Parameter] public string Tooltip { get; set; } = string.Empty;

    private Color Color => State switch
    {
        ChipState.Ok => MudBlazor.Color.Success,
        ChipState.Warning => MudBlazor.Color.Warning,
        ChipState.Error => MudBlazor.Color.Error,
        _ => MudBlazor.Color.Default,
    };

    private string Icon => State switch
    {
        ChipState.Ok => Icons.Material.Filled.CheckCircle,
        ChipState.Warning => Icons.Material.Filled.Warning,
        ChipState.Error => Icons.Material.Filled.Error,
        _ => Icons.Material.Filled.Remove,
    };
}
```

- [ ] **Step 3: Build**

Run: `dotnet build src/AtcWeb/AtcWeb.csproj`
Expected: success.

- [ ] **Step 4: Commit**

```powershell
git add src/AtcWeb/Components/Compliance/ComplianceStatusChip.razor src/AtcWeb/Components/Compliance/ComplianceStatusChip.razor.cs
git commit -m "feat(compliance): add ComplianceStatusChip primitive"
```

---

## Task 6 — ComplianceKpiStrip

**Files:**
- Create: `src/AtcWeb/Components/Compliance/ComplianceKpiStrip.razor`
- Create: `src/AtcWeb/Components/Compliance/ComplianceKpiStrip.razor.cs`

- [ ] **Step 1: Razor**

```razor
@using AtcWeb.Domain.AtcApi.Models.Compliance
@using AtcWeb.Domain.Compliance
@using MudBlazor

<div class="atc-kpi-strip d-flex flex-wrap gap-3 mb-4">
    <KpiTile Label="Repos" Value="@Total.ToString()" />
    <KpiTile Label="C#" Value="@CountByLanguage("C#").ToString()" />
    <KpiTile Label="Python" Value="@CountByLanguage("Python").ToString()" />
    <KpiTile Label="MIT licensed" Value="@PercentText(s => s.Signals.LicenseIsMit)" />
    <KpiTile Label="net10.0" Value="@PercentText(s => s.Signals.GlobalTargetFrameworkIsLatest)" />
    <KpiTile Label="release-please" Value="@PercentText(s => s.Signals.ReleasePleasePresent)" />
    <KpiTile Label=".editorconfig latest" Value="@PercentText(s =>
        s.Signals.EditorConfigStatus.RootIsLatest &&
        s.Signals.EditorConfigStatus.SrcIsLatest &&
        s.Signals.EditorConfigStatus.TestIsLatest)" />
    <KpiTile Label="xUnit v3" Value="@PercentText(s => s.Signals.XunitV3Status == XunitV3Status.Yes)" />
</div>
```

A small inline `KpiTile` razor component:

`src/AtcWeb/Components/Compliance/KpiTile.razor`:

```razor
<div class="atc-kpi-tile">
    <div class="atc-kpi-value">@Value</div>
    <div class="atc-kpi-label">@Label</div>
</div>

@code {
    [Parameter] public string Label { get; set; } = string.Empty;
    [Parameter] public string Value { get; set; } = string.Empty;
}
```

- [ ] **Step 2: Code-behind**

```csharp
namespace AtcWeb.Components.Compliance;

using AtcWeb.Domain.AtcApi.Models.Compliance;

public partial class ComplianceKpiStrip
{
    [Parameter] public IReadOnlyList<RepositoryComplianceSummary> Summaries { get; set; } = [];

    private int Total => Summaries.Count;

    private int CountByLanguage(string lang) =>
        Summaries.Count(s => string.Equals(s.Language, lang, StringComparison.Ordinal));

    private string PercentText(Func<RepositoryComplianceSummary, bool> predicate)
    {
        if (Summaries.Count == 0) return "0%";
        var n = Summaries.Count(predicate);
        return $"{(n * 100 / Summaries.Count)}% ({n})";
    }
}
```

- [ ] **Step 3: Build**

Run: `dotnet build src/AtcWeb/AtcWeb.csproj`
Expected: success.

- [ ] **Step 4: Commit**

```powershell
git add src/AtcWeb/Components/Compliance/ComplianceKpiStrip.razor src/AtcWeb/Components/Compliance/ComplianceKpiStrip.razor.cs src/AtcWeb/Components/Compliance/KpiTile.razor
git commit -m "feat(compliance): add ComplianceKpiStrip with org-level percentage tiles"
```

---

## Task 7 — ComplianceFilterBar

**Files:**
- Create: `src/AtcWeb/Components/Compliance/ComplianceFilterBar.razor`
- Create: `src/AtcWeb/Components/Compliance/ComplianceFilterBar.razor.cs`

- [ ] **Step 1: Razor**

```razor
@using AtcWeb.Domain.Compliance
@using MudBlazor

<div class="atc-filter-bar d-flex flex-wrap align-center gap-2 mb-3">
    <MudTextField T="string"
                  Value="@State.SearchText"
                  ValueChanged="OnSearchChanged"
                  Placeholder="Search repos…"
                  Variant="Variant.Outlined"
                  Margin="Margin.Dense"
                  Adornment="Adornment.Start"
                  AdornmentIcon="@Icons.Material.Filled.Search"
                  Style="min-width: 220px;" />

    <MudSelect T="string"
               Value="@State.Language"
               ValueChanged="OnLanguageChanged"
               Placeholder="Language"
               Variant="Variant.Outlined"
               Margin="Margin.Dense"
               Clearable="true"
               Style="min-width: 140px;">
        <MudSelectItem Value="@("C#")">C#</MudSelectItem>
        <MudSelectItem Value="@("Python")">Python</MudSelectItem>
    </MudSelect>

    <MudSelect T="string"
               Value="@State.Category"
               ValueChanged="OnCategoryChanged"
               Placeholder="Category"
               Variant="Variant.Outlined"
               Margin="Margin.Dense"
               Clearable="true"
               Style="min-width: 180px;">
        @foreach (var cat in Categories)
        {
            <MudSelectItem Value="@cat">@cat</MudSelectItem>
        }
    </MudSelect>

    <MudSelect T="HealthStatus?"
               Value="@State.Health"
               ValueChanged="OnHealthChanged"
               Placeholder="Health"
               Variant="Variant.Outlined"
               Margin="Margin.Dense"
               Clearable="true"
               Style="min-width: 160px;">
        <MudSelectItem T="HealthStatus?" Value="@((HealthStatus?)HealthStatus.Ok)">All green</MudSelectItem>
        <MudSelectItem T="HealthStatus?" Value="@((HealthStatus?)HealthStatus.Warning)">Has warning</MudSelectItem>
        <MudSelectItem T="HealthStatus?" Value="@((HealthStatus?)HealthStatus.Error)">Has error</MudSelectItem>
    </MudSelect>

    <MudSpacer />

    <MudSelect T="string"
               Value="@GroupBy"
               ValueChanged="OnGroupByChanged"
               Label="Group by"
               Variant="Variant.Outlined"
               Margin="Margin.Dense"
               Style="min-width: 160px;">
        <MudSelectItem Value="@("None")">None</MudSelectItem>
        <MudSelectItem Value="@("Category")">Category</MudSelectItem>
        <MudSelectItem Value="@("Health")">Health</MudSelectItem>
        <MudSelectItem Value="@("Language")">Language</MudSelectItem>
    </MudSelect>
</div>
```

- [ ] **Step 2: Code-behind**

```csharp
namespace AtcWeb.Components.Compliance;

using AtcWeb.Domain.Compliance;

public partial class ComplianceFilterBar
{
    [Parameter] public ComplianceFilterState State { get; set; } = new();
    [Parameter] public EventCallback<ComplianceFilterState> StateChanged { get; set; }
    [Parameter] public IReadOnlyList<string> Categories { get; set; } = [];
    [Parameter] public string GroupBy { get; set; } = "None";
    [Parameter] public EventCallback<string> GroupByChanged { get; set; }

    private Task OnSearchChanged(string value)        => Update(s => s.SearchText = value);
    private Task OnLanguageChanged(string value)      => Update(s => s.Language = value);
    private Task OnCategoryChanged(string value)      => Update(s => s.Category = value);
    private Task OnHealthChanged(HealthStatus? value) => Update(s => s.Health = value);
    private Task OnGroupByChanged(string value)       => GroupByChanged.InvokeAsync(value);

    private async Task Update(Action<ComplianceFilterState> mutate)
    {
        mutate(State);
        await StateChanged.InvokeAsync(State);
    }
}
```

- [ ] **Step 3: Build**

Run: `dotnet build src/AtcWeb/AtcWeb.csproj`
Expected: success.

- [ ] **Step 4: Commit**

```powershell
git add src/AtcWeb/Components/Compliance/ComplianceFilterBar.razor src/AtcWeb/Components/Compliance/ComplianceFilterBar.razor.cs
git commit -m "feat(compliance): add ComplianceFilterBar with search, dropdowns, group-by"
```

---

## Task 8 — ComplianceDashboardRowDetail

**Files:**
- Create: `src/AtcWeb/Components/Compliance/ComplianceDashboardRowDetail.razor`
- Create: `src/AtcWeb/Components/Compliance/ComplianceDashboardRowDetail.razor.cs`

- [ ] **Step 1: Razor**

```razor
@using AtcWeb.Domain.AtcApi.Models.Compliance
@using MudBlazor

@if (Summary is not null)
{
    <div class="atc-row-detail pa-3">
        <MudGrid>
            <MudItem xs="12" md="4">
                <MudText Typo="Typo.subtitle2">Frameworks</MudText>
                <div><b>src:</b> @JoinOrDash(Summary.Detail.SrcFrameworks)</div>
                <div><b>test:</b> @JoinOrDash(Summary.Detail.TestFrameworks)</div>
                <div><b>sample:</b> @JoinOrDash(Summary.Detail.SampleFrameworks)</div>
            </MudItem>
            <MudItem xs="12" md="4">
                <MudText Typo="Typo.subtitle2">Analyzer packages</MudText>
                @if (Summary.Detail.AnalyzerPackages.Count == 0)
                {
                    <MudText Typo="Typo.body2"><i>None pinned</i></MudText>
                }
                else
                {
                    foreach (var p in Summary.Detail.AnalyzerPackages)
                    {
                        <div>@p.PackageId <b>@p.Version</b></div>
                    }
                }
            </MudItem>
            <MudItem xs="12" md="4">
                <MudText Typo="Typo.subtitle2">Open issues</MudText>
                <div>Count: <b>@Summary.OpenIssuesCount</b></div>
                <div>Oldest: @FormatDate(Summary.OldestOpenIssueAt)</div>
                <div>Newest: @FormatDate(Summary.NewestOpenIssueAt)</div>
                <MudLink Href="@($"/repository/{Summary.Name}")" Color="Color.Primary">
                    Open full repo page
                </MudLink>
            </MudItem>
        </MudGrid>
    </div>
}
```

- [ ] **Step 2: Code-behind**

```csharp
namespace AtcWeb.Components.Compliance;

using AtcWeb.Domain.AtcApi.Models.Compliance;

public partial class ComplianceDashboardRowDetail
{
    [Parameter] public RepositoryComplianceSummary? Summary { get; set; }

    private static string JoinOrDash(IReadOnlyList<string> values)
        => values.Count == 0 ? "–" : string.Join(", ", values);

    private static string FormatDate(DateTimeOffset? d)
        => d?.ToString("yyyy-MM-dd") ?? "–";
}
```

- [ ] **Step 3: Build + commit**

Run: `dotnet build src/AtcWeb/AtcWeb.csproj`
Expected: success.

```powershell
git add src/AtcWeb/Components/Compliance/ComplianceDashboardRowDetail.razor src/AtcWeb/Components/Compliance/ComplianceDashboardRowDetail.razor.cs
git commit -m "feat(compliance): add ComplianceDashboardRowDetail expanded panel"
```

---

## Task 9 — ComplianceDashboardTable (the centerpiece)

**Files:**
- Create: `src/AtcWeb/Components/Compliance/ComplianceDashboardTable.razor`
- Create: `src/AtcWeb/Components/Compliance/ComplianceDashboardTable.razor.cs`

- [ ] **Step 1: Razor**

```razor
@using AtcWeb.Domain.AtcApi.Models.Compliance
@using AtcWeb.Domain.Compliance
@using AtcWeb.Styles
@using MudBlazor

<MudDataGrid T="RepositoryComplianceSummary"
             Items="@Summaries"
             Dense="true"
             Hover="true"
             Bordered="false"
             Filterable="false"
             SortMode="SortMode.Multiple"
             Groupable="true"
             GroupExpanded="true"
             Virtualize="true"
             Height="700px"
             Class="atc-compliance-grid">
    <Columns>
        <HierarchyColumn T="RepositoryComplianceSummary" />
        <PropertyColumn Property="x => x.Name" Title="Name">
            <CellTemplate>
                <a href="@($"/repository/{context.Item.Name}")" class="atc-link">@context.Item.Name</a>
            </CellTemplate>
        </PropertyColumn>
        <PropertyColumn Property="x => x.Language" Title="Lang" />
        <TemplateColumn Title="License" SortBy="x => x.Signals.LicenseIsMit">
            <CellTemplate>
                <ComplianceStatusChip State="@(context.Item.Signals.LicenseIsMit ? ChipState.Ok : ChipState.Error)"
                                      Label="@(context.Item.LicenseKey ?? "—")"
                                      Tooltip="MIT required" />
            </CellTemplate>
        </TemplateColumn>
        <TemplateColumn Title="Homepage" SortBy="x => x.Signals.HomepageIsAtcWeb">
            <CellTemplate>
                <ComplianceStatusChip State="@(context.Item.Signals.HomepageIsAtcWeb ? ChipState.Ok : ChipState.Warning)"
                                      Label="@(context.Item.Signals.HomepageIsAtcWeb ? "OK" : "Wrong")"
                                      Tooltip="Should be https://atc-net.github.io/repository/{name}" />
            </CellTemplate>
        </TemplateColumn>
        <TemplateColumn Title="README" SortBy="x => x.Signals.HasGoodReadme">
            <CellTemplate>
                <ComplianceStatusChip State="@(context.Item.Signals.HasGoodReadme ? ChipState.Ok : ChipState.Warning)"
                                      Label="@(context.Item.Signals.HasGoodReadme ? "OK" : "Thin")"
                                      Tooltip="readme.md should exist and be > 1500 bytes" />
            </CellTemplate>
        </TemplateColumn>
        <TemplateColumn Title="EditorCfg (R/S/T)">
            <CellTemplate>
                <span>@(Bool(context.Item.Signals.EditorConfigStatus.RootIsLatest))/@(Bool(context.Item.Signals.EditorConfigStatus.SrcIsLatest))/@(Bool(context.Item.Signals.EditorConfigStatus.TestIsLatest))</span>
            </CellTemplate>
        </TemplateColumn>
        <TemplateColumn Title="Updater" SortBy="x => x.Signals.UpdaterTargetIsLatest">
            <CellTemplate>
                <ComplianceStatusChip State="@UpdaterState(context.Item)"
                                      Label="@(context.Item.Signals.UpdaterProjectTarget ?? "–")"
                                      Tooltip="atc-coding-rules-updater.json projectTarget should be DotNet10" />
            </CellTemplate>
        </TemplateColumn>
        <TemplateColumn Title="LangVer" SortBy="x => x.Signals.GlobalLangVersionIsLatest">
            <CellTemplate>
                <ComplianceStatusChip State="@(context.Item.Signals.GlobalLangVersionIsLatest ? ChipState.Ok : ChipState.Warning)"
                                      Label="@(context.Item.Signals.GlobalLangVersion ?? "–")"
                                      Tooltip="LangVersion in Directory.Build.props should be 14.0" />
            </CellTemplate>
        </TemplateColumn>
        <TemplateColumn Title="TFM" SortBy="x => x.Signals.GlobalTargetFrameworkIsLatest">
            <CellTemplate>
                <ComplianceStatusChip State="@(context.Item.Signals.GlobalTargetFrameworkIsLatest ? ChipState.Ok : ChipState.Error)"
                                      Label="@(context.Item.Signals.GlobalTargetFramework ?? "–")"
                                      Tooltip="TargetFramework should be net10.0" />
            </CellTemplate>
        </TemplateColumn>
        <TemplateColumn Title="xUnit v3">
            <CellTemplate>
                <ComplianceStatusChip State="@XunitState(context.Item.Signals.XunitV3Status)"
                                      Label="@context.Item.Signals.XunitV3Status.ToString()"
                                      Tooltip="xunit.v3 in test/Directory.Build.props" />
            </CellTemplate>
        </TemplateColumn>
        <TemplateColumn Title="Workflows">
            <CellTemplate>
                <ComplianceStatusChip State="@WorkflowsState(context.Item.Signals.WorkflowsStatus)"
                                      Label="@WorkflowsLabel(context.Item.Signals.WorkflowsStatus)"
                                      Tooltip="checkout@v6, setup-dotnet@v5, .NET 10, no Java" />
            </CellTemplate>
        </TemplateColumn>
        <TemplateColumn Title="release-please" SortBy="x => x.Signals.ReleasePleasePresent">
            <CellTemplate>
                <ComplianceStatusChip State="@(context.Item.Signals.ReleasePleasePresent ? ChipState.Ok : ChipState.Warning)"
                                      Label="@(context.Item.Signals.ReleasePleasePresent ? "Yes" : "No")"
                                      Tooltip=".github/workflows/release-please.yml present" />
            </CellTemplate>
        </TemplateColumn>
    </Columns>
    <ChildRowContent>
        <ComplianceDashboardRowDetail Summary="@context.Item" />
    </ChildRowContent>
</MudDataGrid>
```

- [ ] **Step 2: Code-behind**

```csharp
namespace AtcWeb.Components.Compliance;

using AtcWeb.Domain.AtcApi.Models.Compliance;

public partial class ComplianceDashboardTable
{
    [Parameter] public IReadOnlyList<RepositoryComplianceSummary> Summaries { get; set; } = [];

    private static string Bool(bool ok) => ok ? "✓" : "✗";

    private static ChipState UpdaterState(RepositoryComplianceSummary s)
    {
        if (!s.Signals.UpdaterPresent) return ChipState.Warning;
        return s.Signals.UpdaterTargetIsLatest ? ChipState.Ok : ChipState.Warning;
    }

    private static ChipState XunitState(XunitV3Status status) => status switch
    {
        XunitV3Status.Yes => ChipState.Ok,
        XunitV3Status.No => ChipState.Warning,
        _ => ChipState.NotApplicable,
    };

    private static ChipState WorkflowsState(WorkflowsStatus w)
    {
        if (w.HasJavaSetup) return ChipState.Error;
        if (!w.CheckoutIsLatest || !w.SetupDotnetIsLatest || !w.DotnetVersionIsLatest) return ChipState.Warning;
        return ChipState.Ok;
    }

    private static string WorkflowsLabel(WorkflowsStatus w)
    {
        if (w.HasJavaSetup) return "Java!";
        if (!w.CheckoutIsLatest) return "co<v6";
        if (!w.SetupDotnetIsLatest) return "sd<v5";
        if (!w.DotnetVersionIsLatest) return ".NET<10";
        return "OK";
    }
}
```

- [ ] **Step 3: Build**

Run: `dotnet build src/AtcWeb/AtcWeb.csproj`
Expected: success.

- [ ] **Step 4: Commit**

```powershell
git add src/AtcWeb/Components/Compliance/ComplianceDashboardTable.razor src/AtcWeb/Components/Compliance/ComplianceDashboardTable.razor.cs
git commit -m "feat(compliance): add ComplianceDashboardTable with 12 signal columns"
```

---

## Task 10 — Cards grid

**Files:**
- Create: `src/AtcWeb/Components/Compliance/ComplianceCardsGrid.razor`
- Create: `src/AtcWeb/Components/Compliance/ComplianceCardsGrid.razor.cs`

Reuses the `repo-card` styles from `_modern.scss` already used by `/introduction/repository-overview`.

- [ ] **Step 1: Razor**

```razor
@using AtcWeb.Domain.AtcApi.Models.Compliance
@using AtcWeb.Domain.Compliance
@using AtcWeb.Styles
@using MudBlazor

<div class="featured-repos-grid">
    @foreach (var s in Summaries)
    {
        <a href="@($"/repository/{s.Name}")"
           class="repo-card @RepositoryCategoryHelper.GetCardCssClass(s.Name)"
           style="text-decoration: none;">
            <div class="repo-card-name">@s.Name</div>
            <div class="repo-card-description">
                @(string.IsNullOrEmpty(s.Description) ? "No description available." : s.Description)
            </div>
            <div class="repo-card-meta d-flex gap-1 flex-wrap">
                @if (!string.IsNullOrEmpty(s.Language))
                {
                    <MudChip T="string" Variant="Variant.Outlined" Size="Size.Small" Color="Color.Primary">@s.Language</MudChip>
                }
                <MudChip T="string"
                         Variant="Variant.Outlined"
                         Size="Size.Small"
                         Color="@(ComplianceHealth.Compute(s) == HealthStatus.Ok ? Color.Success
                                : ComplianceHealth.Compute(s) == HealthStatus.Warning ? Color.Warning
                                : Color.Error)">
                    @ComplianceHealth.Compute(s)
                </MudChip>
            </div>
        </a>
    }
</div>
```

- [ ] **Step 2: Code-behind**

```csharp
namespace AtcWeb.Components.Compliance;

using AtcWeb.Domain.AtcApi.Models.Compliance;

public partial class ComplianceCardsGrid
{
    [Parameter] public IReadOnlyList<RepositoryComplianceSummary> Summaries { get; set; } = [];
}
```

- [ ] **Step 3: Build + commit**

```powershell
git add src/AtcWeb/Components/Compliance/ComplianceCardsGrid.razor src/AtcWeb/Components/Compliance/ComplianceCardsGrid.razor.cs
git commit -m "feat(compliance): add ComplianceCardsGrid reusing repo-card styles"
```

---

## Task 11 — Re-wire the page

**Files:**
- Modify: `src/AtcWeb/Pages/Support/RepositoryComplianceOverview.razor`
- Modify: `src/AtcWeb/Pages/Support/RepositoryComplianceOverview.razor.cs`

- [ ] **Step 1: Replace the page markup**

Replace the entire file contents with:

```razor
@page "/support/repository-compliance-overview"
@using AtcWeb.Components.Compliance
@using AtcWeb.Domain.Compliance
@using AtcWeb.Styles

@inherits RepositoryComplianceOverviewBase

<DocsPage MaxWidth="MaxWidth.ExtraLarge">
    <DocsPageHeader Title="Repository compliance overview"
                    SubTitle="Latest ATC compliance signals across all repositories.">
    </DocsPageHeader>
    <DocsPageContent>
        @if (Summaries is null)
        {
            <MudGrid Justify="Justify.Center">
                <MudProgressCircular Color="Color.Primary" Size="Size.Medium" Indeterminate="true" />
            </MudGrid>
        }
        else
        {
            <ComplianceKpiStrip Summaries="@Summaries" />
            <ComplianceFilterBar State="FilterState"
                                 StateChanged="OnFilterChanged"
                                 Categories="@CategoryNames"
                                 GroupBy="@GroupBy"
                                 GroupByChanged="OnGroupByChanged" />
            <ComplianceDashboardTable Summaries="@Filtered" />
            <MudDivider Class="my-6" />
            <ComplianceCardsGrid Summaries="@Filtered" />
        }
    </DocsPageContent>
</DocsPage>
```

- [ ] **Step 2: Replace the code-behind**

```csharp
namespace AtcWeb.Pages.Support;

using AtcWeb.Domain.AtcApi;
using AtcWeb.Domain.AtcApi.Models.Compliance;
using AtcWeb.Domain.Compliance;
using AtcWeb.Styles;

public class RepositoryComplianceOverviewBase : ComponentBase
{
    [Inject] protected AtcApiGitHubRepositoryClient Client { get; set; } = default!;

    protected List<RepositoryComplianceSummary>? Summaries;
    protected ComplianceFilterState FilterState { get; set; } = new();
    protected string GroupBy { get; set; } = "None";

    protected IReadOnlyList<RepositoryComplianceSummary> Filtered =>
        Summaries is null
            ? []
            : ComplianceFilterEngine.Apply(Summaries, FilterState)
                .Where(s => string.IsNullOrEmpty(FilterState.Category) ||
                            string.Equals(RepositoryCategoryHelper.GetCategory(s.Name), FilterState.Category, StringComparison.Ordinal))
                .ToList();

    protected IReadOnlyList<string> CategoryNames =>
        Summaries is null
            ? []
            : Summaries.Select(s => RepositoryCategoryHelper.GetCategory(s.Name))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToList();

    protected override async Task OnInitializedAsync()
    {
        var (isSuccessful, summaries) = await Client.GetComplianceSummary();
        Summaries = isSuccessful ? summaries : [];
        await base.OnInitializedAsync();
    }

    protected void OnFilterChanged(ComplianceFilterState state)
    {
        FilterState = state;
        StateHasChanged();
    }

    protected void OnGroupByChanged(string value)
    {
        GroupBy = value;
        StateHasChanged();
    }
}
```

- [ ] **Step 3: Build**

Run: `dotnet build src/AtcWeb/AtcWeb.csproj`
Expected: success. The old `RepositorySummery`, `RepositoryDotNetSolution`, `RepositoryDotNetProjects`, `RepositoryCodingRules` references on this page are gone. They remain available for `/repository/{name}`.

- [ ] **Step 4: Commit**

```powershell
git add src/AtcWeb/Pages/Support/RepositoryComplianceOverview.razor src/AtcWeb/Pages/Support/RepositoryComplianceOverview.razor.cs
git commit -m "feat(compliance): rewrite RepositoryComplianceOverview page with dashboard + cards hybrid"
```

---

## Task 12 — Styling

**Files:**
- Modify: `src/AtcWeb/Styles/_modern.scss`

- [ ] **Step 1: Append SCSS**

At the bottom of `_modern.scss`:

```scss
.atc-kpi-strip {
    .atc-kpi-tile {
        background: var(--mud-palette-surface);
        border: 1px solid var(--mud-palette-lines-default);
        border-radius: 8px;
        padding: 12px 16px;
        min-width: 120px;

        .atc-kpi-value {
            font-size: 1.5rem;
            font-weight: 600;
            color: var(--mud-palette-primary);
        }

        .atc-kpi-label {
            font-size: 0.75rem;
            color: var(--mud-palette-text-secondary);
            text-transform: uppercase;
        }
    }
}

.atc-filter-bar {
    position: sticky;
    top: 64px;
    z-index: 10;
    background: var(--mud-palette-background);
    padding: 8px 0;
}

.atc-compliance-chip {
    min-width: 64px;
    justify-content: center;
}

.atc-row-detail {
    background: var(--mud-palette-surface);
    border-left: 3px solid var(--mud-palette-primary);
}

.atc-link {
    color: var(--mud-palette-primary);
    text-decoration: none;
    font-weight: 500;

    &:hover { text-decoration: underline; }
}
```

- [ ] **Step 2: Compile SCSS**

Run from the repo root:

```powershell
sass src/AtcWeb/Styles/AtcWeb.scss src/AtcWeb/wwwroot/css/AtcWeb.css --style=expanded --no-source-map
sass src/AtcWeb/Styles/AtcWeb.scss src/AtcWeb/wwwroot/css/AtcWeb.min.css --style=compressed --no-source-map
```

Expected: success (deprecation warnings about `@import` are harmless per CLAUDE.md).

- [ ] **Step 3: Commit**

```powershell
git add src/AtcWeb/Styles/_modern.scss src/AtcWeb/wwwroot/css/AtcWeb.css src/AtcWeb/wwwroot/css/AtcWeb.min.css
git commit -m "style(compliance): add KPI strip, filter bar, and row-detail styling"
```

---

## Task 13 — Manual UI verification

- [ ] **Step 1: Run the app**

Run: `dotnet run --project src/AtcWeb/AtcWeb.csproj`
Open `http://localhost:5XXX/support/repository-compliance-overview`.

- [ ] **Step 2: Verify**

Check:
- Page loads in under 5 seconds (warm cache should be < 1 s).
- KPI strip shows non-zero counts.
- All 12 columns render with chips.
- Clicking a row expands it, showing frameworks, analyzer packages, issue dates.
- Filter dropdowns: language, category, health all narrow the results.
- Search box filters by name.
- Group-by dropdown changes the grouping.
- Cards grid below the table reflects the same filter state.
- Browser console: no JS errors, no 4xx/5xx network requests.

If any check fails, fix and re-run before continuing.

- [ ] **Step 3: Stop the app and commit any UI fixes**

```powershell
# only if you made fixes
git add -A
git commit -m "fix(compliance): UI verification fixes"
```

---

## Task 14 — Push branch

- [ ] **Step 1: Run full test suite**

Run: `dotnet test`
Expected: all green.

- [ ] **Step 2: Push**

```powershell
git push -u origin feature/compliance-summary
```

Phase 2 done. Phase 3 (insights page) can start on the same branch.
