# Phase 1 — atc-api compliance-summary endpoint

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add `GET /github/repository/compliance-summary` to `atc-api` that returns one pre-aggregated `RepositoryComplianceSummary` per non-archived ATC repo, encoding all 12 dashboard signals + detail fields described in the design spec.

**Architecture:** Follow the existing Handlers + Services pattern. Add one schema set in `Atc.Api.yaml`, a new service interface method, signal-extractor helpers (one per file format: editorconfig, props/csproj, yaml workflows, json updater), one handler, and `IMemoryCache` with 30-min TTL. All extractors are pure functions that take raw text → typed result, so unit tests use string fixtures with no GitHub.

**Tech Stack:** ASP.NET Core minimal APIs, Microsoft.Extensions.Caching.Memory, Octokit, xUnit, FluentAssertions. The repo lives at `D:\Code\atc-net\atc-api`. All paths in this plan are relative to that repo unless explicitly noted.

**Working repo:** `D:\Code\atc-net\atc-api`
**Spec:** `docs/superpowers/specs/2026-05-02-repository-compliance-and-insights-design.md` (in `D:\Code\atc-net\atc-net.github.io`)

**Branch setup (run before Task 1):**

```powershell
cd D:/Code/atc-net/atc-api
git checkout -b feature/compliance-summary
```

All commits in this plan land on that branch. Do not commit on `main`.

---

## File structure

**Create:**
- `src/Atc.Api/Constants/ComplianceConstants.cs` — single source of truth for "latest" versions
- `src/Atc.Api/Models/Compliance/RepositoryComplianceSummary.cs` — top-level DTO
- `src/Atc.Api/Models/Compliance/RepositoryComplianceSignals.cs`
- `src/Atc.Api/Models/Compliance/EditorConfigStatus.cs`
- `src/Atc.Api/Models/Compliance/WorkflowsStatus.cs`
- `src/Atc.Api/Models/Compliance/RepositoryComplianceDetail.cs`
- `src/Atc.Api/Models/Compliance/AnalyzerPackageRef.cs`
- `src/Atc.Api/Models/Compliance/XunitV3Status.cs` (enum)
- `src/Atc.Api/Helpers/Compliance/EditorConfigParser.cs`
- `src/Atc.Api/Helpers/Compliance/DirectoryBuildPropsParser.cs`
- `src/Atc.Api/Helpers/Compliance/WorkflowsParser.cs`
- `src/Atc.Api/Helpers/Compliance/UpdaterJsonParser.cs`
- `src/Atc.Api/Services/IGitHubComplianceService.cs`
- `src/Atc.Api/Services/GitHubComplianceService.cs`
- `src/Atc.Api/Services/GitHubComplianceService.Log.cs`
- `src/Atc.Api/Handlers/GetComplianceSummaryHandler.cs`
- `test/Atc.Api.Tests/Helpers/Compliance/EditorConfigParserTests.cs`
- `test/Atc.Api.Tests/Helpers/Compliance/DirectoryBuildPropsParserTests.cs`
- `test/Atc.Api.Tests/Helpers/Compliance/WorkflowsParserTests.cs`
- `test/Atc.Api.Tests/Helpers/Compliance/UpdaterJsonParserTests.cs`
- `test/Atc.Api.Tests/Integration/ComplianceEndpointTests.cs`

**Modify:**
- `src/Atc.Api/Atc.Api.yaml` — add 7 schemas + 1 path
- `src/Atc.Api/CacheConstants.cs` — add `CacheKeyComplianceSummary` + `ComplianceCacheTtl`

---

## Task 1 — Latest-version constants

**Files:**
- Create: `src/Atc.Api/Constants/ComplianceConstants.cs`

- [ ] **Step 1: Create the constants class**

```csharp
namespace Atc.Api.Constants;

public static class ComplianceConstants
{
    public const string EditorConfigRootLatest = "1.0.7";
    public const string EditorConfigSrcLatest = "1.0.5";
    public const string EditorConfigTestLatest = "1.0.7";

    public const string LangVersionLatest = "14.0";
    public const string TargetFrameworkLatest = "net10.0";

    public static readonly IReadOnlyList<string> TargetFrameworksAccepted =
    [
        "net8.0",
        "net10.0",
        "netstandard2.0",
        "netstandard2.1",
    ];

    public const string UpdaterTargetLatest = "DotNet10";
    public const string CheckoutVersionLatest = "actions/checkout@v6";
    public const string SetupDotnetVersionLatest = "actions/setup-dotnet@v5";
    public const string VisualStudioNameLatest = "Visual Studio 2026";

    public const int ReadmeMinSizeBytes = 1500;
}
```

- [ ] **Step 2: Build to confirm**

Run: `dotnet build D:/Code/atc-net/atc-api/Atc.Api.slnx`
Expected: success.

- [ ] **Step 3: Commit**

```powershell
cd D:/Code/atc-net/atc-api
git add src/Atc.Api/Constants/ComplianceConstants.cs
git commit -m "feat(compliance): add ComplianceConstants with latest-rule constants"
```

---

## Task 2 — DTO models

**Files:**
- Create: `src/Atc.Api/Models/Compliance/RepositoryComplianceSummary.cs`
- Create: `src/Atc.Api/Models/Compliance/RepositoryComplianceSignals.cs`
- Create: `src/Atc.Api/Models/Compliance/EditorConfigStatus.cs`
- Create: `src/Atc.Api/Models/Compliance/WorkflowsStatus.cs`
- Create: `src/Atc.Api/Models/Compliance/RepositoryComplianceDetail.cs`
- Create: `src/Atc.Api/Models/Compliance/AnalyzerPackageRef.cs`
- Create: `src/Atc.Api/Models/Compliance/XunitV3Status.cs`

- [ ] **Step 1: Create the enum**

`src/Atc.Api/Models/Compliance/XunitV3Status.cs`:

```csharp
namespace Atc.Api.Models.Compliance;

public enum XunitV3Status
{
    Yes,
    No,
    NotApplicable,
}
```

- [ ] **Step 2: Create EditorConfigStatus**

`src/Atc.Api/Models/Compliance/EditorConfigStatus.cs`:

```csharp
namespace Atc.Api.Models.Compliance;

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

- [ ] **Step 3: Create WorkflowsStatus**

`src/Atc.Api/Models/Compliance/WorkflowsStatus.cs`:

```csharp
namespace Atc.Api.Models.Compliance;

public sealed class WorkflowsStatus
{
    public IReadOnlyList<string> Actions { get; init; } = [];
    public IReadOnlyList<string> DotnetVersions { get; init; } = [];
    public bool CheckoutIsLatest { get; init; }
    public bool SetupDotnetIsLatest { get; init; }
    public bool HasJavaSetup { get; init; }
    public bool DotnetVersionIsLatest { get; init; }
}
```

- [ ] **Step 4: Create AnalyzerPackageRef**

`src/Atc.Api/Models/Compliance/AnalyzerPackageRef.cs`:

```csharp
namespace Atc.Api.Models.Compliance;

public sealed class AnalyzerPackageRef
{
    public required string PackageId { get; init; }
    public required string Version { get; init; }
    public bool IsLatest { get; init; }
}
```

- [ ] **Step 5: Create RepositoryComplianceDetail**

`src/Atc.Api/Models/Compliance/RepositoryComplianceDetail.cs`:

```csharp
namespace Atc.Api.Models.Compliance;

