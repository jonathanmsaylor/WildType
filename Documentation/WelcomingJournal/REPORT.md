# Welcoming journal and bounded Family Tree

2026-09-26. Branch `milestone-welcoming-journal`, based on local and pushed `milestone-gameplay-clarity` at `f34422c502a1aeb71ded815ae2339fb0de3f65b2`. Main remains `d768797775ad0a7b802eaec0f50899e5d3e2e0df`. No merge, force-push, art replacement or RunAudit change.

## What to try

1. Play either saved scene. Choose **Continue as Eddy**, or enter a name. Naming explicitly pauses the simulation, including on Restart and Reseed. Other founders keep their deterministic names.
2. Open **Tab → Details**. All sixteen existing genes are on page 1 in four colored category cards, with original small silhouette motifs. Hover, focus or activate a gene for its full-precision value and everyday explanation. Higher/lower effects and real tradeoffs are described, including when a gene has no separate penalty or no camouflage survival benefit.
3. Later Details pages retain recorded parent comparisons and mutations, precise current resources and adult calculations, and stable identity/parent IDs and recorded death evidence. Additional mutation pages remain available.
4. **Living descendants** uses three short cards per page: name, generation, relationship, life stage and region/distance. Highlight and Take Control are distinct, and unavailable actions are hidden. Normal HUD and journal use names; actions still resolve stable IDs, never matching names.
5. **Family Tree** replaces the Chronicle tab. It shows two recorded parents above one selected relative and up to two children below. Select the name for Details; **View branch** changes the tree focus, not the controlled creature. Page children or step through relatives without shrinking text. **All Family Records** retains every record previously available in Chronicle, including deceased relatives and earlier controlled branches.
6. Nearby child labels show the stored base name (without the automatically added numeric birth suffix), region, whole-number distance and generation. Separate E/F/R prompts still describe the available action. The stored name and birth order do not change.

Ordinary values round consistently, with “less than 1” for small positive values. Fractional food/care quotes say “about”; mating thresholds round upward so the displayed target is sufficient. Exact care cost/gain is retained in Details for the last player-related transfer. None of these formatted strings feed simulation calculations. Regions consistently read **The Meadow**, **Fernwood**, **Amber Flats**.

## Tree boundaries and truthfulness

This is a bounded branch browser, not a zoomable all-at-once pedigree. Maximum rendered tree nodes: **five**. Child pages hold two; the complete family-record list holds three per page. All records remain bounded by the existing **512-entry** archive; living/actor/food limits remain **24/32/72**. Both parent links come from recorded IDs. Deceased cards require an actual archive death record; absent actors without one are marked unavailable, not dead. Selecting a tree card does not grant care, control or Highlight eligibility. Siblings remain Highlight-only while alive; only living descendants of the currently controlled creature can be controlled.

The archive preserves parent records and stops further births at its existing capacity rather than deleting ancestors. Exact live genomes/resources are unavailable after actor cleanup, as before; birth-time comparison and recorded mutation data remain accessible. No replacement values are invented.

## Rendered inspection versus assistance

Actual Unity 6000.4.7f1 Game view was inspected, not just screenshots generated from a mock UI. Native mouse clicks and keyboard input exercised:

- Painterly saved scene at **1920×1080**: founder default, native F courtship/birth, custom newborn typing (`AlexandertheGrea 1`, existing 16-character base-name limit), native R care, short child label, Living card, Family Tree branch selection with both parents, and all four Details pages. Icons, leg explanation and exact transfer were visible. Child reached adulthood during the session.
- **1366×768**: the painterly gene-card page and identity page fit; native Restart and Reseed each returned to the paused Eddy prompt. Normal HUD and the empty first journal opening fit. The original saved scene also launched, showed paused founder naming, resumed as Eddy and opened the journal at this resolution.
- Native Tab and mouse navigation worked. Brief native Down taps did not visibly advance gene focus in this Editor session, so they are **not** counted as a successful native keyboard-navigation check. Dedicated virtual-device tests exercise the actual Input System navigation path. No physical gamepad was used; custom gamepad text entry still requires a keyboard.

Assistance: a temporary Editor helper placed the parent/partner/child nearby, refilled the parent/partner, lowered the child's energy, and disabled AI for the controlled care inspection. The actual F mating and R sharing actions were native inputs through normal rules. Observed transfer: **18 energy cost / 14.4000006 gain** (float representation of 14.4). No forced death or accelerated growth was used in that native inspection. Automated edge cases separately accelerate cooldowns/refill resources and apply fatal damage; those are fixtures, not natural survival or human usability evidence. The helper only opened existing scenes and captured Game frames; it never saved scene changes, and was removed before the full suites.

## Before / after evidence

Before images are the committed prior clarity milestone captures, not a newly staged baseline. Before-code UI was also inspected natively at the beginning of this task. The after images below are actual Game captures; they show preserved local assets and assisted family setup where described.

- [Before: journal](../GameplayClarity/journal-1080.png)
- [Before: Details at smaller resolution](../GameplayClarity/details-1366.png)
- [After: founder naming](founder-1080.png)
- [After: short child label and care quote](child-label-care-1080.png)
- [After: both recorded parents in Family Tree](family-tree-1080.png)
- [After: grouped genes and leg explanation](genes-help-1080.png)
- [After: grouped genes at 1366×768](genes-1366.png)

The help strip's idle wording on deeper pages was corrected after inspection to point to the record-help button rather than to absent gene rows. Screenshot captures precede this wording adjustment and the final focus underline; layout, cards and data are unchanged by those adjustments.

