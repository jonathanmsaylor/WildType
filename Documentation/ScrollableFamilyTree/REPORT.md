# Scrollable Family Tree and simpler journal — 2026-09-26

Branch: `milestone-scrollable-family-tree`. Base verified locally and remotely: `fe8e03961798ec40b91ff4a0d72f29d55136cd14` (`milestone-welcoming-journal`). The prior branch was pushed normally before this branch was created. Its saved final results were checked: 141/141 Edit, 49/49 Play, no failures/skips. Main remains `d768797775ad0a7b802eaec0f50899e5d3e2e0df`; no merge, reset or force-push.

## What to try

1. Open either saved scene and continue as Eddy. Tab now opens **Family Tree** directly, rather than a separate Living descendants page.
2. After a birth, **Find living descendant** jumps to a living child or later descendant. Repeat to cycle. Green cards also say **Living descendant**, so color is not the only distinction.
3. Scroll or drag the canvas; use the horizontal bar for wide generations. **Explore tree** focuses a card for arrows / D-pad. Focus centers the card; yellow hover does not change selection or control. Left from a generation's first card returns to the menu. Select a card for its complete identity record.
4. **Highlight in world** and **Take Control** remain different, explicit actions. Living siblings/parents can be highlighted when existing family eligibility permits, but are not descendant-control candidates. Care still requires an eligible juvenile direct child under unchanged rules.
5. Details keeps Exact Genes on page 1. Hover or focus a gene/category for practical help and exact values. Parent comparisons, mutations, precise resources and recorded identity/death evidence remain on later pages.
6. Recent births/deaths is inside Family Tree. First-step tips are inside Getting started. Escape opens Menu with Restart, Reseed and Quit; those keys and gamepad bindings are unchanged.

## Implementation and bounds

`FamilyTreeLayout` projects the existing bounded archive/history into one node per stable ID, adding recorded co-parents and their recorded ancestors. Generation rows and stable-ID ordering are deterministic; names never identify an action target. Both recorded parent edges are retained. Missing actors are **Unavailable**, not inferred dead. Previous controlled branches and all previous Chronicle records remain reachable; All Family Records is retained as an optional paged index, not the primary tree.

The canvas uses fixed 326×148 cards with 22-point reference text, a reusable pool of 24 cards, a clipped two-axis viewport, and one clipped edge mesh. Only intersecting cards are active; at most twelve are fully or partially visible in this layout. Links for visible children are drawn when their parents are visible or the link is selected; the complete parent IDs remain in the record and hover/focus explanation. Crossings do not establish relationships. Cards never shrink with family size. Archive limit remains 512; no creature/food/ancestry cap changes.

Help now appears directly on gene rows, category headings/motifs, family cards and major journal actions. Named record-help controls replace generic “What does this mean?” wording. Recycled cards release focus before changing identity, and hidden cards clear hover state. A two-frame Editor-only arrow fallback handles brief native Game-view key taps only when normal UI navigation has not already moved selection; it is disabled while naming or unpaused. This does not alter gameplay controls.

Changed areas: StageHud and its Details/Tree partials; GeneGuide; JournalFocusMarker and the new JournalEditorKeys; PlayerInputBridge's journal/menu destination; new FamilyTreeLayout/Card/Links; focused tests and updated UI regression expectations. No genetics, ecology, survival, care accounting, eligibility, camera, F6, asset or package rule changes.

## Rendered inspection — what actually happened

Used the computer-use skill for native Unity clicks, mouse-wheel scrolling and arrow keys. Both actual saved scenes were rendered at **1920×1080** and **1366×768**. No scene was rebuilt or saved over the protected original. The local dirty assets, not a clean-clone rendering, were used.

An explicitly assisted temporary helper created six inherited actors and 48 historical archive-only records, including duplicate names, a long permitted name, two-parent/shared ancestry, earlier controlled branches, a grandchild, recorded unknown-cause deaths and unavailable actors without death records. It used existing inheritance but bypassed mating costs/timing, set adult birth times, applied one deliberate fatal damage event, positioned actors nearby and disabled their brains. Initial fixture: 67 archived, 56 projected family records, 18 living, 19 actor objects, 72 food slots. These are UI edge cases, **not natural-generation, survival or selection evidence**. Ordinary pending AI events could complete when play resumed.