public sealed class RepositoryComplianceDetail
{
    public IReadOnlyList<string> SrcFrameworks { get; init; } = [];
    public IReadOnlyList<string> TestFrameworks { get; init; } = [];
    public IReadOnlyList<string> SampleFrameworks { get; init; } = [];
    public IReadOnlyList<AnalyzerPackageRef> AnalyzerPackages { get; init; } = [];
    public IReadOnlyList<string> SuppressedRulesRoot { get; init; } = [];
    public IReadOnlyList<string> SuppressedRulesSrc { get; init; } = [];
    public IReadOnlyList<string> SuppressedRulesTest { get; init; } = [];
}
```

- [ ] **Step 6: Create RepositoryComplianceSignals**

`src/Atc.Api/Models/Compliance/RepositoryComplianceSignals.cs`:

```csharp
namespace Atc.Api.Models.Compliance;

public sealed class RepositoryComplianceSignals
{
    public bool HasGoodReadme { get; init; }
    public bool LicenseIsMit { get; init; }
    public bool HomepageIsAtcWeb { get; init; }

    public required EditorConfigStatus EditorConfigStatus { get; init; }

    public bool UpdaterPresent { get; init; }
    public bool UpdaterTargetIsLatest { get; init; }
    public string? UpdaterProjectTarget { get; init; }

    public bool GlobalLangVersionIsLatest { get; init; }
    public string? GlobalLangVersion { get; init; }

    public bool GlobalTargetFrameworkIsLatest { get; init; }
    public string? GlobalTargetFramework { get; init; }

    public XunitV3Status XunitV3Status { get; init; } = XunitV3Status.NotApplicable;

    public required WorkflowsStatus WorkflowsStatus { get; init; }

    public bool ReleasePleasePresent { get; init; }
}
```

- [ ] **Step 7: Create RepositoryComplianceSummary**

`src/Atc.Api/Models/Compliance/RepositoryComplianceSummary.cs`:

```csharp
namespace Atc.Api.Models.Compliance;

public sealed class RepositoryComplianceSummary
{
    public required string Name { get; init; }
    public string? Language { get; init; }
    public string? Description { get; init; }
    public string? Homepage { get; init; }
    public string? LicenseKey { get; init; }
    public string? DefaultBranch { get; init; }
    public IReadOnlyList<string> Topics { get; init; } = [];
    public int StargazersCount { get; init; }
    public int ForksCount { get; init; }
    public int OpenIssuesCount { get; init; }
    public DateTimeOffset? PushedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? OldestOpenIssueAt { get; init; }
    public DateTimeOffset? NewestOpenIssueAt { get; init; }
    public required RepositoryComplianceSignals Signals { get; init; }
    public required RepositoryComplianceDetail Detail { get; init; }
}
```

- [ ] **Step 8: Build to confirm**

Run: `dotnet build D:/Code/atc-net/atc-api/Atc.Api.slnx`
Expected: success.

- [ ] **Step 9: Commit**

```powershell
git add src/Atc.Api/Models/Compliance/
git commit -m "feat(compliance): add RepositoryComplianceSummary DTO models"
```

---

## Task 3 — UpdaterJsonParser (TDD)

**Files:**
- Create: `src/Atc.Api/Helpers/Compliance/UpdaterJsonParser.cs`
- Test: `test/Atc.Api.Tests/Helpers/Compliance/UpdaterJsonParserTests.cs`

- [ ] **Step 1: Write the failing tests**

`test/Atc.Api.Tests/Helpers/Compliance/UpdaterJsonParserTests.cs`:

```csharp
namespace Atc.Api.Tests.Helpers.Compliance;

using Atc.Api.Helpers.Compliance;
using FluentAssertions;
using Xunit;

public class UpdaterJsonParserTests
{
    [Fact]
    public void Parse_ReturnsNull_WhenInputIsEmpty()
    {
        UpdaterJsonParser.Parse(string.Empty).Should().BeNull();
    }

    [Fact]
    public void Parse_ReturnsNull_WhenInputIsInvalidJson()
    {
        UpdaterJsonParser.Parse("{ not json").Should().BeNull();
    }

    [Fact]
    public void Parse_ExtractsProjectTarget_WhenPresent()
    {
        var raw = """{ "projectTarget": "DotNet10" }""";
        UpdaterJsonParser.Parse(raw).Should().Be("DotNet10");
    }

    [Fact]
    public void Parse_ReturnsNull_WhenProjectTargetMissing()
    {
        var raw = """{ "other": "value" }""";
        UpdaterJsonParser.Parse(raw).Should().BeNull();
    }
}
```

- [ ] **Step 2: Run tests to confirm they fail**

Run: `dotnet test D:/Code/atc-net/atc-api/Atc.Api.slnx --filter FullyQualifiedName~UpdaterJsonParserTests`
Expected: compile error (`UpdaterJsonParser` not found).

- [ ] **Step 3: Implement the parser**

`src/Atc.Api/Helpers/Compliance/UpdaterJsonParser.cs`:

```csharp
namespace Atc.Api.Helpers.Compliance;

using System.Text.Json;

