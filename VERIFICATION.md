# WILDTYPE verification — painterly prototype — 2026-09-25

Branch: **milestone-painterly-prototype**, based on pushed continuity **d4d77e89c0545db5552ee148bf49191d937f5c9d**. Main is unchanged. See the [full prototype report with inline Game-view images](Documentation/PainterlyPrototype/REPORT.md).

## Current milestone results

- Final complete suites: **119/119 Edit Mode and 40/40 Play Mode passed**, zero failed/skipped. Play Mode took 502.75 seconds. All temporary inspection helpers were removed before these suites. An initial 37/40 Play run exposed three test-fixture/accounting issues; all four focused rerun checks and both final full suites passed after test-only corrections documented in the report. The preview capacity fixture reached 24 living creatures with 72 food slots and retained the 32-object/512-ancestry bounds.
- Independent saved-scene lineage check: native F produced a child in an initial unassisted run. A separate assisted multi-generation session used real inheritance/mating with positioning, resource refill and accelerated normal growth explicitly disclosed. Native journal clicks verified sibling #015 after taking #014, Locate without control transfer/care, and adult child #016 remaining listed after producing #017. Both relevant Chronicle pages and the complete Living descendants page were inspected. No lineage defect reproduced; thirteen named Gen 0 founders verified. No deaths forced and no survival tuning.
- Actual rendered preview inspected at **1920×1080**, including three founder appearances and an inherited juvenile growing into its adult form. Native F6 toggles and journal Locate worked. Brief native W/E taps were inconclusive; do not treat automated movement/food checks as human playtesting.
- Matched frozen-camera performance, 16 creatures/72 food slots, 300 frames per look after warm-up: original median **3.376 ms**, p95 **3.921 ms**; study median **3.029 ms**, p95 **3.550 ms**. Editor frame intervals, not isolated GPU timings or a 24-moving-creature/build benchmark.
- Isolated sun-component experiment: with/without screenshots are byte-identical. URP recreates removed additional-light-data during rendering. Recommend restoring the default component in a separate deliberate review; its intended removal remains unknown, so the original scene is untouched and uncommitted.
- All five pre-existing dirty asset hashes remain unchanged. Seven accepted mesh/prefab changes remain inherited from `4809a9d`. No RunAudit, main, zoom, gameplay rules, package or external-asset changes.

The new scene is reversible and leaves the existing prototype intact. It is a modest procedural study, not reference-level finished art: visible limb joins, simple grass/flowers, provisional ridges, original scenery beyond the central patch, and no LOD/build benchmark. See the report for precise assistance, evidence, provenance and limitations.

---

# Previous milestone: lineage continuity — 2026-09-25

## Starting state / protection

Verified local and remote `milestone-lineage-chronicle` at **982075e2e85acef8394e00416db89940ae6d1b20**, with the intact chain through lineage-locating, family-care and survival-pressure. `main` was and remains **d768797775ad0a7b802eaec0f50899e5d3e2e0df**; being behind is not missing history. Created **milestone-lineage-continuity** without merging main. Twelve original dirty asset paths were backed up under `%TEMP%/WildTypeContinuity-5eb9e826/Preexisting`, hashed, compared against 982075e and preserved byte-for-byte.

[Exact asset inventory, hashes and disposition](Documentation/LineageContinuity/ASSET_REVIEW.md): five mesh **name-only** diffs match the existing builder, and both prefabs are semantically identical after resolving regenerated local object IDs (19/19 and 45/45 objects, zero canonical property differences). These seven safely understood files are incorporated unchanged. Four material/preset files have no normalized content change and are not staged. The saved scene remains intact and uncommitted: besides equivalent serialization and three descriptive scenery names, its substantive difference is removal of the sun's URP additional-light-data component. No transform, collision, terrain, camera, seed, creature or survival setting difference remains after canonical comparison. Intended component removal is ambiguous; an exact unapplied scene patch is provided for review. No scene regeneration, art pass or separate RunAudit edit.

The equivalent-asset reconciliation is isolated in commit **4809a9d**, separate from the lineage repair. It retains the original Unity-serialized bytes, including an existing blank-name trailing space; source/document whitespace checks exclude the preserved raw patch and serialized assets rather than rewriting user files.

## Diagnosis and scope

The reported screenshot state—controlled **#017 Gen 1**, parents **#001/#002**, one child born, living **#019**, deaths zero—is consistent with changing journal perspective. The HUD counts belong to **#017**, not its former controlled parent. #017 itself belongs in Chronicle as **You**, not in its own descendants. Siblings are not descendants. These observations alone do not show a death; the original ephemeral run/archive is unavailable, so **whether a particular first child died in that earlier run cannot be established**.

Confirmed history defect: the old Chronicle union contained only the current actor's ancestors/self/descendants. Earlier controlled creatures' other children and their branches vanished from the available history after transfer even when alive. Its Locate buttons also shared control-target eligibility, preventing those relatives being located. No missing *living biological descendant of the current actor* was reproduced. Paging retained all five test children. The repair addresses discoverability and scope without inventing parentage or mortality.

### Natural baseline, saved scene, native inputs

Before applying production changes, observed **239.20 simulation seconds** at normal time in the actual local saved scene. AI and survival ran normally, with **12 births, 1 death, end population 24**. Native **F** initiated the player's birth (at clock 1037.82), producing **#018 Sora 1, Gen 2**, parents **#001 + #014**; Gen 2 is correct because the second parent was Gen 1. Native Skip retained the default. No food/resource refill, teleport, growth acceleration or forced birth/death was used. A temporary observer recorded all archived IDs and disabled automatic focus-loss pause. Short E/Tab taps were not consistently routed until Game view regained focus; this is reported, not counted as successful movement/eating verification.

The player founder remained idle/unfed after paying its normal birth cost and naturally starved at **age 270.35 s** (elapsed run 224.26 s). Energy reached zero before health fell at the existing 4 HP/s rule. The fatal source was **Starvation**, not premature old age. The death menu offered #018 alive at **age 186.44 s, HP 100, energy 74.75/87.57**, registered and archive-alive with no death snapshot. Native card click transferred control without refill; another natural AI birth filled the vacancy. Native Tab/Chronicle/Next-page inspection showed the deceased parent with **starvation / lived 270 s**, living ancestors/parent, and #018 as **You**, not as a missing child. At final pause #018 was age 201.38 s, alive, with zero offspring. This observed first child **did not die**. It does not prove the fate of the user's earlier first child or long-run balance.

[Machine-readable snapshots](Documentation/LineageContinuity/baseline-natural-events.jsonl) contain ID, parents, controlled ID, life state, recorded cause, actor registration, age/resources, and list membership for each birth/death/control transition. The old formatter called unrelated excluded records "Ancestor" when directly queried by the observer; those rows were never shown in the old Chronicle. The new formatter has truthful sibling/other-branch fallbacks. [Final baseline state](Documentation/LineageContinuity/baseline-after-control.json) documents all exclusions.

## Repair

- **StageSession / LineageArchive:** bounded, value-only run history of controlled IDs; Chronicle includes current ancestry plus descendants of every controlled creature. New births to an earlier controlled creature refresh the archive revision and remain discoverable. No sibling becomes a control candidate. Restart/reseed clears history.
- **StageHud / LineageChronicle:** scope names the current ID/name. Separate locate/control targets; living parents, siblings and other retained family branches have **locate only**. Current descendants still have control cards. Labels distinguish You, Parent, Child, Descendant, Ancestor, Sibling (including half-sibling), and Other family branch; exact parent IDs are retained. Recorded fatal cause and age survive corpse cleanup. Missing actors remain **Unavailable · no recorded death**. Existing card/page bounds stay unchanged.
- **DescendantLocator:** accepts currently living known family-history members, without changing ownership, walking or care. Existing one-target pulse, expiry and lifecycle clearing remain. No targets outside that known family are accepted. Death-menu Locate remains disabled.
- **FamilyNames / GenerationLoop:** every registered Gen 0 founder gets the same fixed stable-ID cosmetic hash name as existing newborn defaults, **without a child-order suffix**. No RNG sampling, genetics/phenotype/AI/ecology mutation, or prompt for founders. Existing numeric newborn order, player override and duplicate-name ID disambiguation remain. Names are bounded by 512 and re-created deterministically on restart; reseed changes the ID domain.

## Automated checks

Initial complete Edit Mode: **114/114 passed**. Focused Play Mode (continuity + Chronicle + Locate): **9/9 passed**, 111.4545894 s. New coverage: four Edit cases for history union, truthful siblings/half-siblings, unrelated founders and full 512-record bounds; two Play cases for five controlled-parent births across two pages, growth/adult reproduction, transfer and subsequent child, earlier-sibling Locate without control/care, recorded fatal cause/corpse cleanup, all 13 founder names, restart-identical genomes/names, reseed, bounded/reset controlled history and stale-target clearing. Existing naming reset assertions now expect 13 named founders rather than zero entries.

The controlled Play fixture freezes brains, co-locates partners, refills resources and accelerates normal cooldown/growth. Its deliberate fatal damage is **not natural-death evidence**. Diagnostic lines record IDs/parents/current controller, archive and actor state, death evidence, list membership and relationships before/after transfer and cleanup. Existing starvation, aging, food, genealogy, camera, care, Locate, pause/restart and long-run caps remain regression-tested. No survival/lifespan tuning was performed.

Final complete suites, after removing the temporary Editor helper and its .meta: **114/114 Edit Mode passed** (0.2123321 s) and **37/37 Play Mode passed** (483.3222383 s), **zero failures, zero skipped**, both Unity processes exit **0**. Unity 6000.4.7f1 ran graphics-capable batch tests against the original saved scene. [Suite breakdown and evidence locations](Documentation/LineageContinuity/test-results.md). Final raw SHA-256 checks confirmed all twelve protected assets still match their pre-task backups. Production/test source was not changed after these final runs.

The existing three-seed balance regressions each ran 1,200 simulation seconds: seeds 917430 / 925349 / 933268 observed births **48 / 49 / 44**, deaths **38 / 38 / 33**, starvation **1 / 1 / 0**, living **23 / 24 / 24**, maximum generation **7 / 8 / 7**, peak physical creature objects **26** in each. Replacement births resumed in all three; post-120-second population range was 21–24. These are regression observations, not new balance tuning or evidence that a favored trait spreads. Existing food/object/ancestry assertions remained enabled.

## Rendered repaired scene / limitations

