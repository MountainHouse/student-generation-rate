# Initial Generation And Reconciliation

This document describes how Household Simulation creates the initial simulated homes and how it reconciles a reference start year.

## Purpose

The model needs a realistic household state before it can forecast future enrollment. That state includes both observed students and hidden household context:

- students by village and grade
- preschool children who may enter school later
- post-school children who still count as children in family profiles
- sibling grouping inside homes
- household tenure and future turnover risk

Reference data can tell us enrolled students, but it does not tell us all hidden household context. Initialization therefore combines historical home playback with limited reconciliation to known reference counts.

## Initial Home Playback

For every simulation run:

1. Add homes from `homes.csv` in their construction year.
2. Generate a move-in household for each active home using the configured child-count and child-age distributions.
3. Advance each home year by year through ownership changes, child aging, births/new children, and student exits.
4. Stop at the baseline year.

If the requested start year has reference grid and grade data, that year is the baseline. If it does not, the latest prior reference year is used. If there is no prior reference year, the model plays homes forward from construction history only and does not use future reference data.

## Reference-Year Reconciliation

When baseline reference data exists, reconciliation is same-village only. Students from one village are not moved into another village to make the counts fit.

TK is ignored for reconciliation and scoring because TK policy and eligibility changed enough to make it unreliable as a stable cohort signal. Preschool children are also preserved by reconciliation because they are the future K pipeline.

The current reconciliation target is:

```text
K, 1st, 2nd, ..., 12th
```

Special education is still handled as a separate reporting bucket and may need a richer future design because it is an attribute, not a true age/grade.

## Safer Reconciliation Order

For each village, the model should use the least disruptive edits first.

### 1. Retarget Existing School-Age Children

If one grade has too many students and another grade has too few, first change the grade index of existing school-age children in the same village.

Retargeting should choose the closest possible source grade:

```text
target 6th:
  prefer 5th or 7th surplus
  then 4th or 8th
  then 3rd or 9th
  ...
```

This preserves:

- number of children in each home
- family child-count distribution
- same-village enrollment totals

It changes only the child age/grade.

### 2. Convert Post-School Children

If a grade is still short, convert older-than-school children in the same village into the needed school grade.

This preserves family child count, but it changes a hidden older child into an enrolled student. It is less disruptive than adding a new child to a household.

### 3. Add To Smallest Families

If the village is still short after grade retargeting and post-school conversion, add children to active homes with the fewest children first:

```text
0-child homes
1-child homes
2-child homes
3-child homes
4+ child homes last
```

This is the first step that changes family size.

### 4. Remove From Largest Families

If a grade still has too many students after retargeting, remove matching students from active homes with the most children first:

```text
4+ child homes
3-child homes
2-child homes
1-child homes last
```

This reduces the chance that reconciliation creates unrealistically large families.

## What Reconciliation Must Not Do

Reconciliation must not:

- move students between villages
- use TK as a target or source
- spend preschool children to fix K or early-grade counts in the current implementation
- use future reference data in normal forecast mode

## Future Preschool Back-Inference

A future validation-only inference layer may use future reference data to infer hidden baseline preschool children, but it needs careful accounting.

For example:

```text
future K observed
  - K students expected from homes built after baseline
  - K students expected from move-ins after baseline
  - K students expected from births/new children after baseline
  = inferred children already present before baseline
```

That inferred population could then be distributed backward:

```text
K at +1 -> baseline TK/pre-K age
K at +2 -> younger preschool
K at +3/+4/+5 -> toddler/newborn range
```

This should remain optional and clearly labeled as reference-assisted validation. It must not leak future data into normal forecasts.

Any future preschool adjustment should also be capped so the model does not create a cliff in future K cohorts. The remaining preschool pipeline should stay smooth and plausible across years.
