# Foraging choices and descendant decisions — 2026-09-26

## Starting point and protected work

Before editing, local HEAD and GitHub `milestone-creature-profiles` both resolved to **147659817207f09e1bf7720ec7b24c7cb926fd3c**. Created **milestone-foraging-decisions**. Main remains **d768797775ad0a7b802eaec0f50899e5d3e2e0df**; no merge, rewrite or force-push.

All nine starting dirty paths were backed up with relative paths under `C:\Users\jsayl\AppData\Local\Temp\WildTypeForaging-20260926`. The exact inventory and initial hashes are unchanged from [Creature Profiles](../CreatureProfiles/REPORT.md#protected-local-files). The original scene's unresolved sun-component patch is deliberately not part of this work. Rendered checks use the preserved local assets, not a clean-clone art comparison. No RunAudit, asset pack, art pipeline, camera, input binding, genetics, survival, ecology, mating or care rule is changed.

## Diagnosis and bounded decision

Inspected the saved original and painterly scenes, actual journal and gameplay, phenotype/motor/vitals, food generation and regrowth, Vision/line-of-sight AI targeting, regional travel limits and reproduction costs. Existing rules already create opposing choices: nearby fruit can be consumed by another grazer, walking costs energy, sprinting costs additional energy/stamina, and reserves committed to birth/care cannot be spent twice. The ordinary view did not make depletion, potential gain or travel cost sufficiently legible before committing to a route. No economic retuning was justified by this inspection.

**The contained improvement is a local scouting readout**, not a new survival mechanic:

- At most two nearby visible plants, limited by the controlled creature's actual Vision, creature-to-plant obstruction and camera visibility/obstruction. The normal E prompt owns reachable ripe food. Pausing, death and a fresh run hide/clear scouting cues.
- Ripe/empty state, rounded potential gain capped by current missing energy, and a clearly labeled approximate direct-walk energy quote. No hidden food, exact recovery timer or future availability is revealed.
- A grazer is described as approaching only if it is visibly moving toward a nearby ripe plant. This uses position and movement heading, **not the AI's selected target**. It is not a claim that it will eat or win a contest.
- The estimate samples eight points along the direct path to eating reach, applying existing regional walking limits, current growth/hunger, maintenance and movement expenditure. It is not navigation or a promise: acceleration, slope/detour distance, obstacles, future hunger and time spent deciding are excluded. Sprinting is not silently included in a walking quote.

### Recorded baseline

In a fresh original saved-scene run, at simulation clock 1001.60 the founder had 99.43787 energy, 13 living creatures and zero births/deaths. Nearby slots 0–7 were ripe: slot 0 was 4.003895 m away; the others approximately 10 m. By clock 1018.84, slots 0, 1, 2 and 7 had been depleted by autonomous feeding, while slots 3–6 remained ripe. Population was 17 with four births and no deaths.

Assisted held-W input then used the normal player input and motor: 2.48301 m of forward travel consumed **0.57337 energy** (93.38087 → 92.80750), with no meal. The nearest plant was already empty before that walk; this is **not** reported as witnessing a race winner or a forced contest. The player could instead pursue the still-ripe visible alternatives or keep resources for mating/care. No free meals, teleports or forced deaths were used in this baseline.

The fresh after-image uses the same saved scene/camera but a different elapsed time; it is a visual comparison, not a deterministic A/B performance or economy experiment. A short input interrupted by pausing is excluded. A fresh post-change 0.8-second held-W sample covered the same **2.48301 m**, spending **0.56635 energy** (95.65049 → 95.08414); the before sample included 0.02 seconds more simulation time, so the tiny difference is not attributed to the UI. The automated steady-travel accounting test provides a matched setup independently.

### Ordinary player path and consequence

The original saved-scene run used seed 917430 with no placement, resource, age or genetic fixture. Movement used a temporary queued held-W input (the real player input/motor); naming, mating and journal actions used native clicks/keys. Focus-loss auto-pause was disabled for observation only. This is agent-operated gameplay, not human playtesting.

1. Nearby plants became empty through autonomous feeding. The player could wait/keep reserves, pursue farther fruit, or mate with an eligible nearby adult. Native **F** produced **Sora**, ID `917430:018`, generation 2, with recorded parents Eddy and Mira; the newborn prompt was skipped normally. After birth, energy was **57.25485** at clock 1035.379986.
2. The running view offered an empty nearby stem and a ripe more distant patch, with a walking quote of roughly 5 energy. The choice was to travel rather than wait. From clock 1048.659981 to 1055.159978, 6.5 seconds of held-W moved **23.71557 m** horizontally and spent **5.00861 energy** (52.58914 → 47.58053). No meal was gained; the plant at arrival was empty. Time spent deciding allowed the world to continue, so this does **not** establish that a competitor beat the player during those exact 6.5 seconds. There was no scripted winner.
3. Population went from 18 to 20 during that walk and reached the 24-living cap before the journal inspection. At clock 1074.979970, energy was 40.5804, births 11 and deaths 0. Sora remained a living child and had grown to adulthood naturally. The profile and exact Family Record were inspected paused, at both target resolutions.

This demonstrates a visible expenditure and lost opportunity, not that every route is interesting or that the 20–60-second target is proven. It does not establish genetic adaptation.

## Descendants and the single creature page

- Each living-descendant tree card has one short adult comparison with a named available parent. Actual walking/turning, sight/upkeep or reserve/upkeep differences are used; relative differences under 2% are omitted from compact claims only, never rounded in simulation. No overall score or guaranteed advantage. Full names and both parents remain accessible in the profile/records.
- Four colored cards: **Movement**, **Senses & Foraging**, **Survival**, **Inheritance & Coat**. Practical summary first, inherited comparison second, hover/focus/select explanation directly on the page. Larger reserves are not falsely equated with longer survival; maintenance also matters.
- The four-page Details sequence and its next-page control are removed. On the same scrollable page, **Exact Genes**, **Parent Comparisons & Mutations**, and **Family Record** start collapsed. All sixteen precise genes, explanations, exact resources and adult calculations, two-parent comparisons, retained birth text, recorded mutations, stable identity/parent links, birth order/times and confirmed death evidence remain. Mutation details appear only if mutations were recorded.
- Back retains tree scroll and the clicked identity. Highlight resumes the current creature and marks its relative; Take Control remains a separate descendant-only action. Names and duplicate-name targeting, care eligibility, founder/newborn naming, biome entry and F6 are unchanged.
- Dead or unavailable actors do not acquire invented genomes. The archive's retained birth comparisons are displayed as recorded; live actor calculations explicitly become unavailable when required actors are missing. Unregistered does not mean dead.

## Verification

### Rendered saved-scene checks

Both actual saved scenes were opened and run in Unity 6000.4.7f1. Original and painterly profiles were inspected at **1920×1080 and 1366×768**; the final tree callouts were reinspected at both resolutions. Native mouse clicks selected cards, expanded/collapsed records and returned to the same tree selection. Native Down-arrow focus revealed the exact body-size and leg-length explanations, automatically scrolled the selected row into view, and kept its explanation correct despite a stationary mouse. F6 switched the painterly comparison off and on without restarting. The Console showed no gameplay errors; two pre-existing obsolete-API compiler warnings from `PainterlyPlayTests` remain.

The **painterly family comparison was assisted**: partners were placed nearby, their energy refilled, AI decisions frozen, and the founder cooldown accelerated while resources were replenished. The children still came from the existing two-parent mating/inheritance path, initiated with native F, with native default-name choices; no genome was edited or selected for improvement. Other nearby actors could still be the actual eligible partner: the first birth's archive correctly records **Luma**, not the helper's requested Mena. Descendants were positioned nearby for inspection. These conditions are not ordinary survival or balance evidence.

Two eligible children were compared:

| Adult potential (same open-ground conditions) | Sora, Eddy + Luma | Mena, Eddy + Vela |
|---|---:|---:|
| Stable ID | 917430:018 | 917430:019 |
| Walk speed, m/s | 2.845921 | 4.822868 |
| Turning, rad/s | 6.694150 | 7.698129 |
| Vision, m | 25.899920 | 28.655907 |
| Energy reserve | 159.460388 | 72.217354 |
| Passive upkeep, E/s | 0.360143 | 0.394952 |

Eddy's reference walk speed was 3.725013, turn rate 7, Vision 27 and reserve 100. The final compact cards prioritize genuine opposing consequences: Sora **larger reserve / more upkeep than Eddy**; Mena **sees farther / more upkeep than Eddy**. Their profiles also expose different pace, turning, reserves, mating thresholds and both-parent records. Sora had become an adult; Mena was still juvenile, but all inherited comparisons use adult potential rather than current growth.

Native **Highlight** on Sora produced the pale-gold cue and “Locating Sora” while the HUD remained Eddy. Native **Take Control** on Mena changed the HUD to Mena, generation 1, preserving her juvenile state and own resources (62.74381 energy, 93.01627 stamina in the subsequent snapshot). Returning to the tree showed Sora as a sibling, not a controllable descendant. Duplicate-name disambiguation remained visible for Mena. The final source was refreshed and tree/profile/focus checks repeated after the layout and callout fixes.

### Automated verification

The temporary Editor inspection/capture helper and its `.meta` were removed before final full suites; recoverable copies remain only outside the project in the backup directory. No fixture is delivered. All automated test founders choose the Eddy default through the existing test setup.

- Final complete **Edit Mode: 156/156 passed**, zero failed/skipped, 3.331715 seconds, process exit 0.
- Final complete **Play Mode: 60/60 passed**, zero failed/skipped, **564.5284482 seconds**, process exit 0. Test durations exclude startup/import. Raw XML and logs are retained outside the repository in the task backup directory as `edit-final.xml/.log` and `play-final.xml/.log`.
- Six new Edit cases cover actual adult tradeoffs, Vision/upkeep counterexamples, reserve wording that does not promise longer life, comparison thresholds without rounding simulation, every precise gene explanation and preference for an opposing consequence.
- Four new Play cases cover actual sensing/occlusion, depletion/pause/reset/caps, read-only resource observations, matched walking/sprinting accounting, a motion-based approach cue (no hidden AI target), single-page collapsed sections, all exact genes, both-parent and unavailable-parent records, fit checks, pointer/focus priority and tree return state. Existing navigation, duplicate names, death evidence, naming, restart/reseed, inheritance, ecology, ancestry/caps and long-run regressions are retained.
- Matched 12 m steady-travel accounting: walking **2.510483 energy / 3.221465 s**; sprinting **3.483887 energy / 1.836235 s**; displayed walk estimator before rounding **2.510484**. Same phenotype and conditions; no acceleration, detour or future hunger. This establishes cost accounting, not a full navigated route guarantee.

Intermediate failures were in new test setup: a newborn naming pause was left open, then an approach-cue fixture tried to consume food while paused. The fixtures now disable naming prompts for their setup and explicitly unpause before consumption; survival/pause rules were not weakened. A mistaken boolean assertion on the numeric consumption result was corrected. The focused rerun passed all four cases. Native inspection also found and repaired help focus being stolen by auto-scroll, and a left-edge food cue overlapping the ordinary HUD; final cue selection leaves that HUD area clear.

### Existing bounded ecosystem regression observations

The final suite repeated the existing autonomous 1,200-second balance runs. **Two seeds did not reproduce the first run bit-for-bit**; the final results below supersede the provisional figures. The survey sets `Time.captureDeltaTime = .1` after scene loading/restart and uses the actual physics/AI scene, not a fully isolated deterministic simulator. The exact source of the difference was not isolated in this presentation milestone. No economy rule or genetics/random-stream call was changed. Neither suite is a clean pre-milestone economy experiment.

| Seed | Births | Deaths (starvation / old age / unknown) | End living | Living range after 120 s | Peak actor objects | Archive records | Highest generation | Births after first death | Longest observed vacancy to birth | Region crossings |
|---|---:|---|---:|---|---:|---:|---:|---:|---:|---:|
| 917430 | 49 | 38 (2 / 36 / 0) | 24 | 22–24 | 26 | 62 | 11 | 38 | 17 s | 645 |
| 925349 | 50 | 39 (1 / 38 / 0) | 24 | 21–24 | 26 | 63 | 9 | 39 | 19 s | 664 |
| 933268 | 44 | 33 (0 / 33 / 0) | 24 | 21–24 | 26 | 57 | 7 | 33 | 13 s | 574 |

Ripe food slot counts, minimum / mean / maximum over the sampled run:

| Seed | The Meadow (32 slots) | Amber Flats (12 slots) | Fernwood (28 slots) |
|---|---|---|---|
| 917430 | 1 / 12.22 / 32 | 0 / 6.16 / 12 | 4 / 15.15 / 28 |
| 925349 | 0 / 8.94 / 32 | 0 / 5.42 / 12 | 7 / 15.66 / 28 |
| 933268 | 2 / 10.66 / 32 | 0 / 4.29 / 12 | 8 / 16.49 / 28 |

Food remained at the hard **72-slot** bound. The runs and focused cap tests retain **24 living / 32 actor objects / 512 ancestry records** as limits. Turnover and replacement births continued; this is regression evidence, not proof that the UI improves decisions or that evolution has favored a trait. The matched walking test is causal accounting; the evolving-population counts are observations, not causal trait-group comparisons.

For transparency, the first full suite's same seeds recorded: 917430 **49 births / 38 old-age deaths / Gen 8 / 610 crossings**, and 925349 **52 births / 2 starvation + 39 old-age deaths / Gen 10 / 686 crossings**. Their ripe-food means (Meadow, Dry, Woodland) were respectively **10.65, 7.68, 14.18** and **8.96, 6.51, 15.36**. Seed 933268's result/food summaries matched. Both runs ended at 24 living, with peak 26 objects and resumed replacement births. The changed totals are not claimed as a beneficial effect of the UI or evidence for a specific inherited advantage.

### Running-scene evidence

These are unedited Unity Game-view captures, not concept art. Image filenames identify the actual output resolution. Family-comparison images use the explicitly assisted setup above.

- [Before: original scene, fresh-run food without scouting cues](before-original-forage.png).
- [After: original scene depletion and nearby mating choice](original-choice-1080.png). Earlier in the presentation iteration; it also documents the remaining crowded-world-label limitation, not the later left-HUD exclusion fix.
- [Final scouting: empty nearby food versus ripe farther food, painterly 1080p](painterly-forage-final-1080.png). Captured before the assisted family setup in that run.
- [Final Family Tree, 1080p](painterly-tree-final-1080.png) and [1366×768](painterly-tree-final-768.png).
- [Final single creature page, 1080p](painterly-profile-final-1080.png), [Sora at 1366×768](painterly-sora-profile-768.png), and [Mena at 1366×768](painterly-mena-profile-768.png).
- [Native Highlight, still controlling Eddy](painterly-highlight-768.png), [Take Control, now Mena](painterly-control-mena-768.png), and [exact gene keyboard help](painterly-keyboard-exact-768.png).
- [Original scene exact Family Record at 1366×768](original-record-768.png).

## Protected-file end check

**All nine files match their starting backups byte-for-byte by SHA-256**, after all Unity processes exited. Unity's regenerated TMP fallback font was first retained separately as `Unity-generated-fallback-after-tests.asset`, then the exact task-start copy restored. None of these paths is staged or committed:

| Protected path | Final status |
|---|---|
| `Assets/ThirdParty/UnityTextMeshPro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset` | Exact starting bytes restored; generated copy separately retained |
| `Assets/WildType/Materials/Interaction gold.mat` | Unchanged from backup |
| `Assets/WildType/Prefabs/Environment/PrototypeTree.prefab` | Unchanged from backup |
| `Assets/WildType/Prefabs/Food/Brightfruit.prefab` | Unchanged from backup |
| `Assets/WildType/Scenes/CreatureStage_Prototype.unity` | Unchanged from backup; unresolved sun patch remains local |
| `Assets/WildType/ScriptableObjects/Amber Bulwark.asset` | Unchanged from backup |
| `Assets/WildType/ScriptableObjects/Meadow Grazer.asset` | Unchanged from backup |
| `Assets/WildType/ScriptableObjects/Violet Strider.asset` | Unchanged from backup |
| `ProjectSettings/EditorBuildSettings.asset` | Unchanged from backup |

The painterly saved scene is not modified. Unity is left closed to preserve the protected font's exact state.

## Remaining limitations

- Scouting is deliberately limited to two observations, not a complete map. Quotes are approximate direct walks, not obstacle-aware routes or guaranteed meals. Nearby world labels can still compete for space in a crowded scene.
- Trait callouts are a compact selection, not a comprehensive comparison. They name one parent; both-parent and small precise differences remain in expandable records. A parent whose actor is gone cannot supply a fresh phenotype comparison.
- Large families still need scrolling; the existing bounded card pool and archive remain. Precise gene explanations are intentionally denser than the ordinary view.
- Automated tests and assisted Editor inspection are not a novice usability study or physical-gamepad test. The 20–60-second decision target is a design aspiration, not an implemented event timer or a proven cadence across an entire run.
- Two full physics/AI survey seeds differed between runs; the source was not isolated. Deterministic rule tests pass, but the long scene surveys must not be described as bit-identical or used as a precise causal economy comparison.
