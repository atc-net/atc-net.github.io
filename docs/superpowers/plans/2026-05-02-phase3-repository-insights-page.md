# Phase 3 — Repository Insights Page

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add `/support/repository-insights` — a single-page dashboard with three stacked sections: Org Health KPIs, Tech Adoption charts, and an Action List of repos needing attention. Reuses the cached `compliance-summary` payload from Phase 2 (no extra HTTP).

**Architecture:** New page route + four small components in `src/AtcWeb/Components/Insights/`. All aggregations are LINQ over the cached `List<RepositoryComplianceSummary>`. Charts use MudBlazor's built-in `MudChart` (donut + bar) — no new chart library.

**Tech Stack:** Blazor WebAssembly, MudBlazor, C# 14, .NET 10.

**Working repo:** `D:\Code\atc-net\atc-net.github.io` on branch `feature/compliance-summary`.
**Spec:** `docs/superpowers/specs/2026-05-02-repository-compliance-and-insights-design.md`
**Prerequisites:** Phase 1 deployed; Phase 2 complete and merged (or at least its `RepositoryComplianceSummary` DTO + `GetComplianceSummary()` client method exist).

---

## File structure

**Create:**
- `src/AtcWeb/Pages/Support/RepositoryInsights.razor`
- `src/AtcWeb/Pages/Support/RepositoryInsights.razor.cs`
- `src/AtcWeb/Components/Insights/InsightsKpiTiles.razor` (+ `.razor.cs`)
- `src/AtcWeb/Components/Insights/InsightsHealthByCategoryChart.razor` (+ `.razor.cs`)
- `src/AtcWeb/Components/Insights/InsightsAdoptionCharts.razor` (+ `.razor.cs`)
- `src/AtcWeb/Components/Insights/InsightsActionList.razor` (+ `.razor.cs`)

**Modify:**
- `src/AtcWeb/Shared/MainLayout.razor` — add menu entry
- `src/AtcWeb/Styles/_modern.scss` — minor section-header styling
- `src/AtcWeb/wwwroot/css/AtcWeb.css` + `AtcWeb.min.css` — recompile

---

## Task 1 — InsightsKpiTiles

**Files:**
- Create: `src/AtcWeb/Components/Insights/InsightsKpiTiles.razor`
- Create: `src/AtcWeb/Components/Insights/InsightsKpiTiles.razor.cs`

Headline tiles: total repos, % MIT, % net10.0, % release-please, % .editorconfig latest, % xUnit v3.

- [ ] **Step 1: Razor**

```razor
@using AtcWeb.Domain.AtcApi.Models.Compliance

<div class="atc-insights-tiles d-flex flex-wrap gap-3">
    <Tile Label="Total repos" Value="@Total.ToString()" />
    <Tile Label="MIT licensed" Percent="@Pct(s => s.Signals.LicenseIsMit)" />
    <Tile Label="On net10.0" Percent="@Pct(s => s.Signals.GlobalTargetFrameworkIsLatest)" />
    <Tile Label="release-please" Percent="@Pct(s => s.Signals.ReleasePleasePresent)" />
    <Tile Label="EditorCfg latest" Percent="@Pct(s =>
        s.Signals.EditorConfigStatus.RootIsLatest &&
        s.Signals.EditorConfigStatus.SrcIsLatest &&
        s.Signals.EditorConfigStatus.TestIsLatest)" />
    <Tile Label="xUnit v3" Percent="@Pct(s => s.Signals.XunitV3Status == XunitV3Status.Yes)" />
    <Tile Label="Homepage OK" Percent="@Pct(s => s.Signals.HomepageIsAtcWeb)" />
</div>
```

A small inline `Tile` razor component:

`src/AtcWeb/Components/Insights/Tile.razor`:

```razor
<div class="atc-insights-tile">
    @if (Percent is { } p)
    {
        <div class="atc-insights-tile-value">@p%</div>
    }
    else
    {
        <div class="atc-insights-tile-value">@Value</div>
    }
    <div class="atc-insights-tile-label">@Label</div>
</div>

@code {
    [Parameter] public string Label { get; set; } = string.Empty;
    [Parameter] public string Value { get; set; } = string.Empty;
    [Parameter] public int? Percent { get; set; }
}
```

