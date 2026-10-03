# 0005. Interaction effects as sibling components

Status: Accepted
Date: 2026-09-26

## Context

Before this step, "something happens when it is hit or touched" was eleven unrelated
MonoBehaviours. Six went through a `void OnInteract()` interface with no context (a
receiver could not tell a bullet from a footstep); three (`InterectObject`,
`EnemyInFridge`, `StartRocket`) bypassed the interface entirely and read
`OnCollisionEnter` + `TryGetComponent<Bullet>` themselves; pickups were a separate
`Collectable` → `CollectableGun` / `CollectableSkin` inheritance chain reached through
`PlayerCollector.OnTriggerEnter`. A lamp fused toggling and sound in one class; a rocket
inherited from a rigidbody activator to reuse one line; a door existed twice
(`InterectObject`, `EnemyInFridge`) with the same animator strings copy-pasted. Nothing
in this surface was composable and nothing in it was tested.

## Decision

**One channel.** `IInteractable.OnInteract(in InteractionContext)` — a single method, a
readonly struct carrying `InteractionSource` (`Shot` or `Touch`) and `Instigator` (the
bullet's transform, or the player's collector transform). Two senders build the context
and call it: `BulletInteractionHandler` on a bullet hit
([`Weapons/Bullets/BulletInteractionHandler.cs`](../../Assets/_Project/Scripts/Gameplay/Weapons/Bullets/BulletInteractionHandler.cs))
and `PlayerCollector` on a trigger touch
([`Player/PlayerCollector.cs`](../../Assets/_Project/Scripts/Gameplay/Player/PlayerCollector.cs)).
Both senders only know `IInteractable`
([`Interactables/IInteractable.cs`](../../Assets/_Project/Scripts/Gameplay/Interactables/IInteractable.cs)).

**Two hosts.** `Interactable`
([`Interactables/Interactable.cs`](../../Assets/_Project/Scripts/Gameplay/Interactables/Interactable.cs))
reacts to `Shot`, every time, for content that is hit repeatedly (a lamp, a rocket, a
hideout). `Pickup`
([`Interactables/Pickup.cs`](../../Assets/_Project/Scripts/Gameplay/Interactables/Pickup.cs))
reacts to `Touch` once, then adds its score, applies its effects and flies into the
collector before destroying itself — a lifecycle (idle spin, collider off, score,
tween, self-destroy) that every pickup shares and no shot-interactable has. Splitting
that lifecycle into more "effects" would have made the destroy-after-tween ordering
implicit; two lifecycles sharing one effect vocabulary keeps it explicit.

**Effects as sibling components, not a `[SerializeReference]` list.** Both hosts gather
`IInteractionEffect` components from their own GameObject through
[`EffectChain.FromComponents`](../../Assets/_Project/Scripts/Gameplay/Interactables/EffectChain.cs)
(`GetComponents<IInteractionEffect>()`, in Inspector order) and apply them in sequence,
logging and continuing past a throwing effect instead of cancelling the rest. A lamp is
`Interactable` + `ToggleLightEffect` + `PlayAudioSourceEffect` — verified on
[`Prefabs/InteractableLamp.prefab`](../../Assets/_Project/Prefabs/InteractableLamp.prefab),
which carries exactly those three components on its root. A fridge full of enemies is
`Interactable` + `EnemyHideout`; the kept `Misc/FridgeWithEnemy.prefab` door is
`Interactable` + `ToggleDoorEffect` + `ActivateObjectsEffect`. Eleven effect classes
exist, each mapped to the content that needs it (`ToggleLightEffect`,
`PlayAudioSourceEffect`, `PlaySfxEffect`, `FloatAwayEffect`, `ReleaseRigidbodyEffect`,
`RocketThrustEffect`, `ToggleDoorEffect`, `ActivateObjectsEffect`, `PaintEffect`,
`ChangeSkinEffect`, `EquipWeaponEffect`), all under
[`Interactables/Effects/`](../../Assets/_Project/Scripts/Gameplay/Interactables/Effects).

Most renamed effects are `git mv` of the class they replace, so the GUID stays and a
prefab-instance override on the old field survives the rename under a new name (via
`[FormerlySerializedAs]` where needed): `InteractableLamp → ToggleLightEffect`,
`InteractableBalloon → PlaySfxEffect`, `InteractableRigitbodyActivator →
ReleaseRigidbodyEffect`, `StartRocket → RocketThrustEffect`, `EnemyInFridge →
ToggleDoorEffect`, `ShaderPaintablePrototype → PaintEffect`, `CollectableSkin →
ChangeSkinEffect`, `CollectableGun → EquipWeaponEffect`, `Collectable → Pickup`,
`InteractableBox → EnemyHideout`. The frozen field names this preserves —
`EnemyHideout._enemies` / `_nextPointsTransform`, `PlaySfxEffect._clip`,
`Pickup._scoreToAdd`, `PaintEffect._renderer` / `_colorChangeMaterial` — are the ones
carrying live overrides in scenes and prefabs; changing them would silently drop those
overrides on the next save.

