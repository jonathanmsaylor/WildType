# WILDTYPE — Creature Stage vertical slice

A Unity implementation, independent of the earlier Godot project. **Current milestone: survival pressure and ecological balance.** Explore, eat, reproduce and continue through living descendants in a bounded, roughly 250 m-wide Creature-stage ecosystem. Start with twelve autonomous herbivores; total living population is capped at 24.

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
- Fixed 72 food slots with distinct meadow, woodland and dry-region forage. Eating removes fruit; regional timers regenerate it in place. No per-food coroutine or unbounded spawning.
- Shared player/AI energy, stamina, health, starvation and death rules. Starvation costs 4 health per second. AI corpses are removed after four seconds; player death opens descendant choices and restart/reseed options.
- Twelve herbivores with staggered 0.35–0.5 s decisions, vision/line-of-sight-limited food choices, local obstacle steering, short meal memory and extended searches when forage is absent.
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
5. Press **F / north button** to open the family journal. The lineage panel shows your generation, parents, age, living direct children, inherited adult appearance/stat comparisons for both parents, and a recorded notable mutation when present. The selector lists **all living descendants**, including grandchildren and juveniles, with generation, resources, coat, adult size and leg proportions. Select one to take control; the former player becomes AI if alive. No healing, aging reset or resource refill occurs.
6. Death pauses the world and offers the same descendant choices. With no descendants, restart or reseed explicitly; the game never creates a hidden replacement. Taking a branch means future choices are descendants of that newly controlled creature, not its siblings or ancestors.

Limits: **24 living creatures**, **32 creature objects including corpses**, **72 food slots**, **512 particles**, and **512 retained lineage records per run**. Pending courtships reserve capacity. Dead parents remain in the bounded value-data archive; full archive stops new births with a clear message rather than deleting ancestry. Restart/reseed clears the entire run, including age, reservations and family records. No automatic population replenishment.

### Visible, playable inheritance

Look for a broad, thick-bodied amber Bulwark versus the slimmer, long-legged blue/violet Strider. The existing teal Meadow Grazer lies between them. Body size now changes torso width/depth proportionally as well as total scale; leg length changes limb length/thickness, torso elongation and the width of three high-contrast coat bands. Bands wrap over the back and sides rather than floating as tiny dorsal pieces. Coat RGB still comes directly from the inherited genome. These are deterministic expressions of **existing genes**, not new genes, cosmetic random rolls or generation bonuses.

Mate two compatible adults, press **F**, and select their child. The on-screen lineage panel reads **parent A / B -> child**, comparing coat, adult size, leg proportion, sprint speed, steering rate, energy reserve and idle food cost. The parent's values are recorded at birth and survive its death. Juveniles begin small but keep the same adult shape and bands throughout growth. Breed that descendant again to see the next combination; there is no guaranteed improvement. Recorded mutations show before/after values without claiming they are beneficial.

Two visible traits have opposing effects, **all other genes equal**:

- **Longer legs:** greater stride speed, but a lower turning rate. The existing phenotype turn rate now limits actual planar travel direction, not just the visual facing. Shorter legs turn more sharply. Acceleration and braking remain separate from steering; reversing input turns on the ground plane.
- **Larger/broader bodies:** larger energy reserves, but greater absolute food consumption and slower acceleration. Existing metabolism/efficiency/speed/agility genes still affect the final result; color and band width add no bonuses. Compare the final numbers rather than assuming every tall creature beats every short one.

The coat uses one 375-vertex mesh per creature lifetime, a shared material and property-block colors. Growth/animation reuse it; death/restart releases it. The `VisualRoot` boundary remains replaceable. No per-frame mesh or material allocation was introduced. Mouse-wheel zoom, reproduction, mutation probabilities, survival formulas, population limits and turnover rules were not retuned.

See the [running-Editor comparison screenshots](Documentation/VisibleInheritance/README.md) and [verification record](VERIFICATION.md) for actual results and the limits of controlled visual checks.