- [ ] **Step 2: Code-behind**

```csharp
namespace AtcWeb.Components.Insights;

using AtcWeb.Domain.AtcApi.Models.Compliance;

public partial class InsightsKpiTiles
{
    [Parameter] public IReadOnlyList<RepositoryComplianceSummary> Summaries { get; set; } = [];

    private int Total => Summaries.Count;

    private int Pct(Func<RepositoryComplianceSummary, bool> predicate)
    {
        if (Summaries.Count == 0) return 0;
        return Summaries.Count(predicate) * 100 / Summaries.Count;
    }
}
```

- [ ] **Step 3: Build + commit**

Run: `dotnet build src/AtcWeb/AtcWeb.csproj`
Expected: success.

```powershell
git add src/AtcWeb/Components/Insights/InsightsKpiTiles.razor src/AtcWeb/Components/Insights/InsightsKpiTiles.razor.cs src/AtcWeb/Components/Insights/Tile.razor
git commit -m "feat(insights): add InsightsKpiTiles for org-level percentages"
```

---

## Task 2 — InsightsHealthByCategoryChart

**Files:**
- Create: `src/AtcWeb/Components/Insights/InsightsHealthByCategoryChart.razor`
- Create: `src/AtcWeb/Components/Insights/InsightsHealthByCategoryChart.razor.cs`

Stacked bar: x-axis category, three stacks per bar (OK / Warning / Error counts).

- [ ] **Step 1: Razor**

```razor
@using MudBlazor

@if (XAxis.Length > 0)
{
    <MudChart ChartType="ChartType.StackedBar"
              ChartSeries="@Series"
              XAxisLabels="@XAxis"
              Width="100%"
              Height="320px" />
}
else
{
    <MudText Typo="Typo.body2"><i>No data.</i></MudText>
}
```

- [ ] **Step 2: Code-behind**

```csharp
namespace AtcWeb.Components.Insights;

using AtcWeb.Domain.AtcApi.Models.Compliance;
using AtcWeb.Domain.Compliance;
using AtcWeb.Styles;
using MudBlazor;

public partial class InsightsHealthByCategoryChart
{
    [Parameter] public IReadOnlyList<RepositoryComplianceSummary> Summaries { get; set; } = [];

    protected string[] XAxis { get; private set; } = [];
    protected List<ChartSeries> Series { get; private set; } = [];

    protected override void OnParametersSet()
    {
        var byCategory = Summaries
            .GroupBy(s => RepositoryCategoryHelper.GetCategory(s.Name))
            .OrderBy(g => RepositoryCategoryHelper.GetSortOrder(g.Key))
            .ToList();

        XAxis = byCategory.Select(g => g.Key).ToArray();

        double[] ok = new double[XAxis.Length];
        double[] warn = new double[XAxis.Length];
        double[] err = new double[XAxis.Length];

        for (var i = 0; i < byCategory.Count; i++)
        {
            foreach (var s in byCategory[i])
            {
                switch (ComplianceHealth.Compute(s))
                {
                    case HealthStatus.Ok: ok[i]++; break;
                    case HealthStatus.Warning: warn[i]++; break;
                    case HealthStatus.Error: err[i]++; break;
                }
            }
        }

        Series =
        [
            new ChartSeries { Name = "OK", Data = ok },
            new ChartSeries { Name = "Warning", Data = warn },
            new ChartSeries { Name = "Error", Data = err },
        ];
    }
}
```

- [ ] **Step 3: Build + commit**

```powershell
git add src/AtcWeb/Components/Insights/InsightsHealthByCategoryChart.razor src/AtcWeb/Components/Insights/InsightsHealthByCategoryChart.razor.cs
git commit -m "feat(insights): add InsightsHealthByCategoryChart stacked-bar"
```

---

## Task 3 — InsightsAdoptionCharts

