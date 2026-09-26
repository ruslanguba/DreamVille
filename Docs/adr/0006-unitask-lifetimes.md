# 0006. UniTask lifetimes for coroutines: one-shot vs enable-bound

Status: Accepted
Date: 2026-09-26

## Context

Before this step, nine components under `Gameplay/NPC`, `Gameplay/Enemies` and
`Gameplay/Environment` used coroutines (`StartCoroutine`, `WaitForSeconds`, bare
`yield return null`) for timed behaviour: an NPC queue jumping and re-forming, two
hatters taking turns talking, an NPC shooter's fire cycle, a balloon's random drift, an
enemy hideout's open/shrink/close sequence, and the main-menu logo fade. Each one
answered "what happens to a running timer when the object is destroyed, deactivated, or
the game is paused" differently, by accident rather than by design — three ad-hoc
lifetimes, a latent lerp bug in the hideout, two coroutines racing over one list in the
NPC queue, and an `NPCAnimationHandler` with five independent bools that could describe
an impossible pose (talking and applauding at once). [ADR 0004](0004-scene-catalog-and-async.md)
already moved bullet lifetime, pillow recall, one NPC reaction and scene loading to
UniTask; this record covers the rest of the Gameplay assembly and states the lifetime
rule those five cases only implied.

Full inventory, GUID scan and migration steps: spec
[`08-coroutines-to-unitask.md`](../../.claude/refactor/specs/08-coroutines-to-unitask.md).

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
| **One-shot, object-bound** | `this.GetCancellationTokenOnDestroy()`, linked into a per-operation `CancellationTokenSource` where a new call supersedes the old one or a second object's death must end it too | on request | destroy, supersede, or completion | [`EnemyHideout.AbsorbAsync`](../../Assets/_Project/Scripts/Gameplay/Enemies/EnemyHideout.cs) (linked to both the hideout's and the enemy's destroy token), [`ImageFadeOut`](../../Assets/_Project/Scripts/UI/ImageFadeOut.cs) (main-menu logo fade), `NPCMover.MoveToAsync`, `NPCReactionController`'s applause-then-talk |
| **Endless ambient loop, enable-bound** | `_loopCts` created when the loop starts, cancelled and disposed in `OnDisable` | first in `Start`, again in `OnEnable` once `Start` has already run (a `_started` flag) | `OnDisable` (deactivation, destroy, scene unload) | [`NPCDialogueHandler`](../../Assets/_Project/Scripts/Gameplay/NPC/NPCDialogueHandler.cs) (hatters alternating), [`NpcQueueManager`](../../Assets/_Project/Scripts/Gameplay/NPC/NpcQueueManager.cs) (queue jumps), `NPCFire` (NPC shooters), `RandomFloating` (balloon drift) |

