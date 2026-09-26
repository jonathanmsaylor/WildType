# WILDTYPE — Creature Stage vertical slice

A Unity implementation, independent of the earlier Godot project. **Current milestone: welcoming journal and Family Tree**, on `milestone-welcoming-journal`, based on pushed gameplay clarity `f34422c`. `main` is deliberately unchanged. Explore, eat, reproduce, recognize and locate your family, read their recorded lives, optionally support your children, and continue through living descendants in a bounded, roughly 250 m-wide Creature-stage ecosystem. Start with twelve autonomous herbivores; total living population is capped at 24.

## A welcoming journal

Begin, Restart and Reseed ask for your creature's name while the simulation is paused. **Continue as Eddy** needs no typing. Other founders keep their deterministic names; newborn names still receive numeric birth order. Nearby child labels show only the base name, region, rounded distance and generation; their stored full names are unchanged.

**Tab → Living descendants** shows your current creature's children and later descendants as short cards. **Highlight** finds a living relative without changing your creature. **Take Control** is only available for living descendants. Neither highlighting nor reading a sibling's record permits juvenile care.

**Family Tree** replaces Chronicle: two recorded parents above one selected creature, with two paged children below. Select a name for Details or **View branch** to move through the tree. **All Family Records** retains the full previous Chronicle list, including earlier controlled branches and recorded deaths. At most five tree cards (or three record cards) are drawn, never hundreds of shrinking labels. Arrow keys / D-pad follow an explicit focus order; confirm activates the focused button.

**Details page 1: Exact Genes** groups all sixteen existing genes into Movement, Senses & Foraging, Survival, and Inheritance & Coat. Hover, focus or select a gene to see its everyday explanation and full-precision value in the fixed help bubble. Later pages retain parent comparisons / mutation records, precise resources / adult calculations, and identity / death evidence. Stable IDs live in this deeper identity record so duplicate names never change action targets. No death or gene value is inferred when the archive or actor cannot supply it.

Ordinary numbers round to whole units; positive values below one say **less than 1**. Non-integral food and care quotes say **about**, not an exact promise. Mating readiness rounds upward so it never understates the threshold. Simulation values and transfer accounting are untouched. Region names are **The Meadow**, **Fernwood**, and **Amber Flats**.

See [journal evidence, protected-file inventory and verification](Documentation/WelcomingJournal/REPORT.md).

## Start playing — no README required

The optional first-step tips and **Tab → Getting started** explain this loop inside either scene. The journal/menu explicitly pauses the world: no energy, age or cooldown time is spent while reading. Hide or replay the tips from the same menu.

1. Move with **WASD** and find fruit on a plant. **E eat** appears nearby with the actual energy gain. Empty stems regrow. Food restores **energy**, not health; **stamina** is the separate sprint resource.
2. Build energy and approach a healthy adult. **F mate** shows the cost at birth. A failed attempt explains the actual condition preventing it. Stay close while the hearts appear.
3. After a birth, name your child or keep its default. **Tab → Living descendants** follows the creature you currently control. **Family Tree → All Family Records** also retains earlier controlled branches.
4. **Highlight** reveals a living relative while you remain in your creature. **Take Control** is a separate, explicitly labeled button available only for your living descendants. A sibling can be highlighted, not controlled or cared for.
5. Optional **R share** helps a nearby juvenile direct child. The prompt gives an explicitly rounded cost and gain; Details retains the exact last player-related transfer. It neither heals nor guarantees survival.

Ordinary play keeps labeled Energy, Health and Stamina bars, urgent plain-language warnings, identity, population totals and just two recent life events. **Details** contains current resources, inherited tradeoffs, adult values, exact genes, recorded parent comparisons and all retained notable mutations. **Recent births / deaths** shows the full existing four-event history. No fitness score or guaranteed genetic improvement is implied.

Gamepad equivalents remain south/eat, west/mate, right shoulder/share, north/journal and Start/menu. D-pad/stick navigation and south/confirm use an explicit focus order; names can be skipped with the pad, while custom text uses a keyboard. F6 still changes only the painterly comparison's appearance. See [clarity evidence and verification](Documentation/GameplayClarity/REPORT.md).

## Reversible painterly comparison

Open **WildType > Open painterly comparison**, or `Assets/WildType/Scenes/CreatureStage_PainterlyPreview.unity`, then press Play. Focus Game view and tap **F6** to alternate painterly/original presentation at the same camera position without restarting the simulation. All normal gameplay controls remain unchanged. The original prototype is the first build scene; the preview is a separate saved scene. The preserved local build settings currently omit the preview from builds, but it can be opened directly in the Editor. Presentation is constructed on entering Play Mode, not baked into the edit-time scene.

