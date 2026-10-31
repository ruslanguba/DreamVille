# 0011. Enemy archetypes: five numbers in seven config assets, changed first and moved second

Status: Accepted
Date: 2026-10-31

## Context

Five numbers decide how a throwing enemy plays: move speed, chase range, attack range,
attack cooldown and turn rate. Until roadmap 6.3 every enemy carried them as serialized
fields of its own, overriding the base prefab ([ADR 0010](0010-renaming-serialized-fields.md)
tells why those names stayed frozen for so long). `EnemyConfig` held only the knobs that
no instance overrode.

- **The data was scattered.** 137 enemies in 15 prefabs and scenes carried 28 distinct
  hand-set tuples as 331 per-instance overrides in 12 files. A retune meant finding the
  right overrides among them, and nothing said which tuple was a design choice and which a
  slip.
- **The state machine collapses the tuples into a handful of behaviours.**
  - A chase range at or below the attack range never chases: the enemy attacks as soon as
    the player is inside the attack range. Chase range 0 and 1 (attack range 5), or 7
    (attack range 10), behave the same.
  - The move speed of an enemy with no patrol route and no chase never shows in play.
  - A turn rate of 0 snaps the enemy toward the player once when an attack starts, then it
    never tracks the player again.
- **The tail was accidental.** A thrower in Level_04 attacked from 50 m, another enemy had
  an attack range of 1, two had a cooldown of 0.3 s, eight a turn rate of 0. Nobody saw
  them without diffing YAML.

## Decision

**Tuning belongs to a kind of enemy, not to an instance: seven `EnemyConfig` assets hold
the five numbers, and a placed enemy keeps only its placement.**

| Archetype | Move | Chase | Attack | Cooldown | Turn | Reads as | Enemies |
|---|---|---|---|---|---|---|---|
| `Hunter` | 5 | 7 | 5 | 4 | 8 | walks its route, chases from 7 m, throws inside 5 m every 4 s; base-prefab default | 43 |
| `Stalker` | 1 | 10 | 5 | 2 | 1 | creeps after the player from 10 m, throws inside 5 m every 2 s | 20 |
| `RapidSentry` | 4 | 0 | 5 | 1 | 1 | never chases, walks to its post if given one, throws every second | 20 |
| `Sentry` | 0 | 0 | 5 | 2 | 1 | stands where placed, throws inside 5 m every 2 s | 13 |
| `Lookout` | 3 | 0 | 10 | 2 | 8 | never chases, walks its route, throws from 10 m every 2 s | 14 |
| `Runner` | 5 | 0 | 0 | 0 | 8 | no attack; flees when a hideout sends it | 25 |
| `Walker` | 4 | 7 | 5 | 2 | 10 | the fridge walkers, patrol mode `Once` | 2 |

The assets live in `Assets/_Project/Settings/Enemies/`. A chase range of 0 is the sentinel
for "never chases".

- **A placed enemy keeps three fields.** [`Enemy`](../../Assets/_Project/Scripts/Gameplay/Enemies/Enemy.cs)
  serializes `_config`, `_patrolPoints` and `_attackSpawn`. The states read
  `_context.Config` directly, `EnemyTuning` (the per-instance snapshot struct) is deleted,
  and `Enemy.Awake` hands the turn rate to `EnemyRotationHandler.Configure`, so the handler
  has no copy of its own.
- **`Hunter` is the former `Thrower` asset.** It was renamed with `AssetDatabase.RenameAsset`
  and kept its GUID, so the base prefab and the 109 enemies inheriting from it kept their
  reference with no new override. `Stalker`, `RapidSentry`, `Sentry` and `Lookout` are
  copies of it, so every knob except the five numbers equals the old `Thrower`. `Runner` and
  `Walker` kept their names.
