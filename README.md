# WILDTYPE — Creature Stage vertical slice

A Unity implementation, independent of the earlier Godot project. **Current milestone: lineage chronicle.** Explore, eat, reproduce, recognize and locate your descendants, read their recorded lives, optionally support your children, and continue through living descendants in a bounded, roughly 250 m-wide Creature-stage ecosystem. Start with twelve autonomous herbivores; total living population is capped at 24.

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
| Mate with eligible nearby adult | F | West button (X / Square) |
| Family journal / descendant selection | Tab | North button (Y / Triangle) |
| Share energy with nearby juvenile child | R | Right shoulder (RB / R1) |
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
2. Approach another adult. **F mate** appears over an eligible nearby partner; an unavailable request explains its reason in the notice line. Press **F** (gamepad west button) within **3.5 m** of an eligible partner; stay close for **two seconds**. A pulsing heart hovers between partners. Moving away, death or loss of eligibility cancels courtship without a birth charge.
3. Both parents must be adults, have **60+ health**, meet their inherited energy threshold (50–90% maximum), be compatible, and have no cooldown or other courtship. Birth costs **30% of each parent's maximum energy** and applies each parent's inherited cooldown (about 35–104 seconds). One child is born per completed courtship. AI seeks partners through its normal staggered decision loop; AI never initiates mating on the player's behalf.
4. Each child receives its own two-parent genome with probabilistic, signed mutations—never guaranteed improvement. Juveniles start at **48% adult size**, **55% energy**, and mature in **18–42 simulation seconds** according to inherited lifespan. They eat, spend resources, move more slowly, can starve, grow to inherited adult dimensions and eventually reproduce. Natural lifespan is **240–900 seconds**; pause freezes all life timers.
5. Press **Tab** (gamepad north button) to open the family journal. The lineage panel shows your generation, parents, age, living direct children, inherited adult appearance/stat comparisons for both parents, and a recorded notable mutation when present. The selector lists **all living descendants**, including grandchildren and juveniles, with name, stable ID, generation, resources, coat, adult size, leg proportions, region and distance. Select a creature row to take control; the former player becomes AI if alive. The separate **Locate** button briefly reveals any living descendant without transferring control. No healing, aging reset or resource refill occurs.
6. Death pauses the world and offers the same descendant choices. With no descendants, restart or reseed explicitly; the game never creates a hidden replacement. Taking a branch means future choices are descendants of that newly controlled creature, not its siblings or ancestors.

Limits: **24 living creatures**, **32 creature objects including corpses**, **72 food slots**, **512 particles**, and **512 retained lineage records per run**. Pending courtships reserve capacity. Dead parents remain in the bounded value-data archive; full archive stops new births with a clear message rather than deleting ancestry. Restart/reseed clears the entire run, including age, reservations and family records. No automatic population replenishment.

### Family recognition and juvenile care

Nearby living **direct children** have a small name/ID/relationship cue within 25 m when visible, with generation kept as metadata. At most three nearby-child cues appear, not labels over the whole population. Juvenile cues show actual usable energy; adult children remain recognizable. Locating does not extend care eligibility to grandchildren.

Approach a juvenile child within **3.5 m** and press **R** (gamepad right shoulder). The highlighted child's identity and hint identify the recipient and preview the cost. A currently located nearby juvenile direct child takes priority during the brief cue; otherwise the nearest visible juvenile direct child is chosen. A located grandchild never becomes eligible. Sharing is deliberate and optional, not automatic for the player.

Each successful share spends **up to 18 usable parent energy** and gives the child **80%** of that (normally **14.4**). The parent retains at least **25% of maximum energy**; smaller missing capacity or spare reserves reduce the transfer. At least one usable energy must fit. Nutrition/metabolism does not multiply the gift, and it never changes health or stamina. Both creatures must be living members of this run, the donor an adult, the recipient its direct juvenile child, in range and unobstructed, and neither courting. An **eight-simulation-second donor cooldown** and pressed-button input prevent repeated gifts from a held key. Pause freezes the timer. There are no delayed jobs that can feed a subsequently dead target.

Autonomous parents opportunistically share during their existing bounded decision step only when at least **65% full** and the nearby juvenile is below **50%**. They use the exact same cost, reserve, proximity, eligibility and cooldown rules. They do not remotely track children or interrupt foraging with long-range pursuit. Children retain their inherited genomes, ordinary AI, growth, meals, mortality and reproduction. Care buys some time between meals; it neither heals old damage nor guarantees adulthood. Ignoring care remains valid. Adult children cannot receive juvenile care.

Contextual hints use one binding per action: **E interact/eat, F mate, R share, Tab journal, Escape menu**. Gamepad hints switch to the most recently used input type; movement, sprint, orbit, wheel zoom and menu navigation remain. The old M mating/F journal duplicates are removed. The death title offers descendant selection only when living descendants exist; otherwise it clearly offers restart/reseed. Turnover totals/history, survival-pressure rules, inheritance and all existing caps are unchanged.

