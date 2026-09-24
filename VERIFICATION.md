# WILDTYPE verification — Milestone 2A.2 — 2026-09-24

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
