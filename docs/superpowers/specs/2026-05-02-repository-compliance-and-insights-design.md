# Repository Compliance Overview Redesign + New Insights Page

**Status:** Draft for approval
**Date:** 2026-05-02
**Owner:** dka@contextand.com

## 1. Goals

Replace the slow, alphabetical, one-fat-section-per-repo `/support/repository-compliance-overview` page with a fast, scannable, filterable hybrid (dashboard table + cards grid), driven by a new pre-aggregated atc-api endpoint that encodes the latest ATC compliance rule set (taken from `D:\Code\ATC-Overview-DotNet.ps1`).

Add a new `/support/repository-insights` page that turns the same aggregated data into org-wide KPIs, tech-adoption charts, and an actionable "needs attention" punch-list.

## 2. Non-goals

- No changes to the existing `/repository/{name}` deep-dive page.
- No introduction of a new UI framework. We stay on MudBlazor + the current `_modern.scss` palette.
- No changes to the rest of the atc-api beyond adding one new endpoint.
- No live GitHub-event subscription or background refresh. Cache-driven only.
- Activity / contributor analytics (originally option C in brainstorming) — explicitly deferred to a possible separate page later.

## 3. Architecture overview

Three deliverables, in this order:

```
┌──────────────────────────────────────────────────────────┐
│ Phase 1  atc-api: new endpoint                           │
│   GET /github/repository/compliance-summary              │
│   - Aggregates per-repo compliance signals server-side   │
│   - 30-min server cache                                  │
│   - Deployed to Azure Container App                      │
└─────────────────────────┬────────────────────────────────┘
                          │
┌─────────────────────────▼────────────────────────────────┐
│ Phase 2  atc-net.github.io: compliance overview redesign │
│   /support/repository-compliance-overview                │
│   - Dashboard table (sortable, filterable)               │
│   - Cards grid below                                     │
│   - Single fast call to the new endpoint                 │
└─────────────────────────┬────────────────────────────────┘
                          │
┌─────────────────────────▼────────────────────────────────┐
│ Phase 3  atc-net.github.io: new insights page            │
│   /support/repository-insights                           │
│   - KPIs + adoption charts + action list                 │
│   - Reuses the same compliance-summary call              │
└──────────────────────────────────────────────────────────┘
```

## 4. Phase 1 — atc-api `compliance-summary` endpoint

### 4.1 Route

`GET /github/repository/compliance-summary`

Returns one `RepositoryComplianceSummary` per non-archived, non-private ATC repo, with the same exclusions that `GetRepositories` already applies (`atc-dummy`, `atc-template-dotnet-package`).

### 4.2 Response schema (added to `Atc.Api.yaml`)

