# Project Tasks and Follow-Ups

This file collects known engineering tasks, cleanup ideas, and investigation notes. It is not a sprint plan; it is a place to keep issues visible when they are discovered during modeling or UI work.

## Build and Deployment

General note: use quick non-AOT builds for ordinary development. AOT/threaded publish tasks are slower and should be run when the task specifically concerns browser performance, static deployment, or AOT/threading behavior.

## Product Direction

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
