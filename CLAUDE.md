# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Blazor WebAssembly SPA for the ATC-NET organization website (atc-net.github.io). Aggregates and displays content from ATC-NET GitHub repositories. 98% of content lives in those repos and is fetched/rendered here.

## Build & Run

```bash
dotnet build src/AtcWeb/AtcWeb.csproj
dotnet run --project src/AtcWeb/AtcWeb.csproj
```

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
