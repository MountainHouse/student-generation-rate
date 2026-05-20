# School Growth Planning Documentation

This folder documents the enrollment projection model and candidate future model designs.

## Documents

- [Household Simulation Model](household-simulation-model.md) describes the active Household Simulation model, which treats each home and household as a simulated unit.
- [Initial Generation And Reconciliation](initialization-reconciliation.md) describes how homes are played back and how reference-year students are reconciled into same-village households.
- [Current Model](current-model.md) should usually be avoided; read it only when maintaining Deterministic Projection or comparing old behavior.
- [Model 2.0](model-2.0.md) should usually be avoided; read it only when extracting a specific deterministic-model idea to port into Household Simulation.
- [Project Rules and Assumptions](project-rules.md) collects shared domain, data, execution, and validation rules.
- [User Interface Notes](ui.md) describes UI structure and interaction conventions.
- [Deployment](deployment.md) describes hosted, static, AOT, and threaded WebAssembly deployment paths.
- [Project Tasks and Follow-Ups](tasks.md) tracks known investigation and cleanup tasks.

## Model Roadmap

Household Simulation is the primary product path. Avoid expanding Deterministic Projection or Model 2.0 unless the work is specifically about maintaining old behavior or porting a useful idea into Household Simulation.