**Files:**
- Create: `src/AtcWeb/Components/Insights/InsightsAdoptionCharts.razor`
- Create: `src/AtcWeb/Components/Insights/InsightsAdoptionCharts.razor.cs`

Three sub-charts: TFM distribution donut, Atc.Analyzer version distribution bar, Top 20 external NuGet packages list.

- [ ] **Step 1: Razor**

```razor
@using MudBlazor

<MudGrid>
    <MudItem xs="12" md="4">
        <MudText Typo="Typo.h6">.NET TFM distribution</MudText>
        <MudChart ChartType="ChartType.Donut"
                  InputData="@TfmCounts"
                  InputLabels="@TfmLabels"
                  Width="100%"
                  Height="280px" />
    </MudItem>
    <MudItem xs="12" md="4">
        <MudText Typo="Typo.h6">Atc.Analyzer versions</MudText>
        @if (AnalyzerLabels.Length > 0)
        {
            <MudChart ChartType="ChartType.Bar"
                      ChartSeries="@AnalyzerSeries"
                      XAxisLabels="@AnalyzerLabels"
                      Width="100%"
                      Height="280px" />
        }
        else
        {
            <MudText Typo="Typo.body2"><i>No Atc.Analyzer references found.</i></MudText>
        }
    </MudItem>
    <MudItem xs="12" md="4">
        <MudText Typo="Typo.h6">Languages</MudText>
        <MudChart ChartType="ChartType.Donut"
                  InputData="@LanguageCounts"
                  InputLabels="@LanguageLabels"
                  Width="100%"
                  Height="280px" />
    </MudItem>
</MudGrid>
```

- [ ] **Step 2: Code-behind**

```csharp
namespace AtcWeb.Components.Insights;

using AtcWeb.Domain.AtcApi.Models.Compliance;
using MudBlazor;

public partial class InsightsAdoptionCharts
{
    [Parameter] public IReadOnlyList<RepositoryComplianceSummary> Summaries { get; set; } = [];

    protected double[] TfmCounts { get; private set; } = [];
    protected string[] TfmLabels { get; private set; } = [];

    protected string[] AnalyzerLabels { get; private set; } = [];
    protected List<ChartSeries> AnalyzerSeries { get; private set; } = [];

    protected double[] LanguageCounts { get; private set; } = [];
    protected string[] LanguageLabels { get; private set; } = [];

    protected override void OnParametersSet()
    {
        // TFMs (root Directory.Build.props value).
        var tfms = Summaries
            .Select(s => s.Signals.GlobalTargetFramework)
            .Where(v => !string.IsNullOrEmpty(v))
            .Cast<string>()
            .GroupBy(v => v, StringComparer.Ordinal)
            .OrderByDescending(g => g.Count())
            .ToList();
        TfmCounts = tfms.Select(g => (double)g.Count()).ToArray();
        TfmLabels = tfms.Select(g => g.Key).ToArray();

        // Atc.Analyzer versions.
        var analyzerVersions = Summaries
            .SelectMany(s => s.Detail.AnalyzerPackages)
            .Where(p => string.Equals(p.PackageId, "Atc.Analyzer", StringComparison.Ordinal))
            .GroupBy(p => p.Version, StringComparer.Ordinal)
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .ToList();
        AnalyzerLabels = analyzerVersions.Select(g => g.Key).ToArray();
        AnalyzerSeries =
        [
            new ChartSeries
            {
                Name = "Repos",
                Data = analyzerVersions.Select(g => (double)g.Count()).ToArray(),
            },
        ];

        // Languages.
        var langs = Summaries
            .GroupBy(s => string.IsNullOrEmpty(s.Language) ? "Unknown" : s.Language!, StringComparer.Ordinal)
            .OrderByDescending(g => g.Count())
            .ToList();
        LanguageCounts = langs.Select(g => (double)g.Count()).ToArray();
        LanguageLabels = langs.Select(g => g.Key).ToArray();
    }
}
```

- [ ] **Step 3: Build + commit**

