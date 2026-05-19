# Household Simulation Model

Household Simulation is the primary product model. It uses a Monte Carlo simulation that treats each home and household as an individual simulated unit.

Instead of directly applying a fixed student-yield curve, it simulates ownership changes, household composition, children aging, school participation, and children leaving the district.

The final enrollment forecast is the average of many simulated futures.

## Implemented Model

The implemented model simulates homes, household episodes, children, school enrollment, and historical validation. It is the primary model path for the app.

Children track a grade index rather than only a current grade name. That allows the model to create newborn/pre-school children who enter TK several years later, and to keep post-school children in the household count without adding them to enrollment.

The model supports:

- browser-side and server-side execution
- multi-run averaging
- multi-threaded server validation/search
- AOT/threaded WebAssembly static deployment
- start-year anchoring to actual reference data when available
- back-play for homes built before the validation window
- synthetic played-back homes when a reference grid has students but no listed homes
- density-specific direct move-in child-count distributions and birth/new-child rates
- special source grid removal for validation
- TK exclusion from grade scoring
- cohort smoothing for grade validation
- high-school total and high-school grade score terms
- simulation-info output for homes, students, student generation, family profiles, turnover, and longevity
- data-gap diagnostics for synthetic played-back baseline homes
- lifecycle diagnostics for long-range family tenure and student generation

Implemented parameters:

```text
runs
seed
ownership_change_probability
move_in_zero_child_share
move_in_one_child_share
move_in_two_child_share
move_in_three_child_share
move_in_four_child_share   # currently draws exactly 4 children at move-in
student_exit_probability
move_in_preschool_weight
move_in_tk_k_weight
move_in_elementary_weight
move_in_middle_weight
move_in_high_weight
move_in_postschool_weight
special_education_probability
annual_first_new_child_probability
annual_second_new_child_probability
annual_third_new_child_probability
annual_fourth_plus_new_child_probability
tk8_exit_probability
high_school_exit_probability
special_exit_probability
same_school_year_probability
max_degree_of_parallelism
grade_smoothing_window
score_total_weight
score_grid_weight
score_grade_weight
score_high_school_total_weight
score_high_school_grade_weight
density_low_factor
density_medium_factor
density_medium_high_factor
density_high_factor
density_low_first_child_factor
density_low_second_child_factor
density_low_third_child_factor
density_low_fourth_child_factor
density_medium_first_child_factor
density_medium_second_child_factor
density_medium_third_child_factor
density_medium_fourth_child_factor
density_medium_high_first_child_factor
density_medium_high_second_child_factor
density_medium_high_third_child_factor
density_medium_high_fourth_child_factor
density_high_first_child_factor
density_high_second_child_factor
density_high_third_child_factor
density_high_fourth_child_factor
anchor_year_weight
year_weight_slope
year_weight_cap
```

Validation behavior:

```text
1. Clamp the requested start/end years to available construction/reference ranges.
2. If actual grid and grade data exists for the start year, anchor the start year to that actual distribution.
3. Otherwise choose the latest prior reference year as the baseline and back-play from it.
   If there is no prior reference year, play homes forward from construction history only.
4. Exclude Lammersville, Inter-Districts, and MHESD from simulation and validation scoring.
5. Remove their estimated grade shares from district grade reference data before grade comparison.
6. Initialize homes from `homes.csv`; back-play pre-window construction to the baseline year.
7. Generate synthetic played-back homes for reference grids that have students but no listed homes.
8. Reconcile baseline children to actual adjusted grade distribution, excluding TK.
9. Seed the kindergarten pipeline from reference data instead of using TK as a cohort source.
10. Add actual homes built during the validation window.
   Each newly built home is assigned to either the same school year or the next school year using `same_school_year_probability`.
11. Simulate ownership changes, density-adjusted move-in families, density-adjusted new children, student exits, and grade progression.
12. Average results across runs.
13. Compare modeled output to grid totals, grade totals, grade-by-grade accuracy, high-school total, and high-school grade accuracy.
```

Validation defaults currently end at 2024. The 2025 reference data is treated as too recent/incomplete for calibration unless it is explicitly included in a manual experiment.

The Blazor tool is available at:

```text
/
/simulation
```

It supports direct parameter tweaking, named parameter presets, selectable browser/server execution, graph/table tools, simulation-info diagnostics, and bounded parameter search.

### Run Count Guidance