Native observations:

- Found a living child and later descendant, with separate eligible Highlight/Take Control buttons. Scrolled from Gen 2/3 into Gen 4–7, including duplicate names and unavailable versus deceased cards. Selection/action identity did not change merely from wheel movement or hover.
- Native Down moved focus from the controlled Gen 1 creature to Gen 2, then Gen 3; the focused card stayed in view. Native keyboard focus also changed the gene explanation from Leg length to Movement speed. Virtual keyboard/gamepad coverage is separately automated; no physical controller was used.
- Selected a deceased Gen 4 record: stable ID `history:002`, both parents, explicit **cause unknown**, age at death 40 seconds. This was a synthetic recorded death, not a naturally observed death or inferred starvation.
- Painterly: native Take Control selected Luma 2 (Gen 2); reopening retained Dave 1 as its living sibling and found Dave 1 (Gen 3) as its child. Highlight resumed with “Locating Dave 1” while control stayed Luma 2. Original: native Take Control selected the Gen 3 Dave 1, preserving its age/resources.
- Read practical gene hover help and exact numbers at both sizes; inspected category grouping, full records, relocated tips and Escape menu. Cards and supported long names fit without ambiguous presentation truncation. Existing input names are still capped at 16 characters; the long fixture name reflects that existing stored-name rule.
- Caught and corrected oversized scrollbar handles, an overly dense fan of off-screen links, stale recycled focus, hidden-card hover and missed brief native arrow taps. Final screenshots show corrected bars/links and native focus. A final minor help-reset change prevents an old action explanation persisting when another journal page opens; screenshots precede only that help-reset adjustment.

The temporary setup/capture helper and its `.meta` were moved out of Assets before complete suites. Automated tests are not human usability testing.

## Evidence

Before is the committed welcoming-journal capture, not a newly simulated matching family. The prior UI was also inspected at the start of this task. After images are actual Unity Game frames; no mockups or image editing.

- [Before: five-card paged tree](../WelcomingJournal/family-tree-1080.png)
- [Painterly: scrollable family at 1366×768](painterly-tree-768.png)
- [Original: yellow grandchild hover, recorded descendants and actions at 1920×1080](original-tree-hover-1080.png)
- [Original: two recorded parents and duplicate names at 1366×768](original-tree-768.png)
- [Painterly: practical gene explanation at 1920×1080](painterly-gene-hover-1080.png)
- [Original: same explanation at 1366×768](original-help-768.png)
- [Painterly: complete recorded death evidence](painterly-death-record-1080.png)

## Automated results

Final complete Edit Mode: **144/144 passed**, zero failed/skipped, **3.1691706 seconds** excluding import/startup. Final complete Play Mode: **53/53 passed**, zero failed/skipped, **542.859468 seconds**. Both Unity test processes exited 0. Results/logs remain in the task's external backup directory as `edit-final2.xml/.log` and `play-final2.xml/.log`.

The full-archive test centered all 499 historical targets in **52 ms** with a 24-card pool on this machine. This is a bounded UI operation timing, not an FPS/GPU benchmark or a performance guarantee on other hardware.

Three new Edit tests cover full 512-record identity/parentage projection (1,020 truthful edges), recorded co-parent closure, deterministic placement, bounded coordinates, dense 510-child generations without overlap or shrinking, and navigability. Four new Play tests cover 512 archive entries/499 historical targets reachable with bounded card pooling, death evidence versus unavailable actors, scrollbar bounds, recycled-focus safety, pause/reset/reseed, explicit identity/action eligibility, real queued virtual keyboard/D-pad navigation, duplicate-navigation suppression, and every relocated function/help target. Existing welcoming/clarity assertions were updated for the new tree/menu, not removed wholesale.

Intermediate focused runs: first 7/12 passed; five text-fit checks exposed a too-short category heading. Increased its height and removed a missing-font diamond marker. Second 12/12 passed in 42.81909 seconds, before the final native focus/scroll fixes and fourth new test.

