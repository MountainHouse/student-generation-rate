# Current Model

The current model is a deterministic, grid-first enrollment projection implemented in `SchoolGrowth.Core`.

It starts from known reference data, calibrates a few aggregate parameters, projects future students by residence grid, and then rolls those students into school totals.

## Inputs

The model uses four CSV files:

- `homes.csv`: homes built by neighborhood/grid, density, and year.
- `grid.csv`: historical student totals by residence grid.
- `grade.csv`: historical district student counts by grade.
- `schools.csv`: historical school/program totals.

The core limitation is that the data does not include true grade-by-grid counts. Because of that, the model estimates grade-by-grid distributions using district grade shares and school proxy ratios.

## Calibration

Calibration produces:

- density yield per home
- district grade shares
- grade retention rates
- grid trend rates
- school proxy ratios
- baseline TK students
- calibration error

### Density Yield

The current model estimates students generated per home by density.

Conceptually:

```text
student_yield[density]
```

The model uses historical homes and grid totals to fit yield values. Homes are weighted by age using the built-in neighborhood aging curve:

```text
age 0      0.30
age 1      0.60
age 2      0.85
age 3-5    1.00
age 6-10   0.90
age 11-15  0.75
age 16+    0.62
```

The fitting method is ridge regression, which helps keep density yields stable when the data is sparse or correlated.

### Grade Shares

Grade shares are calculated from the latest actual August grade data.

```text
grade_share[grade] = grade_count[grade] / total_grade_count
```

These shares are used to distribute projected grid totals into grades.

### Grid Trend Rates

Each grid receives an observed trend rate calculated from recent August-to-August grid totals.

For each grid:

```text
observed_grid_trend =
    average of last 4 valid values of:
        grid_total[year] / grid_total[year - 1]
```

The observed trend is clamped:

```text
0.92 <= observed_grid_trend <= 1.12
```

Then the UI retention confidence blends the observed trend with a flat trend:

```text
effective_grid_trend =
    observed_grid_trend * retention_confidence
    + 1.0 * (1 - retention_confidence)
```

With retention confidence set to `0.75`, the model trusts 75% of the historical grid trend and pulls 25% toward no growth.

This factor should be understood as broad grid momentum, not pure student retention. It includes neighborhood aging, turnover, existing-home effects, and historical growth not explicitly explained by new scenario homes.

### Grade Retention Rates

The model also calculates historical grade-to-grade retention:

```text
retention[target_grade] =
    average of:
        grade_count[target_grade, year] / grade_count[source_grade, year - 1]
```

The current projection primarily uses grid totals and grade shares, so grade retention is available in calibration but is not the main driver of final grid-grade allocation.

### School Proxy Ratios

Because true grid-by-grade data is unavailable, matching neighborhood schools are used as a proxy for how much of a grid belongs to TK-8.

```text
school_proxy_ratio[grid] =
    recent average of school_total[grid] / grid_total[grid]
```

Special rules:

- MHESD is high-school-only and contributes only grades 9-12 to MHHS.
- Lammersville and Inter-Districts are treated as TK-12 source grids.
- LVLA/LEO remains an actual program total from `schools.csv`, not a residence grid projection.

## Projection

For each future year, the model projects every grid independently.

For each grid:

```text
new_students[grid, year] =
    sum over scenario homes:
        homes[grid, density, built_year]
        * density_yield[density]
        * age_factor[year - built_year]
        * home_yield_multiplier
```

Then:

```text
grid_total[grid, year] =
    grid_total[grid, year - 1] * effective_grid_trend[grid]
    + new_students[grid, year]
```

The model allocates each projected grid total into grades using district grade shares and grid school-proxy logic.

## School Rollup

After grid-grade projection, school totals are calculated.

TK-8 students generally roll to matching neighborhood schools:

```text
Altamont grid TK-8     -> Altamont school
Bethany grid TK-8      -> Bethany school
Cordes grid TK-8       -> Cordes school
Costa grid TK-8        -> Costa school
Hansen grid TK-8       -> Hansen school
Lammersville grid TK-8 -> Lammersville school
Questa grid TK-8       -> Questa school
Wicklund grid TK-8     -> Wicklund school
Pombo grid TK-8        -> Pombo bucket
```

All grades 9-12 from all eligible grids roll to:

```text
MHHS
```

Special education rolls to:

```text
Special Programs
```

## Validation

The validation page runs a historical backtest:

1. Start from an actual historical baseline year.
2. Use actual homes built during the test period as the scenario.
3. Project forward.
4. Compare modeled grid totals to actual grid totals.

The current error metrics include:

```text
error = modeled_total - actual_total
absolute_percentage_error = abs(error) / actual_total
```

## Strengths

- Fast and simple enough for browser use.
- Uses available data directly.
- Supports per-grid and per-school projections.
- Can be tuned through retention confidence, home yield multiplier, and density multipliers.

## Limitations

- Grid trend mixes several effects into one number.
- Home aging is a fixed aggregate curve, not derived from household behavior.
- Grade-by-grid values are estimated, not observed.
- Retention is not yet exposed per grade in the UI.
- Capacity overflow and school assignment policy are out of scope.

