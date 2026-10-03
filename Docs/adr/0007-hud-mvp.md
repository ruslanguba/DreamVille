# 0007. HUD as MVP: passive views, a presenter, a scoring coordinator

Status: Accepted
Date: 2026-09-30

## Context

The gameplay HUD was wired by one class, `GameplayPresenter`, that lived in the
`Bootstrap` (composition root) assembly. It did two jobs. It connected world events to
HUD widgets, and it ran the scoring rules: a kill becomes score, score becomes star
progress, clearing the level saves the best score. It depended on `GameUIRoot`, a
concrete MonoBehaviour, so it could not be tested without the HUD prefab. It subscribed
view methods directly (`ScoreAdded += popup.ShowScore`), so the views' public API was the
event signature and `RicochetUI` even took a Gameplay struct. The "first badge means two
ricochets" rule sat in a view.

Three HUD classes still used coroutines, and two of them had overlap bugs:

- The score popup started a new 3 s timer on every score and never cancelled the old one.
  A kill at t=0 and another at t=2 hid the second popup at t=3, after one second.
- The ricochet badge did the same with its 0.5 s timer.
- The star meter did not raise the next star's fill when a score crossed a threshold.
  With 40 points and a +20 kill, star 2 stayed at 0 instead of 0.2 until the next score.

Full design, GUID scan, smoke plan and test list: spec
`09-hud-presenter.md` (local planning notes, not committed).

## Decision

**Views are passive and sit behind one-method interfaces.**
[`GameUIRoot`](../../Assets/_Project/Scripts/UI/Hud/GameUIRoot.cs) implements
[`IGameHudView`](../../Assets/_Project/Scripts/UI/Hud/IGameHudView.cs), which exposes five
blocks: [`IScoreView`](../../Assets/_Project/Scripts/UI/Hud/IScoreView.cs),
[`IStarsView`](../../Assets/_Project/Scripts/UI/Hud/IStarsView.cs),
[`IPillowView`](../../Assets/_Project/Scripts/UI/Hud/IPillowView.cs),
[`IEnemyCounterView`](../../Assets/_Project/Scripts/UI/Hud/IEnemyCounterView.cs) and
[`IRicochetView`](../../Assets/_Project/Scripts/UI/Hud/IRicochetView.cs). The MonoBehaviours
behind them ([`ScorePopupView`](../../Assets/_Project/Scripts/UI/Hud/ScorePopupView.cs),
[`RicochetView`](../../Assets/_Project/Scripts/UI/Hud/RicochetView.cs),
[`StarsView`](../../Assets/_Project/Scripts/UI/Hud/StarsView.cs) with
[`ScoreStar`](../../Assets/_Project/Scripts/UI/Hud/ScoreStar.cs),
[`PillowView`](../../Assets/_Project/Scripts/UI/Hud/PillowView.cs),
[`EnemyCounterView`](../../Assets/_Project/Scripts/UI/Hud/EnemyCounterView.cs)) take
primitives and `Vector3`, never Gameplay or Scoring types. They only draw. The
files were moved with `git mv`, so script GUIDs, and therefore the HUD prefab, did not change.

**The presenter lives with its views, in the UI assembly.**
[`HudPresenter`](../../Assets/_Project/Scripts/UI/Hud/HudPresenter.cs) is a plain class
(`IInitializable, IDisposable`) that takes six interfaces and translates events into view
calls. It holds no rules. The ricochet rule "badge index = ricochets - 2" is a constant
on the presenter, not in the view. This adds one assembly edge, `UI -> Scoring`, for
`IScoreService` and `IStarProgressService` events. It points down the layers and creates
no cycle ([ADR 0002](0002-assembly-layering.md)). Presenter tests use hand-written fakes
([`HudPresenterTests`](../../Assets/_Project/Tests/EditMode/UI/HudPresenterTests.cs)) and
do not touch the composition root.

**Scoring rules move to `Progression`.**
[`ScoringCoordinator`](../../Assets/_Project/Scripts/Progression/ScoringCoordinator.cs)
owns three rules and knows nothing about the HUD:

| Event | Rule |
|---|---|
| `IEnemyRegistry.EnemyKilled` | `IScoreService.HandleEnemyKilled(ricochets)` |
| `IScoreService.ScoreAdded` (kills, pickups, paint) | `IStarProgressService.AddScore` |
| `IEnemyRegistry.AllEnemiesKilled` | `IBestScoreService.TrySave(TotalScore)` |

The registry raises the last `EnemyKilled` before `AllEnemiesKilled`, so the saved best
score includes the final kill. That ordering contract is now local to one class and
covered by
[`ScoringCoordinatorTests`](../../Assets/_Project/Tests/EditMode/Progression/ScoringCoordinatorTests.cs).
`Progression` gains the `Progression -> Scoring` edge, which the conventions already allowed.