public static class UpdaterJsonParser
{
    public static string? Parse(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(raw);
            return doc.RootElement.TryGetProperty("projectTarget", out var prop) &&
                   prop.ValueKind == JsonValueKind.String
                ? prop.GetString()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
```

- [ ] **Step 4: Run tests to confirm they pass**

Run: `dotnet test D:/Code/atc-net/atc-api/Atc.Api.slnx --filter FullyQualifiedName~UpdaterJsonParserTests`
Expected: 4 passed.

- [ ] **Step 5: Commit**

```powershell
git add src/Atc.Api/Helpers/Compliance/UpdaterJsonParser.cs test/Atc.Api.Tests/Helpers/Compliance/UpdaterJsonParserTests.cs
git commit -m "feat(compliance): add UpdaterJsonParser with tests"
```

---

## Task 4 — EditorConfigParser (TDD)

**Files:**
- Create: `src/Atc.Api/Helpers/Compliance/EditorConfigParser.cs`
- Test: `test/Atc.Api.Tests/Helpers/Compliance/EditorConfigParserTests.cs`

The parser extracts the `# Version: X.Y.Z` line and detects `# Custom - Code Analyzers Rules` suppress rules — same algorithm as `CodingRulesMetadata` in atc-net.github.io.

- [ ] **Step 1: Write the failing tests**

`test/Atc.Api.Tests/Helpers/Compliance/EditorConfigParserTests.cs`:

```csharp
namespace Atc.Api.Tests.Helpers.Compliance;

using Atc.Api.Helpers.Compliance;
using FluentAssertions;
using Xunit;

public class EditorConfigParserTests
{
    [Fact]
    public void GetVersion_ReturnsNull_WhenInputIsEmpty()
    {
        EditorConfigParser.GetVersion(string.Empty).Should().BeNull();
    }

    [Fact]
    public void GetVersion_ExtractsVersionFromComment()
    {
        var raw = "root = true\n# Version: 1.0.7\n";
        EditorConfigParser.GetVersion(raw).Should().Be("1.0.7");
    }

    [Fact]
    public void GetVersion_ReturnsNull_WhenNoVersionLine()
    {
        var raw = "root = true\n[*]\nindent_style = space\n";
        EditorConfigParser.GetVersion(raw).Should().BeNull();
    }

    [Fact]
    public void GetSuppressedRules_ReturnsEmpty_WhenNoCustomSection()
    {
        var raw = "root = true\n# Version: 1.0.7\n[*.cs]\n";
        EditorConfigParser.GetSuppressedRules(raw).Should().BeEmpty();
    }

    [Fact]
    public void GetSuppressedRules_ExtractsRulesFromCustomSection()
    {
        var raw =
            "root = true\n" +
            "# Version: 1.0.7\n" +
            "[*.cs]\n" +
            "# Custom - Code Analyzers Rules\n" +
            "dotnet_diagnostic.CA1014.severity = none # CLS-compliant\n" +
            "dotnet_diagnostic.SA1633.severity = none\n";
        var result = EditorConfigParser.GetSuppressedRules(raw);
        result.Should().BeEquivalentTo(new[] { "CA1014", "SA1633" });
    }

    [Fact]
    public void IsAtLeast_ReturnsTrue_WhenVersionGreaterOrEqual()
    {
        EditorConfigParser.IsAtLeast("1.0.7", "1.0.7").Should().BeTrue();
        EditorConfigParser.IsAtLeast("1.1.0", "1.0.7").Should().BeTrue();
        EditorConfigParser.IsAtLeast("1.0.5", "1.0.7").Should().BeFalse();
        EditorConfigParser.IsAtLeast(null, "1.0.7").Should().BeFalse();
    }
}
```

- [ ] **Step 2: Run tests to confirm they fail**

Run: `dotnet test D:/Code/atc-net/atc-api/Atc.Api.slnx --filter FullyQualifiedName~EditorConfigParserTests`
Expected: compile error.

- [ ] **Step 3: Implement the parser**

`src/Atc.Api/Helpers/Compliance/EditorConfigParser.cs`:

```csharp
namespace Atc.Api.Helpers.Compliance;

public static class EditorConfigParser
{
    private const string VersionPrefix = "# Version: ";
    private const string CustomSectionMarker = "# Custom - Code Analyzers Rules";
    private const string DiagnosticPrefix = "dotnet_diagnostic.";

    public static string? GetVersion(string raw)
    {
        if (string.IsNullOrEmpty(raw))
        {
            return null;
        }

        foreach (var line in raw.Split('\n'))
        {
            if (line.StartsWith(VersionPrefix, StringComparison.Ordinal))
            {
                return line[VersionPrefix.Length..].Trim();
            }
        }

        return null;
    }

    public static IReadOnlyList<string> GetSuppressedRules(string raw)
    {
        if (string.IsNullOrEmpty(raw))
        {
            return [];
        }

        var result = new List<string>();
        var inCustomSection = false;

        foreach (var rawLine in raw.Split('\n'))
        {
            var line = rawLine.TrimEnd('\r');
            if (line.StartsWith(CustomSectionMarker, StringComparison.Ordinal))
            {
                inCustomSection = true;
                continue;
            }

            if (!inCustomSection)
            {
                continue;
            }

            if (!line.StartsWith(DiagnosticPrefix, StringComparison.Ordinal))
            {
                continue;
            }

            var afterPrefix = line[DiagnosticPrefix.Length..];
            var dotIdx = afterPrefix.IndexOf('.', StringComparison.Ordinal);
            if (dotIdx > 0)
            {
                result.Add(afterPrefix[..dotIdx]);
            }
        }

        return result;
    }

    public static bool IsAtLeast(string? actual, string expected)
    {
        if (string.IsNullOrWhiteSpace(actual))
        {
            return false;
        }

        return Version.TryParse(actual, out var actualVersion) &&
               Version.TryParse(expected, out var expectedVersion) &&
               actualVersion >= expectedVersion;
    }
}
```

- [ ] **Step 4: Run tests to confirm they pass**

Run: `dotnet test D:/Code/atc-net/atc-api/Atc.Api.slnx --filter FullyQualifiedName~EditorConfigParserTests`
Expected: 6 passed.

- [ ] **Step 5: Commit**

```powershell
git add src/Atc.Api/Helpers/Compliance/EditorConfigParser.cs test/Atc.Api.Tests/Helpers/Compliance/EditorConfigParserTests.cs
git commit -m "feat(compliance): add EditorConfigParser with version + suppress-rule extraction"
```

---

## Task 5 — DirectoryBuildPropsParser (TDD)

**Files:**
- Create: `src/Atc.Api/Helpers/Compliance/DirectoryBuildPropsParser.cs`
- Test: `test/Atc.Api.Tests/Helpers/Compliance/DirectoryBuildPropsParserTests.cs`

Parses MSBuild XML for `LangVersion`, `TargetFramework(s)`, and selected `PackageReference` versions. Handles both with and without the MSBuild xmlns. Mirrors the PowerShell logic.

- [ ] **Step 1: Write the failing tests**

`test/Atc.Api.Tests/Helpers/Compliance/DirectoryBuildPropsParserTests.cs`:

```csharp
namespace Atc.Api.Tests.Helpers.Compliance;

using Atc.Api.Helpers.Compliance;
using FluentAssertions;
using Xunit;

public class DirectoryBuildPropsParserTests
{
    [Fact]
    public void Parse_ReturnsEmpty_WhenInputIsEmpty()
    {
        var result = DirectoryBuildPropsParser.Parse(string.Empty);
        result.LangVersion.Should().BeNull();
        result.TargetFramework.Should().BeNull();
        result.PackageVersions.Should().BeEmpty();
    }

    [Fact]
    public void Parse_ReturnsEmpty_WhenInputIsInvalidXml()
    {
        var result = DirectoryBuildPropsParser.Parse("<broken");
        result.LangVersion.Should().BeNull();
    }

    [Fact]
    public void Parse_ExtractsLangVersion_WithoutNamespace()
    {
        var raw = "<Project><PropertyGroup><LangVersion>14.0</LangVersion></PropertyGroup></Project>";
        DirectoryBuildPropsParser.Parse(raw).LangVersion.Should().Be("14.0");
    }

    [Fact]
    public void Parse_ExtractsTargetFramework()
    {
        var raw = "<Project><PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>";
        DirectoryBuildPropsParser.Parse(raw).TargetFramework.Should().Be("net10.0");
    }

    [Fact]
    public void Parse_ExtractsTargetFrameworks_WhenPlural()
    {
        var raw = "<Project><PropertyGroup><TargetFrameworks>net10.0;netstandard2.1</TargetFrameworks></PropertyGroup></Project>";
        DirectoryBuildPropsParser.Parse(raw).TargetFramework.Should().Be("net10.0;netstandard2.1");
    }

    [Fact]
    public void Parse_ExtractsPackageVersions_IncludingTrailingSpaceVariant()
    {
        var raw = """
            <Project>
              <ItemGroup>
                <PackageReference Include="Atc.Analyzer" Version="2.0.520" />
                <PackageReference Include="Meziantou.Analyzer" Version="2.0.205" />
                <PackageReference Include="SonarAnalyzer.CSharp" Version="10.18.0.124163" />
              </ItemGroup>
            </Project>
            """;
        var result = DirectoryBuildPropsParser.Parse(raw);
        result.PackageVersions.Should().Contain(new KeyValuePair<string, string>("Atc.Analyzer", "2.0.520"));
        result.PackageVersions.Should().Contain(new KeyValuePair<string, string>("Meziantou.Analyzer", "2.0.205"));
        result.PackageVersions.Should().Contain(new KeyValuePair<string, string>("SonarAnalyzer.CSharp", "10.18.0.124163"));
    }

    [Fact]
    public void DetectXunitV3_ReturnsYes_WhenXunitV3PackageReferenced()
    {
        var raw = "<Project><ItemGroup><PackageReference Include=\"xunit.v3\" Version=\"1.0.0\" /></ItemGroup></Project>";
        DirectoryBuildPropsParser.DetectXunitV3(raw).Should().Be("Yes");
    }

    [Fact]
    public void DetectXunitV3_ReturnsNo_WhenXunit2PackageReferenced()
    {
        var raw = "<Project><ItemGroup><PackageReference Include=\"xunit\" Version=\"2.9.0\" /></ItemGroup></Project>";
        DirectoryBuildPropsParser.DetectXunitV3(raw).Should().Be("No");
    }

    [Fact]
    public void DetectXunitV3_ReturnsNotApplicable_WhenNoXunitReference()
    {
        var raw = "<Project><ItemGroup><PackageReference Include=\"FluentAssertions\" Version=\"6.0.0\" /></ItemGroup></Project>";
        DirectoryBuildPropsParser.DetectXunitV3(raw).Should().Be("NotApplicable");
    }
}
```

- [ ] **Step 2: Run tests to confirm they fail**

Run: `dotnet test D:/Code/atc-net/atc-api/Atc.Api.slnx --filter FullyQualifiedName~DirectoryBuildPropsParserTests`
Expected: compile error.

- [ ] **Step 3: Implement the parser**

`src/Atc.Api/Helpers/Compliance/DirectoryBuildPropsParser.cs`:

```csharp
namespace Atc.Api.Helpers.Compliance;

using System.Xml.Linq;

public static class DirectoryBuildPropsParser
{
    public sealed record Result(
        string? LangVersion,
        string? TargetFramework,
        IReadOnlyDictionary<string, string> PackageVersions);

    public static Result Parse(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return new Result(null, null, new Dictionary<string, string>(StringComparer.Ordinal));
        }

        XDocument doc;
        try
        {
            doc = XDocument.Parse(raw);
        }
        catch (System.Xml.XmlException)
        {
            return new Result(null, null, new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var lang = FindFirstElementValue(doc, "LangVersion");
        var tf = FindFirstElementValue(doc, "TargetFramework")
              ?? FindFirstElementValue(doc, "TargetFrameworks");

        var pkgs = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pkgRef in doc.Descendants().Where(e => e.Name.LocalName == "PackageReference"))
        {
            var include = pkgRef.Attribute("Include")?.Value?.Trim();
            var version = pkgRef.Attribute("Version")?.Value?.Trim();
            if (!string.IsNullOrEmpty(include) && !string.IsNullOrEmpty(version))
            {
                pkgs[include] = version;
            }
        }

        return new Result(lang, tf, pkgs);
    }

    public static string DetectXunitV3(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return "NotApplicable";
        }

        XDocument doc;
        try
        {
            doc = XDocument.Parse(raw);
        }
        catch (System.Xml.XmlException)
        {
            return "NotApplicable";
        }

        var includes = doc.Descendants()
            .Where(e => e.Name.LocalName is "PackageReference" or "Using")
            .Select(e => e.Attribute("Include")?.Value?.Trim() ?? string.Empty)
            .ToList();

        if (includes.Any(i => i.Contains("xunit.v3", StringComparison.OrdinalIgnoreCase) ||
                              i.Contains("AutoFixture.Xunit3", StringComparison.OrdinalIgnoreCase)))
        {
            return "Yes";
        }

        if (includes.Any(i => i.Contains("xunit", StringComparison.OrdinalIgnoreCase)))
        {
            return "No";
        }

        return "NotApplicable";
    }

    private static string? FindFirstElementValue(XDocument doc, string localName)
        => doc.Descendants()
            .Where(e => e.Name.LocalName == localName)
            .Select(e => e.Value?.Trim())
            .FirstOrDefault(v => !string.IsNullOrEmpty(v));
}
```

- [ ] **Step 4: Run tests to confirm they pass**

Run: `dotnet test D:/Code/atc-net/atc-api/Atc.Api.slnx --filter FullyQualifiedName~DirectoryBuildPropsParserTests`
Expected: 9 passed.

- [ ] **Step 5: Commit**

```powershell
git add src/Atc.Api/Helpers/Compliance/DirectoryBuildPropsParser.cs test/Atc.Api.Tests/Helpers/Compliance/DirectoryBuildPropsParserTests.cs
git commit -m "feat(compliance): add DirectoryBuildPropsParser with xUnit v3 detection"
```

---

## Task 6 — WorkflowsParser (TDD)

**Files:**
- Create: `src/Atc.Api/Helpers/Compliance/WorkflowsParser.cs`
- Test: `test/Atc.Api.Tests/Helpers/Compliance/WorkflowsParserTests.cs`

Regex-based scan of one or more YAML workflow strings, mirroring the PowerShell logic.

- [ ] **Step 1: Write the failing tests**

`test/Atc.Api.Tests/Helpers/Compliance/WorkflowsParserTests.cs`:

```csharp
namespace Atc.Api.Tests.Helpers.Compliance;

using Atc.Api.Helpers.Compliance;
using FluentAssertions;
using Xunit;

public class WorkflowsParserTests
{
    [Fact]
    public void Parse_ReturnsEmpty_WhenNoWorkflows()
    {
        var result = WorkflowsParser.Parse(Array.Empty<string>());
        result.Actions.Should().BeEmpty();
        result.DotnetVersions.Should().BeEmpty();
    }

    [Fact]
    public void Parse_ExtractsDistinctActions()
    {
        var w1 = "jobs:\n  build:\n    steps:\n      - uses: actions/checkout@v6\n      - uses: actions/setup-dotnet@v5\n";
        var w2 = "jobs:\n  publish:\n    steps:\n      - uses: actions/checkout@v6\n";
        var result = WorkflowsParser.Parse(new[] { w1, w2 });
        result.Actions.Should().BeEquivalentTo(new[] { "actions/checkout@v6", "actions/setup-dotnet@v5" });
    }

    [Fact]
    public void Parse_ExtractsDotnetVersions_FromSingleLineQuoted()
    {
        var w = "      - uses: actions/setup-dotnet@v5\n        with:\n          dotnet-version: '10.0.x'\n";
        var result = WorkflowsParser.Parse(new[] { w });
        result.DotnetVersions.Should().BeEquivalentTo(new[] { "10.0.x" });
    }

    [Fact]
    public void Parse_ExtractsDotnetVersions_FromMultilinePipe()
    {
        var w =
            "      - uses: actions/setup-dotnet@v5\n" +
            "        with:\n" +
            "          dotnet-version: |\n" +
            "            8.0.x\n" +
            "            10.0.x\n";
        var result = WorkflowsParser.Parse(new[] { w });
        result.DotnetVersions.Should().Contain("10.0.x");
        result.DotnetVersions.Should().Contain("8.0.x");
    }

    [Fact]
    public void HasJavaSetup_ReturnsTrue_WhenSetupJavaPresent()
    {
        var w = "      - uses: actions/setup-java@v3\n";
        var result = WorkflowsParser.Parse(new[] { w });
        result.HasJavaSetup.Should().BeTrue();
    }

    [Fact]
    public void CheckoutIsLatest_ReturnsTrue_WhenOnlyV6Used()
    {
        var w = "      - uses: actions/checkout@v6\n";
        var result = WorkflowsParser.Parse(new[] { w });
        result.CheckoutIsLatest.Should().BeTrue();
    }

    [Fact]
    public void CheckoutIsLatest_ReturnsFalse_WhenOlderUsed()
    {
        var w = "      - uses: actions/checkout@v4\n";
        var result = WorkflowsParser.Parse(new[] { w });
        result.CheckoutIsLatest.Should().BeFalse();
    }

    [Fact]
    public void DotnetVersionIsLatest_ReturnsTrue_WhenAnyContains10()
    {
        var w = "          dotnet-version: '10.0.x'\n";
        var result = WorkflowsParser.Parse(new[] { w });
        result.DotnetVersionIsLatest.Should().BeTrue();
    }
}
```

- [ ] **Step 2: Run tests to confirm they fail**

Run: `dotnet test D:/Code/atc-net/atc-api/Atc.Api.slnx --filter FullyQualifiedName~WorkflowsParserTests`
Expected: compile error.

- [ ] **Step 3: Implement the parser**

`src/Atc.Api/Helpers/Compliance/WorkflowsParser.cs`:

```csharp
namespace Atc.Api.Helpers.Compliance;

using System.Text.RegularExpressions;
using Atc.Api.Constants;
using Atc.Api.Models.Compliance;

public static partial class WorkflowsParser
{
    [GeneratedRegex(@"uses:\s+(actions/[^\s@]+@[^\s]+)", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex UsesRegex();

    [GeneratedRegex(@"dotnet-version:\s*['""]?([^'""\r\n]+)['""]?", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex DotnetVersionInlineRegex();

    [GeneratedRegex(@"dotnet-version:\s*\|\s*\n((?:\s+[^\n]+\n?)+)", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex DotnetVersionPipeRegex();

    public static WorkflowsStatus Parse(IReadOnlyList<string> workflowYamlFiles)
    {
        if (workflowYamlFiles.Count == 0)
        {
            return new WorkflowsStatus
            {
                Actions = [],
                DotnetVersions = [],
            };
        }

        var actions = new HashSet<string>(StringComparer.Ordinal);
        var versions = new HashSet<string>(StringComparer.Ordinal);

        foreach (var content in workflowYamlFiles)
        {
            if (string.IsNullOrEmpty(content))
            {
                continue;
            }

            foreach (Match match in UsesRegex().Matches(content))
            {
                actions.Add(match.Groups[1].Value.Trim());
            }

            if (!content.Contains("actions/setup-dotnet@", StringComparison.Ordinal))
            {
                continue;
            }

            foreach (Match match in DotnetVersionInlineRegex().Matches(content))
            {
                var v = match.Groups[1].Value.Trim();
                if (!string.IsNullOrEmpty(v) && v != "|")
                {
                    versions.Add(v);
                }
            }

            foreach (Match match in DotnetVersionPipeRegex().Matches(content))
            {
                foreach (var line in match.Groups[1].Value.Split('\n'))
                {
                    var v = line.Trim();
                    if (!string.IsNullOrEmpty(v))
                    {
                        versions.Add(v);
                    }
                }
            }
        }

        var actionsList = actions.OrderBy(a => a, StringComparer.Ordinal).ToList();
        var versionsList = versions.OrderBy(v => v, StringComparer.Ordinal).ToList();

        return new WorkflowsStatus
        {
            Actions = actionsList,
            DotnetVersions = versionsList,
            CheckoutIsLatest = actionsList.Any(a => a.StartsWith("actions/checkout@", StringComparison.Ordinal)) &&
                               actionsList.Where(a => a.StartsWith("actions/checkout@", StringComparison.Ordinal))
                                          .All(a => string.Equals(a, ComplianceConstants.CheckoutVersionLatest, StringComparison.Ordinal)),
            SetupDotnetIsLatest = actionsList.Any(a => a.StartsWith("actions/setup-dotnet@", StringComparison.Ordinal)) &&
                                  actionsList.Where(a => a.StartsWith("actions/setup-dotnet@", StringComparison.Ordinal))
                                             .All(a => string.Equals(a, ComplianceConstants.SetupDotnetVersionLatest, StringComparison.Ordinal)),
            HasJavaSetup = actionsList.Any(a => a.StartsWith("actions/setup-java@", StringComparison.Ordinal)),
            DotnetVersionIsLatest = versionsList.Any(v => v.Contains("10", StringComparison.Ordinal)),
        };
    }
}
```

- [ ] **Step 4: Run tests to confirm they pass**

Run: `dotnet test D:/Code/atc-net/atc-api/Atc.Api.slnx --filter FullyQualifiedName~WorkflowsParserTests`
Expected: 8 passed.

- [ ] **Step 5: Commit**

```powershell
git add src/Atc.Api/Helpers/Compliance/WorkflowsParser.cs test/Atc.Api.Tests/Helpers/Compliance/WorkflowsParserTests.cs
git commit -m "feat(compliance): add WorkflowsParser with action and dotnet-version extraction"
```

---

## Task 7 — CacheConstants update

**Files:**
- Modify: `src/Atc.Api/CacheConstants.cs`

- [ ] **Step 1: Read existing file**

Run: open `src/Atc.Api/CacheConstants.cs` and read its current shape.

- [ ] **Step 2: Add new constants**

Append inside the existing static class (preserving the namespace and surrounding members):

```csharp
public const string CacheKeyComplianceSummary = "atc-api.compliance-summary";
public static readonly TimeSpan ComplianceSummaryTtl = TimeSpan.FromMinutes(30);
```

- [ ] **Step 3: Build to confirm**

Run: `dotnet build D:/Code/atc-net/atc-api/Atc.Api.slnx`
Expected: success.

- [ ] **Step 4: Commit**

```powershell
git add src/Atc.Api/CacheConstants.cs
git commit -m "feat(compliance): add cache key + 30-min TTL for compliance-summary"
```

---

## Task 8 — IGitHubComplianceService interface + skeleton

**Files:**
- Create: `src/Atc.Api/Services/IGitHubComplianceService.cs`
- Create: `src/Atc.Api/Services/GitHubComplianceService.cs`
- Create: `src/Atc.Api/Services/GitHubComplianceService.Log.cs`

- [ ] **Step 1: Create the interface**

`src/Atc.Api/Services/IGitHubComplianceService.cs`:

```csharp
namespace Atc.Api.Services;

using Atc.Api.Models.Compliance;

public interface IGitHubComplianceService
{
    Task<IReadOnlyList<RepositoryComplianceSummary>> GetComplianceSummary(
        CancellationToken cancellationToken = default);
}
```

- [ ] **Step 2: Create the Log partial**

`src/Atc.Api/Services/GitHubComplianceService.Log.cs`:

```csharp
namespace Atc.Api.Services;

using Microsoft.Extensions.Logging;

public partial class GitHubComplianceService
{
    private static partial class Log
    {
        [LoggerMessage(EventId = 7000, Level = LogLevel.Information, Message = "Compliance summary cache hit ({Count} repos).")]
        public static partial void CacheHit(ILogger logger, int count);

        [LoggerMessage(EventId = 7001, Level = LogLevel.Information, Message = "Compliance summary cache miss — computing.")]
        public static partial void CacheMiss(ILogger logger);

        [LoggerMessage(EventId = 7002, Level = LogLevel.Warning, Message = "Compliance summary failed for repo {Repo}: {Error}.")]
        public static partial void RepoFailed(ILogger logger, string repo, string error);
    }
}
```

- [ ] **Step 3: Create the service skeleton**

`src/Atc.Api/Services/GitHubComplianceService.cs`:

```csharp
namespace Atc.Api.Services;

using Atc.Api.Models.Compliance;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

public partial class GitHubComplianceService(
    IGitHubRepositoryService repositoryService,
    IMemoryCache memoryCache,
    ILogger<GitHubComplianceService> logger) : IGitHubComplianceService
{
    public async Task<IReadOnlyList<RepositoryComplianceSummary>> GetComplianceSummary(
        CancellationToken cancellationToken = default)
    {
        if (memoryCache.TryGetValue<IReadOnlyList<RepositoryComplianceSummary>>(
                CacheConstants.CacheKeyComplianceSummary, out var cached) && cached is not null)
        {
            Log.CacheHit(logger, cached.Count);
            return cached;
        }

        Log.CacheMiss(logger);

        var repositories = await repositoryService.GetRepositories(cancellationToken);
        var summaries = new List<RepositoryComplianceSummary>(repositories.Count);

        foreach (var repo in repositories)
        {
            try
            {
                summaries.Add(await ComputeForRepository(repo, cancellationToken));
            }
            catch (Exception ex)
            {
                Log.RepoFailed(logger, repo.Name ?? "?", ex.Message);
            }
        }

        memoryCache.Set(
            CacheConstants.CacheKeyComplianceSummary,
            (IReadOnlyList<RepositoryComplianceSummary>)summaries,
            CacheConstants.ComplianceSummaryTtl);

        return summaries;
    }

    private Task<RepositoryComplianceSummary> ComputeForRepository(
        Repository repo,
        CancellationToken cancellationToken)
    {
        // Implemented in Task 9.
        throw new NotImplementedException();
    }
}
```

- [ ] **Step 4: Build to confirm**

Run: `dotnet build D:/Code/atc-net/atc-api/Atc.Api.slnx`
Expected: success (the `NotImplementedException` is fine — only runtime would hit it).

- [ ] **Step 5: Commit**

```powershell
git add src/Atc.Api/Services/IGitHubComplianceService.cs src/Atc.Api/Services/GitHubComplianceService.cs src/Atc.Api/Services/GitHubComplianceService.Log.cs
git commit -m "feat(compliance): add IGitHubComplianceService skeleton with cache lookup"
```

---

## Task 9 — Implement ComputeForRepository

**Files:**
- Modify: `src/Atc.Api/Services/GitHubComplianceService.cs`

- [ ] **Step 1: Replace the placeholder method**

Replace the body of `ComputeForRepository` with the following. Also add the helper methods below it.

```csharp
private async Task<RepositoryComplianceSummary> ComputeForRepository(
    Repository repo,
    CancellationToken cancellationToken)
{
    var repoName = repo.Name!;
    var paths = await repositoryService.GetPathsByRepositoryByName(repoName, repo.DefaultBranch, cancellationToken);
    var pathSet = paths
        .Select(p => p.Path ?? string.Empty)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    string? readmeRaw = null;
    if (HasFile(pathSet, "readme.md"))
    {
        readmeRaw = await repositoryService.GetFileByRepositoryNameAndFilePath(repoName, "readme.md", cancellationToken);
    }

    var directoryBuildPropsRaw = await GetIfPresent(repoName, pathSet, "Directory.Build.props", cancellationToken);
    var testDirectoryBuildPropsRaw = await GetIfPresent(repoName, pathSet, "test/Directory.Build.props", cancellationToken);
    var updaterRaw = await GetIfPresent(repoName, pathSet, "atc-coding-rules-updater.json", cancellationToken);

    var editorConfigRoot = await GetIfPresent(repoName, pathSet, ".editorconfig", cancellationToken);
    var editorConfigSrc = await GetIfPresent(repoName, pathSet, "src/.editorconfig", cancellationToken);
    var editorConfigTest = await GetIfPresent(repoName, pathSet, "test/.editorconfig", cancellationToken);

    var releasePleasePresent = HasFile(pathSet, ".github/workflows/release-please.yml");

    var workflowFiles = paths
        .Where(p => p.IsFile &&
                    !string.IsNullOrEmpty(p.Path) &&
                    p.Path.StartsWith(".github/workflows/", StringComparison.OrdinalIgnoreCase) &&
                    p.Path.EndsWith(".yml", StringComparison.OrdinalIgnoreCase))
        .Select(p => p.Path!)
        .ToList();

    var workflowContents = new List<string>(workflowFiles.Count);
    foreach (var wf in workflowFiles)
    {
        var content = await repositoryService.GetFileByRepositoryNameAndFilePath(repoName, wf, cancellationToken);
        if (!string.IsNullOrEmpty(content))
        {
            workflowContents.Add(content);
        }
    }

    var openIssues = await repositoryService.GetIssuesByRepositoryNameAndState(repoName, "open", cancellationToken);

    var props = DirectoryBuildPropsParser.Parse(directoryBuildPropsRaw ?? string.Empty);
    var rootVersion = EditorConfigParser.GetVersion(editorConfigRoot ?? string.Empty);
    var srcVersion = EditorConfigParser.GetVersion(editorConfigSrc ?? string.Empty);
    var testVersion = EditorConfigParser.GetVersion(editorConfigTest ?? string.Empty);
    var workflowsStatus = WorkflowsParser.Parse(workflowContents);
    var updaterTarget = UpdaterJsonParser.Parse(updaterRaw ?? string.Empty);

    var srcFrameworks = await ExtractFrameworksFromCsprojIn(repoName, paths, "src/", cancellationToken);
    var testFrameworks = await ExtractFrameworksFromCsprojIn(repoName, paths, "test/", cancellationToken, excludeContains: "XUnitTestDataProjectSampleTypes");
    var sampleFrameworks = await ExtractFrameworksFromCsprojIn(repoName, paths, "sample/", cancellationToken);

    var analyzerPackages = BuildAnalyzerPackages(props.PackageVersions);

    var signals = new RepositoryComplianceSignals
    {
        HasGoodReadme = (readmeRaw?.Length ?? 0) > ComplianceConstants.ReadmeMinSizeBytes,
        LicenseIsMit = string.Equals(repo.License?.Key, "mit", StringComparison.OrdinalIgnoreCase),
        HomepageIsAtcWeb = string.Equals(
            repo.Homepage,
            $"https://atc-net.github.io/repository/{repoName}",
            StringComparison.OrdinalIgnoreCase),
        EditorConfigStatus = new EditorConfigStatus
        {
            RootPresent = editorConfigRoot is not null,
            RootIsLatest = EditorConfigParser.IsAtLeast(rootVersion, ComplianceConstants.EditorConfigRootLatest),
            RootVersion = rootVersion,
            SrcPresent = editorConfigSrc is not null,
            SrcIsLatest = EditorConfigParser.IsAtLeast(srcVersion, ComplianceConstants.EditorConfigSrcLatest),
            SrcVersion = srcVersion,
            TestPresent = editorConfigTest is not null,
            TestIsLatest = EditorConfigParser.IsAtLeast(testVersion, ComplianceConstants.EditorConfigTestLatest),
            TestVersion = testVersion,
        },
        UpdaterPresent = updaterRaw is not null,
        UpdaterTargetIsLatest = string.Equals(updaterTarget, ComplianceConstants.UpdaterTargetLatest, StringComparison.Ordinal),
        UpdaterProjectTarget = updaterTarget,
        GlobalLangVersionIsLatest = string.Equals(props.LangVersion, ComplianceConstants.LangVersionLatest, StringComparison.Ordinal),
        GlobalLangVersion = props.LangVersion,
        GlobalTargetFrameworkIsLatest = string.Equals(props.TargetFramework, ComplianceConstants.TargetFrameworkLatest, StringComparison.Ordinal),
        GlobalTargetFramework = props.TargetFramework,
        XunitV3Status = Enum.Parse<XunitV3Status>(DirectoryBuildPropsParser.DetectXunitV3(testDirectoryBuildPropsRaw ?? string.Empty)),
        WorkflowsStatus = workflowsStatus,
        ReleasePleasePresent = releasePleasePresent,
    };

    var detail = new RepositoryComplianceDetail
    {
        SrcFrameworks = srcFrameworks,
        TestFrameworks = testFrameworks,
        SampleFrameworks = sampleFrameworks,
        AnalyzerPackages = analyzerPackages,
        SuppressedRulesRoot = EditorConfigParser.GetSuppressedRules(editorConfigRoot ?? string.Empty),
        SuppressedRulesSrc = EditorConfigParser.GetSuppressedRules(editorConfigSrc ?? string.Empty),
        SuppressedRulesTest = EditorConfigParser.GetSuppressedRules(editorConfigTest ?? string.Empty),
    };

    return new RepositoryComplianceSummary
    {
        Name = repoName,
        Language = repo.Language,
        Description = repo.Description,
        Homepage = repo.Homepage,
        LicenseKey = repo.License?.Key,
        DefaultBranch = repo.DefaultBranch,
        Topics = repo.Topics?.ToList() ?? [],
        StargazersCount = repo.StargazersCount,
        ForksCount = repo.ForksCount,
        OpenIssuesCount = repo.OpenIssuesCount,
        PushedAt = repo.PushedAt,
        UpdatedAt = repo.UpdatedAt,
        CreatedAt = repo.CreatedAt,
        OldestOpenIssueAt = openIssues.Count == 0 ? null : openIssues.Min(i => i.CreatedAt),
        NewestOpenIssueAt = openIssues.Count == 0 ? null : openIssues.Max(i => i.CreatedAt),
        Signals = signals,
        Detail = detail,
    };
}

private static bool HasFile(HashSet<string> paths, string path) => paths.Contains(path);

private async Task<string?> GetIfPresent(
    string repoName,
    HashSet<string> paths,
    string path,
    CancellationToken cancellationToken)
{
    if (!paths.Contains(path))
    {
        return null;
    }

    var content = await repositoryService.GetFileByRepositoryNameAndFilePath(repoName, path, cancellationToken);
    return string.IsNullOrEmpty(content) ? null : content;
}

private async Task<IReadOnlyList<string>> ExtractFrameworksFromCsprojIn(
    string repoName,
    IReadOnlyList<GitHubPath> paths,
    string folderPrefix,
    CancellationToken cancellationToken,
    string? excludeContains = null)
{
    var csprojPaths = paths
        .Where(p => p.IsFile &&
                    !string.IsNullOrEmpty(p.Path) &&
                    p.Path.StartsWith(folderPrefix, StringComparison.OrdinalIgnoreCase) &&
                    p.Path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase) &&
                    (excludeContains is null ||
                     !p.Path.Contains(excludeContains, StringComparison.OrdinalIgnoreCase)))
        .Select(p => p.Path!)
        .ToList();

    var set = new HashSet<string>(StringComparer.Ordinal);
    foreach (var csproj in csprojPaths)
    {
        var raw = await repositoryService.GetFileByRepositoryNameAndFilePath(repoName, csproj, cancellationToken);
        if (string.IsNullOrEmpty(raw))
        {
            continue;
        }

        var props = DirectoryBuildPropsParser.Parse(raw);
        if (!string.IsNullOrEmpty(props.TargetFramework))
        {
            foreach (var f in props.TargetFramework.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                set.Add(f);
            }
        }
    }

    return set.OrderBy(f => f, StringComparer.Ordinal).ToList();
}

private static IReadOnlyList<AnalyzerPackageRef> BuildAnalyzerPackages(IReadOnlyDictionary<string, string> packages)
{
    var result = new List<AnalyzerPackageRef>();
    foreach (var id in new[] { "Atc.Analyzer", "Meziantou.Analyzer", "SonarAnalyzer.CSharp" })
    {
        if (packages.TryGetValue(id, out var version))
        {
            result.Add(new AnalyzerPackageRef
            {
                PackageId = id,
                Version = version,
                IsLatest = false, // Wired up in a later iteration when cross-referencing nuget cache.
            });
        }
    }

    return result;
}
```

- [ ] **Step 2: Add required usings**

At the top of `GitHubComplianceService.cs`, ensure these usings are present (insert any missing ones inside the file's namespace block, following the existing style — Atc projects typically use `GlobalUsings.cs` so most `System.*` and `Microsoft.Extensions.*` types are already imported; verify by build):

```csharp
using Atc.Api.Constants;
using Atc.Api.Helpers.Compliance;
using Atc.Api.Models.Compliance;
using Octokit; // for Repository, GitHubPath etc. as exposed via IGitHubRepositoryService
```

- [ ] **Step 3: Build**

Run: `dotnet build D:/Code/atc-net/atc-api/Atc.Api.slnx`
Expected: success.

- [ ] **Step 4: Run all unit tests**

Run: `dotnet test D:/Code/atc-net/atc-api/Atc.Api.slnx --filter FullyQualifiedName~Helpers.Compliance`
Expected: all parser tests pass.

- [ ] **Step 5: Commit**

```powershell
git add src/Atc.Api/Services/GitHubComplianceService.cs
git commit -m "feat(compliance): implement ComputeForRepository signal aggregation"
```

---

## Task 10 — Handler

**Files:**
- Create: `src/Atc.Api/Handlers/GetComplianceSummaryHandler.cs`

- [ ] **Step 1: Create handler interface + implementation**

`src/Atc.Api/Handlers/GetComplianceSummaryHandler.cs`:

```csharp
namespace Atc.Api.Handlers;

using Atc.Api.Models.Compliance;
using Atc.Api.Services;

public interface IGetComplianceSummaryHandler
{
    Task<IReadOnlyList<RepositoryComplianceSummary>> ExecuteAsync(CancellationToken cancellationToken = default);
}

internal sealed class GetComplianceSummaryHandler(IGitHubComplianceService service) : IGetComplianceSummaryHandler
{
    public Task<IReadOnlyList<RepositoryComplianceSummary>> ExecuteAsync(CancellationToken cancellationToken = default)
        => service.GetComplianceSummary(cancellationToken);
}
```

- [ ] **Step 2: Build**

Run: `dotnet build D:/Code/atc-net/atc-api/Atc.Api.slnx`
Expected: success.

- [ ] **Step 3: Commit**

```powershell
git add src/Atc.Api/Handlers/GetComplianceSummaryHandler.cs
git commit -m "feat(compliance): add GetComplianceSummaryHandler"
```

---

## Task 11 — DI registration + endpoint mapping

**Files:**
- Modify: the file in `src/Atc.Api/Extensions/` that calls `ConfigureDomainServices` — typically `ServiceCollectionExtensions.cs` or similar
- Modify: the file that maps endpoints (search for `MapEndpoints`)

- [ ] **Step 1: Locate the DI extension**

Run: `grep -rn "ConfigureDomainServices" D:/Code/atc-net/atc-api/src/Atc.Api/Extensions/` to find it.

- [ ] **Step 2: Register the new service**

Inside `ConfigureDomainServices(...)`, alongside the existing `services.AddSingleton<IGitHubRepositoryService, GitHubRepositoryService>();` (or `AddScoped`/`AddTransient` — match the existing pattern), add:

```csharp
services.AddSingleton<IGitHubComplianceService, GitHubComplianceService>();
```

- [ ] **Step 3: Locate the endpoint-mapping extension**

Run: `grep -rn "MapEndpoints" D:/Code/atc-net/atc-api/src/Atc.Api/Extensions/` to find it.

- [ ] **Step 4: Add the route**

Inside the same extension where the existing `/github/repository` routes live (or in a sibling method following the same pattern), add:

```csharp
group.MapGet("/compliance-summary", static async (
        IGetComplianceSummaryHandler handler,
        CancellationToken cancellationToken)
    => Results.Ok(await handler.ExecuteAsync(cancellationToken)))
    .WithName("getComplianceSummary")
    .WithSummary("Get compliance summary for all repositories.")
    .WithDescription("Returns one RepositoryComplianceSummary per non-archived ATC repository, with all dashboard signals computed and cached for 30 minutes.")
    .WithTags("github-repository");
```

- [ ] **Step 5: Run the API locally**

Run: `dotnet run --project D:/Code/atc-net/atc-api/src/Atc.Api/Atc.Api.csproj`
Expected: the app starts. Open `https://localhost:7XXX/scalar/v1` and confirm `getComplianceSummary` is listed under the `github-repository` tag.

- [ ] **Step 6: Smoke-test the endpoint**

In a second shell, fetch (substitute the dev port from the run output):

```powershell
curl -k https://localhost:7XXX/github/repository/compliance-summary | Select-Object -First 5
```

Expected: a JSON array; each element has `name`, `signals`, `detail`. First call may take 10–30 s (cold cache). Second call returns instantly (cache hit log entry visible).

- [ ] **Step 7: Stop the local API and commit**

```powershell
git add src/Atc.Api/Extensions/
git commit -m "feat(compliance): register IGitHubComplianceService and map GET /compliance-summary"
```

---

## Task 12 — OpenAPI YAML schema additions

**Files:**
- Modify: `src/Atc.Api/Atc.Api.yaml`

- [ ] **Step 1: Add the path**

Locate the section that contains `/github/repository/nuget-packages-used:` (around line 79). After the closing of that path block, insert:

```yaml
  /github/repository/compliance-summary:
    get:
      tags: [github-repository]
      operationId: getComplianceSummary
      summary: Get compliance summary for all repositories.
      description: Returns one RepositoryComplianceSummary per non-archived ATC repository, with all dashboard signals computed server-side and cached for 30 minutes.
      responses:
        '200':
          description: OK
          content:
            application/json:
              schema:
                type: array
                items:
                  $ref: '#/components/schemas/RepositoryComplianceSummary'
```

- [ ] **Step 2: Add the schemas**

In the `components.schemas:` section (after `DotnetNugetPackageMetadataBase`), append the following block exactly as shown:

```yaml
    RepositoryComplianceSummary:
      title: RepositoryComplianceSummary
      type: object
      required: [name, signals, detail]
      properties:
        name: { type: string }
        language: { type: string, nullable: true }
        description: { type: string, nullable: true }
        homepage: { type: string, nullable: true }
        licenseKey: { type: string, nullable: true }
        defaultBranch: { type: string, nullable: true }
        topics:
          type: array
          items: { type: string }
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
      title: RepositoryComplianceSignals
      type: object
      required: [editorConfigStatus, workflowsStatus, xunitV3Status]
      properties:
        hasGoodReadme: { type: boolean }
        licenseIsMit: { type: boolean }
        homepageIsAtcWeb: { type: boolean }
        editorConfigStatus: { $ref: '#/components/schemas/EditorConfigStatus' }
        updaterPresent: { type: boolean }
        updaterTargetIsLatest: { type: boolean }
        updaterProjectTarget: { type: string, nullable: true }
        globalLangVersionIsLatest: { type: boolean }
        globalLangVersion: { type: string, nullable: true }
        globalTargetFrameworkIsLatest: { type: boolean }
        globalTargetFramework: { type: string, nullable: true }
        xunitV3Status:
          type: string
          enum: [Yes, No, NotApplicable]
        workflowsStatus: { $ref: '#/components/schemas/WorkflowsStatus' }
        releasePleasePresent: { type: boolean }
    EditorConfigStatus:
      title: EditorConfigStatus
      type: object
      properties:
        rootPresent: { type: boolean }
        rootIsLatest: { type: boolean }
        rootVersion: { type: string, nullable: true }
        srcPresent: { type: boolean }
        srcIsLatest: { type: boolean }
        srcVersion: { type: string, nullable: true }
        testPresent: { type: boolean }
        testIsLatest: { type: boolean }
        testVersion: { type: string, nullable: true }
    WorkflowsStatus:
      title: WorkflowsStatus
      type: object
      properties:
        actions:
          type: array
          items: { type: string }
        dotnetVersions:
          type: array
          items: { type: string }
        checkoutIsLatest: { type: boolean }
        setupDotnetIsLatest: { type: boolean }
        hasJavaSetup: { type: boolean }
        dotnetVersionIsLatest: { type: boolean }
    RepositoryComplianceDetail:
      title: RepositoryComplianceDetail
      type: object
      properties:
        srcFrameworks:
          type: array
          items: { type: string }
        testFrameworks:
          type: array
          items: { type: string }
        sampleFrameworks:
          type: array
          items: { type: string }
        analyzerPackages:
          type: array
          items: { $ref: '#/components/schemas/AnalyzerPackageRef' }
        suppressedRulesRoot:
          type: array
          items: { type: string }
        suppressedRulesSrc:
          type: array
          items: { type: string }
        suppressedRulesTest:
          type: array
          items: { type: string }
    AnalyzerPackageRef:
      title: AnalyzerPackageRef
      type: object
      required: [packageId, version]
      properties:
        packageId: { type: string }
        version: { type: string }
        isLatest: { type: boolean }
```

- [ ] **Step 3: Validate YAML**

Run: `dotnet build D:/Code/atc-net/atc-api/Atc.Api.slnx`
Expected: build succeeds. (If the project includes a YAML lint step that runs at build, it will catch malformed YAML.)

- [ ] **Step 4: Commit**

```powershell
git add src/Atc.Api/Atc.Api.yaml
git commit -m "feat(compliance): document /github/repository/compliance-summary in OpenAPI YAML"
```

---

## Task 13 — Integration smoke test

**Files:**
- Create: `test/Atc.Api.Tests/Integration/ComplianceEndpointTests.cs`

- [ ] **Step 1: Write the test**

`test/Atc.Api.Tests/Integration/ComplianceEndpointTests.cs`:

```csharp
namespace Atc.Api.Tests.Integration;

using System.Net;
using System.Net.Http.Json;
using Atc.Api.Models.Compliance;
using FluentAssertions;
using Xunit;

public sealed class ComplianceEndpointTests(ApiTestFactory factory) : IClassFixture<ApiTestFactory>
{
    private readonly HttpClient client = factory.CreateClient();

    [Fact]
    public async Task GetComplianceSummary_ReturnsOk_AndDeserializesShape()
    {
        var response = await client.GetAsync("/github/repository/compliance-summary");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var summaries = await response.Content.ReadFromJsonAsync<List<RepositoryComplianceSummary>>();
        summaries.Should().NotBeNull();
        summaries!.Should().NotBeEmpty();

        var first = summaries[0];
        first.Name.Should().NotBeNullOrEmpty();
        first.Signals.Should().NotBeNull();
        first.Signals.EditorConfigStatus.Should().NotBeNull();
        first.Signals.WorkflowsStatus.Should().NotBeNull();
        first.Detail.Should().NotBeNull();
    }
}
```

- [ ] **Step 2: Run**

Run: `dotnet test D:/Code/atc-net/atc-api/Atc.Api.slnx --filter FullyQualifiedName~ComplianceEndpointTests`
Expected: pass. (First run may take 30+ seconds due to live GitHub fetches; subsequent runs are fast due to caching.)

- [ ] **Step 3: Commit**

```powershell
git add test/Atc.Api.Tests/Integration/ComplianceEndpointTests.cs
git commit -m "test(compliance): smoke-test /github/repository/compliance-summary"
```

---

## Task 14 — Final build, deploy, sanity check

- [ ] **Step 1: Run full test suite**

Run: `dotnet test D:/Code/atc-net/atc-api/Atc.Api.slnx`
Expected: all tests pass.

- [ ] **Step 2: Push to remote**

```powershell
git push origin <branch-name>
```

- [ ] **Step 3: Deploy via existing process**

Follow the existing manual deploy procedure for the atc-api Azure Container App. (Docs in the atc-api repo.)

- [ ] **Step 4: Production smoke test**

After deploy, hit:

```powershell
curl https://atc-api.example.com/github/repository/compliance-summary | Select-Object -First 5
```

(The actual base URL is in `AtcApiConstants.cs` of `atc-net.github.io`. Confirm production status returns 200 and a populated array before starting Phase 2.)

- [ ] **Step 5: Notify team**

Phase 1 done. Phase 2 (frontend redesign) is unblocked and can start.