```powershell
git add src/AtcWeb/Components/Insights/InsightsAdoptionCharts.razor src/AtcWeb/Components/Insights/InsightsAdoptionCharts.razor.cs
git commit -m "feat(insights): add InsightsAdoptionCharts for TFM, analyzer, language distribution"
```

---

## Task 4 — InsightsActionList

**Files:**
- Create: `src/AtcWeb/Components/Insights/InsightsActionList.razor`
- Create: `src/AtcWeb/Components/Insights/InsightsActionList.razor.cs`

For each repo with a non-OK health, lists its specific failing rules grouped by category.

- [ ] **Step 1: Razor**

```razor
@using AtcWeb.Domain.AtcApi.Models.Compliance
@using AtcWeb.Domain.Compliance
@using AtcWeb.Styles
@using MudBlazor

@if (Groups.Count == 0)
{
    <MudAlert Severity="Severity.Success">All repos look healthy. Nothing to do.</MudAlert>
}
else
{
    foreach (var group in Groups)
    {
        <MudText Typo="Typo.h6" Class="mt-4">@group.Category (@group.Items.Count)</MudText>
        <MudList T="string" Dense="true" DisableGutters="true">
            @foreach (var item in group.Items)
            {
                <MudListItem T="string"
                             Icon="@StatusIcon(item.Health)"
                             IconColor="@StatusColor(item.Health)">
                    <MudLink Href="@($"/repository/{item.Repo.Name}")" Color="Color.Primary">@item.Repo.Name</MudLink>
                    <span class="ml-2 text-secondary">@string.Join(" · ", item.FailingRules)</span>
                </MudListItem>
            }
        </MudList>
    }
}
```

- [ ] **Step 2: Code-behind**

```csharp
namespace AtcWeb.Components.Insights;

using AtcWeb.Domain.AtcApi.Models.Compliance;
using AtcWeb.Domain.Compliance;
using AtcWeb.Styles;
using MudBlazor;

public partial class InsightsActionList
{
    [Parameter] public IReadOnlyList<RepositoryComplianceSummary> Summaries { get; set; } = [];

    protected sealed record ActionItem(RepositoryComplianceSummary Repo, HealthStatus Health, IReadOnlyList<string> FailingRules);
    protected sealed record ActionGroup(string Category, IReadOnlyList<ActionItem> Items);

    protected List<ActionGroup> Groups { get; private set; } = [];

    protected override void OnParametersSet()
    {
        var items = Summaries
            .Select(s => new
            {
                Repo = s,
                Health = ComplianceHealth.Compute(s),
                Rules = ComputeFailingRules(s),
            })
            .Where(x => x.Health != HealthStatus.Ok)
            .Select(x => new ActionItem(x.Repo, x.Health, x.Rules))
            .ToList();

        Groups = items
            .GroupBy(i => RepositoryCategoryHelper.GetCategory(i.Repo.Name))
            .OrderBy(g => RepositoryCategoryHelper.GetSortOrder(g.Key))
            .Select(g => new ActionGroup(g.Key, g.OrderBy(x => x.Repo.Name, StringComparer.Ordinal).ToList()))
            .ToList();
    }

    private static IReadOnlyList<string> ComputeFailingRules(RepositoryComplianceSummary s)
    {
        var rules = new List<string>();
        if (!s.Signals.LicenseIsMit) rules.Add("License not MIT");
        if (s.Signals.WorkflowsStatus.HasJavaSetup) rules.Add("setup-java in workflow");
        if (!s.Signals.HasGoodReadme) rules.Add("README too thin");
        if (!s.Signals.HomepageIsAtcWeb) rules.Add("Homepage URL");
        if (string.Equals(s.Language, "C#", StringComparison.Ordinal))
        {
            if (!s.Signals.UpdaterPresent || !s.Signals.UpdaterTargetIsLatest) rules.Add("updater not DotNet10");
            if (!s.Signals.EditorConfigStatus.RootIsLatest ||
                !s.Signals.EditorConfigStatus.SrcIsLatest ||
                !s.Signals.EditorConfigStatus.TestIsLatest) rules.Add(".editorconfig behind");
            if (!s.Signals.GlobalLangVersionIsLatest) rules.Add("LangVersion behind");
            if (!s.Signals.GlobalTargetFrameworkIsLatest) rules.Add("TFM not net10.0");
            if (s.Signals.XunitV3Status == XunitV3Status.No) rules.Add("xUnit not v3");
            if (!s.Signals.WorkflowsStatus.CheckoutIsLatest) rules.Add("checkout < v6");
            if (!s.Signals.WorkflowsStatus.SetupDotnetIsLatest) rules.Add("setup-dotnet < v5");
            if (!s.Signals.WorkflowsStatus.DotnetVersionIsLatest) rules.Add(".NET version < 10");
            if (!s.Signals.ReleasePleasePresent) rules.Add("no release-please");
        }

        return rules;
    }

    private static string StatusIcon(HealthStatus h) => h switch
    {
        HealthStatus.Error => Icons.Material.Filled.Error,
        HealthStatus.Warning => Icons.Material.Filled.Warning,
        _ => Icons.Material.Filled.CheckCircle,
    };

    private static Color StatusColor(HealthStatus h) => h switch
    {
        HealthStatus.Error => Color.Error,
        HealthStatus.Warning => Color.Warning,
        _ => Color.Success,
    };
}
```

