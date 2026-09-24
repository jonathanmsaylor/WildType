# WILDTYPE Milestone 2A.1 handoff

## Scope completed in source

This patch implements the disconnected genetics and reproduction-data foundation. It does not create mating behavior, births or offspring in Play Mode.

### Genome and phenotype

New genes and validated ranges:

| Gene | Range | Default |
|---|---:|---:|
| Fertility | 0.5–1.5 | 1.0 |
| Reproduction threshold | 0.5–0.9 of maximum energy | 0.72 |
| Offspring tendency | 0.4–1.6 | 1.0 |
| Lifespan | 240–900 simulated seconds | 600 |

Phenotype effects:

- Fertility increases reproductive potential and shortens future cooldown, but increases passive maintenance drain.
- A lower reproduction threshold permits earlier mating at the natural cost of leaving less survival reserve.
- Offspring tendency increases future reproductive motivation but also adds maintenance cost.
- Longer lifespan delays maturity and adds maintenance cost.

All four genes participate in validation, copying, seeded founder variation, inheritance, mutation and genetic distance. The three authored preset assets and `PrototypeBuilder` now contain explicit values.

### Inheritance and mutation

`GenomeInheritance.CreateChild`:

1. Copies and validates both parents without modifying them.
2. Recombines every numeric and camouflage channel between both parental values.
3. Uses an injected `IRandomSource` rather than global Unity random state.
4. Applies rare major mutation before considering a small mutation, so the two do not stack on one gene.
5. Gives mutation an unbiased positive or negative direction.
6. Clamps the final value through the centralized gene catalog.
7. Returns mutation counts and at most the configured number of important mutation records.

Default settings:

- Small-mutation chance: 15% per gene
- Small-mutation magnitude: at most 3% of that gene's valid span
- Major-mutation chance: 0.8% per gene
- Major-mutation magnitude: 10–24% of that gene's valid span
- Important-record threshold: 8% of valid span
- Important-record cap: 6 per birth result
- Compatibility threshold: normalized distance 0.34

### Distance, lineage and eligibility

- `GeneticDistance` averages per-gene normalized differences, producing a symmetric finite 0–1 result.
- `CreatureId` stores stable identity independently of GameObjects.
- `CreatureLineageRecord` creates founders and children, stores two parent IDs, calculates generation as `max(parent generations) + 1`, chooses the ordinal-lowest founder ID as the deterministic merged root and caps retained mutations at eight.
- `ReproductionEligibility` is pure and nonmutating. It rejects dead, juvenile, unhealthy, under-energized, cooling-down, reserved, identical, incompatible, invalid or population-blocked pairs.

## Files added

- `Assets/WildType/Scripts/Genetics/GenomeGeneCatalog.cs`
- `Assets/WildType/Scripts/Genetics/EvolutionSettings.cs`
- `Assets/WildType/Scripts/Genetics/RandomSource.cs`
- `Assets/WildType/Scripts/Genetics/GenomeInheritance.cs`
- `Assets/WildType/Scripts/Genetics/GeneticDistance.cs`
- `Assets/WildType/Scripts/Creature/CreatureLineageRecord.cs`
- `Assets/WildType/Scripts/Creature/ReproductionEligibility.cs`
- `Assets/WildType/Tests/EditMode/EvolutionFoundationTests.cs`
- Matching Unity `.meta` files for every new source file

## Files modified

- `Assets/WildType/Scripts/Genetics/Genome.cs`
- `Assets/WildType/Editor/PrototypeBuilder.cs`
- All three assets under `Assets/WildType/ScriptableObjects`
- `README.md`
- `VERIFICATION.md`

## Tests staged

The new suite contributes 24 NUnit cases covering:

- New-gene validation, copy isolation and phenotype tradeoffs
- Deterministic two-parent inheritance and parental immutability
- Positive and negative small mutation, major classification and record caps
- Normalized symmetric distance and invalid-value handling
- Founder/child IDs, parents, generation, age and bounded mutations
- Successful eligibility and every current rejection category

Expected Edit Mode total after installation: 44 tests. This is an expectation based on the existing 20-test baseline; it is not a recorded Unity result.

## Required Windows Unity verification

1. Commit or back up the current project.
2. Overlay the patch at the WILDTYPE project root while preserving paths and `.meta` files.
3. Open Unity 6000.4.7f1 and allow compilation.
4. Fix and report any compiler error before continuing.
5. Run the full Edit Mode suite. Expected discovery: 44 tests.
6. Run the existing Play Mode integration suite once.
7. Open `CreatureStage_Prototype` and perform a short smoke test.
8. Confirm one player, twelve AI, movement, food, starvation, pause/restart and reseed remain unchanged.
9. Confirm that no creature reproduces yet.
10. Record actual results in `VERIFICATION.md` and commit the verified patch.

## Milestone 2A.2 boundary

Next connect this foundation to live creatures by adding runtime age, reproductive state, partner queries, an atomic two-creature reservation handshake, parent energy costs/cooldowns, a hard creature cap, child spawning and juvenile growth. Do not add descendant control or the full ancestry inspector until Milestone 2B.
