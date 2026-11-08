# 0002. Assembly layering

Status: Accepted
Date: 2026-09-12

## Context

Before this step the assembly graph pointed both ways: `DreamVille.Core` held `PauseService` and `InGameMenuController`, which referenced `DreamVille.Gameplay` and `DreamVille.Dialogue` just to answer "is this collider the player" and "is a dialogue running"; `DreamVille.SceneManagement` and `DreamVille.Dialogue` each referenced `DreamVille.Gameplay` for the same question, with three different concrete lookups (`PlayerSafePosition`, `PlayerContext`, `PlayerDetectorTrigger`). Namespaces had also drifted from folders — `Core/Input` compiled into assembly `DreamVille.Input` under namespace `DreamVille.Core.Input`. None of this was a compiler error, because Unity resolves an asmdef graph as long as it is acyclic at the file level, but it meant "what can `Core` see" was not something the folder tree could answer on its own.

## Decision

Layer the fourteen script assemblies so every reference points from a feature toward the infrastructure it needs, never back. The graph below is read directly off the current `.asmdef` files under [`Assets/_Project/Scripts`](../../Assets/_Project/Scripts) — DreamVille-to-DreamVille edges only; every assembly also references whatever Unity/Zenject/UniTask packages it needs.

```mermaid
graph LR
    Configs[DreamVille.Configs]
    Core[DreamVille.Core]
    Input[DreamVille.Input]
    Persistence[DreamVille.Persistence]
    Audio[DreamVille.Audio]
    Scoring[DreamVille.Scoring]
    SceneManagement[DreamVille.SceneManagement]
    Dialogue[DreamVille.Dialogue]
    Gameplay[DreamVille.Gameplay]
    Progression[DreamVille.Progression]
    App[DreamVille.App]
    UI[DreamVille.UI]
    Presentation[DreamVille.Presentation]
    Bootstrap[DreamVille.Bootstrap]

    Audio --> Configs
    Audio --> Core
    Audio --> Persistence
    Scoring --> Configs
    Scoring --> Core
    Scoring --> Persistence
    SceneManagement --> Core
    SceneManagement --> Configs
    Dialogue --> Core
    Gameplay --> Audio
    Gameplay --> Configs
    Gameplay --> Core
    Gameplay --> Input
    Gameplay --> Scoring
    Progression --> Configs
    Progression --> Core
    Progression --> Dialogue
    Progression --> Gameplay
    Progression --> Persistence
    Progression --> Scoring
    App --> Configs
    App --> Core
    App --> Dialogue
    App --> Input
    App --> SceneManagement
    UI --> App
    UI --> Audio
    UI --> Configs
    UI --> Core
    UI --> Dialogue
    UI --> Gameplay
    UI --> Persistence
    UI --> SceneManagement
    UI --> Scoring
    Presentation --> Core
    Bootstrap --> App
    Bootstrap --> Audio
    Bootstrap --> Configs
    Bootstrap --> Core
    Bootstrap --> Dialogue
    Bootstrap --> Gameplay
    Bootstrap --> Input
    Bootstrap --> Persistence
    Bootstrap --> Progression
    Bootstrap --> SceneManagement
    Bootstrap --> Scoring
    Bootstrap --> UI
```

`Configs`, `Core` and `Persistence` reference no other DreamVille assembly; `Input` adds only `Unity.InputSystem`. Nothing below them is needed yet.

Supporting rules, all verified in the current tree:

- `DreamVille.Core` is a handful of seams and static helpers — [`IActiveSceneTracker`](../../Assets/_Project/Scripts/Core/Scenes/IActiveSceneTracker.cs) (the active-scene seam `Audio` listens to instead of `SceneManager`), [`ILevelCompletion`](../../Assets/_Project/Scripts/Core/Scenes/ILevelCompletion.cs), [`IPauseState`](../../Assets/_Project/Scripts/Core/Pause/IPauseState.cs), the player seams in [`Core/Players`](../../Assets/_Project/Scripts/Core/Players) (`IPlayerMarker`, `IPlayerControlSwitch`, `IPlayerZone`, `IPlayerDamageState`, `PlayerDetection`), [`IsolatedEvent`](../../Assets/_Project/Scripts/Core/Events/IsolatedEvent.cs) (raises an event so that one throwing listener cannot stop the others) and the async helpers in [`Core/Async`](../../Assets/_Project/Scripts/Core/Async) (`GameTime`, `Tween`, `Cancellation`) — and nothing else.
- Interfaces live in the module that owns the concept, not in `Core`: `IScoreService`/`IStarProgressService`/`IBestScoreService` in `Scoring`, `ISceneLoader` in `SceneManagement`, `IDialogueService`/`IDialogueState` in `Dialogue`, `IAudioService`/`IAudioSettings`/`IAudioOutput` in `Audio`, `IEnemyRegistry`/`IBulletSpawner`/`IBulletHitSource`/`IPillowThrower`/`IWeapon` in `Gameplay`, `ICursorService`/`IPauseService`/`IGameFlowService`/`IInGameMenu` in `App`. `Core` only holds a contract when the *consumer* sits below the *implementer* in the graph: the player seams (implemented by `PlayerCharacter`, `PlayerInputSwitch`, `PlayerDetectorTrigger` in `Gameplay`; consumed by `SceneManagement`, `Dialogue`, `UI` and `Presentation`), `ILevelCompletion` (see Consequences) and `IPauseState`, the read-only half of `IPauseService`: `PauseService` lives in `App`, but the dialogue input in `Dialogue` must ignore "Next" while the game is paused.
- `DreamVille.Dialogue` references no DreamVille assembly but `Core`. Since the data-driven dialogues ([ADR 0009](0009-data-driven-dialogues.md)) it also references the `UniTask` package (`DialogueService`, `UnityLocalizedTextProvider`) and `Unity.ResourceManager`, the Localization package's own dependency, only to name `AsyncOperationHandle<string>` in `UnityLocalizedTextProvider`.
- Package references beyond `Zenject` and `UniTask` stay where the code needs them: `Input` → `Unity.InputSystem`; `Dialogue` → `Unity.InputSystem` (`DialogueInputHandler` reads its own "Next" action), `Unity.Cinemachine` (`CameraPriorityHandler`), `Unity.Localization`; `UI` → `Unity.InputSystem` (the intro's skip action), `Unity.Localization`, `Unity.TextMeshPro`, `UnityEngine.UI`; `Gameplay` → `Unity.Cinemachine`, `Unity.VisualEffectGraph.Runtime`, `UnityEngine.UI`; `Presentation` → the render pipeline. Reading an action directly is allowed for UI and dialogue controls; gameplay input goes through `IPlayerInput` in `DreamVille.Input`.
- `autoReferenced` is `false` on every DreamVille assembly except `DreamVille.Bootstrap`; a script placed outside an asmdef folder cannot silently see the whole project.
- Namespace equals assembly name for every module (`rootNamespace` matches the asmdef `name`); the one nested case, `DreamVille.Core.Players`, is a folder inside `Core`, not a separate assembly.
- The graph is leaner than first drafted, by design: a planned `Input → Core` edge and a planned `Dialogue → Configs` edge were both dropped in a review pass because neither module's code used anything from the other assembly — the edge was pure risk with no payoff. A planned `UI → Scoring` edge was dropped the same way at first, while `GameplayPresenter` in `Bootstrap` was the only reader of `IScoreService`. It came back with the HUD MVP: `HudPresenter` now lives in `UI` next to its views and needs `IScoreService` and `IStarProgressService` events ([ADR 0007](0007-hud-mvp.md)).

## Consequences

- `DreamVille.Presentation` (added with the player composition, 2026-09-25) is the only assembly that references the render pipeline: the damage vignette listens to `IPlayerDamageState` from `Core`, so `Gameplay` no longer links URP. Nothing references `Presentation`; its component sits on the scene's `Global Volume` and is injected by the scene context.
- The compiler enforces the layering; a future `Gameplay` change that reaches for `DreamVille.UI` fails to compile instead of surfacing as a code-review comment.
- `DreamVille.Progression` references `DreamVille.Gameplay` and `DreamVille.Dialogue` for their public events and interfaces (`IEnemyRegistry`, `INpcReactions`) — allowed because a feature-to-feature edge for interfaces only does not create a layering cycle, and `Progression` sits one layer above both. It also references `DreamVille.Scoring`, because `ScoringCoordinator` joins `IEnemyRegistry` with the scoring services; `Scoring` itself may not reference `Gameplay`, so the join cannot live there. It references `DreamVille.Core` for [`ILevelCompletion`](../../Assets/_Project/Scripts/Core/Scenes/ILevelCompletion.cs), which `LevelDirector` implements so that `SceneManagement` can complete a level without knowing about progress ([ADR 0008](0008-menus-mvp.md)). Like the player-detection seams, it is a `Core` contract because its consumer sits below its implementer.
- `DreamVille.UI` references `DreamVille.Dialogue` for the public `IDialogueService` events and the `SpokenLine`/`DialogueSpeaker` payload types. `DialoguePresenter` lives in `UI` next to `DialogueView`, so the translation from dialogue events to view calls needs this edge; `Dialogue` never references `UI`, so no cycle appears. Like `UI -> Gameplay`, it is a feature-to-feature edge for events and payload types only ([ADR 0009](0009-data-driven-dialogues.md)).
- The real graph outgrew the one drafted in spec `01-assembly-layering-and-di.md` (local planning notes, not committed) in a few places (`UI` first needed no `Dialogue` edge and gained one with the dialogue presenter; `Progression` first needed no `Core` edge and gained one later, see above) — this ADR documents the graph as it compiles today, not the original proposal.
- 2026-11-08 (final review, step 6.5): `Scoring` gained a `Core` edge for `IsolatedEvent`, `UI` lost its `Input` edge (no UI file used `DreamVille.Input`), and `IPauseState` moved the read side of the pause into `Core`. Routing the UI and dialogue actions through `DreamVille.Input` was considered and rejected: it would mean prefab rewiring for no runtime gain.
- Adding a genuinely new cross-cutting concept still means asking "does `Core` need to know this", and the answer should almost always be no — `Core` growing back into a dumping ground is the failure mode this decision exists to prevent.

## Alternatives considered

- **Single `Assembly-CSharp`**: no assembly boundaries at all. Rejected — Unity recompiles the whole project on every script change, and nothing stops a UI class from calling into a physics callback.
- **`Core` as an interfaces dump**: every cross-module interface (`IScoreService`, `ISceneLoader`, `IDialogueService`, ...) declared in `Core`, implementations left in their feature assemblies. Rejected — it turns `Core` into a second copy of the whole feature list and gives every module a reason to reference it, recreating "everything depends on everything" one level removed.
- **Keep `PauseService`/`GameFlowService`/`CursorService`/`InGameMenuController` in `Core`**: their pre-refactor location. Rejected — pause legitimately needs `IPlayerControlSwitch` (fine, a `Core` seam), but the in-game menu also needs `IDialogueState` (`Dialogue`) and scene transitions (`SceneManagement`); keeping them in `Core` would recreate the cycle `Core → SceneManagement → Gameplay → Core` that existed before the refactor. They live in the new `DreamVille.App` assembly instead — one layer above the infrastructure it composes, one layer below `UI`.
