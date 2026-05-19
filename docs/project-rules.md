# Project Rules and Assumptions

This document collects project rules that are useful to both human contributors and coding agents. These are not all implementation details; they are the modeling and product assumptions that should remain stable unless intentionally changed.

## Product Direction

Household Simulation is the primary product path for future development and decision support. It uses a Monte Carlo household model internally.

Avoid using or expanding Deterministic Projection and Model 2.0 unless the task is explicitly about maintaining old behavior, comparing old behavior, or porting a specific idea into Household Simulation.

## Data Rules

Required input files live in `data/`:

```text
homes.csv
grid.csv
grade.csv
schools.csv
simulation-presets.json
```

## Compatibility Rules

There is no standing backward-compatibility requirement for routes, API paths, file names, or UI labels. Do not keep extra aliases or harder code only for compatibility. If a future change appears to need compatibility support, ask the project owner before implementing it.

Do not put optimizer/search result CSVs in the repository root. Use:

```text
artifacts/search-results/
```

`artifacts/`, `.server-logs/`, `bin/`, `obj/`, and logs are local/generated and ignored.

## Deterministic Projection Rules

- The projection is grid-first: project residence grids, then roll up to schools.
- MHHS is the high-school rollup for grades 9-12.
- MHESD is high-school-only; it contributes no TK-8 students.
- Lammersville and Inter-Districts are TK-12 source grids.
- LVLA/LEO is an actual program bucket from `schools.csv`, not a residence grid.
- Capacity overflow assignment is out of scope until capacity data exists.

See `current-model.md` for formulas and rollup details.

## Household Simulation Validation Rules

- TK is hidden/ignored in simulation validation because TK policy changes can skew calibration.
- 2025 can be excluded from calibration when reference data is incomplete or too recent.
- Optimization should score reference years year-over-year, not only the final year.
- Current scoring uses district total, grid, grade, and high-school terms.
- High-school accuracy matters and can have dedicated total and grade-level score terms.
- Generated search traces are disposable unless explicitly saved as benchmark evidence.

See `household-simulation-model.md` for the simulation and scoring details.

## Household Simulation Assumptions

- Physical homes can have many household episodes over time.
- Turnover includes sale, renter change, and next-generation household changes.
- Baseline homes can already have younger household episodes than the physical home age.
- Move-in children include preschool and post-school children, not only enrolled children.
- New children are modeled by child number; 3rd and 4th+ children should be much less likely.
- Density affects household child count and birth probabilities through direct per-density profiles.
- The current UI can still use baseline shares plus density/child-number factors as a calibration helper; the simulation core should consume the resulting direct per-density child-count shares and birth rates.
- Low and medium density are configurable separately, though current presets may keep them similar. Medium-high/high usually has lower higher-child probability.
- Special education is a child attribute assigned with a low district-data-based probability.

## Execution Rules

Execution modes on the Household Simulation page:

- `Auto`: local/dev loopback host uses server API if detected; deployed/static host uses browser.
- `Server API`: force server endpoints.
- `Browser only`: force local WASM execution.

Browser performance depends strongly on AOT. Interpreted WASM can be much slower.

## Deployment Rules

- Real cross-origin isolation headers are preferred for threaded WASM.
- The ASP.NET host sends those headers by default.
- `coi-serviceworker.js` is kept as a static-host fallback for GitHub Pages-like hosts.
- Blazor startup is guarded so the threaded runtime loads only after isolation is ready.
- If startup scripts change, test normal refresh and Ctrl+refresh.

See `deployment.md` for hosting and publish details.