Named presets use `30` runs by default. The startup preset is selected by `"IsDefault": true` in `data/simulation-presets.json`, not by a magic preset name. The current default is `2016-2025 balanced`; the former hand-tuned baseline is retained as `Legacy`.

A May 2026 stability check compared the `2016-2025 balanced` preset over `2016-2025` across seeds `2026`, `3026`, and `4026`.

The test showed that:

- `5` and `10` runs are useful only for very rough interaction; high-school totals moved too much between seeds.
- `20` runs was already close for broad district/grid/grade readings.
- `30` runs gave a good interactive balance, with score and grade/grid metrics close to higher-run results.
- `50` runs did not materially improve normal UI reading over `30`.
- `100` to `200` runs are still preferred for final reporting, close parameter comparisons, or optimizer confirmation.

The observed summary was:

| Runs | Combined score | Total MAE | Grid/year MAE | Grade/year MAE | HS total MAE | HS grade MAE |
|---:|---:|---:|---:|---:|---:|---:|
| 5 | 5.44 +/- 0.10 | 132.7 +/- 5.2 | 80.7 +/- 2.3 | 22.2 +/- 0.2 | 46.3 +/- 4.0 | 16.6 +/- 1.8 |
| 10 | 5.48 +/- 0.12 | 138.3 +/- 5.2 | 80.2 +/- 1.4 | 22.1 +/- 0.8 | 46.3 +/- 4.5 | 17.2 +/- 1.8 |
| 20 | 5.42 +/- 0.03 | 137.0 +/- 0.8 | 79.8 +/- 1.0 | 21.9 +/- 0.4 | 44.7 +/- 1.2 | 16.8 +/- 0.7 |
| 30 | 5.37 +/- 0.04 | 138.7 +/- 1.2 | 79.3 +/- 0.6 | 21.8 +/- 0.4 | 42.0 +/- 2.2 | 16.7 +/- 0.5 |
| 50 | 5.39 +/- 0.07 | 139.0 +/- 1.4 | 79.3 +/- 0.7 | 21.9 +/- 0.4 | 42.3 +/- 2.6 | 17.0 +/- 0.4 |
| 100 | 5.41 +/- 0.04 | 138.7 +/- 0.5 | 79.2 +/- 0.4 | 22.0 +/- 0.2 | 42.0 +/- 1.6 | 16.9 +/- 0.5 |
| 200 | 5.41 +/- 0.03 | 138.7 +/- 0.9 | 79.5 +/- 0.2 | 22.0 +/- 0.2 | 41.7 +/- 0.5 | 16.8 +/- 0.4 |

The command-line wrapper is available in:

```text
src/SchoolGrowth.Cli
```

Example validation run:

```text
dotnet run --project src/SchoolGrowth.Cli -- validate --start 2020 --end 2024 --runs 30
```

Example parameter search:

```text
dotnet run --project src/SchoolGrowth.Cli -- search --start 2020 --end 2024 --search-runs 500 --turnovers 0.02,0.04,0.06 --zero-children 0.03,0.05,0.08 --one-children 0.25,0.30,0.35 --two-children 0.55,0.60,0.65 --three-children 0.03,0.05,0.08 --four-children 0,0.01 --exits 0.005,0.015,0.03
```

Example fuller parameter search:

```text
dotnet run --project src/SchoolGrowth.Cli -- search --start 2017 --end 2024 --search-runs 50 --turnovers 0.04,0.05 --zero-children 0.03,0.05,0.08 --one-children 0.25,0.30,0.35 --two-children 0.55,0.60,0.65 --three-children 0.03,0.05,0.08 --four-children 0,0.01 --first-births 0.02,0.04,0.06 --second-births 0.02,0.03,0.04 --third-births 0.001,0.002,0.004 --fourth-births 0,0.001 --exits 0,0.005,0.015
```

Optional candidate CSV output:

```text
dotnet run --project src/SchoolGrowth.Cli -- search --out artifacts/search-results/simulation-candidates.csv
```

Example 50-year household lifecycle run:

```text
dotnet run --project src/SchoolGrowth.Cli -- lifecycle --years 50 --runs 100 --homes 10000 --turnover 0.05
```

The lifecycle output includes two family longevity tables. These count household episodes, not physical homes, so total family episodes can exceed the number of homes.

```text
completed_family_longevity    -> households that ended through turnover
active_at_end_family_longevity -> households still present when the lifecycle window ended
```

