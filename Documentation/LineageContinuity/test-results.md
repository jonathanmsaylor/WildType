# Final test results — 2026-09-25

Unity 6.4 / 6000.4.7f1, Windows, original saved CreatureStage_Prototype scene. Both complete suites were run with the temporary ContinuityInspection Editor helper and its metadata absent. No production/test changes followed these runs. No packages were installed.

| Complete suite | Passed / total | Failed | Skipped | Duration | Process exit |
|---|---:|---:|---:|---:|---:|
| Edit Mode | 114 / 114 | 0 | 0 | 0.2123321 s | 0 |
| Play Mode | 37 / 37 | 0 | 0 | 483.3222383 s | 0 |

Earlier focused Play Mode (continuity, Chronicle and Locate) also passed 9/9, zero failed/skipped, in 111.4545894 s. The final complete run supersedes that subset.

## Final Play Mode breakdown

| Fixture | Passed / total | Duration |
|---|---:|---:|
| AppearancePlayTests | 3 / 3 | 12.158644 s |
| BalanceSurveyTests | 3 / 3 | 83.355970 s |
| ChroniclePlayTests | 3 / 3 | 30.716267 s |
| EcologyPlayTests | 4 / 4 | 82.907363 s |
| EcosystemTests | 1 / 1 | 77.268253 s |
| FamilyCarePlayTests | 8 / 8 | 30.576115 s |
| GenerationTests | 8 / 8 | 85.651948 s |
| LineageContinuityPlayTests | 2 / 2 | 38.215335 s |
| LineageLocatePlayTests | 4 / 4 | 42.171471 s |
| ScarcityPlayTests | 1 / 1 | 0.292925 s |

New Edit Mode cases verify earlier-controlled branches without falsifying descendants, unrelated founders not becoming siblings, half-sibling labels requiring a real parent, and deduplicated history at the full 512-record limit. New Play cases verify five children and two pages, growth/reproduction after transfer, sibling Locate without care/control, child death/corpse cleanup, all 13 founder names, cosmetic RNG isolation, unchanged restart genomes, reset/reseed history and naming. Existing suites cover duplicate names, unavailable actors without death evidence, mutation bounds, genetics, care accounting, camera, starvation/aging, pause/restart, food and population limits.

The existing EcosystemTests case completed its 102 checks. Three 1,200-second autonomous BalanceSurveyTests runs observed continued replacement births and population 21–24 after the initial growth period, with peak physical creature objects 26 (cap 32); food remained bounded at 72 and ancestry at 512. These are automated checks, not human playtesting or proof of long-term ecological balance.

## Reproduce / local evidence

Close this project's Editor first. Use its installed Unity executable with `-batchmode -projectPath <WILDTYPE> -runTests -testPlatform EditMode -testResults <outside-project XML> -logFile <outside-project log>` and repeat for `PlayMode`. Do not add `-quit` or `-nographics` to these test runs.

Original XML and full logs are outside source control at `%TEMP%/WildTypeContinuity-5eb9e826/EditMode-final.xml`, `PlayMode-final.xml`, and matching `.log` files. Focused results use `PlayMode-focused.xml`. Full machine-specific Editor logs are deliberately not committed; the portable lineage traces, screenshots and this results summary are provided instead.

Rendered inspection used native mouse/key controls through the computer-use skill. Natural gameplay observations and explicitly assisted edge cases are separated in [VERIFICATION.md](../../VERIFICATION.md). The screenshot death at age 14 s is deliberately induced fixture starvation, not an observed premature natural death.
