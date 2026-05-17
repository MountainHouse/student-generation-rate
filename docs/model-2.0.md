# Model 2.0: Deterministic Housing Cohort Model

Model 2.0 is a proposed deterministic replacement for the current aggregate trend model.

The goal is to make the math more explainable by separating three forces:

1. Homes generate students.
2. Students progress through grades.
3. Students may leave or enter between grades through retention factors.

## Design Goal

The current model uses grid trend to explain existing enrollment movement. Model 2.0 should reduce reliance on that trend by making home age, housing density, and grade progression explicit.

Instead of:

```text
grid_total grows by historical grid trend
```

Model 2.0 should say:

```text
students are produced by home cohorts of specific age and density,
then those students progress through grades with retention.
```

## Housing Cohorts

A housing cohort is a group of homes with the same:

```text
grid
density
built_year
```

Each cohort has an age in every projection year:

```text
home_age = year - built_year
```

## Home Age and Student Yield

The model should strongly connect the age of homes to the number and grade distribution of students they produce.

Young neighborhoods usually produce more children because new households often include young families or families that soon have children. Older neighborhoods stabilize as children graduate, households age, and new students mostly arrive through turnover or next-generation ownership changes.

A simple aggregate form:

```text
generated_students[grid, density, built_year, year, grade_group] =
    homes[grid, density, built_year]
    * base_yield[density, grade_group]
    * age_factor[grade_group, home_age]
```

The age curve should stabilize over the long term:

```text
age_factor[grade_group, age >= 15 or 30] = long_term_floor[grade_group]
```

The exact long-term stabilization point should be tunable.

## Grade Groups

Because the available data is limited, grade groups may be more stable than individual grades.

Possible groups:

```text
TK-K
1-5
6-8
9-12
Sp. Ed.
```

A more detailed version can later use individual grades:

```text
TK, K, 1st, ..., 12th, Sp. Ed.
```

## Age Curve By Grade Group

Different grade groups should peak at different home ages.

Example intuition:

```text
TK-K:  peaks earlier
1-5:   peaks soon after neighborhood buildout
6-8:   peaks later
9-12:  peaks later still
```

So the model should use:

```text
age_factor[grade_group, home_age]
```

not only:

```text
age_factor[home_age]
```

This allows a new neighborhood to first create elementary pressure, then middle school pressure, then high school pressure.

## Grade Progression

Students should advance through grades year by year.

Simplified:

```text
students[grid, grade, year] =
    students[grid, previous_grade, year - 1] * retention[grade]
    + new_students_from_homes[grid, grade, year]
```

For now, retention can default to:

```text
retention[grade] = 1.0
```

That means 100 students in one grade become 100 students in the next grade next year.

Later, retention should be tweakable per grade:

```text
retention[K]
retention[1st]
...
retention[12th]
```

This can represent transfers, private school movement, inter-district changes, special programs, and other enrollment shifts.

## Proposed Core Formula

For grid `g`, grade `k`, year `y`:

```text
students[g, k, y] =
    students[g, previous_grade(k), y - 1] * retention[k]
    + housing_generated_students[g, k, y]
    + migration_adjustment[g, k, y]
```

Starting assumptions:

```text
retention[k] = 1.0
migration_adjustment[g, k, y] = 0
```

Housing generation:

```text
housing_generated_students[g, k, y] =
    sum over density d and built year b:
        homes[g, d, b]
        * yield[d, k]
        * age_factor[k, y - b]
```

## Existing Homes and Missing Housing

Some pre-existing homes are not fully represented in `homes.csv`, especially outside-city or older district housing. These should be represented explicitly.

Possible parameter:

```text
pre_existing_homes[grid, density, assumed_built_year_or_age_bucket]
```

Initial assumption discussed:

```text
Lammersville / Inter-Districts / MHESD contain the pre-existing home base.
```

For MHESD, students should contribute only to high school enrollment.

## Calibration

Model 2.0 should choose parameters that best match reference data.

Known reference data:

```text
actual_grid_total[grid, year]
actual_grade_total[grade, year]
actual_school_total[school, year]
homes[grid, density, built_year]
```

Unknown:

```text
actual_grid_grade[grid, grade, year]
```

So the model may infer grid-grade values using district grade shares and school proxy ratios, while still comparing final totals to known grid, grade, and school data.

Possible objective:

```text
error =
    w_grid  * grid_total_error
  + w_grade * grade_total_error
  + w_school * school_total_error
  + w_cohort * cohort_shape_error
```

Example starting weights:

```text
w_grid = 0.40
w_grade = 0.35
w_school = 0.15
w_cohort = 0.10
```

## Parameters To Expose

Model 2.0 should eventually expose:

```text
yield[density, grade_group]
age_factor[grade_group, age_bucket]
long_term_floor[grade_group]
retention[grade]
pre_existing_homes[grid, density, age_bucket]
school_proxy_ratio[grid]
```

## Expected Benefits

- Clearer relationship between homes and students.
- Less reliance on broad grid trend factors.
- Better explanation of neighborhood aging.
- Better grade-level forecasting.
- Easier validation against cohort behavior.

## Risks

- More parameters can overfit limited data.
- True grid-by-grade data is unavailable.
- Pre-existing homes must be estimated.
- The model may need constraints to keep parameters realistic.

