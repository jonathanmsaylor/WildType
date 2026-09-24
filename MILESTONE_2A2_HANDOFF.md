# WILDTYPE 2A.2 — playable generations

Only the Unity project at `C:\Users\jsayl\Desktop\UnityProjects\WILDTYPE` was changed. The earlier Godot project and separate RunAudit product were not modified. The accidental, now-missing RunAudit integration was removed from the Creature scene at the user's request.

## What is playable

- Adult courtship with meaningful age, energy, health, distance, compatibility and cooldown checks. AI seeks eligible partners in its existing decision flow; player mating is an explicit choice.
- Two-parent births through the existing 2A.1 inheritance/mutation implementation. Every child gets an independent validated genome and stable parent IDs. Mutations remain probabilistic and can move traits in either direction.
- Smaller, slower juveniles grow into their inherited adult form, eat, lose resources, can starve, mature, reproduce and die at their inherited lifespan. Visual growth, collision size and camera focus agree.
- Family journal with generation, parents, living direct offspring, inherited-trait comparison and notable mutation. Six-row paginated selection covers all living descendants.
- Take control of a living child or later descendant without restoring resources. The former player becomes AI if alive. Death offers descendants or explicit restart/reseed; there is no silent respawn.
- Bounded population and retained ancestry. Full capacity, interrupted/dead/destroyed partners and paused restart/reseed are handled without deferred stale births.

## Controls

`M` / gamepad west (X / Square): court an eligible partner within 3.5 m. Stay close for two seconds.

`F` / gamepad north (Y / Triangle): family journal. Click a living descendant to continue as it.

Existing controls remain: WASD / left stick movement, Shift / L3 sprint, E / south button eating, mouse / right stick orbit, wheel zoom, Escape / Start pause. Journal buttons provide restart and reseed even after death.

## Changed files

- Added `Scripts/Core/GenerationLoop.cs`: clock, reservations, validated births, capacity and partner queries.
- Added `Scripts/Creature/CreatureLife.cs` and `LineageArchive.cs`: age/growth/lifespan and bounded stable ancestry.
- Updated `StageSession`, `CreatureAgent`, `CreatureMotor`, `CreatureVitals`, `ReproductionEligibility`, `HerbivoreBrain`, `PlayerInputBridge`, `OrbitCamera`, and `StageHud`.
- Added `GenerationRulesTests` and `GenerationTests`; isolated the original `EcosystemTests` from generation timing; extended the Play Mode assembly references for UI testing.
- Removed only the orphaned RunAudit scene object from `CreatureStage_Prototype.unity`; retained the existing light/preset changes. Removed the empty RunAudit asset folder and orphan folder metadata from this project.
- Updated `.gitignore`, `README.md`, `VERIFICATION.md`, and this handoff. All new Unity assets include `.meta` files.

Paths above are under `Assets/WildType` unless otherwise stated. No new packages, paid content, external assets or services were added to gameplay.

## Limits and verification

Hard limits: 24 living creatures, 32 creature objects including corpses, 72 food slots, 512 particles, 512 lineage records per run. Reservations consume available birth capacity. Reaching the lineage cap stops births with a message rather than breaking parent references.

See `VERIFICATION.md` for executed test results and native Editor observations. Automated virtual-input checks are not human playtesting. Physical-controller feel, long-term ecosystem balance, procedural foot sliding and full-session human play remain judgment tasks. There is no save/load or lineage persistence across restarts; no gestation, parental care or close-kin mating restriction.

## Git

Pre-change checkpoint: `dfe62b9` on `milestone-2a2-playable-generations`, following original local `main` commit `d7bb0af`. GitHub's separate initial `main` history at `ba5e99d` contains only its placeholder `README`; it is preserved when reconciling histories. No force-push or project-file discard is used. The final handoff reports the resulting pushed commit.
