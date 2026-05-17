# Monte Carlo Household Simulation Model

The Monte Carlo model is a proposed simulation model that treats each home and household as an individual simulated unit.

Instead of directly applying a fixed student-yield curve, it simulates ownership changes, household composition, children aging, school participation, and children leaving the district.

The final enrollment forecast is the average of many simulated futures.

## First Implemented Version

The first implemented version is intentionally higher level than the full future design.

It uses home agents with simulated children, but keeps child probabilities the same across all densities. Density-specific child behavior can be added later after the basic probability calibration is useful.

Children now track a grade index rather than only a current grade name. That allows the simulator to create newborn/pre-school children who enter TK several years later, and to keep post-school children in the household count without adding them to enrollment.

Current implemented parameters:

```text
runs
seed
ownership_change_probability
move_in_zero_child_share
move_in_one_child_share
move_in_two_child_share
move_in_three_child_share
move_in_four_plus_child_share
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
```

Current validation behavior:

```text
1. Start from a historical baseline year.
2. Initialize homes and seed baseline students from actual grid totals.
3. Allocate baseline grade mix using fair district grade shares.
4. Use Lammersville and Inter-Districts as fair-share TK-12 grids.
5. Use MHESD as high-school-only, grades 9-12.
6. Add actual homes built during the validation window.
   Each newly built home is assigned to either the same school year or the next school year using `same_school_year_probability`.
7. Simulate ownership changes, child-number-adjusted new births, student exits, and grade progression.
8. Average results across runs.
9. Compare modeled output to grid totals, grade totals, and grade-by-grade accuracy within each year.
```

Validation defaults currently end at 2024. The 2025 reference data is treated as too recent/incomplete for calibration unless it is explicitly included in a manual experiment.

The Blazor tool is available at:

```text
/monte-carlo
```

It supports direct parameter tweaking and a bounded grid search for best-fit parameters. The page now uses a smaller default run count and shows status text while validation/search work is running.

The command-line wrapper is available in:

```text
src/SchoolGrowth.Cli
```

Example validation run:

```text
dotnet run --project src/SchoolGrowth.Cli -- validate --start 2020 --end 2024 --runs 1000
```

Example parameter search:

```text
dotnet run --project src/SchoolGrowth.Cli -- search --start 2020 --end 2024 --search-runs 500 --turnovers 0.02,0.04,0.06 --zero-children 0.03,0.05,0.08 --one-children 0.25,0.30,0.35 --two-children 0.55,0.60,0.65 --three-children 0.03,0.05,0.08 --four-plus-children 0,0.01 --exits 0.005,0.015,0.03
```

Example fuller parameter search:

```text
dotnet run --project src/SchoolGrowth.Cli -- search --start 2017 --end 2024 --search-runs 50 --turnovers 0.04,0.05 --zero-children 0.03,0.05,0.08 --one-children 0.25,0.30,0.35 --two-children 0.55,0.60,0.65 --three-children 0.03,0.05,0.08 --four-plus-children 0,0.01 --first-births 0.02,0.04,0.06 --second-births 0.02,0.03,0.04 --third-births 0.001,0.002,0.004 --fourth-births 0,0.001 --exits 0,0.005,0.015
```

Optional candidate CSV output:

```text
dotnet run --project src/SchoolGrowth.Cli -- search --out monte-carlo-candidates.csv
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

Validation therefore separates district total, per-grid, and per-grade accuracy. It does not optimize both grid total and grade total, because those mostly measure the same district-wide count.

```text
total_mape =
    abs(modeled_total_students - actual_grid_total_students)
    / actual_grid_total_students

grid_year_mape =
    average over grids in that year:
        abs(modeled_grid - actual_grid) / actual_grid

grade_year_mape =
    average over grades in that year:
        abs(modeled_grade - actual_grade) / actual_grade
```

`TK` is excluded from `grade_year_mape` because TK eligibility and enrollment rules changed substantially. TK remains visible in detailed output, but it should not steer calibration.

`grade_total_mape` is still reported as a diagnostic reconciliation check, but it is not part of the optimizer score.

The combined search score is:

```text
score =
    0.30 * total_mape
  + 0.20 * grid_year_mape
  + 0.50 * grade_year_mape_excluding_TK
```

This makes the optimizer prefer parameters that match each grade in each year and each neighborhood/grid, without double-counting district total accuracy.

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
ownership_start_year
household
```

### Household

Each household has:

```text
children[]
```

Optional future attributes:

```text
household_type
owner_age
years_in_home
```

### Child

Each child has:

```text
birth_year
in_district_school
```

Grade is derived from age and school year:

```text
grade = grade_from_age(year - birth_year)
```

