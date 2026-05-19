# User Interface Notes

The UI is an analytical planning tool. It should be dense, inspectable, and useful for repeated comparison, not a marketing page.

## Household Simulation Page

The Household Simulation page is organized as a set of tools:

- tools are collapsible, removable, and reorderable
- custom Chart tools can be added more than once, renamed in the panel header, and grouped by the user
- settings are grouped into subpanels
- initially visible tools should focus on model settings and validation summary
- other details can be added through the tool menu

## Tables and Charts

Tables and charts should use the shared patterns:

- reuse existing table/chart blocks and helper components where practical
- avoid duplicating matching UI behavior across pages/tools when a shared component is practical
- if matching behavior exists only as page-local markup/code, consider extracting a shared reusable component before adding another copy
- if extraction is too large for the current change, match nearby behavior and leave a follow-up in `tasks.md`
- tables can sit beside charts when there is enough space
- selectable chart lines use first-column checkboxes and color swatches
- selectable charts default to no selected lines
- if no lines are selected, hide the graph
- table first columns should remain visible during horizontal scroll
- table headers should remain readable when possible
- graph hover should show a vertical guide and point markers
- chart hover values should be integrated into the legend, not shown as separate SVG tooltip boxes
- reserve legend space for the hovered x-value/year so the legend does not jump when hover starts
- repeatable Chart tools should use the same selector model as Validation Summary where practical
- custom Chart panels place the legend to the right of the chart on wide screens and below it on narrow screens
- custom Chart panel titles are editable from the panel header; the displayed title should match normal tool header typography
- when multiple lines are visible, the y-axis scale must use the maximum/minimum across all rendered lines, including pinned lines

## Copy and Controls

- Prefer concise labels over explanatory paragraphs inside the app.
- Use controls that match their task: checkboxes for line visibility, inputs/sliders for numeric parameters, selects for option sets.
- Keep parameter formulas and explanations on validation/model pages where they support tuning.

## Model Settings Organization

Household Simulation settings are grouped roughly as:

- Run Environment
- Planning Window
- Move-In Kids Count
- Move-In Kids Age
- Birth/New Child Rates
- Density Factors
- Exit Probabilities
- Other Settings
- Model Scoring

The exact layout can evolve, but related parameters should remain grouped and easy to compare.
