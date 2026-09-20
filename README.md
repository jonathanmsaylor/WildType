# WILDTYPE — Creature Stage vertical slice

A fresh Unity implementation, independent of the earlier Godot project. Explore, eat, and survive alongside twelve autonomous herbivores in a bounded, roughly 250 m-wide procedural ecosystem.

## Editor and opening

Unity **6.4 / 6000.4.7f1**, Windows 64-bit, Universal Render Pipeline **17.4.0**. Open this folder in Unity Hub, then open **Assets/WildType/Scenes/CreatureStage_Prototype.unity** and press Play. The **WildType > Open prototype scene** menu also opens it.

Unity Personal and official free packages only: Input System 1.19.0, URP 17.4.0, Unity UI/TextMeshPro 2.0.0, and Test Framework 1.6.0. No paid assets, trials, subscriptions, metered services, external art/audio/font downloads, or third-party packages were added. TMP's official bundled Liberation Sans resources live under Assets/ThirdParty/UnityTextMeshPro, including their license; no custom vendor code edits were made.

Configured for Force Text serialization, Visible Meta Files, Linear color, 0.02 s physics, and Active Input Handling **Both**. Gameplay uses the new Input System. Source control is local Git only; no remote or cloud repository.

## Controls

| Action | Keyboard/mouse | Gamepad |
|---|---|---|
| Move | WASD | Left stick |
| Orbit camera | Mouse | Right stick |
| Sprint | Left Shift | Left-stick press (L3) |
| Eat nearby food | E | South button (A / Cross) |
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
- Fixed 72 food slots. Eating removes berries; 18–33 second timers regenerate them in place. No population growth, per-food coroutine, or endless history.
- Shared player/AI energy, stamina, health, starvation and death rules. Starvation costs 4 health per second. AI corpses are removed after four seconds; player death opens the restart panel.
- Twelve herbivores with staggered 0.35–0.5 s decisions, vision-limited food queries, local obstacle steering and stale-target rejection.
- Scalable TMP survival/trait/population HUD, food proximity highlight, eating feedback, dust, death and regeneration particles, and subtle locomotion camera bob.
- A single particle system capped at 512 particles; bounded registries; clean restart/reseed including while paused.

## Architecture and asset replacement

All original game code/content is under **Assets/WildType**.

- **Genetics:** serializable Genome, immutable derived Phenotype, reusable GenomePreset ScriptableObjects.
- **Creature:** CreatureAgent coordinates root-level CreatureMotor, CreatureVitals and CreatureInteraction. OrbitCamera is independent.
- **VisualRoot:** CreatureVisual alone builds and animates the prototype model. Gameplay never depends on individual body-part objects. Replace this child/component with an adapter for a future rig without rewriting survival, genomes, AI or physics.
- **World:** Ecosystem owns the bounded food registry and deterministic placement; FoodPlant owns depletion/regrowth.
- **AI:** HerbivoreBrain sets the same movement intent and uses the same interactions as the player. Its periodic nearest-food query is the future spatial-grid seam.
- **Core:** StageSession coordinates startup/lifecycle, PlayerInputBridge reads controls, FeedbackPool owns bounded effects.
- **UI:** StageHud owns TMP displays and pause controls.
- **Editor:** PrototypeBuilder creates authored meshes, materials, prefabs, presets and the saved scene. Rebuilding replaces these generated prototype assets; do not use it over subsequent hand-authored changes without reviewing them first.
- **Tests:** Edit Mode data/unit tests and Play Mode full-scene regression/soak.

Future imported assets belong under **Assets/ThirdParty/PublisherOrPackName**. Keep original vendor content unmodified and place adapters, material overrides and gameplay prefabs under Assets/WildType.

## Verification

Open **Window > General > Test Runner** to run Edit Mode and Play Mode tests.

Edit Mode covers nonfinite/out-of-range genes, immutable presets, all required trait tradeoffs, seeded variation, frame-rate-independent resources, stamina hysteresis, starvation and invalid inputs.

Play Mode exercises virtual keyboard/mouse/gamepad events, the actual saved scene, camera collision, multiple meals, regrowth, AI feeding and target invalidation, pause/restart/reseed, a ten-minute accelerated simulation with object/population/particle caps, and starvation/game-over recovery. Its test-only input settings route virtual events independently of Game-view focus and are restored afterward.

Final verification: **20 Edit Mode tests passed**, plus a **102-check Play Mode integration test** with **600 simulated seconds** of bounded ecosystem soak. See **VERIFICATION.md** for the actual results and manual limits. Generated logs/results are not source assets.

## Known limitations / deferred systems

- Local steering is not a path planner. Dense obstacle contacts can still produce imperfect routes; this is not a production navigation system.
- Gait is procedural rigid-mesh animation, not skeletal animation or ground-contact IK; some foot sliding or slope penetration is expected.
- No persistence, sound, rebinding interface, gamepad rumble, or in-game graphics menu. URP quality can be adjusted in editor settings for weaker hardware.
- Genome and food layout randomization is deterministic on this runtime, not promised across future engine versions.
- Food depletion/regrowth is bounded, but this is a prototype balance rather than a tuned natural-selection simulation.
- No predators, mating, reproduction, inherited mutation, species tracking, lineage history, creature editor, other stages, multiplayer or open world.

## Next milestone

Add mating eligibility and partner choice, bounded inheritance and mutation, offspring spawning with explicit population limits, selection of an offspring to control, and continuation through descendants. First extend the current tests to prove parental genome isolation and predictable inheritance, then add a small lineage summary. Keep this ecosystem and rendering scope.