Keeping these separate avoids mixing a true completed tenure with a household that is still active at the observation boundary.

The lifecycle output also reports realized turnover:

```text
realized_turnover_rate = turnover_events / active_home_years
```

This can differ from `ownership_change_probability`, because the configured probability is modified by the tenure curve.

## School-Year Timing For New Homes

Construction year is not always the school year when a household appears in enrollment.

If a home is completed late in the calendar year, the students may not appear until the next August count. The implemented model now handles this with:

```text
same_school_year_probability
```

For each newly built home:

```text
if random() < same_school_year_probability:
    active_school_year = built_year
else:
    active_school_year = built_year + 1
```

A starting value of `0.50` means half of newly built homes affect the same school year and half affect the next school year.

The home exists in the simulation before its active school year, but it does not advance household events or contribute students until it becomes active.

## Year-By-Year Grade Accuracy

District total accuracy is not enough. A model can match total enrollment while putting too many students in one grade and too few in another.

Validation separates district total, per-grid, per-grade, high-school total, and high-school grade accuracy. Grade accuracy can be smoothed across nearby cohort years to reduce sensitivity to construction timing and reference-data timing.

```text
total_mape =
    abs(modeled_total_students - actual_grid_total_students)
    / actual_grid_total_students

grid_year_mape =
    average over grids in that year:
        abs(modeled_grid - actual_grid) / actual_grid

grade_year_mape =
    average over grades in that year:
        abs(modeled_grade - smoothed_adjusted_actual_grade) / smoothed_adjusted_actual_grade

high_school_total_mape =
    abs(modeled_hs_students - adjusted_actual_hs_students)
    / adjusted_actual_hs_students

high_school_grade_mape =
    average over 9th..12th:
        abs(modeled_grade - smoothed_adjusted_actual_grade) / smoothed_adjusted_actual_grade
```

`TK` is excluded from `grade_year_mape` because TK eligibility and enrollment rules changed substantially. TK remains visible in detailed output, but it should not steer calibration.

`grade_total_mape` is still reported as a diagnostic reconciliation check, but it is not part of the optimizer score.

Reference years are also weighted. The anchored start year can receive a lower `anchor_year_weight`, and later years can receive increasing weight:

```text
reference_year_weight =
    anchor_year_weight                     if year == start_year and start year is anchored
    min(year_weight_cap,
        1.0 + (year - start_year) * year_weight_slope)
```

The combined search score uses normalized score weights:

```text
score =
    normalized(score_total_weight)              * total_mape
  + normalized(score_grid_weight)               * grid_year_mape
  + normalized(score_grade_weight)              * grade_year_mape_excluding_TK
  + normalized(score_high_school_total_weight)  * high_school_total_mape
  + normalized(score_high_school_grade_weight)  * high_school_grade_mape
```

The current default weights emphasize grid spread and high-school accuracy while still retaining district-total and grade-level checks.

## Why Use Simulation

The deterministic models approximate the relationship:

```text
homes -> students
```

The household simulation models the underlying process:

```text
homes -> households -> children -> students
```

This can make neighborhood aging emerge naturally from household behavior.

Young homes produce more students because new households often have children or add children soon after moving in. Older homes stabilize because children graduate and new students mostly enter through ownership turnover.

## Simulation Units

### Home

Each simulated home has:

```text
home_id
grid
density
built_year
active_school_year
ownership_start_year
current_year
household
```

### Household

Each household has:

```text
children[]
```

### Child

Each child has:

```text
grade_index
is_special_education
```

Grade is derived from the grade index:

```text
grade = grade_from_index(grade_index)
```

Current mapping:

```text
index -4..-1 -> preschool / not enrolled
index 0      -> TK
index 1      -> K
index 2      -> 1st
...
index 13     -> 12th
index 14+    -> post-school / not enrolled
index 200    -> post-school child generated at move-in
```

## Annual Events

Each simulation year, each home processes events.

### Ownership Change

Each home has a probability that ownership changes:

```text
P(ownership_change | home_age, density)
```

In this model, ownership change is broader than a recorded home sale. It represents an effective household refresh event that resets the simulated family profile. This includes owner sale/new buyer, renter household change, and next-generation family formation inside an existing household.

Because of that, `ownership_change_probability` can be higher than a normal real-estate sale turnover rate. Interpret it as an effective household turnover probability, not strictly as home sales.