- [ ] **Step 3: Build + commit**

```powershell
git add src/AtcWeb/Components/Insights/InsightsActionList.razor src/AtcWeb/Components/Insights/InsightsActionList.razor.cs
git commit -m "feat(insights): add InsightsActionList grouping repos by category with failing rules"
```

---

## Task 5 — RepositoryInsights page

**Files:**
- Create: `src/AtcWeb/Pages/Support/RepositoryInsights.razor`
- Create: `src/AtcWeb/Pages/Support/RepositoryInsights.razor.cs`

- [ ] **Step 1: Razor**

```razor
@page "/support/repository-insights"
@using AtcWeb.Components.Insights
@inherits RepositoryInsightsBase

<DocsPage MaxWidth="MaxWidth.ExtraLarge">
    <DocsPageHeader Title="Repository insights"
                    SubTitle="Org-wide health, technology adoption, and a punch-list of repos needing attention." />
    <DocsPageContent>
        @if (Summaries is null)
        {
            <MudGrid Justify="Justify.Center">
                <MudProgressCircular Color="Color.Primary" Size="Size.Medium" Indeterminate="true" />
            </MudGrid>
        }
        else
        {
            <MudText Typo="Typo.h5" Class="mt-2 mb-3">Org Health</MudText>
            <InsightsKpiTiles Summaries="@Summaries" />
            <div class="mt-4">
                <InsightsHealthByCategoryChart Summaries="@Summaries" />
            </div>

            <MudDivider Class="my-6" />

            <MudText Typo="Typo.h5" Class="mb-3">Tech Adoption</MudText>
            <InsightsAdoptionCharts Summaries="@Summaries" />

            <MudDivider Class="my-6" />

            <MudText Typo="Typo.h5" Class="mb-3">Needs attention</MudText>
            <InsightsActionList Summaries="@Summaries" />
        }
    </DocsPageContent>
</DocsPage>
```

- [ ] **Step 2: Code-behind**

```csharp
namespace AtcWeb.Pages.Support;

using AtcWeb.Domain.AtcApi;
using AtcWeb.Domain.AtcApi.Models.Compliance;

public class RepositoryInsightsBase : ComponentBase
{
    [Inject] protected AtcApiGitHubRepositoryClient Client { get; set; } = default!;

    protected List<RepositoryComplianceSummary>? Summaries;

    protected override async Task OnInitializedAsync()
    {
        var (isSuccessful, summaries) = await Client.GetComplianceSummary();
        Summaries = isSuccessful ? summaries : [];
        await base.OnInitializedAsync();
    }
}
```

- [ ] **Step 3: Build + commit**

Run: `dotnet build src/AtcWeb/AtcWeb.csproj`
Expected: success.