The study adds a continuous haunch/body/neck/muzzle silhouette, leaf-shaped ears, tapered jointed legs and tail, procedural coat brush variation and inherited bands. Existing size, leg length, coat, markings and juvenile growth remain genome-driven. A roughly **68 m-wide meadow patch around the starting area** adds bent grass, wildflowers, branching trees at existing trunk locations, ground color washes and distant atmospheric ridges. The rest of the ecosystem intentionally retains the original look. Existing food models, colliders, navigation, resource rules and genetics are unchanged.

This is an original Unity-generated style study, not a finished art pipeline or a recreation of the reference painting. No external art, textures, packs, paid tools or services were used. Geometry is generated once per actor/patch; a shared material and property blocks avoid per-frame mesh/material creation. The comparison scene disables static mesh batching so reversible mesh swaps remain readable. Do not run the old prototype builder to create this preview: it would rebuild the original scene.

See [comparison images, lineage checks, lighting investigation and limitations](Documentation/PainterlyPrototype/REPORT.md). This milestone preserves nine initially dirty asset/settings paths; their exact inventory and final hashes are in the welcoming-journal report. The unresolved original-scene sun change is not folded into this UI work.

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

Focus the Game view for input. Focus loss pauses the simulation. Food interaction is proximity-based; nothing requires hovering. There is no jump in this slice. Pause includes Resume, family views, Details, recent events, Getting started, tip visibility, Restart same seed, Reseed new run, and Quit. Restart/reseed clears this run's history. Quit exits Play Mode in the editor and exits the application in a build.

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
5. Press **Tab** (gamepad north button) to open the paused family journal. Three Living cards per page show names, generations, relationships and life stage; stable IDs and parent records are in **Details**, alongside age, resources, exact genes and birth-time comparisons. **Take Control** switches to a living descendant; selecting Details does not. The former player becomes AI if alive. **Highlight** briefly reveals a relative without transferring control. No healing, aging reset or resource refill occurs.
6. Death pauses the world and offers the same descendant choices. With no descendants, restart or reseed explicitly; the game never creates a hidden replacement. Taking a branch means future **control choices** are descendants of that newly controlled creature, not its siblings or ancestors. Earlier controlled creatures' children remain discoverable in **Family Tree → All Family Records**, including siblings and their branches; living family records have Highlight without granting control or care.

Limits: **24 living creatures**, **32 creature objects including corpses**, **72 food slots**, **512 particles**, and **512 retained lineage records per run**. Pending courtships reserve capacity. Dead parents remain in the bounded value-data archive; full archive stops new births with a clear message rather than deleting ancestry. Restart/reseed clears the entire run, including age, reservations and family records. No automatic population replenishment.

### Family recognition and juvenile care

Nearby living **direct children** have a small base-name, region, rounded-distance and generation cue within 25 m when visible. At most three nearby-child cues appear, not labels over the whole population. Energy and health are in Details, not the floating label; adult children remain recognizable. Locating does not extend care eligibility to grandchildren.

Approach a juvenile child within **3.5 m** and press **R** (gamepad right shoulder). The highlighted child's identity and hint identify the recipient and preview the cost. A currently located nearby juvenile direct child takes priority during the brief cue; otherwise the nearest visible juvenile direct child is chosen. A located grandchild never becomes eligible. Sharing is deliberate and optional, not automatic for the player.

Each successful share spends **up to 18 usable parent energy** and gives the child **80%** of that (normally **14.4**). The parent retains at least **25% of maximum energy**; smaller missing capacity or spare reserves reduce the transfer. At least one usable energy must fit. Nutrition/metabolism does not multiply the gift, and it never changes health or stamina. Both creatures must be living members of this run, the donor an adult, the recipient its direct juvenile child, in range and unobstructed, and neither courting. An **eight-simulation-second donor cooldown** and pressed-button input prevent repeated gifts from a held key. Pause freezes the timer. There are no delayed jobs that can feed a subsequently dead target.

Autonomous parents opportunistically share during their existing bounded decision step only when at least **65% full** and the nearby juvenile is below **50%**. They use the exact same cost, reserve, proximity, eligibility and cooldown rules. They do not remotely track children or interrupt foraging with long-range pursuit. Children retain their inherited genomes, ordinary AI, growth, meals, mortality and reproduction. Care buys some time between meals; it neither heals old damage nor guarantees adulthood. Ignoring care remains valid. Adult children cannot receive juvenile care.

