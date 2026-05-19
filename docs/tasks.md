# Project Tasks and Follow-Ups

This file collects known engineering tasks, cleanup ideas, and investigation notes. It is not a sprint plan; it is a place to keep issues visible when they are discovered during modeling or UI work.

## Build and Deployment

General note: use quick non-AOT builds for ordinary development. AOT/threaded publish tasks are slower and should be run when the task specifically concerns browser performance, static deployment, or AOT/threading behavior.

## Product Direction

### Improve anchored household hidden-state inference

Current behavior: when a start year has actual grid and grade reference data, Household Simulation reconciles enrolled K-12 students to the observed grade/grid counts. That is correct for known enrollment, but hidden household state is approximate: preschool siblings, post-school siblings, sibling grouping, and household tenure are generated from general model probabilities.

Goal: design an optional inference layer for anchored starts that allocates observed students into plausible households and infers hidden family context without changing the known grade/grid counts.

Acceptance criteria:

- Observed anchored grade/grid student counts remain preserved.
- The inferred preschool pipeline improves K/early-grade forecasts without using future reference data.
- The inferred high-school/post-school tail improves HS forecasts and family child-count diagnostics.
- The method is documented as inference, not observed data.

### Consider vacancy as a separate home state

Current behavior: Household Simulation does not distinguish vacant homes from occupied zero-child households. This is acceptable for current calibration because long-term vacancy is expected to be small and new-construction timing is partly handled by `same_school_year_probability`.

Goal: evaluate whether future versions should model vacancy explicitly, especially if we add occupancy data or need student generation per occupied dwelling unit.

Acceptance criteria:

- Vacant homes do not generate move-in children or births/new children until occupied.
- Occupied zero-child households remain distinct from vacancy.
- Student generation can be reported per DU and per occupied DU.
- Calibration documentation explains whether child-count shares and birth/new-child rates are conditional on occupied homes or all homes.

### Evaluate deterministic model retirement or migration

Current direction: Household Simulation is the primary product path. The deterministic projection and validation pages remain useful as a simple comparison/reference, but they may become maintenance cost.

Goal: decide whether to retire the deterministic pages/model, keep them as explicit legacy tools, or port useful UI/data views into the Household Simulation page.

Acceptance criteria:

- Existing useful deterministic views are identified.
- Any view worth preserving has a Household Simulation equivalent or a migration task.
- README/docs clearly describe what remains legacy versus primary.

### Evaluate internal Monte Carlo type naming

Current direction: user-facing UI, routes, API paths, and data files should say Household Simulation. Internal C# types still use `MonteCarlo` where that is the most precise algorithm name.

Goal: decide whether renaming internal C# types from `MonteCarlo...` to `HouseholdSimulation...` improves clarity enough to justify the churn.

Acceptance criteria:

- Internal type names clearly distinguish product names from algorithm names.
- No old route/API/file aliases are kept only for backward compatibility.

### Investigate hosted Blazor publish fingerprint handling

Observed behavior: the hosted `SchoolGrowth.Web` publish can leave `index.html` with an unresolved Blazor fingerprint placeholder or otherwise serve an unprocessed client asset map. For static deployment, prefer the one-command client publish script:

```powershell
.\scripts\publish-static-aot-threads.ps1
```

Goal: find the correct SDK/project configuration so hosted publish produces a complete, processed Blazor `wwwroot` without manual copying when a hosted AOT artifact is needed.

Relevant files:

- `src/SchoolGrowth.Blazor/SchoolGrowth.Blazor.csproj`
- `src/SchoolGrowth.Web/SchoolGrowth.Web.csproj`
- `src/SchoolGrowth.Blazor/wwwroot/index.html`
- `docs/deployment.md`

Acceptance criteria:

- `dotnet publish src\SchoolGrowth.Web\SchoolGrowth.Web.csproj -c Release -p:RunAOTCompilation=true -p:WasmEnableThreads=true -o artifacts\web-aot-threads` produces a runnable threaded hosted artifact without manual copy.
- Published `index.html` has resolved framework script/import map references.
- Hosted threaded app starts after a clean browser cache.