Used the computer-use skill for native Unity clicks/key presses and rendered Game-view inspection at 1920×1080. The repaired edge-case session was explicitly assisted: frozen AI, partner co-location, resource refills and accelerated normal cooldown/growth produced five children of #001/#002 (#014–#018). Native Next-page clicks exposed the fifth child on page 2/2. A native card click took control of the fourth child, **#017 Tala 4**; it then had #019 through the real mating/birth path with #005. This reproduced the reported combination: controlled #017 Gen 1, parents #001/#002, one child born, #019 alive, **zero deaths**.

Native Tab/Chronicle/Next-page clicks traversed all three Chronicle pages. Page 1 showed named Gen 0 parents **#001 Asha / #002 Sela** and the first child **#014 Mira 1** as a living sibling. Clicking the disabled sibling card did not transfer control. Clicking its **Locate** resumed play with “Locating Mira 1 · #014” and the temporary off-screen left cue (11 m); control stayed #017. Page 2 retained #015/#016 and #017 as You. Page 3 showed **#018 Sora 5 — Sibling / locate only** beside **#019 Mena 1 — Child / select to control**, both with exact parents. No living record disappeared. [Living sibling and child screenshot](Documentation/LineageContinuity/journal-living-family.png).

For the death edge case only, the helper deliberately exhausted #019 and advanced its starvation vitals; this was **forced, not a natural premature death**. After corpse cleanup, native Tab inspection on page 3 retained **#019 / Child / Deceased · starvation / Lived 14 s / record only**, with no Locate button. The HUD correctly showed living children 0 / born 1, Deaths 1; sibling #018 remained alive and locatable. [Recorded death screenshot](Documentation/LineageContinuity/journal-recorded-death.png). The trace records #019 as archive-deceased, actor unregistered, absent from Living descendants and present in Chronicle. Console showed zero warnings/errors during this rendered fixture.

The natural multi-birth baseline above and this controlled reproduction serve different purposes. Neither is presented as human playtesting. The temporary Editor helper and its .meta were removed before the final complete suites. All twelve original protected asset hashes still matched after native inspection.

All rendered checks use the original dirty local saved scene, not a clean clone; its lighting-data discrepancy remains for review. Physical gamepad ergonomics, every display resolution, a standalone build and the user's exact vanished runtime session are not verified. Family history remains run-only, not persisted. Collateral family labels intentionally stay compact rather than calculating a large genealogical tree; parent IDs provide the precise links. Family counts are not evidence of genetic selection.

---

Historical Chronicle verification follows.

# WILDTYPE verification — lineage chronicle — 2026-09-25

## Lineage chronicle

### Starting state and protected local assets

Confirmed local and pushed `milestone-lineage-locating` at **806915f5554739945555a22770836cc9987ff281** before editing. Created **milestone-lineage-chronicle** from that commit. No merge into `main`, history rewrite, force-push or unrelated project edit. All twelve paths initially reported modified were copied byte-for-byte to `%TEMP%/WildTypeChronicle/Preexisting` and checked by SHA-256 after rendered inspection and final verification; none is staged in this milestone.

The eight content-changing paths are the saved **CreatureStage_Prototype.unity**, **PrototypeTree.prefab**, **Brightfruit.prefab**, and **Grass / Terrain / Scenery Fern canopy / Scenery Warm bark / Scenery Weathered rock** mesh assets. The five mesh diffs are serialized mesh-name changes (e.g. Grass → Batched meadow grass). Prefab diffs include serialized object/reference reordering and local positions; the saved scene has extensive reordered objects/positions. **Interaction gold.mat** and the three authored genome preset assets also appeared dirty but had no content diff under Git normalization. All twelve files—not just the eight substantive diffs—are protected.

Their authorship/intended integration is unresolved. This milestone does not repair, regenerate, stage, discard or save them. The rendered inspection and saved-scene Play Mode tests use the **existing local scene/assets**, not a fresh Git checkout. A clean clone therefore need not look identical to the screenshots. Chronicle behavior depends on runtime IDs/ancestry/vitals and the existing runtime-created journal, not on resolving these art/scene differences. No painterly work or new art was begun.

### Implementation and bounds

Inspected README/verification, ancestry records/archive, birth/death flow, names, descendant selection, Locate, control transfer, archive limits, saved scene and relevant tests first.

- **LineageArchive.cs:** retains one value-only confirmed death snapshot per archived identity (time plus validated fatal cause), independent of the four-event turnover ring. First evidence wins; nonfinite/pre-birth times and unknown identities are rejected. A bounded family query includes the controlled creature, its actual ancestors and transitive descendants. It uses registration order and does not equate a canonical founder ID with complete ancestry. A revision invalidates journal caches after births, removals or confirmed deaths. The archive stays capped at **512**, with no pruning or new spawn allowance.
- **GenerationLoop.cs:** at the existing fatal notification, records the same cause/time alongside the existing turnover event. Destruction/unregister without fatal vitals continues to remove availability but supplies **no death evidence**. Existing one-time death totals, cleanup and reset behavior remain. No death, survival, reproduction, energy or AI rule changes.
- **LineageChronicle.cs / StageHud.cs:** add a **Chronicle / Living descendants** toggle to the existing paused journal. Three four-line records per Chronicle page (versus the existing four living cards), using the same four-card pool and Next page control. Name, stable ID, generation, relationship, actual living stage or confirmed death cause, parent IDs, children born and age/lifespan remain readable. Only a currently living descendant has a selectable control card and Locate; all other records say **record only**. Missing actors say **Unavailable · no recorded death**. Changing control or restarting/reseeding resets page/view. The normal play HUD, care, naming, Locate effect, controls and zoom are unchanged.

The history follows the current branch: ancestors, self and descendants, not every sibling/cousin. Other parents remain identified by ID even if outside this view. Counts are descriptive, not evidence of genetic selection. No save persistence or large family-tree UI was added.

### Automated verification

Initial complete Edit Mode: **110/110 passed in 0.2448931 s**. Initial targeted Chronicle Play Mode: **3/3 passed in 31.1629437 s**. Unity exits were 0; no failed assertions. Rendered inspection then simplified the age/lifespan wording without changing rules.

Final verification after removing the helper: complete **Edit Mode 110/110 passed in 0.2335247 s**, complete **Play Mode 35/35 passed in 440.5482171 s**. Both Unity exits were 0, with **zero failures, skips or inconclusive cases**. No project parser/compile, missing-reference or gameplay runtime error was reported. The staged milestone diff passes whitespace validation; pre-existing saved-scene whitespace is untouched. All six source/test files matched the reviewed versions, and all **12 protected local asset files** still matched their starting SHA-256 hashes after the full suites. No temporary helper remains in Assets.

The existing three-seed 1,200-second autonomous regression reported births **47 / 49 / 44**, deaths **36 / 39 / 33**, end population **24 / 23 / 24**, and peak objects **26 / 26 / 26**, retaining 72 food slots. Recorded starvation/old-age deaths were **1/35, 1/38, 0/33**, respectively; unknown causes were zero. Seeds were 917430, 925349 and 933268. These are regression observations with ordinary timing variation, not a matched selection/balance study or evidence that any trait was favored.

New Edit coverage: **eight cases** for multi-generation ancestry through dead relatives, excluding sibling branches, immutable cause/time after recent-event eviction, removal without a death claim, unknown causes, three invalid-time cases, the full **512-record** archive with retained parent links, and refresh of reproduction counts. New Play coverage: **three saved-scene cases** creating real two-parent births with controlled co-location/refills/accelerated growth. They test duplicate full names distinguished by ID; deceased child plus living grandchild; actual fatal starvation cause and corpse cleanup; no care eligibility for a grandchild; only eligible Locate/control actions; text fit; control transfer with deceased ancestors; paging; paused restart/reseed clearing names, deaths, view and page; and destroyed living actors remaining unavailable rather than deceased. Existing 24-living/32-object/72-food/512-ancestry assertions remain.

These automated cases use deliberate fixtures to isolate behavior, not unaided human play. The full suite also retains existing genetics, naming, Locate, camera, care, ecology, turnover, survival and long-run regressions. No tuning was performed to influence population or trait outcomes.

### Rendered Editor inspection

Used Unity **6000.4.7f1 Personal**, DX12 and the actual local saved **CreatureStage_Prototype** at a **1920×1080 Game-view resolution**. The computer-use skill supplied native Windows journal clicks and Tab input. A temporary Editor-only helper froze AI, co-located mating partners, used the real mating/inheritance flow for two generations, automatically named the first child Tavi, accelerated its growth and refilled two partners once for the second birth. The AI-born grandchild's existing name system also produced **Tavi 1**. The helper deliberately exhausted the child's energy and advanced its normal Vitals starvation tick to create a confirmed fatal starvation record. This is a **forced test setup, not a natural ecological death or unaided human playthrough**.

After the child's corpse had disappeared, clicked **Chronicle** in the native Game view. Saw **#001 (you), #014 Tavi 1 (Gen 1, deceased child, starvation, lived 33 s, one child born), and #015 Tavi 1 (Gen 2, living juvenile descendant)**. Parent IDs linked #015 back to #014 + #005. Only #015 had Locate and select-to-control; the dead child's record was readable but not actionable. The two identical names were clearly distinguished by ID/generation.

Clicked #015's control card, then pressed **Tab**, opened Chronicle and clicked **Next page**. The new perspective showed its living ancestors on page 1 and **#014 as its deceased parent** beside its own #015 record on page 2. Parent identity/cause/lifespan survived control transfer. The Console displayed **zero warnings and zero errors** in the inspected session. The normal HUD retained its existing appearance.

The helper never saved scene/assets; it and its `.meta` were backed up outside the project and removed before the final complete suites. [Screenshots](Documentation/LineageChronicle/README.md) document both journal perspectives. Raw logs/XML, temporary-helper backup and protected assets remain outside Git in `%TEMP%/WildTypeChronicle`.

### Limitations / return-to-PC checks

Open the saved scene, reproduce, press **Tab → Chronicle**, and compare a living descendant with a deceased family member. Use Next page, Locate and the living creature card; after control transfer, revisit Chronicle to see its parents. Causes not proven by the fatal notification remain unknown, and missing actors have no invented death. Check the readability at your preferred resolution and input setup. Physical gamepad ergonomics, every resolution, a standalone build and a clean-clone visual match were not manually verified. History is run-only, bounded by the existing archive, and not a genetic-selection analysis.

---

Historical lineage-locating verification follows.

# WILDTYPE verification — lineage identity and locating — 2026-09-25

## Lineage identity and locating

### Git, scope and implementation

