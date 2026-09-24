# WILDTYPE — Creature Stage vertical slice

A Unity implementation, independent of the earlier Godot project. **Milestone 2A.2: playable generations.** Explore, eat, reproduce and continue through living descendants in a bounded, roughly 250 m-wide Creature-stage ecosystem. Start with twelve autonomous herbivores; total living population is capped at 24.

## Editor and opening

Unity **6.4 / 6000.4.7f1**, Windows 64-bit, Universal Render Pipeline **17.4.0**. Open this folder in Unity Hub, then open **Assets/WildType/Scenes/CreatureStage_Prototype.unity** and press Play. The **WildType > Open prototype scene** menu also opens it.

Unity Personal and official free packages only: Input System 1.19.0, URP 17.4.0, Unity UI/TextMeshPro 2.0.0, and Test Framework 1.6.0. No paid assets, trials, subscriptions, metered services, external art/audio/font downloads, or third-party packages were added. TMP's official bundled Liberation Sans resources live under Assets/ThirdParty/UnityTextMeshPro, including their license; no custom vendor code edits were made.

Configured for Force Text serialization, Visible Meta Files, Linear color, 0.02 s physics, and Active Input Handling **Both**. Gameplay uses the new Input System. The project repository is [jonathanmsaylor/WildType](https://github.com/jonathanmsaylor/WildType), using `main`. The original GitHub placeholder file `README` is preserved alongside this detailed `README.md`. Generated caches, logs, IDE state and builds are excluded from Git.

## Controls

| Action | Keyboard/mouse | Gamepad |
|---|---|---|
| Move | WASD | Left stick |
| Orbit camera | Mouse | Right stick |
| Sprint | Left Shift | Left-stick press (L3) |
| Eat nearby food | E | South button (A / Cross) |
| Mate with eligible nearby adult | M | West button (X / Square) |
| Family journal / descendant selection | F | North button (Y / Triangle) |
| Zoom | Mouse wheel | — |
| Pause/resume | Escape | Start |
| Menu navigation | Mouse / keyboard | D-pad / stick and south button |

Focus the Game view for input. Focus loss pauses the simulation. Food interaction is proximity-based; nothing requires hovering. There is no jump in this slice. Pause includes Resume, Restart Prototype, Reseed Ecosystem, and Quit. Quit exits Play Mode in the editor and exits the application in a build.

## Implemented

- Three authored genome presets: teal Meadow Grazer, large amber Bulwark, and small long-legged Violet Strider. Each spawn receives its own validated runtime copy.
- Gene-driven maximum energy, movement, stride, turning, vision, metabolism, stamina, efficiency and color, with explicit opposing costs.
- Third-person acceleration/braking, camera-relative control, gravity, slope alignment, sprint/stamina recovery lock, sphere-swept camera collision and zoom.
- Procedural multi-part creature visuals, jointed legs, feet, eyes, tail, markings, idle/gait motion and feeding nod.
- Meadow, dry flats and woodland, rolling terrain, rocky perimeter hills, sun, fog, shadows and batched static vegetation.
- Fixed 72 food slots. Eating removes berries; 18–33 second timers regenerate them in place. No per-food coroutine or unbounded spawning.
- Shared player/AI energy, stamina, health, starvation and death rules. Starvation costs 4 health per second. AI corpses are removed after four seconds; player death opens descendant choices and restart/reseed options.
- Twelve herbivores with staggered 0.35–0.5 s decisions, vision-limited food queries, local obstacle steering and stale-target rejection.
- Scalable TMP survival/trait/population HUD, food proximity highlight, eating feedback, dust, death and regeneration particles, and subtle locomotion camera bob.
- A single particle system capped at 512 particles; bounded registries; clean restart/reseed including while paused.

### Milestone 2A.1 foundation

- Four validated inheritable genes were added: fertility, reproduction threshold, offspring tendency and lifespan.
- The immutable Phenotype exposes maturity age, lifespan, reproductive energy threshold, motivation, fertility/cooldown and explicit maintenance costs.
- `GenomeGeneCatalog` gives every numeric and camouflage gene one normalized range for inheritance, mutation and genetic-distance calculations.
- `GenomeInheritance` recombines both parents through an injected deterministic random source, then applies unbiased small or rare major mutations. Important mutation records are capped.
- `GeneticDistance` returns a finite normalized, symmetric compatibility distance without allowing large-unit traits to dominate.
- `CreatureId` and `CreatureLineageRecord` provide GameObject-independent founder, parent, generation, age, descendant-count and bounded mutation value data.
- `ReproductionEligibility` is a pure nonmutating rules evaluator for maturity, health, energy, cooldown, reservation, compatibility and population capacity.
- These systems now power the live 2A.2 loop; inheritance and mutation were reused, not replaced.

### Playing generations (2A.2)

1. Eat brightfruit to build **energy** reserves. Stamina is a separate, quickly recovering sprint resource.
2. Approach another adult. The bottom mating hint reports readiness, partner distance, energy requirement, cooldown or capacity. Press **M / west button** within **3.5 m** of an eligible partner; stay close for **two seconds**. Moving away, death or loss of eligibility cancels courtship without a birth charge.
3. Both parents must be adults, have **60+ health**, meet their inherited energy threshold (50–90% maximum), be compatible, and have no cooldown or other courtship. Birth costs **30% of each parent's maximum energy** and applies each parent's inherited cooldown (about 35–104 seconds). One child is born per completed courtship. AI seeks partners through its normal staggered decision loop; AI never initiates mating on the player's behalf.
4. Each child receives its own two-parent genome with probabilistic, signed mutations—never guaranteed improvement. Juveniles start at **48% adult size**, **55% energy**, and mature in **18–42 simulation seconds** according to inherited lifespan. They eat, spend resources, move more slowly, can starve, grow to inherited adult dimensions and eventually reproduce. Natural lifespan is **240–900 seconds**; pause freezes all life timers.
5. Press **F / north button** to open the family journal. It shows your generation, parents, age, living direct children, a notable trait difference from the parental average, and recorded notable mutation. The selector lists **all living descendants**, including grandchildren and juveniles, with generation and resources. Select one to take control; the former player becomes AI if alive. No healing, aging reset or resource refill occurs.
6. Death pauses the world and offers the same descendant choices. With no descendants, restart or reseed explicitly; the game never creates a hidden replacement. Taking a branch means future choices are descendants of that newly controlled creature, not its siblings or ancestors.

Limits: **24 living creatures**, **32 creature objects including corpses**, **72 food slots**, **512 particles**, and **512 retained lineage records per run**. Pending courtships reserve capacity. Dead parents remain in the bounded value-data archive; full archive stops new births with a clear message rather than deleting ancestry. Restart/reseed clears the entire run, including age, reservations and family records. No automatic population replenishment.

## Architecture and asset replacement

All original game code/content is under **Assets/WildType**.

- **Genetics:** serializable Genome, immutable derived Phenotype, reusable GenomePreset ScriptableObjects.
- **Creature:** CreatureAgent coordinates root-level CreatureMotor, CreatureVitals, CreatureInteraction and CreatureLife. LineageArchive retains bounded, GameObject-independent ancestry. OrbitCamera is independent and follows current juvenile height.
- **VisualRoot:** CreatureVisual alone builds and animates the prototype model. Gameplay never depends on individual body-part objects. Replace this child/component with an adapter for a future rig without rewriting survival, genomes, AI or physics.
- **World:** Ecosystem owns the bounded food registry and deterministic placement; FoodPlant owns depletion/regrowth.
- **AI:** HerbivoreBrain sets the same movement intent and uses the same interactions as the player. Its periodic nearest-food query is the future spatial-grid seam.
- **Core:** StageSession coordinates startup/control transfer; GenerationLoop owns the simulation clock, partner reservations, birth validation and archive; PlayerInputBridge reads controls; FeedbackPool owns bounded effects.
- **UI:** StageHud owns TMP displays and pause controls.
- **Editor:** PrototypeBuilder creates authored meshes, materials, prefabs, presets and the saved scene. Rebuilding replaces these generated prototype assets; do not use it over subsequent hand-authored changes without reviewing them first.
- **Tests:** Edit Mode data/unit tests and Play Mode full-scene regression/soak.

Future imported assets belong under **Assets/ThirdParty/PublisherOrPackName**. Keep original vendor content unmodified and place adapters, material overrides and gameplay prefabs under Assets/WildType.

## Verification

Open **Window > General > Test Runner** to run Edit Mode and Play Mode tests.

Edit Mode covers nonfinite/out-of-range genes, immutable presets, trait tradeoffs, seeded variation, frame-rate-independent resources, stamina hysteresis, starvation, deterministic inheritance, unbiased mutation, bounded mutation history, normalized genetic distance, parentage and reproduction eligibility. The 2A.2 cases extend this with sibling isolation, transitive/dead-parent ancestry, archive capacity, meaningful health gating and 1,000 successive mutated genomes checked against every gene bound.

Play Mode exercises virtual keyboard/mouse/gamepad events, the actual saved scene, camera collision, multiple meals, regrowth, AI feeding and target invalidation, pause/restart/reseed, a ten-minute accelerated simulation with object/population/particle caps, and starvation/game-over recovery. Its test-only input settings route virtual events independently of Game-view focus and are restored afterward.

The legacy 102-check/600-second survival regression explicitly disables the generation clock to isolate its original assumptions. Separate `GenerationTests` exercise the enabled generational loop. See **VERIFICATION.md** for current executed results and the distinction between automated tests and native Editor inspection. `MILESTONE_2A_HANDOFF.md` is historical 2A.1 planning; the current request brought descendant control into 2A.2.

## Known limitations / deferred systems

- Local steering is not a path planner. Dense obstacle contacts can still produce imperfect routes; this is not a production navigation system.
- Gait is procedural rigid-mesh animation, not skeletal animation or ground-contact IK; some foot sliding or slope penetration is expected.
- No persistence, sound, rebinding interface, gamepad rumble, or in-game graphics menu. URP quality can be adjusted in editor settings for weaker hardware.
- Genome and food layout randomization is deterministic on this runtime, not promised across future engine versions.
- Food depletion/regrowth is bounded, but this is a prototype balance rather than a tuned natural-selection simulation.
- Generations and ancestry are session-only: no save/load, species tracking, mate sex, gestation, kinship restriction, parental care, predators, creature editor, other stages, multiplayer or open world. Genetically compatible relatives may mate in this milestone. The lineage archive cap requires a restart for extremely long runs.

## Next milestone

Tune and playtest the Creature generational loop before expanding scope. The long-term Cell → Creature → community → planetary society → space direction remains design context, not implemented systems.