**One installer owns the HUD.**
[`UiInstaller`](../../Assets/_Project/Scripts/Bootstrap/UiInstaller.cs) binds
`IGameHudView` to `GameUIRoot` from the prefab and `BindInterfacesTo<HudPresenter>()`
next to it. The coordinator is bound in
[`ProgressionInstaller`](../../Assets/_Project/Scripts/Bootstrap/ProgressionInstaller.cs)
beside the scoring services. `GameplayInstaller` had nothing left to bind and was
removed, so gameplay scenes now install four installers: `SceneServicesInstaller`,
`CombatInstaller`, `ProgressionInstaller`, `UiInstaller`.
[`SceneCompositionTests`](../../Assets/_Project/Tests/EditMode/Composition/SceneCompositionTests.cs)
pins that set.

**The newest show owns the hide timer.**
[`Cancellation.Restart`](../../Assets/_Project/Scripts/Core/Async/Cancellation.cs) cancels
and disposes the previous run, then returns the token of a new source linked to the
object's destroy token. It works on array elements (`ref _hideCts[i]`). This is the
[ADR 0006](0006-unitask-lifetimes.md) one-shot pattern with the supersede step in one
tested helper. Effects:

- The popup hides 3 s after the latest score, not the first one.
- Each ricochet badge hides 0.5 s after its own latest show.
- `ScoreStar`'s pop animation restarts cleanly on a new unlock.
- Both durations are `[SerializeField]` fields, not magic numbers.
- `RicochetView` deactivates every badge in `Awake`. Before, they were active and
  off-screen at scene start, so the first show of each badge did not replay its pulse.

**The star-meter overflow is fixed.** After the unlock loop,
[`StarProgressService.AddScore`](../../Assets/_Project/Scripts/Scoring/StarProgressService.cs)
raises progress again for the new current star. 40 + 20 now shows star 2 at 0.2 at once.

## Consequences

- `HudPresenter` depends only on interfaces, so the whole HUD reaction table is tested
  with fakes and no scene. `ScoringCoordinator` is tested with real scoring services over
  in-memory storage.
- `Bootstrap` holds bindings only. A reader looking for "where is the scoring rule" finds
  one 60-line class instead of a presenter that also draws.
- The coroutine allow-list in `CoroutineGuardTests` shrank from six files to three
  (`OnText`, `HintDestroyOnTime`, `FadeCanvasText`). Those three belong to roadmap 5.4.
  (Since emptied: see the [ADR 0006 update](0006-unitask-lifetimes.md#update-54).)
- Two behaviours changed on purpose and are visible: overlapping popups and badges stay
  up longer, and the next star fills immediately after an unlock. Everything else (look,
  durations, pause freezing the HUD) is unchanged.
- Presenter and coordinator handlers of one event run in unspecified order. It does not
  matter: both finish in the same frame.
- The HUD still shows no running total or best score. `TotalScoreChanged` and
  `GetBest()` have no consumer yet.
- 436 EditMode tests (434 pass, 2 intentionally ignored), up from 403 before this step.

## Alternatives considered

- **Keep the presenter in `Bootstrap`.** No asmdef change, but presenter tests would depend
  on the composition root and Bootstrap would keep holding logic. Rejected.
- **A separate `DreamVille.UI.Hud` assembly.** Ceremony for about 15 files. Rejected.
- **`ScoringCoordinator` inside `LevelDirector`.** Level completion and score bookkeeping
  change for different reasons. Rejected.
- **`ScoringCoordinator` in `Scoring`.** It would need `Scoring -> Gameplay` to see
  `IEnemyRegistry`; infrastructure must not reference a feature. Rejected.
- **The presenter runs the scoring rules too (status quo).** Two responsibilities, and
  the "last kill counts toward best score" ordering stays untested. Rejected.
- **Keep the `*UI` class names** (`RicochetUI : IRicochetView`). Reads like an unfinished
  rename; the `git mv` renames cost nothing because the HUD prefab is never instanced in
  a scene. Rejected.
- **Keep `GameplayInstaller` as the home of `HudPresenter`.** No editor work, but the HUD
  concern would stay split across two installers, and "Gameplay" would name nothing it
  binds. Rejected.
- **Views subscribe to services themselves (no presenter).** The view would know domain
  types again and could not be tested without a scene. Rejected.

Related: [ADR 0001](0001-zenject-composition-root.md) (installers),
[ADR 0002](0002-assembly-layering.md) (the new edges),
[ADR 0006](0006-unitask-lifetimes.md) (the lifetime pattern `Cancellation.Restart` extends).