### Ecological selection

The same map changes your foraging choices. **Coral brightfruit** is meadow forage that slows under repeated grazing. In green **Fernwood**, look for low, broad **violet fernberries**: small meals that recover quickly. On the brown **Amber flats**, tall **gold sunpods** hold larger meals but are sparse and take much longer to return. Empty plants remain visible; their fruit returns in place. There is no automatic replenishment of creatures or guarantee of survival.

Food details (reference numbers, deliberately not added to the normal HUD):

| Region | Fixed slots | Raw nutrition per meal | Seeded regrowth |
|---|---:|---:|---:|
| Meadow | 32 | 42 | 25–33 s base, up to 120 s extra after repeated harvests |
| Fernwood | 28 | 22 | 18–26 s |
| Amber flats | 12 | 72 | 85–115 s |

All meals restore **energy**, through the existing metabolism-based nutrition factor; unused nutrition above maximum energy is lost. Each slot's base wait is deterministic for the seed; the current meadow wait also depends on its harvest/rest history. Placement respects the existing region boundaries, rejects scenery collisions and enforces regional spacing; no new world or food objects appear on regrowth. Pause freezes recovery, and restart/reseed clears the run and its ecology observations.

**Inherited tradeoff:** open ground retains the original stride speed. Fernwood's tight understory caps travel by the existing steering rate (`min(original speed, turn rate × 0.45 m)`). This is one environmental cap, not a second genetic speed bonus. It blends smoothly from x = -35 to -43 rather than snapping at the edge. Matched short-legged / long-legged genomes walked at **3.21 / 3.91 m/s in open ground**, but **3.21 / 2.74 m/s in woodland** in the actual motor tests. Agility and other inherited genes still matter. Sprint does not spend stamina when brush prevents a meaningful speed increase; juvenile scaling and low-energy fatigue still apply normally.

Large bodies keep their existing reserve-versus-food-cost tradeoff: a full reserve lasts longer between meals, but each little woodland bite refills less of it and greater maintenance consumes more food. No new metabolism, inheritance, mutation, reproduction or generation bonuses were added. A faster local body is not necessarily the most energy-efficient one, and no trait is forced to spread.

Hungry AI evaluates **only ripe food within vision and line of sight**, using useful nutrition and estimated travel cost. It remembers one recent meal location for at most 50 seconds, without knowing whether unseen fruit has returned. When nothing is viable, it extends its exploratory walk, can cross a nearby terrain edge, and abandons prolonged or obstructed approaches. It receives the same movement cap, meals, starvation and reproduction rules as the player—no remote food knowledge, teleporting or survival subsidy.

Brief region/scarcity observations reuse the existing notice line, defer to birth/eating feedback, and have cooldowns. The normal HUD names the region without reporting the global food inventory; birth/death counts and the four-entry history are unchanged. Try leaving a depleted patch, banking a sunpod meal before a longer trip, and taking a differently proportioned descendant between meadow and woodland. No weather cycle was added.

### Survival pressure: rest the meadow patch

Only **meadow brightfruit** has changed. Its first harvest still regrows in 25–33 seconds. Every successful harvest adds **45 seconds** to that plant's stored delay for its **next** harvest, capped at **120 extra seconds**. If immediately harvested whenever ripe, its waits are base, base + 45, base + 90, then base + 120 (at most **153 seconds**). Failed eating attempts do not add pressure. Ripe fruit is never taken away by this rule.

When a ripe plant is left untouched, its stored delay decreases by **0.5 seconds per simulation second**: at most **240 seconds of ripe rest** returns the next harvest to its base wait. Empty time completes the current regrowth; it does not also erase grazing pressure. Fresh or rested meadow patches, steady woodland berries and rich dry-region sunpods offer alternatives to camping one grazed site. Empty stems and disappearing/returning fruit remain visible. A brief existing notice explains nearby grazing when forage runs out; there is no added HUD panel or global famine cycle.