```powershell
git add src/AtcWeb/Pages/Support/RepositoryInsights.razor src/AtcWeb/Pages/Support/RepositoryInsights.razor.cs
git commit -m "feat(insights): add /support/repository-insights page"
```

---

## Task 6 — Nav menu entry

**Files:**
- Modify: `src/AtcWeb/Shared/MainLayout.razor`

- [ ] **Step 1: Add menu item**

In `MainLayout.razor`, find the existing maintenance menu block:

```razor
<MudText Typo="Typo.body2"
         Class="px-4 py-2"><b>Maintenance</b></MudText>
<MudMenuItem Href="/support/repository-compliance-overview">Compliance overview</MudMenuItem>
<MudMenuItem Href="/support/api-rate-limits">API rate limits</MudMenuItem>
```

Insert a new menu item between "Compliance overview" and "API rate limits":

```razor
<MudMenuItem Href="/support/repository-insights">Repository insights</MudMenuItem>
```

- [ ] **Step 2: Build + commit**

Run: `dotnet build src/AtcWeb/AtcWeb.csproj`
Expected: success.

```powershell
git add src/AtcWeb/Shared/MainLayout.razor
git commit -m "feat(insights): add Repository insights to maintenance menu"
```

---

## Task 7 — Styling

**Files:**
- Modify: `src/AtcWeb/Styles/_modern.scss`

- [ ] **Step 1: Append SCSS**

```scss
.atc-insights-tiles {
    .atc-insights-tile {
        background: var(--mud-palette-surface);
        border: 1px solid var(--mud-palette-lines-default);
        border-radius: 10px;
        padding: 16px 20px;
        min-width: 140px;

        .atc-insights-tile-value {
            font-size: 1.75rem;
            font-weight: 600;
            color: var(--mud-palette-primary);
        }

        .atc-insights-tile-label {
            font-size: 0.75rem;
            color: var(--mud-palette-text-secondary);
            text-transform: uppercase;
            letter-spacing: 0.05em;
        }
    }
}
```

- [ ] **Step 2: Recompile SCSS**

```powershell
sass src/AtcWeb/Styles/AtcWeb.scss src/AtcWeb/wwwroot/css/AtcWeb.css --style=expanded --no-source-map
sass src/AtcWeb/Styles/AtcWeb.scss src/AtcWeb/wwwroot/css/AtcWeb.min.css --style=compressed --no-source-map
```

- [ ] **Step 3: Commit**

```powershell
git add src/AtcWeb/Styles/_modern.scss src/AtcWeb/wwwroot/css/AtcWeb.css src/AtcWeb/wwwroot/css/AtcWeb.min.css
git commit -m "style(insights): add insights tile styling"
```

---

## Task 8 — Manual UI verification

- [ ] **Step 1: Run the app**

Run: `dotnet run --project src/AtcWeb/AtcWeb.csproj`
Open `http://localhost:5XXX/support/repository-insights`.

- [ ] **Step 2: Verify**

Check:
- Page loads (warm cache should be sub-second since Phase 2 already populated `compliance-summary`).
- KPI tiles render with non-zero values.
- Health-by-category stacked bar shows recognizable categories.
- TFM donut, Atc.Analyzer bar, Languages donut all render.
- Action list groups repos by category and lists their failing rules.
- Repos that are entirely healthy appear under no group; empty list shows the success alert.
- All repo names link to `/repository/{name}` and navigate correctly.
- No JS console errors, no failed network calls.

If any check fails, fix and re-run.

- [ ] **Step 3: Stop the app and commit fixes (if any)**

```powershell
git add -A
git commit -m "fix(insights): UI verification fixes"
```

---

## Task 9 — Final test + push

- [ ] **Step 1: Run full test suite**

Run: `dotnet test`
Expected: all green.

- [ ] **Step 2: Push**

```powershell
git push origin feature/compliance-summary
```

Phase 3 complete. Open a PR from `feature/compliance-summary` → `main`. The PR should encompass all three phases of frontend work; Phase 1's atc-api PR is separate and needs to be merged + deployed first.