Simplified mapping:

```text
age 4  -> TK
age 5  -> K
age 6  -> 1st
...
age 17 -> 12th
```

## Annual Events

Each simulation year, each home processes events.

### Ownership Change

Each home has a probability that ownership changes:

```text
P(ownership_change | home_age, density)
```

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

Later this can vary by home age:

```text
age 0-3    lower turnover
age 4-10   moderate turnover
age 11-20  higher turnover
age 21+    stable long-term turnover
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
P(4+ children) = 0.00
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

Move-in child-count shares are adjusted by density before they are normalized:

```text
RL/RM:
  1 child = 1.00x
  2 child = 1.00x
  3 child = 1.00x
  4+ child = 1.00x

RMH:
  1 child = 0.95x
  2 child = 0.70x
  3 child = 0.20x
  4+ child = 0.05x

RH:
  1 child = 0.90x
  2 child = 0.50x
  3 child = 0.05x
  4+ child = 0.00x
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
P(1st new child)  = 4.00%
P(2nd new child)  = 2.00%
P(3rd new child)  = 0.80%
P(4th+ new child) = 0.12%
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

Future refinement can add a residence-age factor on top of child-number and density probability:

```text
P(next_child) =
    child_number_probability
  * density_multiplier
  * residence_age_factor
```

### Student Exit Without Home Change

A child may stop attending district schools even if the household does not move.

Each child each year:

```text
P(exit_district_school | child_age, grade)
```

Starting estimate:

```text
P(exit_district_school) = 0.01 to 0.02 per year
```

Later this can vary by grade:

```text
TK/K: possible higher uncertainty
1-8:  stable
9:    possible high-school transition effect
10-12 stable or slightly declining
```

Optional future parameter:

```text
P(reenter_district_school | child_age, years_out)
```

Initial model can set re-entry to zero.

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
        school
```

Run many simulations:

```text
for run in 1..1000:
    simulate future
```

Then summarize:

```text
mean enrollment
median enrollment
p10 enrollment
p90 enrollment
standard deviation
```

## Backward Tracking And Initialization

The hardest part is initializing households before the first known reference year.

There are two possible strategies.

### Warm-Up Simulation

Start from the first construction year, around 2003, and simulate forward.

Pros:

- Natural household history.
- Home age behavior emerges from the beginning.

Cons:

- Requires assumptions for pre-existing homes.
- Early random choices can affect later years.

### Backfit Initial Households

At the first reference year, generate households so simulated grid and grade totals match observed data.

Pros:

- Starts close to actual enrollment.
- Better for short validation windows.

Cons:

- Less natural household history.
- Requires an initialization algorithm.

## Calibration

The model should tune probability parameters so simulated output matches historical reference data.

Parameters:

```text
ownership_change_probability
initial_child_count_distribution
initial_child_age_distribution
birth_probability_by_child_number
birth_probability_by_year_since_move_in
student_exit_probability_by_grade
density_multiplier
```

Calibration objective:

```text
error =
    w_grid   * grid_total_error
  + w_grade  * grade_total_error
  + w_school * school_total_error
  + w_cohort * cohort_error
```

Example starting weights:

```text
w_grid = 0.35
w_grade = 0.35
w_school = 0.10
w_cohort = 0.20
```

Because the model is random, calibration should compare reference data to expected values across many runs:

```text
modeled_value = average(simulation_result[run])
```

## Density Effects

The first raw version can keep probabilities the same for all densities:

```text
density_factor[density] = 1.0
```

Later, density can affect:

```text
initial child count
child age distribution
ownership turnover
birth probability
district-school participation
```

## Outputs

The simulation should produce the same kind of output as the deterministic model:

```text
projection[].gridTotals
projection[].gridGrades
projection[].schoolTotals
projection[].schoolGrades
```

It should also include uncertainty:

```text
mean
p10
p50
p90
standard_deviation
```

## Expected Benefits

- Home aging emerges from household behavior.
- Ownership turnover can be modeled directly.
- Existing children at move-in can be represented.
- Student exits without home movement can be represented.
- Forecast includes uncertainty bands, not only one number.

## Risks

- More complex to calibrate.
- Random simulations can be slower.
- Many probability parameters may be weakly identified by current data.
- Requires careful validation to avoid plausible but wrong household assumptions.

## Relationship To Model 2.0

Model 2.0 is deterministic and easier to tune manually.

The Monte Carlo model is more realistic and explanatory, but harder to calibrate.

A practical roadmap:

```text
1. Keep current model as baseline.
2. Build Model 2.0 deterministic housing cohort model.
3. Use Monte Carlo simulation as an experimental validation and uncertainty model.
4. Compare all models against the same reference data.
```