Nutrition, all 72 slots, inherited genes, AI vision/search, movement/metabolism, birth costs and caps are unchanged. Travel still spends energy; resting avoids movement expenditure but not maintenance. Larger reserves buffer complete meal gaps, while larger bodies spend more maintaining themselves between small meals. Long legs retain open-ground efficiency but arrive more slowly in tight woodland. Existing speed-scaled drain means that slower woodland arrival is **not** an additional per-metre energy penalty compared with short legs.

Pause freezes both recovery processes. Restart/reseed clears all plant pressure, counts and history. There are no forced deaths, genotype targets, guaranteed improvements or automatic creature replacements. Three matched 1,200-second seed surveys went from **0/0/0** starvation deaths to **0/2/1**, with all changed-rule runs ending at 24 living creatures. This is modest survival pressure, **not proof of adaptation or long-term balance**; [VERIFICATION.md](VERIFICATION.md) contains the full before/after results and controlled-test limitations. See the [actual Editor captures](Documentation/SurvivalPressure/README.md) for depleted meadow forage, travel into woodland, a normal meal and the death/restart UI.

See [ecology Editor evidence](Documentation/EcologicalSelection/README.md) and [verification results](VERIFICATION.md). The bounded simulation is an observation, not proof of long-term equilibrium or region-specific genetic adaptation.

### Turnover visibility

The upper-right HUD shows **Population, Births and Deaths** together. The lower-right panel retains the **four most recent births/deaths**, newest first, with creature ID, generation, parents when known and the recorded founder lineage. Entries do not time out or disappear when a birth fills a death's vacant slot; only a newer event can evict the oldest entry. Totals remain cumulative for the run even after entries leave the list. Initial founders and object cleanup are not counted as births/deaths. Restart/reseed clears both totals and events.

Death causes are captured from the actual fatal damage source: **starvation**, **old age**, or **cause unknown** for other/unspecified damage. Energy or age alone is never used to guess a cause. The existing lineage identifier is a canonical founder reference; it does not imply that a two-parent descendant has only one founding ancestor. Events remain readable while paused or choosing a descendant.

## Architecture and asset replacement

All original game code/content is under **Assets/WildType**.

- **Genetics:** serializable Genome, immutable derived Phenotype, reusable GenomePreset ScriptableObjects.
- **Creature:** CreatureAgent coordinates root-level CreatureMotor, CreatureVitals, CreatureInteraction and CreatureLife. LineageArchive retains bounded, GameObject-independent ancestry. OrbitCamera is independent and follows current juvenile height.
- **VisualRoot:** CreatureAppearance derives immutable deterministic adult presentation inputs; CreatureVisual builds and animates the prototype model. Gameplay never depends on individual body-part objects. Replace this child/component with an adapter for a future rig without rewriting survival, genomes, AI or physics. InheritanceSummary stores compact parent/child comparisons in the existing lineage archive.
- **World:** EcologyRules owns region/food/clearance constants; Ecosystem owns the bounded food registry, deterministic placement, visible-forage query and restrained observations; FoodPlant owns depletion/regrowth and procedural regional appearance.
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
- Food depletion/regrowth is bounded, but this is a prototype balance rather than a tuned natural-selection simulation. Woodland clearance is a region-wide approximation, not collision with individually simulated branches. High agility, food abundance and founder geography can outweigh leg length; sparse dry sites may still be too generous or harsh across other seeds.
- Generations and ancestry are session-only: no save/load, species tracking, mate sex, gestation, kinship restriction, parental care, predators, creature editor, other stages, multiplayer or open world. Genetically compatible relatives may mate in this milestone. The lineage archive cap requires a restart for extremely long runs.

## Next milestone

Tune and playtest the Creature generational loop before expanding scope. The long-term Cell → Creature → community → planetary society → space direction remains design context, not implemented systems.