The implemented version treats `ownership_change_probability` as the target annual turnover rate for established households, then reduces turnover in the first years after move-in and forces turnover after 50 years:

```text
ownership_age = current_year - ownership_start_year

if ownership_age >= 50:
    P(ownership_change) = 1.00
else:
    P(ownership_change) =
        ownership_change_probability
      * early_ownership_multiplier[ownership_age]
```

Current early-ownership multipliers:

```text
year 0 = 0.15
year 1 = 0.35
year 2 = 0.60
year 3 = 0.80
year 4-25 = 1.00
year 26-30 = 1.25
year 31-35 = 1.60
year 36-40 = 2.10
year 41-45 = 3.00
year 46 = 4.00
year 47 = 5.50
year 48 = 8.00
year 49 = 12.00
year 50+ = forced turnover
```

When ownership changes:

1. The old household leaves.
2. A new household is generated.
3. Children already in the household are generated from move-in probabilities.

### Initial Children At Move-In

When a household moves in, it may already have children.

Example child count distribution:

```text
P(0 children)  = 0.05
P(1 child)     = 0.30
P(2 children)  = 0.60
P(3 children)  = 0.05
P(4 children) = 0.00
```

For each child, assign age using an initial child-age distribution:

```text
P(child age 0-4)
P(child age 5-10)
P(child age 11-13)
P(child age 14-17)
P(child age 18+ / post-school)
```

This is important because many families already have children when they move into a new home.

The current implemented move-in age distribution uses band weights:

```text
move_in_preschool_weight   -> pre-school, not counted yet, ages into TK later
move_in_tk_k_weight        -> TK/K
move_in_elementary_weight  -> grades 1-5
move_in_middle_weight      -> grades 6-8
move_in_high_weight        -> grades 9-12
move_in_postschool_weight  -> older child, counts toward family size but not enrollment
```

The default move-in age weights are initialized from cohort additions in the reference data. Positive additions to grades 1-12 imply roughly 58% elementary, 27% middle, and 16% high school among school-age move-ins. The pre-school through 5th-grade bands are balanced so each age/grade slot has the same relative weight:

```text
move_in_preschool_weight   = 0.464  # 4 slots * 0.116
move_in_tk_k_weight        = 0.232  # 2 slots * 0.116
move_in_elementary_weight  = 0.580  # 5 slots * 0.116
move_in_middle_weight      = 0.270
move_in_high_weight        = 0.160
move_in_postschool_weight  = 0.030
```

This matters for calibration. The move-in child-count distribution represents children in the household, not only enrolled students. A household can therefore move in with two children while only one is currently enrolled in the district.

The current model does not distinguish a vacant home from an occupied household with zero children. For current calibration this is acceptable because long-term vacancy is expected to be small, and ordinary vacancy is absorbed into the fitted move-in and birth/new-child parameters. Newly constructed homes also have a separate timing proxy through `same_school_year_probability`, which can cover delayed occupancy or delayed enrollment in the first school year.

Future versions may add vacancy as a separate home state:

```text
home -> vacant
home -> occupied household with 0 children
home -> occupied household with children
```

That would allow student generation to be reported per dwelling unit and per occupied dwelling unit separately, and would avoid treating "no family in the home" as the same thing as "family with no children." It would also slightly change calibrated move-in child-count distributions and birth/new-child probabilities, because those would be conditional on occupancy rather than all homes.

The simulation core consumes direct density profiles:

```text
move_in_child_count_distribution[density] =
    P(0 children), P(1 child), P(2 children), P(3 children), P(4 children)

birth_rate[density] =
    P(1st child/year), P(2nd child/year), P(3rd child/year), P(4th+ child/year)
```

The current UI and optimizer still expose baseline shares plus density factors because that is convenient for broad calibration. Before a simulation run starts, those settings are converted into the direct density profiles above. Move-in child-count shares are adjusted by density before they are normalized:

```text
RL:
  1 child = 1.00x
  2 child = 1.00x
  3 child = 1.00x
  4 child = 1.00x

RM:
  1 child = 1.00x
  2 child = 1.00x
  3 child = 1.00x
  4 child = 1.00x

RMH:
  1 child = 0.95x
  2 child = 0.70x
  3 child = 0.20x
  4 child = 0.05x

RH:
  1 child = 0.90x
  2 child = 0.50x
  3 child = 0.05x
  4 child = 0.00x
```