- **Two commits, on purpose.** `d467dbb` changed values only (`fix(enemies)`, gameplay):
  every enemy now held the numbers of its archetype. Its 80 changed rows in
  [`EnemyTuningSnapshot.tsv`](../../Assets/_Project/Tests/EditMode/Enemies/EnemyTuningSnapshot.tsv)
  were reviewed one by one, next to Play Mode measurements taken before and after.
  `e2d8a7b` moved them (`refactor(enemies)`): the numbers went onto the config assets and
  the per-instance fields and overrides left the enemies. Its snapshot diff only appends a
  `config` column; the first nine columns are byte-identical to the file after `d467dbb`.
  A reviewer can tell a balance change from a refactor because they are not in one diff.
- **Tests hold the rules.**
  [`EnemyContentTests`](../../Assets/_Project/Tests/EditMode/Enemies/EnemyContentTests.cs)
  rejects a chase range that is neither 0 nor beyond the attack range, and a thrower
  without a positive attack range, cooldown and turn rate.
  [`EnemyTuningSnapshotTests`](../../Assets/_Project/Tests/EditMode/Enemies/EnemyTuningSnapshotTests.cs)
  rejects a stationary enemy (move speed 0) that has a patrol route, and still fails on any
  lost data: the snapshot keeps the resolved values per enemy and adds the config name.
  [`EnemyPatrolStateTests`](../../Assets/_Project/Tests/EditMode/Enemies/EnemyPatrolStateTests.cs)
  pins that chase range 0 attacks even at point-blank range, the case the first rule relies
  on.
- **The removed data follows ADR 0010.** The five dead names (`Enemy._moveSpeed`,
  `_chaseRange`, `_attackRange`, `_attackCooldown` and `EnemyRotationHandler._rotationSpeed`)
  went into the retired-name ledger, and their overrides were dropped by editing each
  instance root's `m_Modification.m_Modifications` through `SerializedObject`, never with
  `SetPropertyModifications`.

## Consequences

- Balance is an asset edit. Change a value on one archetype and the snapshot test fails
  until the rows of every enemy it reaches are updated, so the TSV diff lists them.
- A genuinely new per-enemy need is a new archetype, not an override. Placed copies are
  expected to differ in placement only.
- The first commit changed gameplay on purpose. Of the 54 enemies in the five build scenes
  that have enemies, 28 were untouched, 14 changed numbers without changing behaviour (a
  chase range at or below the attack range, a speed on an enemy that never moves) and 12
  changed behaviour:
  - Six stationary throwers that used to snap toward the player once and then freeze now
    track it (Level_01, Level_03, Lobby_01). One Lobby_01 sentry turns slower than before.
  - Level_04's 50 m thrower throws inside 5 m.
  - Level_01's practice target throws every 2 s instead of every 4 s and still never moves.
  - One Level_02 thrower reaches 10 m (was 8 m), throws every 2 s (was 4) and still never
    chases.
  - One Level_02 hunter chases at 5 m/s (was 3).
  - Two Level_04 guards walk to their posts at 4 m/s (was 1). They already stand on those
    posts at load, so the effect is small.
- The other 54 changed rows of the first commit are in prefabs and the `Scenes/Max`
  prototype levels, which are not in the build. They were normalised in the same commit so
  that the second one could stay value-neutral. The `Max` scenes are covered by the
  snapshot only and were not played.
- The archetypes mirror what the designers had built, so some values (a Stalker moving at
  1 m/s) are history rather than a considered balance. They are now visible and one edit
  away.
- The state machine itself is unchanged: an archetype only decides which numbers it reads.

## Alternatives considered

- **Keep per-instance overrides.** The tail stays invisible and a retune stays a hunt
  through 331 overrides. Rejected.
- **A prefab variant per archetype.** Re-instancing 137 placed enemies would break the
  kill-group and patrol references that point at them. Rejected.
- **One config per tuple.** 28 assets would make the accidents official (an attack range
  of 50 gets a name). Rejected.
- **A config plus optional per-field overrides.** Every override is a new tuple, so the
  original problem returns one exception at a time. Rejected.
- **One commit for values and structure.** A reviewer could not tell a balance change from
  a refactor, and the byte-identical snapshot gate would not exist. Rejected.

Related: [ADR 0010](0010-renaming-serialized-fields.md) (the procedure for removed fields
and the enemy snapshot this builds on).
