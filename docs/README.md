# School Growth Model Documentation

This folder documents the enrollment projection model and candidate future model designs.

## Documents

- [Current Model](current-model.md) describes the model implemented in the C# core library today.
- [Model 2.0](model-2.0.md) describes a deterministic next-generation model based on housing cohorts, home age, grade progression, and tunable retention.
- [Monte Carlo Household Model](monte-carlo-model.md) describes a future simulation model that treats each home and household as a simulated unit.

## Model Roadmap

The current implementation is useful as a fast browser-based projection and validation tool, but it mixes several real-world effects into aggregate trend factors. Model 2.0 should separate those effects so the math is easier to inspect and tune. The Monte Carlo model goes one step further by simulating household events directly and deriving enrollment from many possible futures.