The permanent bottom-left control legend is gone. **E eat** plus the food's name appears above a nearby ripe plant only while you can eat it; **F mate** appears by a ready nearby partner; **R share** appears beside the target child's identity only when care is valid. If food and a child are both nearby, **E always eats food and R always shares with the child**: no hidden priority switch or accidental parental-energy donation. Gamepad south eats and right shoulder shares. Failed requests still explain why in the existing notice line. The family panel offers Tab for the journal; Escape also opens/closes the menu. The control table above retains every function for reference.

Normal-play panels are smaller and lighter: survival **300 × 270**, family **340 × 255**, turnover **500 × 190** in the 1920 × 1080 reference layout. Background opacity is **36%**, down from 90%, with subtle text outlines. The family essentials use 20-point text with extra spacing, rather than a dense 16-point list. Detailed parent/child trait comparisons expand only while the journal is open, not over the ordinary play scene. Menu and nested-panel opacities are also reduced. Four turnover entries and all cumulative counts remain available.

See [running-Editor evidence](Documentation/FamilyCare/README.md) and [verification details](VERIFICATION.md), including assisted-fixture limitations.

The journal's right side has **four larger cards per page** in a wider 620-point column: a 21-point identity/name heading, then 18-point resources and region/trait details. Locate is separate from the control-transfer card. All living descendants remain accessible through paging.

### Lineage chronicle

Press **Tab**, then choose **Chronicle** at the bottom of the existing journal's right panel. Three roomier records per page show the controlled creature, its actual ancestors and all its descendants retained this run, including deceased children and grandchildren. Each record keeps the **name + stable ID + generation**, relationship to you, living adult/juvenile or recorded deceased status, parent IDs, **children born**, and current age or recorded lifespan. Matching first names or full names remain distinct by ID. **Next page** cycles through the bounded archive view; **Living descendants** returns to the original four-card selector.

Only living descendants can be selected to take control or have a **Locate** button. Ancestors, yourself, deceased and unavailable entries say **record only**. Locate retains its existing pulse and does not transfer control; it remains disabled on the death menu. Taking control resets the view to the new creature's living descendants, while Chronicle still includes its ancestors, including dead parents. Siblings/cousins are not automatically included merely because they share a founder; the view follows actual ancestry links, not a single founder label. A descendant's other parent remains identified by ID even when outside this branch's view.

Death details are retained with the existing **512-record archive**, independently of the four-event turnover HUD. **Starvation** or **old age** comes only from the actual fatal-damage notification; other confirmed deaths say **cause unknown**. A removed/missing actor without a recorded death says **Unavailable · no recorded death**, never an invented death. Corpse cleanup cannot erase or rewrite a recorded cause. Life records are ordered by archive registration (founders first, then births), not a new event feed or interactive tree.

The journal stays paused. It adds no normal-play HUD panel or controls, and changes no naming, care, Locate, mating, survival, ecology or genetics rules. Counts describe this family history, **not evidence of genetic selection**. Restart/reseed clears the archive, death records, names, selection and journal page/view; no persistence is added. See [Chronicle screenshots](Documentation/LineageChronicle/README.md) and [verification](VERIFICATION.md), including the preserved local asset differences.

### Locating living descendants

Choose **Locate** beside any living descendant, including a grandchild. Play resumes with your current creature still controlled and the notice **Locating Dave 1 · #014** (for example). The selected creature receives a soft pale-gold pulse for **four simulation seconds**; a compact temporary name/ID/generation cue identifies it. An offscreen cue adds **Left / Right / Above / Below / Behind** and distance. A terrain-hidden onscreen target is described as **Obscured**, not shown through geometry. There is no automatic movement, teleport, persistent beacon or camera takeover.

Open the journal again to repeat the same pulse or select a different descendant. Only one target exists; pause freezes its timer, and expiry, target/player death, control transfer, restart and reseed clear it. Locate is disabled on the death menu: use descendant control or explicit restart/reseed there. Nearby direct-child recognition remains separate. Location information is a player convenience and is never available to AI.

The original coat remains visible beneath a restrained tint (at most 38% toward pale gold). One presenter caches the selected creature's renderers and original property blocks, then restores them on expiry or target changes. No new assets, per-frame mesh/material allocation, persistent labels for the population or changes to VisualRoot are introduced. See [lineage locating evidence](Documentation/LineageLocating/README.md).

### Naming newborn children

When the currently controlled creature has a child, a skippable naming prompt pauses the simulation. Enter a name and choose **Name child** or press Enter; the numeric birth order is added automatically: **Dave 1**, **Dave 2**, or **Alice 3** for the third child of that parent. The counter includes earlier unnamed/dead children, not just currently living children. It is fixed at birth and does not change when control transfers or siblings die. This is the controlled parent's birth order, not the total world-birth counter.