Contextual hints use one binding per action: **E interact/eat, F mate, R share, Tab journal, Escape menu**. Gamepad hints switch to the most recently used input type; movement, sprint, orbit, wheel zoom and menu navigation remain. The old M mating/F journal duplicates are removed. The death title offers descendant selection only when living descendants exist; otherwise it clearly offers restart/reseed. Turnover totals/history, survival-pressure rules, inheritance and all existing caps are unchanged.

The permanent bottom-left control legend is gone. **E eat** plus the food's name appears above a nearby ripe plant only while you can eat it; **F mate** appears by a ready nearby partner; **R share** appears beside the target child's identity only when care is valid. If food and a child are both nearby, **E always eats food and R always shares with the child**: no hidden priority switch or accidental parental-energy donation. Gamepad south eats and right shoulder shares. Failed requests still explain why in the existing notice line. The family panel offers Tab for the journal; Escape also opens/closes the menu. The control table above retains every function for reference.

Normal-play panels use a 1920 × 1080 reference layout: survival **340 × 235**, family **340 × 150**, and a conditional two-event summary **560 × 145**. Labeled values use 24-point text; bounded warnings have a stronger translucent backing for bright scenery. Full records and raw gene coefficients are in the paused journal, not ordinary play. Cumulative Population/Births/Deaths remain visible; the four-event history remains available in the journal.

See [running-Editor evidence](Documentation/FamilyCare/README.md) and [verification details](VERIFICATION.md), including assisted-fixture limitations.

Living descendants and All Family Records use **three cards per page** with 23-point text and separate Highlight, Take Control and Details buttons. Family Tree uses up to five linked cards. Paging preserves access to every retained entry. Buttons are labeled rather than distinguished by color alone.

### Family Tree and retained Chronicle records

Press **Tab → Family Tree** for linked parent/focus/child cards. **All Family Records** retains the previous Chronicle: the controlled creature, actual ancestors, descendants and **descendants of creatures you previously controlled**. Earlier children and branches remain after control transfer. Short cards show name, generation, truthful relationship and life state. **Details page 4** retains stable IDs, parent IDs, children born, birth time and exact age/death evidence. Matching names remain distinct internally. Previous/Next page browses the bounded record list; Living descendants returns to the current creature's descendant list.

Only living **descendants of the current creature** have a **Take Control** button. Other eligible living family members have **Highlight**, not control. Siblings (including half-siblings) are labeled Sibling; more distant collateral branches say Other family branch. Yourself, deceased and unavailable entries retain Details without invalid action buttons. Highlight keeps its pulse, does not walk or transfer control, and is unavailable on the death menu. It never grants direct-child care. Taking control resets Living descendants to the new creature while preserving earlier family branches. Relationships use actual ancestry and controlled-history IDs, not merely a common founder label. Tree links show recorded parents, with stable parent IDs also available in Details.

Death details are retained with the existing **512-record archive**, independently of the four-event turnover HUD. **Starvation** or **old age** comes only from the actual fatal-damage notification; other confirmed deaths say **cause unknown**. A removed/missing actor without a recorded death says **Unavailable · no recorded death**, never an invented death. Corpse cleanup cannot erase or rewrite a recorded cause. All Family Records is ordered by archive registration (founders first, then births); the tree is a bounded relationship projection of those same records.

The journal stays paused until Locate/control/resume. It adds no normal-play HUD panel or controls, and changes no care, mating, survival, ecology or genetics rules. The controlled-ID history is value-only and cannot exceed the existing 512-record archive; no additional creature objects are created. Counts describe family history, **not evidence of genetic selection**. Restart/reseed clears archive, death records, prior controlled IDs, names, selection and journal page/view, then names the 13 newly created founders. No persistence is added. See [continuity evidence and asset reconciliation](Documentation/LineageContinuity/README.md) and [verification](VERIFICATION.md).

**Why a child can leave Living descendants:** after you take control of that child, it becomes **You**, not its own descendant; its siblings are not its descendants either. Your HUD's born/living-child counts now belong to the newly controlled ID. Use Family Tree or All Family Records to find the earlier family. A living descendant must still appear in the current selector (possibly on a later page). Disappearance alone is never reported as a death.

### Locating living descendants

Choose **Locate** beside any living descendant, including a grandchild. Play resumes with your current creature still controlled and the notice **Locating Dave 1 · #014** (for example). The selected creature receives a soft pale-gold pulse for **four simulation seconds**; a compact temporary name/ID/generation cue identifies it. An offscreen cue adds **Left / Right / Above / Below / Behind** and distance. A terrain-hidden onscreen target is described as **Obscured**, not shown through geometry. There is no automatic movement, teleport, persistent beacon or camera takeover.