The zero-child share is not directly reduced. As larger-family shares are reduced and the distribution is normalized, the zero-child share rises naturally in denser zoning.

Special education is now modeled as an attribute on each child:

```text
is_special_education = random() < special_education_probability
```

The child keeps an underlying age/grade index for aging and graduation. While the child is school-aged and enrolled, the reporting bucket is `Sp. Ed.` instead of the underlying grade. Newborns, pre-school move-in children, and current students can all receive the attribute. A starting value near `1.0%` to `1.2%` matches the rough district share in the available grade data.

### Additional Children After Move-In

Each year after move-in, the household may add another child. The implemented version uses direct annual probabilities by next child number:

```text
next_child_number = current_non_special_child_count + 1

P(next_child) =
    annual_first_new_child_probability        if next_child_number == 1
    annual_second_new_child_probability       if next_child_number == 2
    annual_third_new_child_probability        if next_child_number == 3
    annual_fourth_plus_new_child_probability  if next_child_number >= 4
```

With default values:

```text
P(1st new child)  = 4.50%
P(2nd new child)  = 3.30%
P(3rd new child)  = 0.34%
P(4th+ new child) = 0.00%
```

Birth/new-child probabilities are also adjusted by density:

```text
RL/RM:
  1st = 1.00x
  2nd = 1.00x
  3rd = 1.00x
  4th+ = 1.00x

RMH:
  1st = 0.95x
  2nd = 0.65x
  3rd = 0.15x
  4th+ = 0.05x

RH:
  1st = 0.85x
  2nd = 0.45x
  3rd = 0.05x
  4th+ = 0.00x
```

### Student Exit Without Home Change

A child may stop attending district schools even if the household does not move.

Each child each year:

```text
P(exit_district_school | child_age, grade)
```

Implemented exit probabilities are split by bucket:

```text
P(exit_district_school) =
    special_exit_probability      if special education
    tk8_exit_probability          if TK-8
    high_school_exit_probability  if 9-12
    student_exit_probability      otherwise
```

Current defaults are zero because the observed retention signal is very high. There is no student re-entry model.

## Simulation Loop

For one simulation run:

```text
initialize homes and households

for year in start_year..end_year:
    for each home:
        maybe ownership changes

        if ownership changed:
            remove old household
            generate new household
            generate children already present at move-in

        maybe add new child

        for each child:
            age child
            maybe child exits district school
            remove child from school counts after 12th grade

    aggregate students by:
        grid
        grade
        simulation-info buckets
```

Run many simulations:

```text
for run in 1..1000:
    simulate future
```

Then summarize:

```text
mean enrollment
mean homes/students/family profiles/turnover diagnostics
validation errors against reference data
```

## Backward Tracking And Initialization

Initialization uses a hybrid of home back-play and reference-data anchoring.

If the requested start year has actual grid and grade data, that start year is treated as an anchor. The model still initializes households and homes, but the start-year validation comparison receives `anchor_year_weight`, which is usually lower because the model is deliberately reconciled to that year.

For an anchored start, the observed enrolled K-12 population is known. It does not matter much whether those known students originally moved in, were born in-district, or arrived through earlier turnover; reconciliation places the known grade/grid counts into available homes. The hidden household context remains approximate:

```text
pre-school children who will enter school later
post-school children who affect household child count but not enrollment
sibling grouping inside homes
household / ownership age and turnover state
```

This means anchored forecasts start from a solid enrolled-student count, but the future kindergarten pipeline and household composition are still model assumptions.

If the requested start year does not have actual reference data, the model uses the latest prior reference year as a baseline and simulates forward from there. If there is no earlier reference year, the model must not use future reference data. It plays every listed home forward from its construction year, including ownership changes, move-in households, births/new children, exits, and aging. This makes a direct pre-reference start, such as 2015 when the first reference year is 2016, comparable to running the full homes-only history from 2003 through 2015.

Before the validation window, homes from `homes.csv` are added in their construction years and advanced to the baseline year. That lets older baseline homes already have turnover episodes and younger households. If a reference grid has students but no listed homes, synthetic homes are created and played back so the model has homes to hold the reference children.

Baseline children are reconciled to the adjusted actual grade distribution, excluding TK. The kindergarten pipeline is seeded from reference data so missing TK history does not create artificial cohort holes.