Every offspring receives a default at birth, including autonomous AI births. A fixed hash of its stable, seeded ID reproducibly chooses from 24 short pronounceable names, without consuming Unity or gameplay/genetics randomness. Skip, blank submission or Escape (gamepad east button) retains that default, shown in the prompt. A typed name replaces the default; the numeric suffix remains. AI-born names use the first recorded parent's birth order; player births use the controlled parent's order. Repeated names are allowed and distinguished by the adjacent stable ID, not by pretending generation is a name.

Names appear beside stable IDs in child/locating cues, mating prompts and journal cards, and on the controlled descendant's family panel. IDs, parentage, genomes and genetics are untouched. Typed names are limited to 16 characters (letters, numbers, spaces, hyphens and apostrophes); the suffix is additional. Names remain unchanged through growth, control transfer, death and corpse cleanup and are bounded by the 512-record ancestry limit. Typing F/R/E or moving sticks cannot trigger gameplay while naming. Gamepad can navigate/confirm or skip; arbitrary text entry uses the keyboard, with no new on-screen keyboard. Names exist only during this run and clear on restart/reseed; no persistence is added. The compact birth/death history continues to use stable IDs.

### Visible, playable inheritance

Look for a broad, thick-bodied amber Bulwark versus the slimmer, long-legged blue/violet Strider. The existing teal Meadow Grazer lies between them. Body size now changes torso width/depth proportionally as well as total scale; leg length changes limb length/thickness, torso elongation and the width of three high-contrast coat bands. Bands wrap over the back and sides rather than floating as tiny dorsal pieces. Coat RGB still comes directly from the inherited genome. These are deterministic expressions of **existing genes**, not new genes, cosmetic random rolls or generation bonuses.

Mate two compatible adults, press **Tab**, and select their child. The on-screen lineage panel reads **parent A / B -> child**, comparing coat, adult size, leg proportion, sprint speed, steering rate, energy reserve and idle food cost. The parent's values are recorded at birth and survive its death. Juveniles begin small but keep the same adult shape and bands throughout growth. Breed that descendant again to see the next combination; there is no guaranteed improvement. Recorded mutations show before/after values without claiming they are beneficial.

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
- **Creature:** CreatureAgent coordinates root-level CreatureMotor, CreatureVitals, CreatureInteraction and CreatureLife. LineageArchive retains bounded, GameObject-independent ancestry and confirmed death snapshots; its revisioned family query includes actual ancestors and descendants. OrbitCamera is independent and follows current juvenile height.
- **VisualRoot:** CreatureAppearance derives immutable deterministic adult presentation inputs; CreatureVisual builds and animates the prototype model. Gameplay never depends on individual body-part objects. Replace this child/component with an adapter for a future rig without rewriting survival, genomes, AI or physics. InheritanceSummary stores compact parent/child comparisons in the existing lineage archive.
- **World:** EcologyRules owns region/food/clearance constants; Ecosystem owns the bounded food registry, deterministic placement, visible-forage query and restrained observations; FoodPlant owns depletion/regrowth and procedural regional appearance.
- **AI:** HerbivoreBrain sets the same movement intent and uses the same interactions as the player. Its periodic nearest-food query is the future spatial-grid seam.
- **Core:** StageSession coordinates startup/control transfer; GenerationLoop owns the simulation clock, partner reservations, birth validation and archive; FamilyCare coordinates validated direct-child sharing using pure FamilyCareRules and per-life cooldowns; DescendantLocator owns one short-lived, validated player selection independently of care; FamilyNames stores bounded run-only names and birth order without modifying identity/genetics; PlayerInputBridge reads controls; FeedbackPool owns bounded effects.
- **UI:** StageHud owns TMP displays and pause controls, with living-descendant and compact Chronicle views sharing the existing card pool. LineageChronicle formats factual life records and resolves living actors without inferring death from absence. FamilyWorldCues reuses three child labels, one temporary locating cue and up to twelve procedural CourtshipHeart graphics. DescendantPulse temporarily tints the selected VisualRoot's existing renderers using cached/restored property blocks, without allocating creature meshes/materials per frame.
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
- Generations and ancestry are session-only: no save/load, species tracking, mate sex, gestation, kinship restriction, predators, creature editor, other stages, multiplayer or open world. Care is a modest energy transfer, not following, packs, nests or protection. Genetically compatible relatives may mate in this milestone. The lineage archive cap requires a restart for extremely long runs.

## Next milestone

Tune and playtest the Creature generational loop before expanding scope. The long-term Cell → Creature → community → planetary society → space direction remains design context, not implemented systems.
