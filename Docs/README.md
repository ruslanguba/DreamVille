# Documentation

Index of everything under `Docs/`.

- **Architecture decision records** — [`adr/`](adr/README.md) — one page per significant decision: composition root, assembly layering, persistence, scene/async patterns, interaction effects as sibling components, UniTask lifetimes for coroutines, HUD as MVP, menus as MVP, data-driven dialogues, renaming serialized fields, enemy archetypes. See [`adr/README.md`](adr/README.md) for the numbered list.
- **[`ARCHITECTURE.md`](ARCHITECTURE.md)** — assembly dependency diagram, composition-root walkthrough (`ProjectContext` vs `SceneContext` vs `GameObjectContext`, installer table), sequence diagrams for the core flows (bootstrap → menu → level, shooting pipeline, level completion, save/load), cross-cutting rules (fault isolation, pause, async) and the conventions that tests enforce.
- **`modules/`** — one page per module complex enough to need it: [`enemy-state-machine.md`](modules/enemy-state-machine.md) (the guarded enemy FSM and its archetype configs) and [`aim-and-ricochet.md`](modules/aim-and-ricochet.md) (the ricochet preview that matches the bullet).