```yaml
RepositoryComplianceSummary:
  type: object
  required: [name, language, signals]
  properties:
    name: { type: string }
    language: { type: string, nullable: true }    # "C#", "Python", ...
    description: { type: string, nullable: true }
    homepage: { type: string, nullable: true }
    licenseKey: { type: string, nullable: true }
    defaultBranch: { type: string, nullable: true }
    topics: { type: array, items: { type: string } }
    stargazersCount: { type: integer, format: int32 }
    forksCount: { type: integer, format: int32 }
    openIssuesCount: { type: integer, format: int32 }
    pushedAt: { type: string, format: date-time, nullable: true }
    updatedAt: { type: string, format: date-time }
    createdAt: { type: string, format: date-time }
    oldestOpenIssueAt: { type: string, format: date-time, nullable: true }
    newestOpenIssueAt: { type: string, format: date-time, nullable: true }
    signals: { $ref: '#/components/schemas/RepositoryComplianceSignals' }
    detail:  { $ref: '#/components/schemas/RepositoryComplianceDetail' }

RepositoryComplianceSignals:
  description: Boolean/enum signals used as dashboard columns.
  type: object
  properties:
    hasGoodReadme: { type: boolean }            # readme.md exists AND > 1500 bytes
    licenseIsMit: { type: boolean }
    homepageIsAtcWeb: { type: boolean }         # matches https://atc-net.github.io/repository/{name}
    editorConfigStatus: { $ref: '#/components/schemas/EditorConfigStatus' }
    updaterPresent: { type: boolean }           # atc-coding-rules-updater.json exists
    updaterTargetIsLatest: { type: boolean }    # projectTarget == "DotNet10"
    updaterProjectTarget: { type: string, nullable: true }
    globalLangVersionIsLatest: { type: boolean } # Directory.Build.props LangVersion == "14.0"
    globalLangVersion: { type: string, nullable: true }
    globalTargetFrameworkIsLatest: { type: boolean } # net10.0
    globalTargetFramework: { type: string, nullable: true }
    xunitV3Status: { type: string, enum: [Yes, No, NotApplicable] }
    workflowsStatus: { $ref: '#/components/schemas/WorkflowsStatus' }
    releasePleasePresent: { type: boolean }     # .github/workflows/release-please.yml

EditorConfigStatus:
  type: object
  description: Latest versions per editorconfig location.
  properties:
    rootPresent: { type: boolean }
    rootIsLatest: { type: boolean }
    rootVersion: { type: string, nullable: true }     # e.g. "1.0.7"
    srcPresent: { type: boolean }
    srcIsLatest: { type: boolean }
    srcVersion: { type: string, nullable: true }
    testPresent: { type: boolean }
    testIsLatest: { type: boolean }
    testVersion: { type: string, nullable: true }

WorkflowsStatus:
  type: object
  properties:
    actions: { type: array, items: { type: string } }       # distinct, e.g. "actions/checkout@v6"
    dotnetVersions: { type: array, items: { type: string } }# e.g. "10.0.x"
    checkoutIsLatest: { type: boolean }   # actions/checkout@v6 used (and no older)
    setupDotnetIsLatest: { type: boolean }# actions/setup-dotnet@v5 used (and no older)
    hasJavaSetup: { type: boolean }       # any actions/setup-java@* — red flag
    dotnetVersionIsLatest: { type: boolean } # any version contains "10"

RepositoryComplianceDetail:
  description: Heavy fields shown only in the expanded row / repo detail.
  type: object
  properties:
    srcFrameworks:    { type: array, items: { type: string } }
    testFrameworks:   { type: array, items: { type: string } }
    sampleFrameworks: { type: array, items: { type: string } }
    analyzerPackages: { type: array, items: { $ref: '#/components/schemas/AnalyzerPackageRef' } }
    suppressedRulesRoot: { type: array, items: { type: string } }
    suppressedRulesSrc:  { type: array, items: { type: string } }
    suppressedRulesTest: { type: array, items: { type: string } }

AnalyzerPackageRef:
  type: object
  required: [packageId, version]
  properties:
    packageId: { type: string }      # "Atc.Analyzer", "Meziantou.Analyzer", "SonarAnalyzer.CSharp"
    version:   { type: string }
    isLatest:  { type: boolean }     # cross-referenced with NuGet latest
```

### 4.3 Server-side aggregation

A new `GetComplianceSummaryHandler.cs` + an `IGitHubComplianceSummaryService` that:

1. Takes the cached repository list (the existing `GetAllRepositoriesHandler` already has caching).
2. For each repo, in parallel (bounded), fetches:
   - `readme.md` (GET, then `content.Length > 1500` — atc-api doesn't expose HEAD)
   - `Directory.Build.props`
   - `test/Directory.Build.props`
   - `atc-coding-rules-updater.json`
   - `.editorconfig` × 3 locations
   - `.github/workflows/*.yml` (path listing then per-file fetch)
   - Open-issues list
3. Parses each (XML for csproj/props, JSON for updater, YAML/regex for workflows, line scan for editorconfig version + suppress rules).
4. Cross-references analyzer package versions against the existing `nuget-packages-used` cache.
5. Caches the full computed list under key `compliance-summary` for **30 minutes** (`IMemoryCache`, mirroring existing TTL knobs in `CacheConstants`).

### 4.4 Latest-version constants (centralized in atc-api)

These constants currently live in `atc-net.github.io` (`CodingRulesMetadata.cs`, `RepositoryMetadata.cs`) and in the PowerShell script. Move the source of truth to atc-api, expose them on a new `/github/repository/compliance-rules` companion endpoint so the frontend can render "compared against rule set X.Y" and "rule X is failing" labels without redeploying both apps when a target bumps.

```yaml
ComplianceRules:
  type: object
  properties:
    editorConfigRootLatest: { type: string }   # "1.0.7"
    editorConfigSrcLatest:  { type: string }   # "1.0.5"
    editorConfigTestLatest: { type: string }   # "1.0.7"
    langVersionLatest:      { type: string }   # "14.0"
    targetFrameworkLatest:  { type: string }   # "net10.0"
    targetFrameworksAccepted: { type: array, items: { type: string } }
    updaterTargetLatest:    { type: string }   # "DotNet10"
    checkoutVersionLatest:  { type: string }   # "actions/checkout@v6"
    setupDotnetVersionLatest:{ type: string }  # "actions/setup-dotnet@v5"
    visualStudioNameLatest: { type: string }   # "Visual Studio 2026"
```

### 4.5 Deployment

Manual deploy of atc-api to its Azure Container App (existing process). The frontend work in Phase 2/3 starts only after this endpoint is live.

## 5. Phase 2 — Compliance Overview Redesign

### 5.1 Page route

`/support/repository-compliance-overview` (unchanged).

### 5.2 Layout

```
┌──────────────────────────────────────────────────────────┐
│ HEADER: Repository compliance overview                   │
│ KPI strip:                                               │
│  [42 repos] [38 C#] [4 Py] [89% MIT] [62% net10.0]       │
│  [54% release-please] [71% editorconfig latest]          │
├──────────────────────────────────────────────────────────┤
│ FILTER BAR (sticky):                                     │
│  [Search] [Lang ▼] [Category ▼] [Health ▼]               │
│  [More filters ▼]   [Group by: None ▼]   [Refresh]       │
├──────────────────────────────────────────────────────────┤
│ DASHBOARD TABLE (sortable, expandable rows)              │
│  Name│Lang│Lic│HP│RM│EditorCfg│Updater│LV│TFM│xUnit│Wf│RP│
│  ▶ atc-rest-api-generator   …                            │
│  ▶ atc-coding-rules         …                            │
│   …                                                      │
├──────────────────────────────────────────────────────────┤
│ CARDS GRID (same filters apply)                          │
│  [card] [card] [card] [card]                             │
└──────────────────────────────────────────────────────────┘
```

### 5.3 Components

New, in `src/AtcWeb/Components/Compliance/`:

- `ComplianceKpiStrip.razor` — top counters.
- `ComplianceFilterBar.razor` — search, dropdowns, group-by.
- `ComplianceDashboardTable.razor` — `MudDataGrid` based, virtualized, with `RowDetail` template.
- `ComplianceDashboardRow.razor` — one row, status chips per column.
- `ComplianceDashboardRowDetail.razor` — expanded detail (frameworks, analyzer packages, suppressed rules, oldest issue, link to `/repository/{name}`).
- `ComplianceStatusChip.razor` — green/amber/red MudChip with tooltip. Reused everywhere.

Modified:

- `RepositoryComplianceOverview.razor` + `.razor.cs` — hosts the new components, calls one new service method.

### 5.4 Frontend data flow

```
RepositoryComplianceOverviewBase.OnInitializedAsync
  └─> GitHubRepositoryService.GetComplianceSummariesAsync()
        └─> AtcApiGitHubRepositoryClient.GetComplianceSummary()
              ├─ memory cache (12h)  ─┐
              ├─ browser cache (30m)  │
              └─ HTTP GET .../compliance-summary
```

Existing per-repo `GetRepositories` / `populateMetaDataAdvanced` paths stay untouched — `/repository/{name}` and other consumers continue to use them.

### 5.5 Filtering, sorting, grouping

- Filtering happens client-side over the in-memory `List<RepositoryComplianceSummary>`. The list is small (<60 repos), no server round-trip needed.
- Filter chips: language, category (computed via existing `RepositoryCategoryHelper`), health (`AnyError`, `AnyWarning`, `AllGreen` derived from `signals`), free-text search on name/description.
- "More filters…" dropdown: each individual signal as a "is failing" toggle (e.g. "Behind on TFM", "Missing release-please", "Old .editorconfig").
- Sort: any column. Default sort is "by health" (worst first) so the page opens on what needs work.
- Group-by dropdown: None / Category / Health / Language. Adds `MudDataGrid` group rows with mini health summary per group.

### 5.6 Status chip semantics (single component, reused)

| Status | Meaning | Mud Color |
|---|---|---|
| OK / latest | green | Success |
| Present but outdated | amber | Warning |
| Missing / wrong | red | Error |
| Not applicable (e.g. xUnit on Python) | grey dash | Default |

Tooltip text comes from a small `ComplianceMessageProvider` that maps `(signalName, value)` → human message — keeps i18n-ready and one place to change wording.

## 6. Phase 3 — Repository Insights Page

### 6.1 Route

`/support/repository-insights` (new). Linked from the Support menu in `MainLayout.razor`.

### 6.2 Layout (three stacked sections)

```
┌──────────────────────────────────────────────────────────┐
│ § Org Health KPIs                                        │
│  Big tiles: total / MIT / net10.0 / release-please / …   │
│  Stacked bar: per-category health breakdown              │
├──────────────────────────────────────────────────────────┤
│ § Tech Adoption                                          │
│  - Atc.Analyzer version distribution (bar)               │
│  - .NET TFM distribution (donut)                         │
│  - Top external NuGet packages (table, top 20)           │
│  - "Behind latest" packages list                         │
├──────────────────────────────────────────────────────────┤
│ § Action List — needs attention                          │
│  Grouped by category. Each repo lists its red signals.   │
│  Click → /repository/{name}                              │
└──────────────────────────────────────────────────────────┘
```

### 6.3 Components

New, in `src/AtcWeb/Components/Insights/`:

- `InsightsKpiTiles.razor`
- `InsightsHealthByCategoryChart.razor`
- `InsightsAdoptionCharts.razor`
- `InsightsActionList.razor`

Charts use **MudBlazor's chart components** (`MudChart` for donut/bar) — no new charting dep.

### 6.4 Data source

Same single `GetComplianceSummary()` call as Phase 2. The page derives all aggregations client-side via LINQ. Reuses the cached list in memory if already loaded by the compliance overview page during the same session (the `IMemoryCache` entry is shared).

## 7. Caching strategy summary

| Layer | Key | TTL | Notes |
|---|---|---|---|
| atc-api `IMemoryCache` | `compliance-summary` | 30 min | Single source of truth |
| Frontend `IMemoryCache` | `compliance-summary` | 12 h | Existing default in `CacheConstants` |
| Frontend `localStorage` | `compliance-summary` | 30 min stale-while-revalidate | Existing `BrowserCacheService` pattern |

Cold load goal: ≤ 2 s for a fully populated dashboard table (vs. current cold load of ~30+ s on the fat per-repo flow).

## 8. Build sequence (delivers value in slices)

1. **atc-api: schemas + handler skeleton** returning empty list — establishes contract.
2. **atc-api: per-signal extractors** added one at a time, each unit-tested against fixtures.
3. **atc-api: deploy to Azure Container App.**
4. **frontend: new client method + DTOs + cached call.**
5. **frontend: `ComplianceStatusChip` + `ComplianceDashboardTable` minimal (read-only, no filters).** Replaces the current page body but keeps the existing route.
6. **frontend: filters, group-by, KPI strip, expanded row detail.**
7. **frontend: cards grid below table (reusing `repo-card` styles from `_modern.scss`).**
8. **frontend: insights page** built on top of the same client method.
9. **frontend: nav menu entry** for insights under Support.

Each step is independently merge-able and ships value.

## 9. Migration / removal

- The current `RepositoryComplianceOverviewBase.OnInitializedAsync` call to `GetRepositoriesAsync(populateMetaDataBase: true, populateMetaDataAdvanced: true)` is removed from this page. That heavy code path is still used by `/repository/{name}` and stays.
- The `RepositorySummery`, `RepositoryDotNetSolution`, `RepositoryDotNetProjects`, `RepositoryCodingRules` components are no longer referenced by the compliance page after redesign, but stay for the per-repo deep-dive page.
- The Python "TODO: Data rendering is not implemented yet." block goes away — Python repos render their applicable signals only (license/homepage/readme/issues) and `–` for .NET-only signals.

## 10. Testing

- atc-api: extractor unit tests with fixture files (`.editorconfig` samples at v1.0.5/1.0.7, `Directory.Build.props` snapshots, `release-please.yml`, workflows with mixed action versions).
- frontend: component tests with hand-rolled `RepositoryComplianceSummary` fixtures driving the dashboard table, filter logic, and KPI math.
- Integration: keep the existing test/AtcWeb.Domain.Tests pattern (real HTTP calls to atc-api dev) but add a smoke test for `GetComplianceSummary()`.

## 11. Risks

- **GitHub rate limits when atc-api computes signals for ~50 repos every 30 min.** Mitigation: stagger fetches behind the existing GitHub HTTP client (already token-authenticated and cached at file level), and reuse already-cached file responses where possible.
- **Server-side parsers drift from the PowerShell rule set.** Mitigation: extract latest-version constants to a single static class in atc-api referenced by both the handler and the new `compliance-rules` endpoint, so the rules are visible and versioned in one place.
- **MudDataGrid virtualization with grouping has historical edge cases.** Mitigation: prototype grouping early; fall back to a flat `MudDataGrid` if grouping breaks virtualization.

## 12. Open questions

None blocking. The atc-api endpoint is the long pole; everything else is layered on top of one cached HTTP call.