Future improvement: anchored starts could infer richer household histories for the homes that receive observed students. That would mean allocating observed students into plausible sibling groups, then inferring likely younger preschool siblings, older post-school siblings, and household tenure. This is intentionally not implemented yet because it is a separate inference layer and must preserve the exact observed grade/grid counts.

## Calibration

The model tunes probability parameters so simulated output matches historical reference data.

Parameters:

```text
ownership_change_probability
initial_child_count_distribution
initial_child_age_distribution
birth_probability_by_child_number
student_exit_probability_by_bucket
density_group_multiplier
density_child_number_multiplier
grade_smoothing_window
reference_year_weights
```

Calibration objective:

```text
error =
    w_total    * district_total_error
  + w_grid     * grid_year_error
  + w_grade    * smoothed_grade_year_error
  + w_hs_total * high_school_total_error
  + w_hs_grade * high_school_grade_error
```

Default score weights:

```text
w_total    = 1.00
w_grid     = 6.00
w_grade    = 1.00
w_hs_total = 2.00
w_hs_grade = 1.00
```

Because the model is random, calibration should compare reference data to expected values across many runs:

```text
modeled_value = average(simulation_result[run])
```

## Density Effects

Density affects both move-in child count and annual new-child probability.

The model applies two layers:

```text
effective_density_factor =
    density_group_factor[density]
  * built_in_density_child_profile[density, child_number]
  * tunable_density_child_factor[density, child_number]
```

For move-in child count, the zero-child share is not directly multiplied. The 1, 2, 3, and 4-child shares are adjusted, then all shares are normalized. This means denser housing can naturally produce a higher normalized zero-child share when larger-family shares are reduced.

For annual new-child probability, the same density idea is applied to the next child number. The tunable child-number factor exists for 1st, 2nd, 3rd, and 4th+ children. The final direct per-density birth rates are clamped to probability range before simulation uses them.

## Outputs

Validation produces year-by-year comparison output:

```text
comparisons[].actualGridTotals
comparisons[].modeledGridTotals
comparisons[].actualGrades
comparisons[].adjustedActualGrades
comparisons[].modeledGrades
comparisons[].error metrics
```

It also reports synthetic baseline home diagnostics:

```text
syntheticHomeDiagnostics[].grid
syntheticHomeDiagnostics[].baselineYear
syntheticHomeDiagnostics[].adjustedBaselineStudents
syntheticHomeDiagnostics[].listedHomesThroughBaseline
syntheticHomeDiagnostics[].expectedStudentsPerHome
syntheticHomeDiagnostics[].syntheticHomes
```

These diagnostics identify grids where reference data has students but the housing file has no listed homes through the baseline year. The synthetic homes are a baseline initialization repair, not forecast construction.

It also produces simulation-info diagnostics:

```text
simulationInfo[].totalHomes
simulationInfo[].homes by density
simulationInfo[].K12Students / TKStudents / K8Students / HighSchoolStudents
simulationInfo[].studentsPerHome
simulationInfo[].lowMedium generation
simulationInfo[].mediumHighHigh generation
simulationInfo[].family child-count shares
simulationInfo[].turnoverEvents
simulationInfo[].turnoverFactor
simulationInfo[].averageLongevityBeforeTurnover
```

Lifecycle diagnostics produce:

```text
initialHouseholds
turnoverHouseholds
endingHouseholds
realizedTurnoverRate
turnoverByYear
completedFamilyLongevity
activeAtEndFamilyLongevity
years[].studentsPerHome
```

## Expected Benefits

- Home aging emerges from household behavior.
- Ownership turnover can be modeled directly.
- Existing children at move-in can be represented.
- Student exits without home movement can be represented.
- Multi-run averages reduce random noise and leave room for future uncertainty bands.

## Risks

- More complex to calibrate.
- Random simulations can be slower.
- Many probability parameters may be weakly identified by current data.
- Requires careful validation to avoid plausible but wrong household assumptions.

## Relationship To Model 2.0

Model 2.0 is design reference only. Avoid expanding it as a separate product path unless the work is explicitly about extracting a specific idea into Household Simulation.

Current roadmap:

```text
1. Use Household Simulation as the primary model.
2. Keep Deterministic Projection only as old-behavior reference unless it is retired.
3. Port useful deterministic/model-2.0 ideas into Household Simulation.
4. Keep validation and scoring centered on historical reference data.
```