The first complete Play run passed 47/53 in 527.4650093 seconds. Six older regression tests still searched for removed Living-descendants cards/headings and their old per-card Locate/Take control buttons. Updated the FamilyCare, Generation, LineageContinuity and LineageLocate tests to use actual tree selection and its explicit actions, preserving transfer accounting, numeric naming, sibling eligibility, history paging, death evidence and descendant-control assertions. A local-name collision in the test edit briefly prevented compilation and was corrected. All six focused regressions then passed in 55.6424748 seconds. Complete final suites were rerun after these test updates; no temporary helper was present.

The final run's existing three-seed 1,200-second balance regression observed:

| Seed | Births | Deaths (starvation / old age) | Living at end | Maximum generation |
|---|---:|---:|---:|---:|
| 917430 | 47 | 36 (1 / 35) | 24 | 9 |
| 925349 | 50 | 39 (1 / 38) | 24 | 9 |
| 933268 | 44 | 33 (0 / 33) | 24 | 7 |

All three ended with 26 actor objects and recorded births after deaths. The separate 600-second bounded soak completed 102 internal checks, including starvation/game-over/restart. These are existing gameplay regressions, not additional test-case counts, deterministic balance guarantees or evidence of adaptation from this UI milestone.

## Protected local files

All nine starting dirty files were matched to the previous milestone's verified backups before editing and copied, with relative paths, to `C:\Users\jsayl\AppData\Local\Temp\WildTypeScrollable-20260926`. Original backups also remain in `C:\Users\jsayl\AppData\Local\Temp\WildTypeWelcoming-84f7c6fb`. Their exact starting SHA-256 values are in the [previous protected-file inventory](../WelcomingJournal/REPORT.md#protected-local-changes).

| Protected path | Final disposition |
|---|---|
| TMP `LiberationSans SDF - Fallback.asset` | Exact starting backup restored after Unity exited; matched; excluded |
| `Assets/WildType/Materials/Interaction gold.mat` | Matched backup; excluded |
| `Assets/WildType/Prefabs/Environment/PrototypeTree.prefab` | Matched backup; excluded |
| `Assets/WildType/Prefabs/Food/Brightfruit.prefab` | Matched backup; excluded |
| `Assets/WildType/Scenes/CreatureStage_Prototype.unity` | Matched backup; excluded |
| `Assets/WildType/ScriptableObjects/Amber Bulwark.asset` | Matched backup; excluded |
| `Assets/WildType/ScriptableObjects/Meadow Grazer.asset` | Matched backup; excluded |
| `Assets/WildType/ScriptableObjects/Violet Strider.asset` | Matched backup; excluded |
| `ProjectSettings/EditorBuildSettings.asset` | Matched backup; excluded |

Final SHA-256 comparisons confirmed **all nine files byte-for-byte identical to their starting backups**. Before restoring the font, Unity's regenerated version was preserved as `Unity-generated-fallback-after-tests.asset` in the task backup directory. Unity was left closed after batch verification so it would not regenerate that protected file again before handoff.

The original scene's unresolved sun-component change and substantial pre-existing serialization differences remain local, intact and uncommitted. Prefab/material/preset edits are not normalized or incorporated. Build settings still omit the comparison scene, which tests load by its existing Editor path. No separate RunAudit files were touched.

## Remaining limits

- The tree is a readable generation grid, not a sophisticated pedigree layout: large same-generation families require horizontal travel. Shared-parent links can cross; focus/hover and the exact parent names/IDs disambiguate them. There is no new search or minimap. The index remains an alternate route.
- At 1366×768, tree type is roughly 16 pixels and exact-gene rows roughly 14 pixels; there is no adjustable UI scale. Hover help uses fixed existing explanation areas, not floating bubbles over buttons.
- Dense labels/action prompts can still overlap in the world when assisted actors are clustered. This milestone does not redesign those world cues.
- Names can repeat. Stable IDs are deliberately in full records; no identity/death inference, archived-genome reconstruction or save persistence was added.
- No physical-gamepad ergonomics check, standalone build, novice human study or new balance claims. Virtual input and synthetic large-family fixtures verify mechanics, not human comprehension.