Confirmed local and remote `milestone-family-care` at `d44396ab98210b14b2778b0b84be669e396f35e7`, fetched that branch and created `milestone-lineage-locating` from it. `main` was not merged, advanced or pushed. Twelve pre-existing asset paths appeared dirty; eight had substantive diffs (five procedural mesh assets, two prefabs and the saved Creature scene). Those eight were backed up outside the repository and preserved byte-for-byte. All twelve paths are excluded from this milestone commit. No discard, force-push or unrelated-project edit.

Inspected the saved scene, Git history/status, README/verification, IDs/ancestry, journal, naming, family-care targeting, procedural visuals, input and tests before changing code. Survival, mating costs, AI decisions, genetics/random streams, population limits, controls and zoom are not retuned.

- **DescendantLocator** separates one temporary presentation selection from direct-child care. It uses the existing transitive living-descendant query, validates life/run ownership, resumes play with the same controlled actor, and says **Locating [name] · [ID]**. Repeat selection restarts four simulation seconds; another selection replaces it. Pause freezes the timer. Death, control transfer, restart/reseed and expiry clear it. Dead-player Locate is disabled; continuation still uses the descendant card.
- **DescendantPulse** softly blends existing renderer colors toward pale gold, at most 38%, with smooth pulses/fades. It caches/restores exact property blocks and never instantiates materials, meshes, lights or per-creature effects. **FamilyWorldCues** adds one reusable temporary name/ID/generation cue, with a small offscreen direction/distance or an honest Obscured indication. Existing nearby direct-child cues remain limited to three. No automatic walking, camera takeover or production teleport.
- **FamilyCare** retains its existing transfer rules and direct-child check. A selected grandchild is never accepted for care. A selected, nearby juvenile direct child can retain the existing target-priority convenience during the brief selection only.
- **FamilyNames** assigns every birth a pronounceable default chosen from 24 names by a fixed unsigned hash of the seeded stable ID. This is reproducible cosmetic randomization, with no Unity/System/genetics random-stream draw. Player naming overrides the default while preserving numeric parent birth order; skipping retains the displayed default. AI uses the first recorded parent's birth order. Duplicate first names (and even full names) remain unambiguous through adjacent stable IDs. Growth, death, corpse removal and control transfer do not rename entries; duplicate birth recording is idempotent. Names are run-only and bounded by the 512-record archive.
- **StageHud / StageSession** expose Locate on every living descendant card, including grandchildren, keep the optional naming prompt, and reset selection on lifecycle transitions. No broad layout redesign or new controls.

### Verification performed

Initial complete Edit Mode: **102/102 passed**, 0.2235877 seconds. Focused Play Mode (existing family-care plus new lineage-locating suite): **12/12 passed**, 73.1247855 seconds. Both Unity processes exited 0.

After removing the temporary Editor helper, the complete suites passed again: **Edit Mode 102/102 in 0.2229888 seconds; Play Mode 32/32 in 406.1159342 seconds**, with **zero failures, skips or inconclusive cases** and both Unity exits 0. The full suites include camera, inheritance, ecology/scarcity, AI, mating, growth, family care, turnover and cap regressions. No project compile/runtime errors were reported. The staged milestone diff passes `git diff --check`; existing whitespace in the user's uncommitted saved-scene changes is intentionally untouched. All ten changed/new C# source/test files matched their reviewed copies, and the eight substantive pre-existing asset diffs retained their backup hashes after verification.

New coverage adds **13 Edit Mode cases** and **four Play Mode tests**. Checks include deterministic clean default names across 512 IDs without altering Unity random state; duplicate names; finite/bounded pulse timing; offscreen bearing; direct child plus grandchild journal buttons; repeat selection and pause; unchanged controlled creature/movement intent; rejection of grandchild care without energy cost; exact property-block restoration with unchanged shared material; cue expiry/death; stable names through growth and corpse cleanup; duplicate birth-recording safety; control transfer; paused restart/reseed and stale-target rejection; AI-initiated mating assigning a default without pausing the player. Existing family-care tests now exercise Locate, including gamepad journal submission. Assertions retain the 24-living/32-object/72-food/512-ancestry bounds and finite resource/position values.

The saved-scene fixtures deliberately freeze/reposition/refill actors when isolating relationships and lifecycle behavior. The AI-name test enables an actual herbivore brain to decide and initiate mating with a co-located eligible partner; it does not fabricate the birth or edit inheritance. These are controlled regressions, not natural ecological observations or a claim of unchanged long-term balance.

The existing 1,200-second autonomous balance regressions also ran with the new presentation/name code:

| Seed | Births | Deaths (starvation / old age / unknown) | End living | Peak objects | Archive records | Max generation |
|---|---:|---|---:|---:|---:|---:|
| 917430 | 49 | 1 / 37 / 0 | 24 | 27 | 62 | 8 |
| 925349 | 52 | 2 / 39 / 0 | 24 | 26 | 65 | 9 |
| 933268 | 44 | 0 / 33 / 0 | 24 | 26 | 57 | 7 |

All retained 72 food slots and sampled 21–24 living after 120 seconds. Births after the first death were 38 / 41 / 33, respectively. These are regression observations, not a matched balance study: Unity simulation trajectories can vary with timing, and no survival/genetic rules were changed to achieve these outcomes.

### Actual rendered Editor inspection

Used Unity **6000.4.7f1**, DX12 rendering, the actual saved **CreatureStage_Prototype** scene and a **1920×1080 Game view**. The computer-use skill provided native Windows clicks/text/key input. A temporary Editor helper froze founder brains, co-located an eligible partner and sent virtual F through the normal PlayerInputBridge. Native input entered **Dave** and clicked **Name child**; the preview and resulting child were **Dave 1, #014, Gen 1**. The default preview before typing was **Mira 1**, not an unnamed ID.

The helper accelerated juvenile growth, then refilled the adult child's and one partner's reserves once, co-located them and enabled the child's actual AI decision flow. Normal courtship produced **Tavi 1, #015, Gen 2**, without a player naming prompt. The helper placed the relatives at roughly **7 m / 9 m**, froze brains and opened the journal. Both cards displayed readable names, IDs, generation and independent **Locate** buttons. The direct-child relationship query for the grandchild returned false.

Native clicks selected Dave and Tavi separately in the Game view. Each resumed play and highlighted the corresponding creature with a soft pale-gold tint while preserving its visible bands. The original **#001** remained controlled and stationary. Tavi's temporary world label showed **Tavi 1 / #015 · Gen 2 · 9 m**; it did not offer direct-child care. A repeated selection after assisted relocation behind the camera showed **Behind · Tavi 1 / #015 · Gen 2 · 39 m**. No persistent beacon appeared. The helper slowed time to 0.2× only during captures, so the normal four-simulation-second effect could be inspected; expiry cleared the selection. The helper reopened the journal after expiry for inspection convenience—production does not automatically reopen it.

The Console showed **zero warnings and zero errors** during this pass. The temporary helper and its `.meta` were backed up outside the project and removed before final full-suite verification. No scene or asset was saved by the helper. Screenshots: [LineageLocating evidence](Documentation/LineageLocating/README.md). Raw XML/logs and helper backup remain outside Git under `%TEMP%/WildTypeLineageLocate`.

This is **assisted Editor inspection, not unaided human playtesting**. Native naming, Tab and Locate buttons were exercised; births/growth/co-location and capture timing were assisted. Physical gamepad ergonomics, every display resolution/coat/background contrast and a standalone build were not manually verified. The effect is intentionally brief and subtle; long-distance/terrain-hidden targets rely on the temporary direction/name cue and can be selected again. Names have no save persistence, and repeated names intentionally require their IDs for disambiguation. No new ecological-balance claim is made.

---

Historical family-care verification follows; its Find/Unpin and skip-without-name descriptions document the earlier accepted version, superseded by Locate/default naming above.

# WILDTYPE verification — family recognition and juvenile care — 2026-09-24

## Family recognition and juvenile care

### Integration, scope and rules

Confirmed accepted `d768797775ad0a7b802eaec0f50899e5d3e2e0df` locally and remotely with a clean working tree. Fetched origin, fast-forwarded `main` from `9091ed0` to `d768797`, and pushed normally. Local checkpoint: `checkpoint-before-family-care-20260924`. New branch: `milestone-family-care`. No history rewriting, discarded user edits, scene replacement, packages, paid services, external art, zoom changes or separate RunAudit/Godot edits. Subsequent source copies were hash-guarded and backed up outside the project.

Inspected IDs, immutable ancestry/archive, generation reservations, vitals, interaction, AI feeding/mating, input, HUD, saved scene and existing tests before implementation. Reused the existing two-parent genetics, juvenile growth, food/survival and descendant-control systems without retuning them.

- `FamilyCareRules`: direct living-child relationship and finite, bounded usable-energy accounting. Maximum parent cost **18**, child efficiency **80%**, parent reserve **25% of maximum**, minimum child gain **1**. Capacity-limited or reserve-limited gifts are smaller. No nutrition multiplier, health/stamina restoration or new energy source.
- `FamilyCare`, `CreatureVitals`, `CreatureLife`, `StageSession`: validate same-run membership, living adult donor, living direct juvenile recipient, **3.5 m**, clear path, no courtship, lifetime and **8-second donor cooldown**. Transfer is instantaneous after revalidation, not a delayed target job. Cooldowns use the existing simulation clock. Each creature keeps a bounded scalar care counter/timer; restart destroys them. One player locator is revalidated on every use and cleared on control transfer/reset.
- `HerbivoreBrain`: existing staggered decisions may share locally when parent energy is at least **65%**, a nearby juvenile is below **50%**, and the same shared validation succeeds. No long-range child pursuit, AI-only energy, teleporting, new genome or guaranteed outcome. Ordinary feeding and mating continue.
- `FamilyWorldCues` / `CourtshipHeart`: three reused direct-child labels, one of which can be an explicitly pinned locator; nearby unpinned children require visible range **25 m**. Juveniles display usable energy; adults keep their ID cue. At most **12** procedural heart graphics pulse/bob between courting pairs within 40 m. Two reused target prompts identify usable food/mating. No creature mesh/material allocation per frame or VisualRoot changes.
- `PlayerInputBridge` / `GenerationLoop` / `StageHud`: **F mate**, **Tab journal**, **R share**, **E interact/eat**, **Escape menu**; gamepad west/north/right-shoulder/south/start respectively. Other bindings remain. Input hints show one device's binding. Journal Find/Unpin preserves control; descendant rows still transfer control without refill. Empty-lineage death title now says **No living descendants / Restart or reseed**.

