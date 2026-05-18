# Agent Instructions

Purpose: preserve the minimum project-specific operating rules that coding agents must follow before editing this repository.

For human-facing orientation and detailed project rules, read:

- `README.md`
- `docs/README.md`
- `docs/project-rules.md`
- `docs/current-model.md`
- `docs/household-simulation-model.md`
- `docs/ui.md`
- `docs/deployment.md`
- `docs/tasks.md`

## Non-Negotiable Guardrails

- Keep model logic in `src/SchoolGrowth.Core`; UI should present model outputs, not duplicate model math.
- Keep source data in `data/`; do not delete or relocate required CSV/preset files.
- Put generated search/optimization output under `artifacts/search-results/`, not the repository root.
- Preserve established domain rules unless the user explicitly changes them; see `docs/project-rules.md`.
- Do not add or preserve route/API/file aliases only for backward compatibility. If compatibility support seems useful, ask the project owner first.
- If changing model parameters/defaults, update presets and model docs.
- If changing Household Simulation validation/scoring, update `docs/household-simulation-model.md`.
- If changing UI interaction patterns, update `docs/ui.md`.
- Reuse existing UI components/patterns when adding controls. Avoid duplicating matching UI behavior when a shared component is practical. If matching behavior exists only as page-local markup/code, consider extracting a shared reusable component before adding another copy.
- If changing deployment/startup behavior, update `docs/deployment.md` and test hosted/static assumptions.

## Architecture Map

```text
src/SchoolGrowth.Core/        Shared deterministic and household simulation model logic
src/SchoolGrowth.Blazor/      Browser UI and static WASM assets
src/SchoolGrowth.Web/         ASP.NET host and simulation server endpoints
src/SchoolGrowth.Cli/         CLI validation/search/optimization/lifecycle tools
tests/SchoolGrowth.Core.Tests Core smoke tests
```

## Core Product Principle

Keep the project explainable. Prefer explicit parameters, visible formulas, and reference comparisons over hidden heuristics. When a heuristic is necessary, document it and make it tunable where practical.

## Execution Guardrail

The app supports both browser-side and backend Household Simulation computation. Preserve both paths unless the user explicitly changes that direction. Detailed execution and deployment behavior lives in `docs/project-rules.md` and `docs/deployment.md`.

## Build and Test Commands

Use relevant checks before handing work back:

```powershell
dotnet build SchoolGrowthSimulator.sln
dotnet run --project tests/SchoolGrowth.Core.Tests
dotnet build src\SchoolGrowth.Cli\SchoolGrowth.Cli.csproj -c Release --no-restore
dotnet build src\SchoolGrowth.Blazor\SchoolGrowth.Blazor.csproj -c Release --no-restore
```

Prefer quick non-AOT builds while developing. Use AOT/threaded publishes only when the user asks for browser performance/deployment validation or when fixing AOT/threading-specific issues.

For AOT/threaded browser testing:

```powershell
dotnet publish src\SchoolGrowth.Blazor\SchoolGrowth.Blazor.csproj -c Release -p:RunAOTCompilation=true -p:WasmEnableThreads=true -o artifacts\blazor-aot-threads
dotnet publish src\SchoolGrowth.Web\SchoolGrowth.Web.csproj -c Release -p:RunAOTCompilation=true -p:WasmEnableThreads=true -o artifacts\web-aot-threads
```

Known follow-up tasks and temporary workarounds are tracked in `docs/tasks.md`.
