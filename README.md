# School Growth Planning Toolkit

School Growth Planning Toolkit is a C#/.NET project for exploring school enrollment growth from housing construction, neighborhood/grid history, grade history, and household simulation.

The project currently has one primary modeling path and two supporting/historical paths:

- Household Simulation is the primary product path for future development and decision support. It uses a Monte Carlo household model internally.
- Deterministic Projection is a legacy reference/comparison tool; avoid expanding it unless the task is explicitly about old deterministic behavior.
- Model 2.0 is design/reference documentation only; useful ideas should be ported into Household Simulation or retired.

The UI is a Blazor WebAssembly app backed by shared C# model code. It can run as:

- a hosted ASP.NET app with server-side household simulation endpoints
- a browser-only static Blazor WASM app
- an AOT + WebAssembly threads static app when cross-origin isolation is available

## Repository Layout

```text
data/                         Source CSVs and parameter presets used by the app
docs/                         Model, UI, and deployment documentation
src/SchoolGrowth.Core/        Shared deterministic and household simulation model logic
src/SchoolGrowth.Blazor/      Browser UI
src/SchoolGrowth.Web/         ASP.NET host and server API endpoints
src/SchoolGrowth.Cli/         CLI validation, search, optimization, lifecycle tools
tests/SchoolGrowth.Core.Tests Core smoke tests
artifacts/                    Generated builds/search results, ignored by git
.server-logs/                 Local server logs, ignored by git
```

The runtime input data is the small CSV set in `data/`:

```text
homes.csv
grid.csv
grade.csv
schools.csv
simulation-presets.json
```

The Blazor project links these files into build/publish output under `wwwroot/data/`, so `data/` is the only source copy to edit. Do not add or edit duplicate data files under `src/SchoolGrowth.Blazor/wwwroot/data/`.

Generated optimizer CSVs should go under `artifacts/search-results/`, not the repository root.

## Quick Start

Build everything:

```powershell
dotnet build SchoolGrowthSimulator.sln
```

Run the hosted web app:

```powershell
dotnet run --project src/SchoolGrowth.Web
```

Run core smoke tests:

```powershell
dotnet run --project tests/SchoolGrowth.Core.Tests
```

Run a household simulation validation from the CLI:

```powershell
dotnet run --project src/SchoolGrowth.Cli -- validate --start 2020 --end 2024 --runs 30
```

Save optimizer/search traces under artifacts:

```powershell
dotnet run --project src/SchoolGrowth.Cli -- search --out artifacts/search-results/simulation-candidates.csv
```

## Main UI Pages

- `/` - Household Simulation, parameter tuning, visual comparisons, and simulation info
- `/simulation` - explicit Household Simulation route
- `/projection` - Deterministic Projection
- `/projection-validation` - Projection Validation for the deterministic model

The Household Simulation page is organized into collapsible/reorderable tools. The default visible tools are simulation parameters, future homes, and simulation summary; additional diagnostics, model-fit weights, run setup, and custom chart tools can be added from the Tools menu.

## Model Summary

Deterministic Projection projects students by residence grid, then rolls those into schools:

- TK-8 students go to the matching neighborhood school.
- MHHS receives grades 9-12 from all eligible grids.
- MHESD is high-school-only.
- Lammersville and Inter-Districts are TK-12 source grids.
- LVLA/LEO remains an actual program total, not a residence grid projection.

Household Simulation uses a Monte Carlo model to simulate homes and household episodes:

- move-in household child count
- move-in child age distribution
- ownership/household turnover
- new children by child number
- direct per-density child-count and birth-rate profiles, currently configured through density and child-number factors
- school-year timing for new homes
- student exits and special education assignment
- grade progression and high-school totals

Current calibration practice ignores TK in simulation validation because TK policy changes make it a noisy target.

Start with [docs/household-simulation-model.md](docs/household-simulation-model.md) for the active Household Simulation model. Avoid [docs/current-model.md](docs/current-model.md) and [docs/model-2.0.md](docs/model-2.0.md) unless you are intentionally maintaining Deterministic Projection, comparing old behavior, or extracting a specific idea to port into Household Simulation.

## Deployment

For static browser deployment:

- One-command threaded AOT static publish:

```powershell
.\scripts\publish-static-aot-threads.ps1
```

Use `-NoRestore` for faster local reruns after packages/workloads are already restored.

- GitHub Actions publishes the threaded AOT static app to GitHub Pages on every `master` commit.
- The expected project-site URL is `https://mountainhouse.github.io/student-generation-rate/`.

- Publish Blazor WASM with AOT for browser performance.
- Use WebAssembly threads when the host can provide cross-origin isolation.
- Hosts with real headers, such as Cloudflare Pages or Netlify, are preferred.
- GitHub Pages can work with the included `coi-serviceworker.js` fallback, but real headers are cleaner.

The ASP.NET host sends the isolation headers by default:

```text
Cross-Origin-Opener-Policy: same-origin
Cross-Origin-Embedder-Policy: require-corp
Cross-Origin-Resource-Policy: same-origin
```

See [docs/deployment.md](docs/deployment.md).

## Future Work

Known tasks and follow-ups are tracked in [docs/tasks.md](docs/tasks.md).

- Improve model explainability around high-school forecasts.
- Continue tuning Household Simulation parameters against grade, grid, and high-school targets.
- Evaluate retiring the deterministic projection/validation pages or porting useful views into Household Simulation.
- Keep generated experiments in `artifacts/`.