The user's follow-up UI requests are included: remove the permanent lower-left controls legend; show usable E/F/R target prompts instead. Normal survival/family/turnover panel dimensions are **300×270 / 340×255 / 500×190**, with background alpha **.36** rather than .9. Family essentials are spaced 20-point text; full inheritance comparisons expand only in the journal. The right journal column is widened to 620 points with **four larger cards per page**, 21-point identity and 18-point detail lines; Find remains independent of control transfer. Menu/nested panels are lighter, with text outlines for contrast. Turnover retains all four events and cumulative counts. Keyboard/mouse/gamepad control reference remains in README.

Player births now open an optional paused naming prompt. Names use **numeric per-parent birth order**, e.g. Dave 1 / Dave 2, not a number word. Stable IDs remain alongside names; offspring order counts previous dead/unnamed siblings and is retained after control changes. Strings are cleaned/bounded to 16 characters plus suffix, dictionary bounded by the 512-record archive; restart/reseed clears them. No save persistence, genetics change or random-name generation. Keyboard types text; gamepad can navigate/confirm/skip, with no on-screen keyboard. F/R/E/Tab gameplay input is gated while naming.

When food and a child overlap, E (gamepad south) remains food-only and R (right shoulder) remains care-only. Prompts explicitly say **E eat** plus food name and **R share** beside the child's identity, rather than an ambiguous interaction label. The overlap test checks both prompts and proves E consumes food without sharing, then R spends energy on care.

### Verification performed

Final verification after removing the temporary Editor helper: complete **Edit Mode 89/89 passed in 0.1905257 seconds** and complete **Play Mode 27/27 passed in 365.359442 seconds**, with both Unity processes exiting 0. This includes the existing camera, inheritance, lifecycle, ecology, scarcity, turnover and cap regressions. The user's final food/child ambiguity request then changed one presentation string from E interact to E eat and added an overlap regression; that affected family suite was rerun separately (result below). No motor, zoom, resource or AI rule changed in that final wording adjustment.

Final affected-suite rerun: **FamilyCarePlayTests 8/8 passed in 30.9541078 seconds**, Unity exit 0 (`explicit-actions-final.xml`). Together with the complete run, **28 distinct Play Mode tests** have passed: the original complete 27, with seven family cases rerun and one new overlap case added. No temporary helper remains in Assets. `git diff --check` passed. Source evidence, `.meta` files, README and this report are included; generated caches and raw test logs/XML remain outside Git.

New Edit Mode coverage: 11 cases for direct versus transitive/same-lineage family, dead/invalid identities, energy conservation, reserve and recipient caps, partial gifts, full/empty/negative/nonfinite rejection; five further cases cover numeric name suffixes and safe bounded text. Initial pre-naming complete Edit run: **84/84 passed**.

New Play Mode coverage: eight saved-scene tests for actual R and right-shoulder care, held/repeated input, exact cost versus gain without healing, pause/cooldown, range and obstruction, full/dead/destroyed/unrelated targets, starvation reserve, autonomous sharing through the brain, gamepad journal Find submission, tracking/control transfer, restart/reseed, rendered heart geometry, F/Tab remapping, multiple children, juvenile-to-adult recognition, no-descendant wording, contextual food availability and smaller/translucent text layout. Naming coverage checks paused resources, gameplay-letter blocking, actual TMP submit callback, Dave 1, inherited ID retention, journal card fit, descendant control, restart clearing and gamepad skip. Multiple-child coverage asserts birth order 1 then 2. Non-naming reproduction fixtures explicitly disable only the prompt to keep simulation time advancing; the naming test re-enables production behavior. A food/child overlap case verifies both explicit target prompts and independent E/R actions. Existing growth, mating, inheritance, turnover and cap assertions remain. The existing inheritance-panel test now opens the journal to inspect the same detailed comparisons, matching the requested compact normal HUD.

The matched meal-gap fixture compares the child's actual paid transfer against an isolated Vitals component with the **same phenotype and original energy**, then advances equal food-free time. It demonstrates additional reserve/nonfatal starvation margin, not a probability estimate or guaranteed survival. Other controlled tests explicitly freeze/reposition/feed actors to isolate lifecycle and UI behavior; they are not ecosystem balance observations.

The complete-suite 1,200-second autonomous regression surveys with care enabled observed:

| Seed | Births | Deaths (starvation / age / unknown) | End living | Peak objects | Retained ancestry | Max generation |
|---|---:|---|---:|---:|---:|---:|
| 917430 | 49 | 1 / 37 / 0 | 24 | 26 | 62 | 12 |
| 925349 | 50 | 1 / 38 / 0 | 24 | 26 | 63 | 9 |
| 933268 | 44 | 0 / 33 / 0 | 24 | 26 | 57 | 7 |

All three sampled 21–24 living after the first 120 seconds and kept all 72 food slots; pending-birth, 32-object and 512-ancestry assertions remained enabled. Births continued after deaths. These are regression observations with evolving populations, not matched before/after evidence of care's survival benefit or long-term balance. Care can change AI decisions and trajectories; no trait spread or death cause was forced.

Initial focused failures were fixture assumptions: the autonomous newborn had eaten before it was frozen, so a valid partial transfer cost less than 18; a second child was placed 3.61 m away, outside the 3.5 m rule. Corrected setup/positions without relaxing gameplay rules. Actual rendered inspection found the custom heart's legacy mesh path produced no visible geometry despite active state; switched to VertexHelper mesh generation and added a rendered-mesh assertion. A temporary new assertion used an obsolete GetMesh overload; corrected it to this installed Unity version and reimported successfully. The six-test focused rerun passed **6/6 in 22.5170371 seconds**, including the new contextual HUD checks.

The added food/child overlap test initially assumed slot zero was still ripe, but the newborn had already consumed it before the fixture froze its AI. Corrected the test to select an actually available plant; no food regrowth, care or input rule was changed to satisfy it.

### Actual Editor observations and limitations

Used Unity 6000.4.7f1 Personal and the **actual saved CreatureStage_Prototype scene**, with rendering. An explicitly temporary Editor fixture froze the twelve founder brains, placed one existing eligible founder within mating range, and supplied virtual F/R/Tab keyboard events through the real PlayerInputBridge. Initial half-speed capture made the two-second courtship observable. It did not edit genomes, spawn a replacement child, refill resources or override the new child's AI. Native Windows input was used for Tab and the journal Find/descendant buttons. This is **assisted Editor inspection, not unaided human playtesting**.

- Normal F courtship produced one inherited child, #014, with its real parent IDs. The repaired procedural pink heart was visibly rendered between the parents; it disappeared after courtship.
- The autonomous child ate a real nearby plant immediately. R subsequently charged the parent about **3.2 energy** and delivered about **2.6**, rather than giving a full 14.4 into an almost-full reserve. The notice and child energy cue showed the result. Full 18→14.4 accounting is separately tested with a hungry child.
- Clicked **Find** in the actual Game view: the parent remained controlled and a single child locator followed #014 as it moved away. Native **Tab** opened the journal. The child retained its brain and reached adulthood at about **31.54 s**, with **76.278/98.45 energy**, 100 health and one ordinary meal in the observed run. This survival is not attributed solely to the small gift.
- Triggered parent death explicitly to inspect continuation (not a natural-death claim), then clicked the living adult descendant row. Control moved to #014 with resources/age preserved. A second deliberately triggered death, with no descendants, displayed **No living descendants / Restart or reseed**. The turnover panel correctly marked both fixture deaths **cause unknown**, not starvation or age.
- In the final naming/layout pass, clicked the real TMP name field, typed **Dave** with native Windows input and observed preview **Dave 1**. Clicked **Name child**; the child cue and widened journal card both showed **#014 · Dave 1**, with real juvenile energy/health and region. The journal's larger text and separate Find button were visibly readable without overflow at the inspected 1920×1080 Game-view resolution. No Console errors or warnings were shown in this pass.
- In the compact-HUD pass, virtual gamepad movement walked the actual parent to a ripe plant; the contextual prompt appeared above it. A sustained virtual E event consumed the real plant through PlayerInputBridge, and the prompt disappeared. Native brief E taps did not reliably register with this automation's timing; this is not a claim of physical keyboard/controller testing. The final wording was clarified from E interact to E eat for food/child disambiguation.

Screenshots and further final UI inspection results: [FamilyCare evidence](Documentation/FamilyCare/README.md). Raw XML/logs and the removed helper backup live outside source control under `%TEMP%/WildTypeFamilyCare`.

Care is intentionally modest and local. Youngsters can leave the parent quickly; AI does not chase them. The juvenile window remains 18–42 simulation seconds. A successful gift buys time, not protection, healing or guaranteed adult reproduction. The explicit player Find locator is convenient live location information, not AI omniscience or persistent tracking. No physical gamepad usability session, standalone build or long-term parental-care balance claim is made.

---

Historical accepted survival-pressure verification follows.

## Survival pressure and ecological balance

### Integration and diagnosis before tuning

Confirmed a clean working tree and accepted commit `9091ed0` on `milestone-ecological-selection`. Fetched origin, fast-forwarded `main` from `b4d000d` to `9091ed0`, pushed normally, preserved local branch `checkpoint-before-survival-pressure-20260924`, and created `milestone-survival-pressure`. No history rewriting, discarded files or changes to the separate RunAudit/Godot projects.

Inspected the saved Creature scene, README/verification, phenotype, vitals, actual motor, AI food valuation and relocation, food slots/regrowth, aging, inheritance, birth costs, caps and tests. Before modifying production rules, added and ran the same three-seed observation fixture described below against the accepted ecology rules. Baseline: **3/3 passed, 72.3392818 seconds**. Then made one ecological change and repeated the unchanged survey: **3/3 passed, 81.2944492 seconds**. There was no second tuning pass to force deaths in every seed.

Why old age dominated:

- The midpoint food-regrowth budget is approximately **83 raw nutrition/second** if all 72 plants are continually harvested: meadow ~46, woodland ~28, dry ~9. This is a potential supply ceiling, not actual consumption. Observed mean survival expenditure was only **17.45–19.08 energy/second** across the whole living population, excluding one-off birth charges. Even allowing for unused nutrition at the energy cap, ripe stocks were high.
- The 24-creature cap stops further population growth and mating charges when full. Parents pay 30% of their own maximum energy per birth; children start at 55%. Those rules are unchanged. Lifespan turnover opens slots, and well-fed adults refill them quickly.
- AI starts searching below at least 78% energy (or reproduction threshold + 4%), rather than waiting until critical hunger. It scores visible food by useful nutrition and travel cost, avoids prolonged targets and searches across nearby boundaries. There is no remote-food query or resource subsidy.
- Starvation remains zero-energy damage at 4 health/second, while the existing low-energy motor slowdown begins below 15%. Aging remains independent. Zero fatal starvation did **not** mean zero food stress: two baseline seeds included small amounts of nonfatal low-energy/health exposure.