## Automated results

Final complete Play Mode suite: **49/49 passed, zero failed or skipped**, 533.13 seconds excluding startup/import, after removing the temporary inspection helper. Final complete Edit Mode suite: **141/141 passed, zero failed or skipped**, 0.27 seconds excluding startup/import. Twelve new Edit cases cover rounding, sub-unit/invalid values, exact preservation, conservative eligibility thresholds, gene-guide coverage/categories, region capitalization, and both-parent child projection using current archive records. Five new Play cases cover paused founder naming on begin/restart/reseed, long names/base labels/stable IDs, all sixteen explanation handlers and page text fit, real virtual keyboard/gamepad navigation, five-node tree/child paging, deceased records, earlier sibling Highlight without control/care, and identity stability through control/death. Existing duplicate-name, ancestry-cap, ecology, survival, naming and painterly regression suites remain in the full run.

First focused run: **7/11 passed**, exposing two undersized gene-row labels, one tree footer overflow and an incorrect test expectation for the existing name-length limit. Increased label height, shortened the footer and corrected that expectation. Second focused run: **4/4 new journal Play tests passed** before adding the fifth virtual-navigation test. These intermediate failures are not omitted from the report.

First complete run: **141/141 Edit**, **48/49 Play** (530.90 seconds). The one Play failure expected the literal old “3.5” care-range wording; the new message says to move closer until Share appears. The out-of-range rejection and unchanged-energy checks remain intact. Updated that assertion. All five new journal Play cases passed, including queued keyboard/D-pad/confirm input. Review also identified invisible selection tint on transparent tree-card buttons; added a small focus underline to all journal buttons, with explicit selection-state assertions. The final 49/49 Play rerun includes that adjustment.

## Protected local changes

Before editing, all nine initially dirty paths were backed up with directory structure and SHA-256 hashes outside the project. No protected path is staged in this milestone. The original scene has the unresolved Afternoon sun component difference **plus substantial existing serialization changes** relative to the branch; this task does not characterize that large patch as merely the sun removal. The prefab diffs include internal IDs/order and corresponding transform entries. Their intended edits remain unresolved, and the originals are preserved rather than normalized. Local build settings exclude the painterly preview even though its saved scene exists; the painterly regression loads the explicit saved scene path in the Editor instead of changing those settings.

| Protected path (project-relative) | Starting SHA-256 | Delivery |
|---|---|---|
| `Assets/ThirdParty/UnityTextMeshPro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset` | `A5504615B989E02CFE23A1066497FFAAB2819FC16A602F5E0DE5EF4A3CFCFDFD` | Byte-identical; excluded |
| `Assets/WildType/Materials/Interaction gold.mat` | `4297FF229276B9A0F3165110DF6CBAE3BAF893697C02367CC062DEB49462D28F` | Byte-identical; excluded |
| `Assets/WildType/Prefabs/Environment/PrototypeTree.prefab` | `683376F21C75A95DA83D7B016345FF90E537F4A059729BE832069D2C6F7BBA66` | Byte-identical; excluded |
| `Assets/WildType/Prefabs/Food/Brightfruit.prefab` | `A2533897380FA5D083CC9FEE02C34C5ECD69E86BFE082AF3F4B26990F96EEDCB` | Byte-identical; excluded |
| `Assets/WildType/Scenes/CreatureStage_Prototype.unity` | `6115D05D1CF5C17E3819DA13224A16A353F111F86F8D365237CF3E7FBC8AFBAA` | Byte-identical; excluded |
| `Assets/WildType/ScriptableObjects/Amber Bulwark.asset` | `2A58674D432E0614019EF7D8F8A3F08DDDB3613FDC65E286E52242D94788B0C5` | Byte-identical; excluded |
| `Assets/WildType/ScriptableObjects/Meadow Grazer.asset` | `32DCC8D3D3CB434D5C4F5757EB8009059BD303A90ED6753E0C1C03DBAAEBDC80` | Byte-identical; excluded |
| `Assets/WildType/ScriptableObjects/Violet Strider.asset` | `213ED18D9C29A56BB49EC003F18A0AB7CD8BA75C187C3C74CE3AF7CE45C56570` | Byte-identical; excluded |
| `ProjectSettings/EditorBuildSettings.asset` | `EE752D01F7CEF9207F9E3549EB4022B9066C819DECA1AD164A3536EE3B448810` | Byte-identical; excluded |

The font fallback atlas was automatically reserialized during Unity inspection/test shutdown. After the final Editor/test shutdown, its original backed-up bytes were restored and all nine SHA-256 hashes were verified identical to the starting backups. No simulation constants, controls, F6 behavior, genetics/random stream or scene/prefab content is intentionally changed.

## Remaining usability limits

- Not a novice human usability study. Native keyboard arrow behavior needs a human check in a focused Game view/standalone player; virtual devices are not proof of hardware feel.
- Exact Genes is intentionally still numerical; category grouping and explanations reduce, not eliminate, its density. At 1366×768 gene text is approximately 14 pixels. No adjustable UI scale added.
- Five-node branching requires paging for broad families. A shared ancestor can be reached through either recorded parent but is not duplicated into a global graph. Duplicate first names require opening the identity record to distinguish IDs.
- The UI retains the existing 16-character input limit and no save persistence. Past deceased actors do not regain exact genomes that were never archived.
- Native inspection did not repeat every possible mutation page, maximum-512 record case or duplicate-name case; those are automated checks. No balance/selection claims arise from this presentation milestone.
