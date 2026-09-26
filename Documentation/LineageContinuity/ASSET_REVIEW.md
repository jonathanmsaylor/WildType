# Protected local assets: reconciliation inventory

Compared with **982075e** on 2026-09-25. Twelve original files were backed up outside the project, with exact SHA-256 and size in [the inventory](protected-assets.json). Original filesystem bytes remain intact. Git normalizes line endings when storing text; the recorded local hash is deliberately the raw byte hash, not the normalized Git blob hash.

| Path (under Assets/WildType) | Disposition |
|---|---|
| Art/Prototype/Grass.asset | Incorporated: descriptive mesh name only |
| Art/Prototype/Scenery Fern canopy.asset | Incorporated: descriptive mesh name only |
| Art/Prototype/Scenery Warm bark.asset | Incorporated: descriptive mesh name only |
| Art/Prototype/Scenery Weathered rock.asset | Incorporated: descriptive mesh name only |
| Art/Prototype/Terrain.asset | Incorporated: descriptive mesh name only |
| Materials/Interaction gold.mat | No normalized content change; NOT staged |
| Prefabs/Environment/PrototypeTree.prefab | Incorporated: equivalent ID reserialization |
| Prefabs/Food/Brightfruit.prefab | Incorporated: equivalent ID reserialization |
| Scenes/CreatureStage_Prototype.unity | Preserved; NOT staged — see scene review |
| ScriptableObjects/Amber Bulwark.asset | No normalized content change; NOT staged |
| ScriptableObjects/Meadow Grazer.asset | No normalized content change; NOT staged |
| ScriptableObjects/Violet Strider.asset | No normalized content change; NOT staged |

## Semantic comparison, not a raw line-count guess

The five mesh files change **only m_Name**: Grass → Batched meadow grass; Terrain → Rolling ecosystem terrain; Scenery Fern canopy / Warm bark / Weathered rock → Fern canopy / Warm bark / Weathered rock combined. Vertices, indices, bounds, submeshes and other serialized fields are identical. These names match the existing PrototypeBuilder's generated names. They affect asset/Inspector identity text, not geometry, terrain collision or creature rules. Accepted without rewriting originals.

PrototypeTree has **19 serialized objects on both sides**; Brightfruit has **45 on both sides**. Canonicalization follows each Transform's parent/ordered children, matches components by type and sibling position, and resolves local fileID references to that hierarchy. After this, **zero properties differ** in either prefab. Raw apparent crown/fruit/leaf position changes pair different serialized objects, not changed effective placements. Materials, meshes, colliders, food nutrition/regrowth and component links are unchanged. The externally referenced Brightfruit component remains fileID 8946544362440021764; the only in-project external reference is the StageSession foodPrefab. No external PrototypeTree reference was found. These equivalent generator reserializations are incorporated separately from gameplay changes.

## Scene: leave intact for review

The saved scene has **1234 → 1233 serialized objects**. After resolving local IDs/hierarchy and the three scenery renderer object renames, only the Afternoon sun GameObject's component list and its removed **UniversalAdditionalLightData** differ. Terrain, obstacle placements/colliders, camera, StageSession settings/prefab links, seed and creature rules match. The three renderer GameObjects have the same descriptive combined names as their meshes. There is no evidence here of moved terrain or a deleted creature component.

The removed URP component has script GUID **474bcb49853aa07438625e644c072ee6**, pipeline shadow bias enabled, additional-light shadow tier 2, pipeline soft-shadow quality, cookie size (1,1), offset (0,0), and rendering/shadow masks 1. Its intent cannot be established. URP's installed source uses defaults without it for bias/resolution and can lazily recreate it for light layers; the current pipeline enables light layers. Before that recreation, per-light shadow quality can take a different fallback path. This is a **lighting-data difference, not lineage/survival data**; no visual pixel-equivalence claim is made.

Do not apply [the exact local scene patch](uncommitted-scene.patch) automatically: it is an inventory artifact for review against 982075e, not a repair script. The original local scene remains byte-identical and uncommitted. Its missing lighting-data component is not reintroduced or replaced. All saved-scene tests and screenshots use this preserved local scene; clean-checkout lighting/serialized object IDs may differ. No scene regeneration, main merge or painterly work was performed.

Interaction gold.mat and the three genome presets have no normalized content difference from the commit (line-ending/working-tree status only). They remain untouched and unstaged. Their values were not tuned.