### Exact, single rule change

**Consumption-dependent recovery pressure on each meadow brightfruit plant only.** Base regrowth remains 25–33 seconds. On successful consumption, the current timer is base plus the previously accumulated delay; the plant then gains 45 seconds of stored delay for its next harvest, capped at 120. Repeated immediate harvests therefore wait base, base + 45, base + 90, then base + 120; production meadow timers never exceed **153 seconds**. Duplicate/failed consumption adds nothing. No existing ripe fruit is removed.

While ripe and untouched, stored delay decays at **0.5 seconds per simulation second**, fully clearing in at most 240 ripe seconds. Empty time completes the existing countdown but does not additionally restore the reserve. A tick crossing into ripe time applies rest only to its leftover duration. This is deterministic from seed/layout and consumption history, not a random or global weather/famine event. Pause freezes both processes; restart/reseed resets them. Fixed food objects are reused.

Dry and woodland food, nutrition, placement, all 72 slots, movement costs, AI, genomes, mutation, aging, reproduction, population/ancestry limits, and zoom are unchanged. Production edits are limited to `EcologyRules.cs`, `FoodPlant.cs` and `Ecosystem.cs` (a brief existing notice explains nearby grazing). No HUD panel, scene replacement, new package, art or service. In addition to these files, source changes include focused scarcity tests, the repeatable survey, documentation and `.meta` files.

### Matched before/after observations

Method: three seeds, normal saved-scene founders and all ordinary food/AI/survival/reproduction. The inputless player becomes an ordinary AI; its dead object is retained only as a camera anchor after natural death, incapable of eating or reproducing. No feeding, healing, kills, relocation, mate selection or genome editing. Each run lasts approximately 1,200 simulation seconds with a fixed **0.1-second presentation/AI step and normal 0.02-second physics**, sampled each simulation second. Seeds and this fixture are identical before/after. Small Unity startup/physics timing differences remain possible: fixed random streams are not a claim of cross-machine bitwise ecosystem determinism.

| Seed | Rules | Births | Deaths: starvation / age / unknown | End living | Population range after 120 s | Max generation | Longest observed vacancy-to-next-birth |
|---|---|---:|---|---:|---|---:|---:|
| 917430 | Before | 46 | 0 / 35 / 0 | 24 | 22–24 | 8 | 11 s |
| 917430 | After | 47 | 0 / 36 / 0 | 24 | 22–24 | 8 | 7 s |
| 925349 | Before | 48 | 0 / 38 / 0 | 23 | 21–24 | 12 | 13 s |
| 925349 | After | 47 | 2 / 34 / 0 | 24 | 18–24 | 8 | 47 s |
| 933268 | Before | 46 | 0 / 35 / 0 | 24 | 21–24 | 9 | 22 s |
| 933268 | After | 47 | 1 / 35 / 0 | 24 | 21–24 | 6 | 27 s |

The population begins at 13, rises through births, and never exceeds 24. In the changed-rule surveys it finishes at 24 in all three seeds; none goes extinct. Each has 36 births after its first death. Peak observed objects were 26 and archive records 59–61 across all six runs; samples asserted 24 living/32 objects including reservations, 72 food, 512 ancestry and 512 particles. No silent respawning.

Mean ripe food stocks (minimum in parentheses); these count actual available objects, not projected nutrition:

| Seed | Meadow before → after / 32 | Dry before → after / 12 | Woodland before → after / 28 |
|---|---|---|---|
| 917430 | 18.94 (9) → 9.26 (2) | 5.95 (0) → 5.43 (1) | 21.88 (16) → 16.03 (5) |
| 925349 | 19.78 (11) → 7.54 (1) | 7.60 (2) → 4.27 (0) | 19.02 (11) → 17.01 (7) |
| 933268 | 18.30 (8) → 9.30 (2) | 5.66 (1) → 6.38 (0) | 21.04 (13) → 15.53 (7) |

Food availability recovers rather than decreasing permanently. In seed 925349, the meadow's 100-second mean falls to **2.69 ripe plants** in the 300–400 s window, later rises to **9.41** in 1000–1100 s, and ends with a population of 24. Its two fatal starvations occur before the 600 s sample; subsequent births continue. This is a spatially changing stock observation, not proof that every individual plant completed a full 240-second rest (that rule is isolated in targeted tests).

Region samples meadow/dry/woodland and sampled crossings:

| Seed | Before region samples | After region samples | Crossings before → after |
|---|---|---|---|
| 917430 | 16456 / 3835 / 7952 | 10227 / 3473 / 14563 | 490 → 584 |
| 925349 | 13825 / 2742 / 11626 | 10839 / 4299 / 12528 | 571 → 765 |
| 933268 | 14950 / 3915 / 9419 | 9020 / 3203 / 16002 | 600 → 624 |

These are repeated observations of living actors, not unique visits, preference measurements or proof of heritable migration. More woodland use is consistent with seeking alternative forage; the existing isolated AI test verifies actual discovery and consumption beyond an initially unseen region edge without teleportation, plus abandoning destroyed/empty targets.

### Inherited trait outcomes versus causal tests

In the evolving samples, body size below 1 versus at least 1 is a descriptive grouping, **not matched genomes**. Other genes, birth time, ancestry, lifespan, location and food competition differ. Low-energy exposure is the number of living actor samples below 15% maximum energy:

| Seed | Small: low-energy samples before → after | Large: before → after | Starvation after: small / large |
|---|---|---|---|
| 917430 | 20/12625 → 44/11995 | 0/15618 → 0/16268 | 0 / 0 |
| 925349 | 12/14397 → 139/10808 | 0/13796 → 148/16858 | 1 / 1 |
| 933268 | 0/12573 → 18/9676 | 0/15711 → 114/18549 | 0 / 1 |

For legs below 1.1 versus at least 1.1, changed-rule cohort counts were **44/16**, **36/24**, **35/25**; fatal starvation counts **0/0**, **1/1**, **1/0** respectively. Size-group changed-rule cohort counts were **24/36**, **25/35**, **20/40**. These small, correlated and right-censored cohorts do not establish that compact bodies or any leg gene were selected for. There is no forced spread or scripted improvement.

The separate **causal matched-size test** changes only body size (.65 versus 1.7). Both independent vitals receive the same four-meal schedule generated by one repeatedly harvested midpoint meadow plant, with ordinary resting drain: waits 29, 74, 119 and 149 s, total **371 s**. Small body ends at **74.61/74.61 energy**, large at **126.02/236.96 (53%)**, both alive. Conversely, a completely foodless gap starting full can last **271 versus 342 seconds** before zero energy: the larger reserve has a genuine opposing benefit. This isolates the physiological effect and removes travel, aging and competition; it is not a natural death or adaptation test.

Matched legs (.6 / 1.6), other genes equal: a steady 100 m walk takes **31.13 / 25.57 s in meadow**, **31.13 / 36.51 s in woodland**. Calculated ordinary energy expenditure is **23.36 / 19.19** and **23.36 / 22.92**, respectively. An initial new test incorrectly expected longer legs to cost more energy per metre than short legs in woodland; it failed (72/73 Edit tests passed), revealing the existing speed-scaled movement drain. Corrected the unsupported test assumption, not the production survival formula. Long woodland legs arrive later and cost more than their own open-ground journey, but still cost slightly less energy per metre than the short-legged peer. No duplicate penalty was added to make a desired story true.

### Automated and Editor verification

Initial complete suites after correcting the one new test assumption: **73/73 Edit Mode** (0.1831569 s), **20/20 Play Mode** (339.4369966 s), zero failures. The complete Play suite reran the three-seed survey with the same birth/death totals. The older variable-frame 16x, 1,200-second ecology soak additionally observed **47 births, 36 deaths (1 starvation, 35 old age), 24 living, generation 10**, peak 27 creature objects; this is a separate run, not substituted into the matched-seed table.

New focused coverage: repeated harvest limits and unchanged other habitats/nutrition; no duplicate-harvest pressure; ripe rest versus empty time; crossing-tick frame independence; invalid/nonfinite input rejection; matched size/leg resource tests; pause while depleted and while ripe; paused restart/reseed resetting delay, objects, ancestry, births/deaths; and the three bounded evolving-population surveys. Existing full input/camera/survival, AI relocation/feeding/stale targets, real motor tradeoffs, inheritance/mutation, juvenile growth, descendant control, natural lifespan, turnover, capacity and 600-second regression all ran unchanged.

**Final clean suites, after removing the helper: 73/73 Edit Mode passed (0.1784286 s), 20/20 Play Mode passed (339.3691991 s), zero failures or skips; both Unity processes exited 0.** Files: `%TEMP%/WildTypeBalance/final-edit.xml` / `.log`, `final-play.xml` / `.log`. No parser/compiler errors, missing references or gameplay exceptions were found. Rendering-capable Unity 6000.4.7f1 batch runs used the saved project, without `-nographics` or new packages.

The final three fixed-step surveys retained starvation totals **0 / 2 / 1** and ended at 24 each. The third seed changed to **51 births / 40 deaths (1 starvation, 39 old age), generation 8, 64 archive records, 27 peak objects**, rather than the comparison run's 47/36. The first two retained 47/36, though sample crossings in seed 917430 varied. The old variable-frame soak in this final run observed **47 births / 37 deaths, all old age, 23 living, generation 9**, instead of its earlier 1-starvation outcome. These reruns underline the limits of the short samples and startup/frame timing; they are not hidden or substituted into the matched comparison. A final reporting-only cleanup corrected the initial 100-second window's denominator to include its initial sample; the whole-run means and other windows above are unaffected.

### Actual saved-scene Editor observations

Used the computer-use skill for native Unity window, Play/stop, pause, restart, reseed and Console inspection at Full HD. A **temporary Editor-only inspection helper** supplied a virtual gamepad through the unchanged `PlayerInputBridge`, following selected food sites with normal local steering, and saved unedited Game-view captures. **No teleport, energy/health refill, food manipulation, genome edits, injected births/deaths or AI disabling** occurred. Time scale was 1–3x during the observation. This is assisted input in the actual Editor, not a human/physical-controller playtest.