`EffectChain` is a plain class, not a MonoBehaviour: `EffectChain.Apply` isolates a
throwing effect (`try`/`catch` per effect, `Debug.LogException` with the effect as
context) so one broken effect never blocks its siblings — a case that could not happen
before, when every interactable was a single class with a single body.

`ChangeSkinEffect` (namespace `DreamVille.Gameplay.Interactables.Effects`) reaches the
player through `IPlayerSkins`
([`Player/Skins/IPlayerSkins.cs`](../../Assets/_Project/Scripts/Gameplay/Player/Skins/IPlayerSkins.cs),
implemented by
[`PlayerSkins`](../../Assets/_Project/Scripts/Gameplay/Player/Skins/PlayerSkins.cs))
resolved from `context.Instigator.GetComponentInParent<IPlayerSkins>()`. This is a
namespace back-reference — `Interactables.Effects` depends on `Player.Skins`, while a
skin pickup lives in the scene, not the player — but both namespaces are folders inside
the single `DreamVille.Gameplay` assembly, so it is not an assembly-graph cycle
(assembly layering, [ADR 0002](0002-assembly-layering.md), is unaffected). No new
Zenject binding was needed: the only consumer of `IPlayerSkins` is a scene-placed
effect, which cannot see the player's `GameObjectContext` sub-container bindings anyway,
so it reaches the interface the same way `CollectableSkin` reached the concrete
`SkinHandler` before this change — through the touching object's transform, not through
DI.

## Consequences

- A reviewer reads composition straight off the Inspector: select a lamp, see
  `Interactable + ToggleLightEffect + PlayAudioSourceEffect`; no base class, no
  `OnCollisionEnter` duplicated across three files.
- Adding a new interactable is "add `Interactable` or `Pickup`, add the effect
  components it needs" — no new host code, no subclass.
- `EffectChain`, `PickupTween` and `PaintProgress` are plain classes with unit tests
  ([`Tests/EditMode/Interactables/`](../../Assets/_Project/Tests/EditMode/Interactables));
  the MonoBehaviour hosts only gather components and forward Unity callbacks.
- Effects that used to fuse two behaviours (lamp: light + sound; balloon: sound + float)
  are now two independent components; removing one no longer risks breaking the other,
  and a prefab can mix and match (a silent lamp is just `Interactable` +
  `ToggleLightEffect`, no `PlayAudioSourceEffect`).
- `Interactables.Effects → Player.Skins` is a namespace dependency a future assembly
  split would have to resolve (for example by moving `IPlayerSkins` up a level); it is
  accepted now because both live in one assembly and splitting `Gameplay` further is out
  of scope for this step.

## Alternatives considered

- **`[SerializeReference] IInteractionEffect[]` on the host.** Rejected: Unity 6 ships no
  type picker for a `SerializeReference` list (a custom `PropertyDrawer` would be editor
  tooling built for its own sake), Zenject cannot inject into list entries without
  handing the host a `DiContainer` (forbidden by the project's DI conventions), and
  overrides on individual list entries are fragile across prefab variants. Sibling
  components give a normal Inspector, normal prefab overrides, normal `[Inject]`, and a
  GUID-preserving `git mv` from the classes they replace.
- **One `Interactable` with a "source" and "single-use" flag instead of two host
  classes.** Rejected: the pickup's tween/score/destroy lifecycle is not "one more
  effect" — it owns the order effects run in (collider off and score added before the
  tween starts) and the destroy at the end. Encoding that as configuration on a single
  host would hide the ordering that two explicit classes make visible.
- **Keep the old per-class `OnInteract()` with no context.** Rejected: without a
  `Source`, a receiver cannot refuse a touch it should ignore (a bullet passing through
  a lamp would have "interacted" with it) — the three classes that bypassed the channel
  existed because the old interface could not express "shots only".
- **`HitPoint` / `HitDirection` fields on `InteractionContext`.** Rejected for now: no
  effect reads them today; the struct adds fields only when an effect needs one, since
  `Instigator` alone already serves the two current readers (`Pickup`'s fly-to target,
  `ChangeSkinEffect`'s player lookup).

Related: [ADR 0002 — assembly layering](0002-assembly-layering.md) (why the namespace
back-reference does not cross an assembly boundary), spec `06-interactables-composition.md` (local planning notes, not committed)
(the full migration, GUID scan and test list).
