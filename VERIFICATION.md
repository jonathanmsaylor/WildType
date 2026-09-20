# WILDTYPE verification — 2026-09-20

## Environment

- New, independent project: `C:\Users\jsayl\Desktop\UnityProjects\WILDTYPE`.
- Installed stable Unity 6.4, **6000.4.7f1 (f3c3c4248748)**, Universal 3D template, URP 17.4.0, Windows 64-bit target.
- Normal editor rendering used Direct3D 12 on the NVIDIA GeForce RTX 4070 SUPER.
- Confirmed Linear color, Force Text, Visible Meta Files, Both input backends, and 0.02-second fixed timestep (Unity serializes the float as approximately 0.01999999).
- No physical gamepad was reported by the Input System. Gamepad tests used virtual device events, not a connected controller.
- No existing Godot or other Unity project was changed. No paid content, extra editor, remote repository, or cloud project was created.

## Automated results

Final Edit Mode run: **20 passed, 0 failed**, 0.036697 seconds reported by Unity Test Framework.

Final Play Mode run: **1 full-scene integration test passed, 0 failed**, with **102 explicit checks**, 77.5764167 seconds reported by Unity Test Framework. This includes **600 simulated seconds** of accelerated ecosystem soak, not ten wall-clock minutes.

Coverage actually exercised:

- Nonfinite and out-of-range genome values, runtime-copy isolation, deterministic seeded variation, derived-stat tradeoffs, and nonmutating phenotype construction.
- Resource updates at different timesteps, stamina exhaustion/recovery hysteresis, starvation, and single death notification.
- Saved prototype scene startup: one player, twelve AI creatures, 72 food slots, grounded player, multipart replaceable visuals, and different preset sizes/movement limits.
- Held virtual W movement, Shift sprint, braking, energy cost, stamina drain/recovery; virtual left/right gamepad sticks, L3 sprint, Start pause/resume, south-button eating, and mouse-wheel zoom.
- Escape pause/resume; camera retraction against a temporary solid obstacle and recovery after removal.
- Three separate plant meals, no double consumption, regrowth, unchanged food-slot cap, AI detection/feeding, and destroyed/depleted target rejection.
- Paused simulation freezing, restart while paused, changed-seed restart, AI starvation/corpse cleanup, player game-over, and recovery through restart.
- Sixty soak checkpoints for population <=13, food slots <=72, and particles <=512; final object-count growth bound and finite player position.

No C# compiler warnings/errors, gameplay exceptions, or missing-reference failures appeared in the final test runs. Test-run logs contain normal stack traces for informational assertions; those are not failures. Unity can show an Input Manager deprecation notice because **Both** was explicitly requested; this is not a project compiler warning and has not been suppressed.

Test evidence was written outside the source project to the Windows temporary folder: `WildTypeUnity-edit-final.xml`, `WildTypeUnity-play-final.xml`, and corresponding `.log` files. These temporary files are not committed.

## Native editor / visual checks

The computer-use skill was used to operate Unity Hub and the Editor and inspect the running scene directly. The scene rendered its terrain, vegetation, shadows, food, different creature forms, procedural motion, and survival HUD. Energy decreased and AI food consumption was observed. The normal Play Mode Console showed **0 warnings and 0 errors** during this visual pass.

Confirmed native Escape opens the pause panel; mouse clicks on Resume, Restart Prototype, and Reseed Ecosystem work. Restart restores full resources and the original population/food bounds; reseed changes the displayed seed and actor/food arrangement. Mouse interaction changed camera orbit/framing. An Editor Escape-routing issue was corrected with a single-toggle GUI fallback, and pointer recapture ignores its first delta.

Inspected the HUD and pause panel at **1920x1080** and **1366x768** Game-view resolutions: text, bars, population/food count, controls and menu buttons remain inside the view without overlap.

## Limits of verification

- Sustained movement/sprinting and precise camera/zoom assertions were validated with virtual Input System events. Native automation provides brief key taps, not a human held-key play session; a human feel pass is still worthwhile.
- No physical controller was connected, so physical controller mapping, deadzones, and menu navigation feel remain unverified.
- No standalone executable was built, and no frame-time benchmark or GPU/CPU memory profile was recorded. The automated object/particle/population checks catch runaway spawning, not every possible memory leak.
- Procedural gait has no foot-contact IK; feet can slide or intersect slopes. AI uses local steering rather than complete pathfinding and may take imperfect routes around dense obstacles.
- Prototype survival balance needs extended human play. There is no save/load, audio, rebinding, rumble, or in-game graphics menu.
- Reproduction, inheritance/mutation, offspring control, predators and lineage systems remain deliberately deferred.

The authored scene and assets are saved; the final handoff leaves the Editor on `CreatureStage_Prototype` out of Play Mode. Local Git contains source, settings, assets and metadata only; generated caches, test logs, build outputs and local IDE state are ignored.