Open the journal again to repeat the same pulse or select a different descendant. Only one target exists; pause freezes its timer, and expiry, target/player death, control transfer, restart and reseed clear it. Locate is disabled on the death menu: use descendant control or explicit restart/reseed there. Nearby direct-child recognition remains separate. Location information is a player convenience and is never available to AI.

The original coat remains visible beneath a restrained tint (at most 38% toward pale gold). One presenter caches the selected creature's renderers and original property blocks, then restores them on expiry or target changes. No new assets, per-frame mesh/material allocation, persistent labels for the population or changes to VisualRoot are introduced. See [lineage locating evidence](Documentation/LineageLocating/README.md).

### Naming newborn children

When the currently controlled creature has a child, a skippable naming prompt pauses the simulation. Enter a name and choose **Name child** or press Enter; the numeric birth order is added automatically: **Dave 1**, **Dave 2**, or **Alice 3** for the third child of that parent. The counter includes earlier unnamed/dead children, not just currently living children. It is fixed at birth and does not change when control transfers or siblings die. This is the controlled parent's birth order, not the total world-birth counter.

Every offspring receives a default at birth, including autonomous AI births. A fixed hash of its stable, seeded ID reproducibly chooses from 24 short pronounceable names, without consuming Unity or gameplay/genetics randomness. **All 13 initial Gen 0 founders now use the same cosmetic naming method**, without a child-order suffix; ID and generation remain separate metadata. Skip, blank submission or Escape (gamepad east button) retains the newborn default, shown in the prompt. A typed name replaces that default; the numeric suffix remains. AI-born names use the first recorded parent's birth order; player births use the controlled parent's order. Repeated names are allowed and distinguished by the adjacent stable ID, not by pretending generation is a name.

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

The upper-right HUD shows **Population, Births and Deaths** together. The lower-right panel summarizes the **two most recent** events with names/IDs; **Tab → Recent births / deaths** retains the full existing **four most recent events**, newest first, with ID, generation, parent IDs, founder lineage and honest recorded death causes. Entries do not time out or disappear when a birth fills a death's vacant slot; only a newer event evicts the oldest. Totals remain cumulative after entries leave the list. Founders and object cleanup are not counted as births/deaths. Restart/reseed clears totals and events.

Death causes are captured from the actual fatal damage source: **starvation**, **old age**, or **cause unknown** for other/unspecified damage. Energy or age alone is never used to guess a cause. The existing lineage identifier is a canonical founder reference; it does not imply that a two-parent descendant has only one founding ancestor. Events remain readable while paused or choosing a descendant.

## Architecture and asset replacement

All original game code/content is under **Assets/WildType**.

- **Genetics:** serializable Genome, immutable derived Phenotype, reusable GenomePreset ScriptableObjects.
- **Creature:** CreatureAgent coordinates root-level CreatureMotor, CreatureVitals, CreatureInteraction and CreatureLife. LineageArchive retains bounded, GameObject-independent ancestry and confirmed death snapshots; its revisioned family query includes actual ancestors and descendants. OrbitCamera is independent and follows current juvenile height.
- **VisualRoot:** CreatureAppearance derives immutable deterministic adult presentation inputs; CreatureVisual builds and animates the prototype model. Gameplay never depends on individual body-part objects. Replace this child/component with an adapter for a future rig without rewriting survival, genomes, AI or physics. InheritanceSummary stores compact parent/child comparisons in the existing lineage archive.
- **World:** EcologyRules owns region/food/clearance constants; Ecosystem owns the bounded food registry, deterministic placement, visible-forage query and restrained observations; FoodPlant owns depletion/regrowth and procedural regional appearance.
- **AI:** HerbivoreBrain sets the same movement intent and uses the same interactions as the player. Its periodic nearest-food query is the future spatial-grid seam.
- **Core:** StageSession coordinates startup/control transfer; GenerationLoop owns the simulation clock, partner reservations, birth validation and archive; FamilyCare coordinates validated direct-child sharing using pure FamilyCareRules and per-life cooldowns; DescendantLocator owns one short-lived, validated player selection independently of care; FamilyNames stores bounded run-only names and birth order without modifying identity/genetics; PlayerInputBridge reads controls; FeedbackPool owns bounded effects.
- **UI:** StageHud owns TMP displays and pause controls. Living descendants and All Family Records share three cards; the partial Details and Tree files own category/help presentation and five-node family browsing. JournalReadout formats display values only; GeneGuide explains existing rules. LineageChronicle formats factual life records without inferring death from absence. FamilyWorldCues reuses three child labels, one temporary locating cue and up to twelve procedural CourtshipHeart graphics. DescendantPulse temporarily tints existing renderers using cached/restored property blocks, without per-frame mesh/material allocation.
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