- The saved seed 917430 run began with normal founders. Near the spawn, AI ate brightfruit #0 at about 15 s, it returned at 48 s, was consumed again at 54 s (about 72 s remaining), and later reached the capped pressure of 120. Empty stems were visible. The player had 61 energy while resting by the depleted meadow patch in the first evidence image.
- Virtual left-stick input moved the hungry player from `(0, 1.97, 2.73)` to approximately `(-48.29, .19, -7.35)` in Fernwood; recorded moving duration 13.8 s, energy 34.31 at arrival. Local motor limits, expenditure and the existing food prompt remained active. The first woodland meal at about 216 s restored **22 energy** through ordinary South-button input (captured at 40/100); a second at about 284 s also succeeded. No direct `Eat` call or refill was used by the helper.
- While the player was away, brightfruit #0 naturally became ripe at **246.4 s** with stored delay **119.8**; by **282.5 s** it was still ripe and delay was **101.8**, demonstrating the new rest recovery in the live scene. Later AI ate it again. Full 240-second rest is verified by isolated tests, not claimed from this short inspection.
- A return trip to the meadow took 13.5 s and ended at a plant another creature had already depleted. Moving to another nearby ripe plant at low reserves slowed normally. That plant was also consumed by an AI before the delayed virtual eat request; the player gained no energy. Waiting while arranging screenshots was costly: it naturally starved at about **399.8 s**. This is an observation of missed timing/competition and long inspection pauses, not a controlled measure of human difficulty or autonomous selection.
- Before that player death, the ordinary ecosystem reached 24 living/11 births, then four natural old-age deaths opened slots. Births #025–#028 restored population to 24/15 births. The visible four-entry log retained **old age #007** under **birth #028, generation 4**, and subsequently added **starvation #001** while cumulative deaths became 5 and population 23. No death or replacement was staged. There was no living player descendant: the normal death menu paused and offered restart/reseed, without creating a replacement.
- Clicked native **Restart Prototype**: observed 13 founders, zero births/deaths, empty history and zero nearby plant pressure. Pressed Escape: verified the simulation clock, player energy and a depleted timer stayed unchanged across observations. Clicked **Reseed Ecosystem** while paused: seed changed 917430 → 925349, again 13 founders, zero counts and fresh food. Logs confirm both normal restarts.
- The restrained grazing notice rendered on the existing line without overlapping the Full-HD panels. Birth/death HUD remained readable. Native Console showed **0 gameplay warnings / 0 errors** throughout. The existing cold-start Input Manager deprecation advisory is not a new runtime error.

The temporary helper and its `.meta` were backed up outside the project and **removed before final suites**; it never became a saved scene component. Input settings were cloned/restored and the virtual device removed on leaving Play Mode. Scene/prefab assets remain unchanged. [Unedited captures and captions](Documentation/SurvivalPressure/README.md).

### Remaining balance risks

This is evidence of **modest food-driven survival pressure and recovery**, not convincing long-term genetic adaptation. One changed-rule seed still has zero starvation, intentionally accepted. Old age remains the dominant fatal cause; woodland remains a generous refuge, and the cap is often reached. No optimum genotype, equilibrium or sustained population resilience beyond these short runs is proven. More seeds, longer observation and human play may reveal camping strategies, unlucky AI routing or excessive crowding. The local steering system is not full pathfinding.

All raw XML/logs remain outside source control under `%TEMP%/WildTypeBalance`. Baseline/after observations above use `baseline.*` and `after.*`; complete-suite reruns are separately identified rather than silently replacing the recorded comparison. The fixed-step survey is retained as an automated regression, not embedded in the scene or runtime build. Temporary Editor input/capture assistance is removed before final verification. No standalone build, physical controller usability benchmark or long-term balance claim.

---

Historical accepted ecological-selection verification follows; the section above describes the current balance milestone.

## Ecological selection

### Scope and integration

Confirmed accepted commit `b4d000d` locally and a clean working tree. Fetched and checked the remote, preserved checkpoint branch `checkpoint-before-ecological-selection-20260924`, fast-forwarded `main` from `8407155` to `b4d000d`, and pushed normally. This milestone is on `milestone-ecological-selection`. No rewritten history, force push, unrelated project edits or discarded work.

Inspected the saved scene and terrain/scenery builder (region edges x ±35), the 72-slot food registry, regeneration, player/AI movement and consumption, metabolism and inherited phenotype, reproduction/mutation, ancestry/object caps, HUD and all relevant tests before changing rules. The original source/preset/scene architecture remains.

Changed production files: new `World/EcologyRules.cs`; regional placement/visible-forage/observations in `Ecosystem.cs`; shared-material regional food appearance and defensive regrowth in `FoodPlant.cs`; local food valuation, finite memory/search and obstruction recovery in `HerbivoreBrain.cs`; a smoothly blended woodland steering-derived travel cap in `CreatureMotor.cs`; existing notice/food prompt/region text in `StageHud.cs`. New files include `.meta` files. No scene/prefab asset replacement, new gene, artificial improvement, evolution setting, reproduction threshold, metabolism formula, population cap, zoom, package or paid-service change.

Region quotas are **32 meadow / 12 dry / 28 woodland**, totaling 72. Meadow food: 42 nutrition / 25–33 s; woodland: 22 / 18–26 s; dry: 72 / 85–115 s. All raw values still pass through the same nutrition factor and maximum-energy clamp. Regrowth reuses the same objects. Woodland clearance caps original travel by existing turn rate × .45 m, blended over an 8 m boundary strip; sprint cannot charge stamina when it cannot provide a meaningful speed increase. Existing juvenile/fatigue/energy rules still apply. No weather cycle: depletion and recovery provide the local availability change.

### Verification record

Final complete suites after removing the temporary harness: **68/68 Edit Mode passed in 0.1778256 seconds; 16/16 Play Mode passed in 257.6401841 seconds**, zero failures or skips. Both Unity batch processes exited 0. Results/logs are retained outside the source tree at `%TEMP%/WildTypeEcology/final-EditMode.xml` / `.log` and `final-PlayMode.xml` / `.log`. Runs use the installed Editor with `-batchmode -runTests -testPlatform EditMode` / `PlayMode`, explicit results/log paths, and rendering (no `-nographics`). No automated check is described as human playtesting.

First implementation run: **68/68 Edit Mode passed** (0.2115534 s); **15/16 Play Mode passed** (255.1454942 s). The failing new relocation assertion demanded an extended-search counter before the AI could eat. Diagnostics showed the AI had already crossed from x=32 to x=60.51 and eaten the initially unseen sunpod during its initial exploratory walk (energy 96.11, alive). The test now verifies that successful cross-boundary meal, then removes all ripe food and verifies a separate extended search. The focused rerun passed (1/1, 4.4805727 s). Existing regression expectations were not weakened. An AI refinement also skips a temporarily rejected target in the forage query so other visible food remains selectable.

New Edit Mode coverage: saved region edges; 10,800 deterministic candidate comparisons across 50 seeds without Unity RNG consumption; finite/bounded nutrition/regrowth variants; matched-genome travel advantage reversal; continuous/no-bonus canopy transition; reserve versus maintenance cost; actual food timer frame independence and invalid ticks; 200 normally mutated descendants evaluated in all regions. Existing inheritance isolation/parentage/mutation tests continue unchanged.

New Play Mode coverage: exact regional quotas and collision-free sites, consumption and individual recovery in all 72 slots, pause, deterministic restart and changed reseed, actual two-genome motor speeds in two regions, player nutrition and depletion in all regions, initially unseen AI forage across an edge, destroyed target handling and hungry extended search. A 1,200-second autonomous run checks finite actors, reproduction after deaths, multigeneration ancestry, all region use and 24 living / 32 objects / 72 food / 512 ancestry / 512 particle bounds. The original 102-check/600-second survival and camera regression, generations, death/continuation, turnover, capacity and visible-inheritance suites remain in the complete run.

Autonomous-run method: seed 917430, normal starting genomes, food, lifecycle and reproduction. The inputless player is given the ordinary AI brain so the run does not depend on a human feeding it. No creature is fed, healed, moved, replaced or assigned a favorable mate/genome. When that observer creature naturally dies, only its dead object is retained for camera/reference safety; it cannot consume or reproduce. Simulation runs at 16× with normal .02 s fixed steps and samples living creatures every ten simulation seconds. The retained corpse counts against the object cap. This is distinct from the older regression's explicitly fed-player soak.

Final bounded-run observation: **1,200.9 simulation seconds, 46 births, 35 deaths, 24 living, generation 10 reached**, peak 24 living / 25 objects including the observer corpse, 59 archive records. Births continued after deaths freed slots. Recorded fatal sources were **35 old age, 0 starvation, 0 unknown** in this run. Across ten-second samples, region occupancy counts were meadow 1,420 / dry 353 / woodland 1,051, with 312 sampled crossings. These are repeated actor observations, not unique visits or proof of preference.

| Final location | Living | Mean body size | Mean leg gene |
|---|---:|---:|---:|
| Meadow | 14 | 1.119 | 1.090 |
| Amber flats | 3 | 1.068 | 1.148 |
| Fernwood | 7 | 1.061 | 1.095 |

The 13 founders began with mean size 1.064 and leg proportion 1.113; final whole-population means were 1.096 and 1.099. No assertion demands these values or that a favored allele spreads. The first implementation run instead ended at 49 births / 38 deaths / mean size .955 / legs 1.162, also at generation 10; small timing/AI changes alter who mates and survives. **No convincing regional genetic adaptation or stable equilibrium is demonstrated here.** The matched-genome motor tests establish a causal local tradeoff; these ecological runs establish bounded turnover and viable reproduction. The zero-starvation final run is a real balance risk: food can be generous for this seed and cohort. The separate ordinary Editor session did exhibit a natural starvation/replacement event without intervention.

### Actual Editor inspection

Opened the actual saved `CreatureStage_Prototype` scene in Unity 6000.4.7f1 Personal with rendering. A temporary Editor-only harness initially positioned the existing player near an edge to avoid spending the inspection on travel. It then supplied movement intent through the **normal motor, growth, fatigue and survival rules**, with ordinary AI and food still running. It did not refill resources, edit genomes, spawn replacement creatures, alter food placement or change reproduction. Native Unity UI was used to inspect the Game view and Console and to start/stop Play Mode. All harness files are removed before delivery and final tests.

