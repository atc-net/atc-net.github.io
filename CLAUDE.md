# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Blazor WebAssembly SPA for the ATC-NET organization website (atc-net.github.io). Aggregates and displays content from ATC-NET GitHub repositories. 98% of content lives in those repos and is fetched/rendered here.

## Build & Run

```bash
dotnet build src/AtcWeb/AtcWeb.csproj
dotnet run --project src/AtcWeb/AtcWeb.csproj
```

Local dev host (Aspire, `src/AtcWeb.AppHost`) — dashboard at https://localhost:17200, site at https://localhost:45365:

```bash
dotnet run --project src/AtcWeb.AppHost                                                       # pick LOCAL/DEPLOYED in the dashboard
dotnet run --project src/AtcWeb.AppHost -- --Parameters:atc-api-url=https://localhost:51612/   # preset the choice
```

- The `atc-api-url` parameter (LOCAL `https://localhost:51612/` or DEPLOYED) backs an external service `atc-api` with a `/health` check, and the site waits for it. LOCAL means you run atc-api yourself (e.g. its own AppHost).
- The site is standalone Blazor WASM, so resource environment variables never reach the browser. The AppHost starts it with `dotnet run -p:AtcApiBaseAddress=<url>`; `AtcWeb.csproj` turns that into a `WasmEnvironmentVariable` (`AtcApi__BaseAddress`) in the boot config, which `Program.cs` prefers over `wwwroot/appsettings*.json`. `AddProject` can't do this because it runs `--no-build`. That build goes to `artifacts/apphost-web` (`--artifacts-path`), so IDE/`dotnet build` output can't overwrite the running site.

## Tests

```bash
dotnet test                                    # run all tests
dotnet test --filter "FullyQualifiedName~TestName"  # run a single test
```

Tests are integration tests in `test/AtcWeb.Domain.Tests/` using xUnit, AutoNSubstituteData, and real HTTP calls to the ATC API.

## SCSS / CSS

SCSS source is in `src/AtcWeb/Styles/`, entry point `AtcWeb.scss`. Compile with the globally installed `sass` CLI:

```bash
sass src/AtcWeb/Styles/AtcWeb.scss src/AtcWeb/wwwroot/css/AtcWeb.css --style=expanded --no-source-map
sass src/AtcWeb/Styles/AtcWeb.scss src/AtcWeb/wwwroot/css/AtcWeb.min.css --style=compressed --no-source-map
```

Both output files (`AtcWeb.css` and `AtcWeb.min.css`) must be committed. Deprecation warnings about `@import` are expected and harmless.

## Deployment

Release is manual (`workflow_dispatch`) on the `stable` branch. `dotnet publish` outputs to `release/wwwroot/`, then deploys to `gh-pages`. `index.html` is copied to `404.html` for SPA routing.

## Architecture

Two projects with clean separation:

- **`src/AtcWeb.Domain/`** — Domain layer. API clients, caching, data models. No UI dependencies.
- **`src/AtcWeb/`** — Blazor WASM frontend. Pages, components, styles, state. Depends on Domain.

### Data Flow

Pages → `GitHubRepositoryService` → API clients (`AtcApiGitHubRepositoryClient`, `AtcApiGitHubApiInformationClient`) → ATC proxy API (Azure Container App) → GitHub API.

The proxy API base URL is in `AtcApiConstants.cs`. Key endpoints: `/github/repository/`, `/github/repository/{name}/paths`, `/github/repository/{name}/file`, `/github/repository/{name}/issues/{state}`.

### Caching

Two-tier caching to minimize API calls:
1. **Memory cache** (`IMemoryCache`) — 12-hour absolute expiration
2. **Browser localStorage** (`BrowserCacheService` via JSInterop) — 30-minute TTL, stale-while-revalidate pattern

Semaphores prevent duplicate concurrent requests for the same data.

### State Management

`StateContainer` (singleton) manages global theme state (dark/light mode) with `OnThemeChange` event. MudBlazor provides the UI framework (Material Design).

### Repository Categories

Repo cards are categorized in `src/AtcWeb/Styles/RepositoryCategoryHelper.cs`. **Order of checks matters** — more specific categories (e.g., IoT) must appear before broader ones (e.g., Azure) so they take precedence. When adding a category, update all four methods (`GetCategory`, `GetCardCssClass`, `GetIcon`, `GetSortOrder`), the NavMenu skeleton in `NavMenu.razor`, and the card accent in `_modern.scss`.

## Key Patterns

- API clients return `(bool IsSuccessful, T Data)` tuples
- Pages use `DocsPage`/`DocsPageHeader`/`DocsPageContent` wrapper components
- Dynamic repository pages route via `/repository/{RepositoryName}`
- Markdown rendered with Markdig, sanitized with HtmlSanitizer
