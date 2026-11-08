# 0006. UniTask lifetimes for coroutines: one-shot vs enable-bound

Status: Accepted
Date: 2026-09-26
Updated: 2026-10-03, see [Update (5.4)](#update-54)

## Context

Before this step, nine components under `Gameplay/Npc`, `Gameplay/Enemies` and
`Gameplay/Environment` used coroutines (`StartCoroutine`, `WaitForSeconds`, bare
`yield return null`) for timed behaviour: an NPC queue jumping and re-forming, two
hatters taking turns talking, an NPC shooter's fire cycle, a balloon's random drift, an
enemy hideout's open/shrink/close sequence, and the main-menu logo fade. Each one
answered "what happens to a running timer when the object is destroyed, deactivated, or
the game is paused" differently, by accident rather than by design — three ad-hoc
lifetimes, a latent lerp bug in the hideout, two coroutines racing over one list in the
NPC queue, and an `NpcAnimationHandler` with five independent bools that could describe
an impossible pose (talking and applauding at once). [ADR 0004](0004-scene-catalog-and-async.md)
already moved bullet lifetime, pillow recall, one NPC reaction and scene loading to
UniTask; this record covers the rest of the Gameplay assembly and states the lifetime
rule those five cases only implied.

Full inventory, GUID scan and migration steps: spec
`08-coroutines-to-unitask.md` (local planning notes, not committed).

## Decision

**A tiny Core toolbox owns scaled time, so "pause freezes it" lives in one place.**
[`GameTime`](../../Assets/_Project/Scripts/Core/Async/GameTime.cs) wraps
`UniTask.Delay`/`UniTask.Yield` on `PlayerLoopTiming.Update` with `DelayType.DeltaTime`
(scaled game time; `Time.timeScale = 0` freezes every wait exactly like `WaitForSeconds`
did) and guarantees at least one frame per wait, so a zero-length delay inside a loop
never spins synchronously. [`Tween`](../../Assets/_Project/Scripts/Core/Async/Tween.cs)
runs a per-frame interpolation with progress clamped to `[0, 1]`.
[`Cancellation.CancelAndDispose`](../../Assets/_Project/Scripts/Core/Async/Cancellation.cs)
is the cancel-dispose-null sequence every restartable operation needs. `Core` did not
reference any third-party package before; `DreamVille.Core.asmdef` now references
`UniTask`, which the project's conventions already allow for `Core`.

**Two lifetime patterns, picked per case — the answer to "which token?".**

| Pattern | Token | Starts | Stops | Used for |
|---|---|---|---|---|
| **One-shot, object-bound** | `this.GetCancellationTokenOnDestroy()`, linked into a per-operation `CancellationTokenSource` where a new call supersedes the old one or a second object's death must end it too | on request | destroy, supersede, or completion | [`EnemyHideout.AbsorbAsync`](../../Assets/_Project/Scripts/Gameplay/Enemies/EnemyHideout.cs) (linked to both the hideout's and the enemy's destroy token), [`ImageFadeOut`](../../Assets/_Project/Scripts/UI/ImageFadeOut.cs) (main-menu logo fade), `NpcMover.MoveToAsync`, `NpcReactionController`'s applause-then-talk, [`ProximityHint`](../../Assets/_Project/Scripts/UI/Hints/ProximityHint.cs) (added in 5.4) |
| **Endless ambient loop, enable-bound** | `_loopCts` created when the loop starts, cancelled and disposed in `OnDisable` | first in `Start`, again in `OnEnable` once `Start` has already run (a `_started` flag) | `OnDisable` (deactivation, destroy, scene unload) | [`AmbientConversation`](../../Assets/_Project/Scripts/Gameplay/Npc/AmbientConversation.cs) (hatters alternating; `NpcDialogueHandler` until 6.5), [`NpcQueue`](../../Assets/_Project/Scripts/Gameplay/Npc/NpcQueue.cs) (queue jumps), `NpcFire` (NPC shooters), `RandomFloating` (balloon drift) |

The destroy token alone is the wrong choice for an endless loop: a loop that outlives
`OnDisable` keeps calling `Animator.SetBool` on a deactivated NPC (Unity logs "Animator
is not playing an AnimatorController" every cycle) and keeps moving a queue member that
should have frozen with the rest of the scene. Starting the loop in `Start`, not
`OnEnable`, matters too: `NpcQueue`'s first step jumps immediately, and a mover on
another GameObject may not have run its own `Awake` yet if the loop started there
instead.

**Pause is `Time.timeScale = 0`, freezing all of it.** Every wait in this spec goes
through `GameTime.DelayAsync`, which uses `DelayType.DeltaTime` — the same clock
`Time.deltaTime`-driven loops always used. `DelayType.UnscaledDeltaTime` was rejected:
NPCs would keep talking, shooting and drifting behind the pause menu.

**A zero-length wait still yields one frame.** `GameTime.DelayAsync(0, token)` and
`Tween.RunAsync(0, ...)` were guarded on purpose: `UniTask.Delay` rejects a negative
span, and a zero-length wait that completed synchronously inside a `while (true)` loop
(for example `RandomFloating` already at its target with `stopTime 0`) would hang the
editor. `Tween.RunAsync` with a non-positive duration applies progress `1` once instead
of looping.

**`NpcAnimationHandler.SetState(NpcState)` is the single owner of an NPC's pose.**
[`NpcState`](../../Assets/_Project/Scripts/Gameplay/Npc/NpcState.cs) is one enum
(`Idle`, `Talk`, `Greeting`, `Applause`) instead of four independent bools plus a fifth,
unread one. [`NpcAnimationState`](../../Assets/_Project/Scripts/Gameplay/Npc/NpcAnimationState.cs)
writes the three mood flags as one operation through
[`INpcAnimatorParameters`](../../Assets/_Project/Scripts/Gameplay/Npc/INpcAnimatorParameters.cs)
(implemented by
[`AnimatorBoolParameters`](../../Assets/_Project/Scripts/Gameplay/Npc/AnimatorBoolParameters.cs),
which ignores bool parameters a given controller does not declare — `PluhNPC` has
talk/greeting/applause, `Townsman_2_greetings_only` has greeting/applause/swim). Its
`ApplyStartPose` rule is: apply the authored start mood unless a state was already
requested, in which case the earlier request wins. This removes a frame-order dependency
several directors used to work around by waiting a frame after scene start before
reacting.
[`NpcAnimationHandler`](../../Assets/_Project/Scripts/Gameplay/Npc/NpcAnimationHandler.cs)
is the MonoBehaviour shell: it finds the NPC's `Animator` in `Awake` and exposes
`SetState`; `NpcReactionController` calls it instead of writing bools by name.

**The NPC queue's two racing coroutines become one sequential loop.**
`NpcQueue` used to run an endless loop and a per-jump coroutine that both read and
wrote the same jumper list, with correctness depending on which coroutine Unity resumed
first after an equal-length wait. It is now one `while (true)` loop over a tested
[`RotatingQueue<T>`](../../Assets/_Project/Scripts/Gameplay/Npc/RotatingQueue.cs): jump,
wait, rotate the queue, then move everybody (the re-queued member included) to their
slots with `UniTask.WhenAll`. The order — re-queue before the move — was chosen to match
what Unity's scheduler happened to do before, verified against a Play Mode baseline.

**`CoroutineGuardTests` pins an allow-list that may only shrink.**
[`CoroutineGuardTests`](../../Assets/_Project/Tests/EditMode/Architecture/CoroutineGuardTests.cs)
scans every `.cs` file under `Assets/_Project/Scripts` for `StartCoroutine`,
`StopCoroutine`, `IEnumerator`, `WaitForSeconds`, string `Invoke` and `InvokeRepeating`,
and fails if any of them appear outside three named UI files: `UI/OnText.cs`,
`UI/Hints/HintDestroyOnTime.cs`, `UI/FadeCanvasText.cs`. (It started with six; the HUD
files `ScorePopupUI`, `RicochetUI` and `ScoreStar` left the list when roadmap 5.1 moved
them to UniTask, see [ADR 0007](0007-hud-mvp.md).) A second test fails if an allow-listed file no
longer uses a coroutine API, so the list is a checked invariant that can only shrink as
phase 5 (`5.1` UI presenters, `5.4` proximity hints) rewrites those classes — not a
README claim nobody re-checks. That is the state this record was written in; roadmap 5.4
emptied the list, see [Update (5.4)](#update-54).

**The `PluhNPC.controller` fix.**
[`PluhNPC.controller`](../../Assets/_Project/Art/CitizensModels/Animations/PluhNPC.controller)
drove `PluchNpc`, every `PluhTalking` and every `Ploh_idle` in `Lobby_01` through
transitions that detoured through `Idle`: `Applause → Idle` fired immediately (`SetTalk`
clears `isApplause` in the same write), then `Idle → Talk` waited for 78% of the idle
clip's exit time before actually talking; `Talk → Idle` similarly held the old pose for
90% of its clip. `SetState` now writes the mood as one operation, but the controller
still needed a content fix: `Applause → Talk`, `Idle → Talk` and `Talk → Idle` transitions
have `hasExitTime = false`, and every transition into `Idle` requires all three mood
flags off, so exactly one transition is valid when `SetState` switches moods and none of
them waits for an exit time. This closes roadmap 2.11 hypothesis B (the "pluh animation
switching" bug) and is verified by
[`PluhNpcControllerTests`](../../Assets/_Project/Tests/EditMode/Npc/PluhNpcControllerTests.cs).

## Consequences

- One class ([`GameTime`](../../Assets/_Project/Scripts/Core/Async/GameTime.cs)) states
  the pause/time-scale policy instead of eleven hand-written `Time.deltaTime` loops each
  getting to answer it independently.
- `grep`-ing `Assets/_Project/Scripts` for a coroutine API returned only the three UI files
  `CoroutineGuardTests` names — a checked fact, not a README claim, and it could only get
  shorter. Since 5.4 it finds no coroutine API in code at all (comments may still name
  one), see [Update (5.4)](#update-54).
- An NPC's mood is an enum with one owner
  ([`NpcAnimationHandler.SetState`](../../Assets/_Project/Scripts/Gameplay/Npc/NpcAnimationHandler.cs)),
  so "talking while applauding" is no longer representable, and a director may react
  before an NPC's `Start` without its request being silently overwritten.
- The NPC queue's jump/move order is deterministic (`RotatingQueue<T>` plus one loop)
  instead of depending on Unity's coroutine resume order.
- Endless loops (dialogue, queue, NPC fire, floating props) now restart when their
  GameObject is re-enabled, which a coroutine never did; nothing in the current content
  toggles these components at runtime, so this is not observed today, but it is a
  documented behaviour change (spec 08 §3.8, D1).
- `403` EditMode tests (401 pass, 2 intentionally ignored), up from 357 before this step.

## Update (5.4)

2026-10-03. Roadmap step 5.4 retired the last three UI coroutines. No coroutine remains
under `Assets/_Project/Scripts`. The decisions above stand; this is what changed.

- **`OnText` and `HintDestroyOnTime` became one component,**
  [`ProximityHint`](../../Assets/_Project/Scripts/UI/Hints/ProximityHint.cs). It is the
  one-shot pattern from the table: the show is a `UniTaskVoid` whose token comes from
  `Cancellation.Restart(ref _showCts, this.GetCancellationTokenOnDestroy())`, the same
  helper the HUD views use ([ADR 0007](0007-hud-mvp.md)). Entering the trigger again
  supersedes the running show; destroying the hint ends it. The wait and the fade run
  through `GameTime.DelayAsync` and `Tween.RunAsync`, so pause freezes them. A `_showOnce`
  flag replaces the second class: it destroys the trigger after the first fade.
- **The prefab kept its identity.** `TriggerForText.prefab` was renamed to
  [`ProximityHint.prefab`](../../Assets/_Project/Prefabs/ProximityHint.prefab) with its
  GUID, and the serialized field names stayed, so the instances in seven scenes (four
  build scenes, three excluded `Scenes/Max` scenes) kept their overrides.
- **Dead code was deleted, not ported.** `FadeCanvasText`, `CanvasTrigger` and `Tooltip`
  had no callers; they went with two orphan prefabs. The planned `UiFader` and
  `PlayerProximityToggle` helpers were not built for the same reason, and their shared
  mechanics (`GameTime`, `Tween`, `Cancellation`) already live in `Core/Async`.
- **`CoroutineGuardTests` keeps an empty allow-list.** The allow-list stays in the test
  as the one place for a deliberate exception, each entry with a reason comment. That was
  a deliberate choice over a strict ban. The second test, which fails when a listed file
  stops using coroutines, passes on an empty list and checks again once an entry exists.
  The pattern also catches `StopAllCoroutines`, `WaitForSecondsRealtime`, `WaitForEndOfFrame`,
  `WaitForFixedUpdate`, `new WaitUntil`/`new WaitWhile` (not `UniTask.WaitUntil`),
  `CancelInvoke`, `Invoke(nameof(...))` and `Coroutine` fields, and has line-level tests
  for what it must flag and what it must ignore.
- **Content tests pin the hints.**
  [`ProximityHintContentTests`](../../Assets/_Project/Tests/EditMode/UI/ProximityHintContentTests.cs)
  compares all 14 hints in the scenes (target, durations, `showOnce`, text) with
  [`ProximityHintSnapshot`](../../Assets/_Project/Tests/EditMode/UI/ProximityHintSnapshot.cs).
  Scene and prefab tests fail on missing scripts: the net under the GUID scan done before
  each deletion. A composition test requires every hint target in a gameplay scene to sit under
  a screen-space canvas: Level_02's three hints had never been drawn because no canvas was
  above them, a pre-existing bug fixed in the same step.
- **`610` EditMode tests** (608 pass, 2 intentionally ignored).

## Alternatives considered

- **One destroy-bound token for every case**, as the roadmap's shorthand originally
  suggested. Rejected for endless loops: it keeps a loop's `Animator`/`Rigidbody` calls
  firing against a deactivated object and never stops on deactivation, the one thing a
  coroutine always did for free.
- **`PlayerLoopTiming.LastUpdate`** to mimic the exact frame slot a coroutine resumed in.
  Rejected — it would break the codebase's single existing convention
  (`PlayerLoopTiming.Update`, already used by every UniTask call site before this step)
  for no observable gain; the difference is at most one frame at a wait's boundary.
- **A shared `AmbientLoop` base class** for the four enable-bound loops. Rejected:
  inheritance for eight lines of `OnEnable`/`OnDisable`/`Start` boilerplate across four
  otherwise-unrelated components is ceremony; each loop's body is different enough that a
  base class would only save the lifetime plumbing, which `Cancellation.CancelAndDispose`
  already shares.
- **Deferring the UI coroutines into this step instead of an allow-list.** Rejected:
  `ScorePopupUI`, `RicochetUI` and `ScoreStar` had known overlap bugs that belonged to the
  `5.1` UI-presenter rewrite (done, see [ADR 0007](0007-hud-mvp.md)), and `OnText`/`HintDestroyOnTime`/`FadeCanvasText` belong to
  `5.4`'s proximity-hint rewrite (done, see [Update (5.4)](#update-54)); converting them
  here would duplicate work those steps already own.
- **An `ICoroutineRunner`**: one MonoBehaviour that runs coroutines on behalf of plain
  classes (considered for 5.4). Rejected. The run's lifetime is detached from its owner: a
  destroyed or unloaded owner leaves the coroutine running against destroyed objects, the
  same class of teardown failure [ADR 0009](0009-data-driven-dialogues.md) fixed for
  dialogues (restart or exit to menu used to log a `MissingReferenceException`). A
  coroutine takes no cancellation token and returns no value. And it would be a second
  async model next to `Core/Async`.
- **Keeping coroutines for simple UI effects.** Defensible: a fade is a few lines either
  way. Rejected as the default because two models mean two sets of cancel, pause and test
  rules for the same job. The empty allow-list keeps the door open one file at a time, with
  a written reason.
- **Unity 6 `Awaitable`** (built-in async/await, no package). Not adopted. UniTask stays
  for its allocation-free waits, `WhenAll`/`WhenAny` (`NpcQueue` uses `WhenAll`),
  a `PlayerLoopTiming` chosen per call and test ergonomics, and because the project had
  already standardised on it: switching would touch every call site for no gain the code
  needs. No benchmark was run here; the choice rests on features and consistency.

Related: [ADR 0004 — scene catalog and UniTask for gameplay async](0004-scene-catalog-and-async.md)
(bullet lifetime, pillow recall, one NPC reaction and scene loading — the UniTask
patterns this record extends to the rest of Gameplay), spec
`08-coroutines-to-unitask.md` (local planning notes, not committed)
(the full migration, GUID scan and test list).
