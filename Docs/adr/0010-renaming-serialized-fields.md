# 0010. Renaming serialized fields: one procedure, verified data, no attributes left behind

Status: Accepted
Date: 2026-10-31

## Context

Unity stores a serialized field under its C# name, in two places: the component's own data
in a scene, prefab or asset, and every prefab-instance override that addresses it by
property path (`m_Modifications`). Renaming the field in code alone makes Unity load the
default value without an error.

- The enemy refactor (roadmap 4.1) froze seven names (`Enemy.moveSpeed`, `chaseRange`,
  `attackRange`, `attackCooldown`, `patrolPoints`, `attackSpawn` and
  `EnemyRotationHandler.rotationSpeed`): level designers had tuned them per enemy
  instance, and `[FormerlySerializedAs]` was believed not to remap override paths. The
  conventions carried that as a documented exception, and the list grew twice afterwards
  ([ADR 0005](0005-interaction-effects-as-components.md) and
  [ADR 0009](0009-data-driven-dialogues.md) record two of those frozen names).
- The dialogue migration (roadmap 5.3) measured the belief. The attribute does remap
  instance overrides at load, but on save Unity writes them again under the new name and
  leaves the old entries in place as dead duplicates (223 had to be removed). The risk is
  not lost values; it is dead data and an assumption nobody verified per component.
- Phases 2-5 had renamed 56 fields on 26 classes behind `[FormerlySerializedAs]` and kept
  the attributes. Fifty of the old names occurred in no asset any more, but six were
  load-bearing: `Prefabs/Misc/AudioManager.prefab` stored its mixer and both audio sources
  only under the old keys. Deleting those three attributes would have silenced every sound
  in the game, with no error at load.
- Overrides can also address nothing at all: 50 overrides of `Enemy.target` and
  `throwForce`, fields that left `Enemy` in roadmap 4.2, and about 1,800 modifications
  project-wide whose target objects no longer exist in their source prefab.

## Decision

**A serialized field is renamed by one procedure, and its data moves in the same commit.**

1. Rename the field and add `[FormerlySerializedAs("old")]` for the editor session only.
2. Gate: dump the resolved values of every scene, prefab and asset that holds the
   component and compare them with a baseline taken before the rename. Equality proves the
   attribute remaps this component; nothing is saved before the gate passes.
3. Re-serialize the base data: `AssetDatabase.ForceReserializeAssets` for prefabs and
   assets, open-mark-save for scenes, one at a time.
4. Rewrite the prefab-instance overrides explicitly, nested prefabs first: per outermost
   instance root, rename the property paths of the renamed fields and drop overrides of
   fields that no longer exist on that component. Everything else stays byte-identical.
   When a root also holds overrides of objects that have left its source prefab, edit the
   instance's `m_Modification.m_Modifications` through `SerializedObject`:
   `PrefabUtility.SetPropertyModifications` would rewrite their targets as `{fileID: 0}`.
5. Reopen everything from disk: resolved values equal the baseline, no key or path uses
   the old name.
6. Remove the attribute, record the old name in the ledger, run the tests and a Play Mode
   smoke, and commit code and assets together.

**The attribute is a migration tool, never a fixture.** No `[FormerlySerializedAs]` is
committed, and the legacy ones were retired once the three prefabs that still loaded
through them had been re-serialized. Asset files are never edited as text.

**Tests, not reviews, hold the end state.**
[`NamingConventionsTests`](../../Assets/_Project/Tests/EditMode/Architecture/NamingConventionsTests.cs)
reflects over the 14 runtime assemblies: no public instance fields, `_camelCase` private
fields, `[SerializeField]` only on private fields, no `[FormerlySerializedAs]`.
[`SerializedDataHygieneTests`](../../Assets/_Project/Tests/EditMode/Architecture/SerializedDataHygieneTests.cs)
reads the ledger
[`RetiredSerializedFields`](../../Assets/_Project/Tests/EditMode/Architecture/RetiredSerializedFields.cs):
no field takes a retired name again, no scene, prefab or asset stores data under one (by
script GUID, across every YAML file under `Assets/`), and no prefab-instance override on a
ledger owner addresses a retired or missing field.
[`EnemyTuningSnapshotTests`](../../Assets/_Project/Tests/EditMode/Enemies/EnemyTuningSnapshotTests.cs)
pins the per-instance tuning of all 137 enemies in a checked-in TSV that has to pass
unchanged across a rename.

## Consequences

- The naming conventions have no exceptions. Roadmap 6.2 renamed the frozen enemy fields
  and four smaller components this way: 618 enemy overrides were rewritten in four level
  prefabs and eight scenes and the 50 dead ones dropped, and the per-asset dumps and the
  enemy snapshot read back identical to the baseline. Then all 56 legacy attributes went.
- Dead overrides on other components stay where they are: they load into nothing and
  removing them belongs to the clean-up step, not to a rename. The one dead pair on a
  ledger owner that is too widespread for a rename batch,
  `InputReader._interractAction` on the player in nine assets, is listed by name; the
  hygiene test tolerates exactly that pair and fails once the data is gone, so the list
  can only shrink.
- The "frozen serialized name" passages of ADR 0005 and ADR 0009 are superseded by this
  record; those fields are free to be renamed by the procedure above. The dead dialogue
  strings ADR 0009 left on Lobby_01's `FinalLevel` instance were removed in 6.2.
- A rename costs an editor session with measurements instead of a quick refactoring. The
  baseline and measurement tool live in the local planning notes, not in the repository.

## Alternatives considered

- **Freeze legacy names forever as documented exceptions.** The evidence behind the freeze
  was measured and did not hold, and the exception list had already grown twice. Rejected.
- **Keep `[FormerlySerializedAs]` permanently.** Cheap, but it hides data that loads only
  through the attribute (the `AudioManager` case) until someone deletes it. Rejected.
- **Attribute plus re-save, no explicit override rewrite.** Leaves dead duplicate overrides
  behind, as measured in 5.3. Rejected.
- **`PrefabUtility.RemoveUnusedOverrides` on whole instance roots.** Also strips about 1,800
  unrelated dead overrides (around 500 in `Level_02` alone) and turns a reviewable diff into
  an unreviewable one. Rejected.
- **Edit the YAML as text.** Bypasses Unity's serializer and the project rule against
  hand-editing scenes and prefabs. Rejected.

Related: [ADR 0005](0005-interaction-effects-as-components.md) and
[ADR 0009](0009-data-driven-dialogues.md) (the frozen names this replaces; 0009's migration
measured the attribute).
