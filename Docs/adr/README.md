# Architecture Decision Records

This folder records the decisions that shape DreamVille's codebase: the composition root, assembly layering, persistence, the scene/async patterns, the interactables channel, UniTask lifetimes and the HUD. Each record is one page — Context, Decision, Consequences, Alternatives considered — written after the decision shipped and checked against the code under [`Assets/_Project/Scripts`](../../Assets/_Project/Scripts), not as a design proposal. Records are numbered sequentially and never renumbered; a decision that gets superseded earns a new record that says so, and the old one stays as history.

| # | Title | Status | Date |
|---|---|---|---|
| [0001](0001-zenject-composition-root.md) | Zenject composition root | Accepted | 2026-09-12 |
| [0002](0002-assembly-layering.md) | Assembly layering | Accepted | 2026-09-12 |
| [0003](0003-persistence-abstraction.md) | Persistence abstraction | Accepted | 2026-09-12 |
| [0004](0004-scene-catalog-and-async.md) | Scene catalog and UniTask for gameplay async | Accepted | 2026-09-12 |
| [0005](0005-interaction-effects-as-components.md) | Interaction effects as sibling components | Accepted | 2026-09-26 |
| [0006](0006-unitask-lifetimes.md) | UniTask lifetimes for coroutines: one-shot vs enable-bound | Accepted | 2026-09-26 |
| [0007](0007-hud-mvp.md) | HUD as MVP: passive views, a presenter, a scoring coordinator | Accepted | 2026-09-30 |