- Meadow → Fernwood: x=-28 to -41.78 at z=-91.69, about 13.8 m of actual travel in 3.96 s. The blended local walking limit was 3.19 m/s at arrival. Low violet fernberries are distinct from meadow coral fruit; consumed fruit disappears but the plant remains. The normal E prompt names fernberry. A sustained virtual E through the unchanged PlayerInputBridge consumed one and returned energy to 99.91/100.
- Meadow → Amber flats: x=28 to 48.21 at z=-70.30, about 20.2 m of actual travel in 5.56 s, local walking limit 3.73 m/s. Tall gold sunpods and wider gaps are readable against the brown terrain. The restrained entry notice reads “Amber flats: rich sunpods, long waits. Keep a reserve.” A later virtual E consumed the visible sunpod; the captured +38 energy is the useful amount after the existing maximum-energy clamp, not a changed 38-nutrition rule.
- The ordinary simulation produced births during inspection, including later generations and a full 24-creature population. The original compact four-entry birth/death panel and cumulative counters remain readable. No extra ecology number panel was added; global food inventory was removed from the normal HUD.
- While observing normal recovery, the consumed sunpod became ripe again without timer manipulation. Later an AI consumed it. The live HUD recorded **#021, generation 1, starvation**, then **#025, generation 3, birth** while population returned to **24** (12 births / 1 death). The starvation entry stayed visible beneath the replacement birth. Neither event was injected by the harness. Console showed zero gameplay errors/warnings during inspection.
- Native automation's brief E taps did not register reliably. The successful consumption evidence uses a .25-second virtual key state in the real Editor, with cloned/restored input settings. This verifies the real input bridge and gameplay path, not physical keyboard usability. Automated suites separately cover keyboard/gamepad input. No production input workaround was added.

Unedited Game-view screenshots and captions: [ecological selection evidence](Documentation/EcologicalSelection/README.md).

### Balance and verification limits

Regional survival/mating remains emergent; no fitness score, favored allele, guaranteed mutation, genome replacement or scripted population recovery exists. A single seed and short accelerated run cannot prove equilibrium or adaptation. Crossing counts are sampled transitions, and per-region survivor means are location snapshots, not heritable migration preferences or proof of selective causation. Timing/physics/AI competition can vary between runs even though food layout and genetic random streams are seeded.

The woodland clearance rule is a region-wide abstraction, not individually simulated branch collision. Existing high agility and other genes can offset long-leg steering costs. Large reserves help meal gaps but do not guarantee success on dry flats. Abundant regions may reach the population cap readily; sparse regions and local steering need broader seed/human balance testing. No new weather, predators, stages, genes, nav package or art pipeline. No physical gamepad, standalone build, exhaustive profiler pass or long-duration human balance session was performed. The visual checks use initial placement and scripted walking, not a complete human-controlled journey across the map. Zoom and the separate RunAudit project are untouched.

---

Historical verification follows; the ecology section above is the current milestone.

## Visible, playable inheritance

### Scope and checkpoint

Started from clean `main` / `origin/main` at `8407155`. Inspected the saved scene, creature prefab, genome/presets, phenotype rules, procedural model, input/motor, life/lineage/mating, survival, HUD and existing tests. Preserved a local checkpoint branch `checkpoint-before-visible-inheritance-20260924`; work is on `milestone-visible-inheritance`. No Godot, separate RunAudit, package, camera/zoom, genome schema, mutation probability or population-limit changes. The existing GitHub README and prior history remain intact.

Permanent code changes: `CreatureAppearance.cs` (new deterministic presentation inputs), `CreatureVisual.cs` (silhouette/limbs/coat mesh), `CreatureMotor.cs` (existing turn-rate limit applied to travel), `InheritanceSummary.cs` (new birth-time comparisons), `GenerationLoop.cs` (stores summaries) and `StageHud.cs` (compact comparison and descendant rows). Tests: new `VisibleInheritanceTests.cs` and `AppearancePlayTests.cs`; the legacy multi-part visual assertion now counts nested renderers instead of relying on ten direct children, because coat bands are nested under the torso. README and this record explain the result. New Unity source files include their generated `.meta` files.

### Automated verification

- Unchanged baseline: **52 Edit Mode + 9 Play Mode passed**, zero failures.
- Final complete suites after removing the temporary Editor fixture: **60 Edit Mode passed in 0.0854998 seconds + 12 Play Mode passed in 174.8295634 seconds**, zero failures; both Unity processes exited 0. Final XML/logs are under `%TEMP%/WildTypeInheritance/final-EditMode.*` and `final-PlayMode.*`.
- Eight added Edit Mode cases: deterministic appearance and untouched Unity random state; invalid-input sanitation without mutating source; 100 ordinary offspring combining both parents' body/leg/color inputs; 500 default probabilistic-mutation samples with shorter, longer and unchanged legs and all gene bounds checked; long-leg speed/steering opposition; broad-body reserve/food/acceleration opposition; planar 180-degree reversal, timestep agreement and braking; parent comparisons and neutral mutation wording. Existing inheritance isolation, parentage and mutation-bound tests also pass.
- Three added full-scene Play Mode cases: coat topology/shared material/stable mesh across frames and released mesh on destruction; two normal inherited generations, juvenile-to-adult visual continuity, descendant control, measured walking speed, resource consumption and parent summary retention after death; paused restart/reseed reproducing same-genome appearance, releasing old meshes, clearing counts and respecting population bounds. Inheritance HUD text is asserted not to overflow.
- Existing 102-check / 600-simulated-second movement, camera, food, survival, input and restart regression passed. Existing reproduction, natural lifespan, player-death continuation and turnover tests passed. Fatal starvation remains visible after an inherited replacement birth restores the previous population, including corpse cleanup and pause. No change to cause attribution.
- Autonomous generation soak: **11 births, 24 living, peak 24 objects, 24 archive records over 126.2 simulated seconds**. Controlled capacity fixture separately reaches 24 via 11 births, rejects another mating without spending energy, then confirms juvenile starvation frees a slot. Limits remain 24 living / 32 objects / 72 food / 512 particles / 512 ancestry records.
- An initial Play Mode run caught a new material-property-block initializer being called from a MonoBehaviour constructor. It was moved to the build phase and the complete suites passed afterward; the failure was not suppressed.

These are automated tests and controlled checks, not human playtesting. Test runs use the installed Unity 6000.4.7f1 Editor with `-batchmode -runTests -testPlatform EditMode` / `PlayMode`, the saved project and explicit XML/log paths; no `-nographics` rendering substitution.

### Actual Editor inspection and evidence

Used the computer-use skill to operate the real Unity Editor and inspect its Full HD Game view and Console. A **temporary Editor-only fixture** arranged existing founders, used the normal eligible two-parent mating/birth path, supplemented food and accelerated real growth to compare generations. Genomes, inheritance rules and mutation probabilities were not replaced. Temporary nameplates and fixed comparison-camera placement are evidence aids, not permanent game features. The fixture froze AI/aging at the end only to permit stable native inspection; it and its `.meta` were removed before final tests.

Observed:

- Teal Meadow founder #001 (size 1.00 / legs 1.00), broad amber founder #003 (1.45 / 0.86), and slim violet founder #004 (0.73 / 1.38) have legible different silhouettes, limb thickness/length, coat colors and band widths.
- Normal child #014 of #001 + #003 has a green coat and intermediate size 1.19 / leg proportion 0.93. Inspected it at juvenile scale and again at inherited adult scale: same pattern and body proportions, no visual reroll.
- #014 paired with founder #005 to produce generation-two #015 (size 0.993 / legs 1.025). The later descendant visibly returns toward the teal, slimmer other parent's appearance. The journal preserves values for both parents instead of labeling ordinary mixing as mutation.
- Pressed **F** in the native Game view, inspected the two-line descendant choice (coat/adult size/legs/resources), and activated #015's row. Control transferred to generation two and the camera retargeted. One rapid automated click was missed by Editor input routing; repeating the activation succeeded. This was not a physical-mouse usability benchmark.
- After native selection, the controlled fixture drove the actual descendant motor: measured sprint **6.107 m/s**, expected **6.107 m/s**, steering **6.744 rad/s**. Stamina fell to about **76/101**, energy to **102.1/104.6**, and a right turn changed the planar travel heading before braking. This was a scripted movement measurement in the actual Editor, not sustained human WASD play.
- Read the parent/child comparison, reserve/cost rows and two retained birth events in active play and the paused journal. No clipping at 1920×1080; Console showed **0 gameplay warnings / 0 errors** in the controlled session.
- After final tests, reopened the ordinary saved scene without the fixture. Observed autonomous births (three before restart, four in the next run), the full four-entry turnover panel, and no runtime warnings/errors. Used the native paused journal to restart: restored 13 creatures, founder identity and zero births/deaths with an empty history. User input was detected afterward, so the Editor was left running for the user rather than interrupting it for another native reseed; paused reseed is verified by the automated suites. Cold startup still emits the existing Input Manager deprecation advisory, unrelated to this milestone.

Unedited Game-view captures: [founders](Documentation/VisibleInheritance/01-founders.png), [parents and juvenile](Documentation/VisibleInheritance/02-parents-juvenile.png), [same child as adult](Documentation/VisibleInheritance/03-parents-adult.png), [generation-two family](Documentation/VisibleInheritance/04-generation-two.png), [controlled descendant](Documentation/VisibleInheritance/05-descendant-control.png). Captions explicitly identify the controlled fixture. Source code, not screenshot annotations, implements the inherited appearance.

### Remaining limitations

Existing procedural rigid-part gait still has no foot-contact IK; sliding/slope intersections and local AI steering imperfections remain possible. Bands share one pattern family and vary continuously in width using the existing leg gene, not a new independent pattern gene. RGB blending can yield muted colors; no mutation or stronger descendant is guaranteed. Agility, speed, metabolism and efficiency also affect final stats: leg/body tradeoffs are comparisons with other genes held equal.

HUD was visually inspected at Full HD, not every aspect ratio. No physical gamepad, standalone build, profiler benchmark, exhaustive allocation audit or long-term natural-selection balance session was performed. Birth-site/food assistance in the visual fixture is not evidence of natural mating success; the independent autonomous regression supplies that evidence. Rare important mutations were covered by tests/code, not a staged rare-mutation Editor screenshot. Session-only ancestry and the existing survival/ecosystem limits remain; no extra stage or art pipeline was begun. Zoom is unchanged as requested.

---

The following sections preserve historical milestone verification; the visible-inheritance section above is the current handoff.

## Follow-up: zoom response and turnover visibility — 2026-09-24