The destroy token alone is the wrong choice for an endless loop: a loop that outlives
`OnDisable` keeps calling `Animator.SetBool` on a deactivated NPC (Unity logs "Animator
is not playing an AnimatorController" every cycle) and keeps moving a queue member that
should have frozen with the rest of the scene. Starting the loop in `Start`, not
`OnEnable`, matters too: `NpcQueueManager`'s first step jumps immediately, and a mover on
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

**`NPCAnimationHandler.SetState(NpcState)` is the single owner of an NPC's pose.**
[`NpcState`](../../Assets/_Project/Scripts/Gameplay/NPC/NpcState.cs) is one enum
(`Idle`, `Talk`, `Greeting`, `Applause`) instead of four independent bools plus a fifth,
unread one. [`NpcAnimationState`](../../Assets/_Project/Scripts/Gameplay/NPC/NpcAnimationState.cs)
writes the three mood flags as one operation through
[`INpcAnimatorParameters`](../../Assets/_Project/Scripts/Gameplay/NPC/INpcAnimatorParameters.cs)
(implemented by
[`AnimatorBoolParameters`](../../Assets/_Project/Scripts/Gameplay/NPC/AnimatorBoolParameters.cs),
which ignores bool parameters a given controller does not declare — `PluhNPC` has
talk/greeting/applause, `Townsman_2_greetings_only` has greeting/applause/swim). Its
`ApplyStartPose` rule is: apply the authored start mood unless a state was already
requested, in which case the earlier request wins. This removes a frame-order dependency
several directors used to work around by waiting a frame after scene start before
reacting.
[`NPCAnimationHandler`](../../Assets/_Project/Scripts/Gameplay/NPC/NPCAnimationHandler.cs)
is the MonoBehaviour shell: it finds the NPC's `Animator` in `Awake` and exposes
`SetState`; `NPCReactionController` calls it instead of writing bools by name.

**The NPC queue's two racing coroutines become one sequential loop.**
`NpcQueueManager` used to run an endless loop and a per-jump coroutine that both read and
wrote the same jumper list, with correctness depending on which coroutine Unity resumed
first after an equal-length wait. It is now one `while (true)` loop over a tested
[`RotatingQueue<T>`](../../Assets/_Project/Scripts/Gameplay/NPC/RotatingQueue.cs): jump,
wait, rotate the queue, then move everybody (the re-queued member included) to their
slots with `UniTask.WhenAll`. The order — re-queue before the move — was chosen to match
what Unity's scheduler happened to do before, verified against a Play Mode baseline.

**`CoroutineGuardTests` pins an allow-list that may only shrink.**
[`CoroutineGuardTests`](../../Assets/_Project/Tests/EditMode/Architecture/CoroutineGuardTests.cs)
scans every `.cs` file under `Assets/_Project/Scripts` for `StartCoroutine`,
`StopCoroutine`, `IEnumerator`, `WaitForSeconds`, string `Invoke` and `InvokeRepeating`,
and fails if any of them appear outside six named UI files: `UI/OnText.cs`,
`UI/Hints/HintDestroyOnTime.cs`, `UI/FadeCanvasText.cs`, `UI/ScorePopupUI.cs`,
`UI/RicochetUI.cs`, `UI/ScoreStar.cs`. A second test fails if an allow-listed file no
longer uses a coroutine API, so the list is a checked invariant that can only shrink as
phase 5 (`5.1` UI presenters, `5.4` proximity hints) rewrites those six classes — not a
README claim nobody re-checks.

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
[`PluhNpcControllerTests`](../../Assets/_Project/Tests/EditMode/NPC/PluhNpcControllerTests.cs).

## Consequences

- One class ([`GameTime`](../../Assets/_Project/Scripts/Core/Async/GameTime.cs)) states
  the pause/time-scale policy instead of eleven hand-written `Time.deltaTime` loops each
  getting to answer it independently.
- `grep`-ing `Assets/_Project/Scripts` for a coroutine API returns only the six UI files
  `CoroutineGuardTests` names — a checked fact, not a README claim, and it can only get
  shorter.
- An NPC's mood is an enum with one owner
  ([`NPCAnimationHandler.SetState`](../../Assets/_Project/Scripts/Gameplay/NPC/NPCAnimationHandler.cs)),
  so "talking while applauding" is no longer representable, and a director may react
  before an NPC's `Start` without its request being silently overwritten.
- The NPC queue's jump/move order is deterministic (`RotatingQueue<T>` plus one loop)
  instead of depending on Unity's coroutine resume order.
- Endless loops (dialogue, queue, NPC fire, floating props) now restart when their
  GameObject is re-enabled, which a coroutine never did; nothing in the current content
  toggles these components at runtime, so this is not observed today, but it is a
  documented behaviour change (spec 08 §3.8, D1).
- `403` EditMode tests (401 pass, 2 intentionally ignored), up from 357 before this step.

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
  `ScorePopupUI`, `RicochetUI` and `ScoreStar` have known overlap bugs that belong to the
  `5.1` UI-presenter rewrite, and `OnText`/`HintDestroyOnTime`/`FadeCanvasText` belong to
  `5.4`'s proximity-hint rewrite; converting them here would duplicate work those steps
  already own.

Related: [ADR 0004 — scene catalog and UniTask for gameplay async](0004-scene-catalog-and-async.md)
(bullet lifetime, pillow recall, one NPC reaction and scene loading — the UniTask
patterns this record extends to the rest of Gameplay), spec
[`08-coroutines-to-unitask.md`](../../.claude/refactor/specs/08-coroutines-to-unitask.md)
(the full migration, GUID scan and test list).