- Zoom-only commit `30f9ae3`: changed `OrbitCamera` wheel multiplier from `.8f` to `2f`; limits and smoothing unchanged. Tested wheel input in both directions in the native Unity Game view. Existing `FullVerticalSliceAndBoundedSoak` passed all 102 checks, including wheel zoom and camera collision/recovery (77.6051737 seconds).
- Added cumulative Deaths beside Population/Births and a four-entry, newest-first birth/death panel. Value snapshots retain creature ID, generation, parent IDs and the existing canonical founder lineage after corpse removal and control transfer. Entries persist until evicted by newer events; totals persist until restart/reseed. Founder spawning and cleanup are not deaths or births.
- Causes are explicit fatal-source metadata: starvation from survival drain, old age from the inherited-lifespan path, otherwise unknown. Nonfatal starvation, low energy or advanced age do not imply a cause. Duplicate death notification/corpse cleanup cannot increment Deaths twice.
- **52 Edit Mode tests passed, 0 failed** (0.0716669 seconds), including bounded history with cumulative totals, reset, ancestry snapshots and fatal-cause timing/unknown fallback.
- **9 Play Mode tests passed, 0 failed** (163.2374235 seconds), including the existing full regressions. New turnover fixture: death reduces population from 13 to 12; a normal inherited birth restores 13 within 2.2 simulation seconds; the Deaths total and death entry remain visible, including after corpse cleanup and while paused. HUD text includes both events, IDs, generations, parents and lineage without text overflow. Paused restart/reseed clear events and totals; destroying a living object is not reported as a gameplay death. Existing fed-lifespan test additionally confirms an old-age event from the real lifespan path.
- Inspected the actual rendered Editor HUD at 1920×1080 using computer-use. A **temporary Editor-only controlled fixture** produced three fatal-starvation events and one normal inherited birth to fill all four rows. Observed Population 11/24, Births 1, Deaths 3, newborn #014 / Gen 1 / parents #001 + #002 / lineage #001, plus all three death rows with starvation labels. Panel remained readable below the family journal while paused, with no clipping; also checked active play. Console showed 0 gameplay warnings/errors. This was controlled visual verification, not a natural ecosystem or human playtesting session. The temporary fixture and its metadata were removed before delivery.
- Evidence outside Git: `%TEMP%\WildType2A2\zoom-regression.xml`, `turnover-edit.xml`, `turnover-play.xml` and corresponding logs. No new gameplay feature beyond turnover visibility, no packages/assets, and no survival balance changes. Existing limitations below still apply; physical-wheel feel across other mice and display sizes remains a human judgment task.

The sections below retain the original 2A.2 verification record; the follow-up counts above are the current complete-suite results.

## Environment and scope

- Existing Unity project: `C:\Users\jsayl\Desktop\UnityProjects\WILDTYPE`.
- Unity 6.4 **6000.4.7f1**, URP 17.4.0, Windows 64-bit, Direct3D 12 on NVIDIA RTX 4070 SUPER for rendered Editor checks.
- Existing scene: `Assets/WildType/Scenes/CreatureStage_Prototype.unity`. No packages, external art, paid assets or gameplay services added.
- Inspected README, previous verification/handoff, scene, prefab architecture, genetics, lineage, eligibility, AI, survival and tests before implementation.
- Confirmed 2A.1 supplied functioning source-level inheritance/mutation/lineage/eligibility, but no live age, mating, births or descendant controls. Its pre-change baseline passed **44 Edit Mode tests** and the existing **102-check Play Mode regression**.
- Preserved the existing light/preset/source edits in checkpoint `dfe62b9`. The user subsequently confirmed the RunAudit files were an accidental copy and had removed them. Only its orphan scene object, empty asset directory and folder `.meta` were cleaned from WILDTYPE; no separate RunAudit product was changed. The orphan folder metadata remains recoverable in the task's temporary cleanup folder, and the original scene is in the checkpoint.

## Automated tests

Final Edit Mode suite: **49 passed, 0 failed**, 0.132115 seconds reported by Unity Test Framework.

Final Play Mode suite: **8 passed, 0 failed**, 158.0787504 seconds reported by Unity Test Framework, exercising the saved scene in a rendered-capable batch run. Both final Unity test processes exited with code 0.

The autonomous generation soak observed **11 births, 24 living creatures, peak 24 creature objects, and 24 lineage records over 126.2 simulated seconds**. The separate capacity fixture also reached 24 living creatures, rejected another mating without charging resources, and verified that juvenile starvation freed a slot. The fed-lifespan fixture verified natural AI death at simulation clock 1365.2 (about 365.2 elapsed simulation seconds). The original 102-check regression completed its separate 600-simulated-second soak.

Coverage:

- All original genome validation, preset/runtime isolation, phenotype tradeoffs, deterministic variation, stamina, energy, starvation and invalid-input tests.
- Existing 2A.1 inheritance, signed probabilistic mutations, bounded mutation history, normalized genetic distance, lineage and eligibility cases.
- New meaningful health/invalid-age checks; child/sibling and mutation-record isolation; retained dead-parent and transitive ancestry; orphan rejection; hard archive cap; 1,000 successive inherited/mutated genomes checked against every gene range, with positive, negative and absent mutations observed.
- Original **102-check, 600-simulated-second** survival/movement regression: virtual keyboard/mouse/gamepad, locomotion/sprint/braking, camera collision, meals and regrowth, AI feeding and destroyed food targets, pause, restart/reseed and starvation. This legacy test explicitly disables the generation clock to preserve its original 13-creature assumptions; it is not a reproduction soak.
- Native game-code mating through virtual `M` and gamepad west; virtual `F` and north-button journal; actual descendant-button callback; camera retargeting; control transfer without healing/refill; old player becomes AI.
- Paused courtship freezes age and births. Completed births spend both parents' energy, set cooldowns, create independent genomes and retain exact parent IDs. Children are initially smaller in both visuals and collision, eat, spend energy, grow and reproduce into generation two.
- Player death pauses without replacement; living descendants can continue; no-descendant death retains restart/reseed; dead or ancestral transfer targets are rejected.
- Partners dying, being destroyed or moving away cancel courtship; no delayed birth charge. Paused reseed and repeated restart calls clear reservations, old objects and ancestry.
- Autonomous ecosystem births with only the test player supplemented with energy; living/physical/reservation/archive bounds and finite transforms asserted throughout.
- Controlled capacity fixture reaches **24 living creatures via 11 inherited births**, rejects another mating without spending energy, then kills a juvenile by starvation and verifies corpse cleanup and retained lineage.
- Separate fed-creature lifespan test confirms natural AI death at its inherited lifespan while retaining the dead ancestor record. This fixture supplements food to isolate aging from starvation.

These are automated tests and controlled fixtures, not human playtesting. Test XML/logs live outside source control under `%TEMP%\WildType2A2`; final runs use `edit-verified.xml` / `.log` and `play-verified.xml` / `.log`.

## Native Editor and rendered-scene inspection

Used the computer-use skill to operate the actual Unity Editor and inspect its rendered Game view at Full HD (1920×1080, displayed scaled inside the Editor).

Observed terrain, vegetation, food, lighting/shadows, AI movement, autonomous births and visibly smaller juveniles. Inspected the survival HUD, family HUD, empty/populated journal and descendant row without overlap or clipping at this resolution.

The native pass caught brief `F`/`M` taps not consistently reaching the Input System through Editor routing. Extended the existing Escape IMGUI fallback to those actions with held-key and per-frame duplicate protection, then retested successfully. AI now retains an eligible partner between decisions instead of randomly abandoning the approach each decision interval.

Actually performed through native controls:

- Pressed `M`, observed the courtship message, a player offspring birth, and the parent's energy cost.
- Pressed `F`, saw living juvenile **#017 / generation 1**, clicked its row and transferred control. The child's retained resources, parents **#001 + #002**, age and inherited vision comparison appeared in the HUD.
- Resumed as the juvenile, observed growth into an adult, pressed `M` again, and saw living offspring **#023 / generation 2** in its journal.
- Observed **23 living creatures / 10 births** in this native session; did not claim this was a full-capacity stress test.
- Clicked Restart while paused: returned to founder identity with cleared family history. Clicked Reseed while paused: restarted at 13 creatures / zero births / 72 foods and changed seed from 917430 to 925349.
- Inspected Console after these operations: **0 warnings, 0 errors**. No missing RunAudit script remained.

After the final automated suites and Git reconciliation, reopened the saved Editor, entered Play Mode, and pressed `F` again. Confirmed the improved empty-family guidance and hidden unnecessary pagination, with **0 gameplay warnings/errors**. Cold Editor startup does emit Unity's Input Manager deprecation advisory because the existing project retains Active Input Handling **Both**; this is not a missing-script or runtime failure. Kept that existing input configuration rather than changing baseline compatibility during this milestone.

Native mouse automation and short key taps are not equivalent to sustained human controller play. Automated tests cover player-death continuation, starvation, full capacity and natural lifespan; those scenarios were not individually recreated through native input during this visual pass.

## Limits and remaining judgment

- Hard caps: 24 living creatures, 32 creature objects including corpses, 72 food slots, 512 particles and 512 ancestry records per run. Pending courtships reserve space; archive saturation stops births with a message. Full runtime archive saturation is covered by pure archive tests and guarded in code, not a 512-birth Editor session.
- No physical gamepad was connected. Controller input paths were exercised by virtual Input System events; hardware mapping, deadzones and navigation feel remain unverified.
- Long-term balance, mutation readability and survival difficulty need extended human play. AI uses local steering, not complete pathfinding; crowding and imperfect routes remain possible. Creature collisions remain disabled against other creatures, as in the baseline.
- Procedural rigid-mesh gait has no foot-contact IK, so feet may slide/intersect slopes. Fine dorsal markings may soften during movement; no new art pipeline or rendering overhaul was undertaken.
- No standalone build, profiler benchmark or exhaustive memory-leak audit was performed. Object/record/particle limits prevent unbounded authored spawning but are not a complete memory profile.
- Session-only lineage: no save/load, gestation, parental care, kinship restriction or species tracking. No predators, creature editor, other evolutionary stages, multiplayer or open world.

## Source control

The final handoff reports the pushed `main` commit. Local starting history and GitHub's separate placeholder README history are reconciled without force-pushing or deleting project source. Unity source, authored assets, settings, documentation and `.meta` files are included; Library/Temp/Logs/obj/builds, test evidence, IDE state and source-export ZIPs are excluded.
