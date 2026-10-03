# 0009. Data-driven dialogues: definitions on a string table, an async run API, a presenter over a passive view

Status: Accepted
Date: 2026-10-03

## Context

Dialogue text lived in scenes, and three classes shared the job badly.

- `DialogueTrigger` held two parallel lists of `Dialogue { IsPluh, Sentence }`, one per
  language (`_sentencesRu`, `_sentencesEn`), and chose one with
  `LocalizationSettings.SelectedLocale.Identifier == "en"`. Nothing kept the lists in
  step, and the text bypassed the Localization package the UI already used.
- That shipped a bug. With English selected, eight of the nine build-scene triggers had
  no English lines. `TriggerDialog` logged an error and returned, so `DialogueStarted`
  never fired. No finale ran, no portal opened (Level_02 was the exception) and Lobby 2
  never granted its pillows.
- `DialogueManager`, a MonoBehaviour in the feature assembly, only instantiated the
  `DialogueUI` prefab through an injected `DiContainer`, forwarded two events to it and
  created an `EventSystem` at runtime when a scene had none. `DialogueUI` held an
  `[Inject]`ed service, and its Next button reached that service through an Inspector
  persistent call, which [ADR 0008](0008-menus-mvp.md) forbids.
- `IDialogueService.Begin(IReadOnlyList<Dialogue>, Action onCompleted)` completed through
  a callback, carried raw strings and could not be cancelled. The data class was named
  `Dialogue`, inside the namespace `DreamVille.Dialogue`.

[ADR 0007](0007-hud-mvp.md) and [ADR 0008](0008-menus-mvp.md) settled the view and
presenter pattern for the HUD and the menus. Dialogues were the last screen in the old
shape. Full design, migration batches and test list: spec `11-dialogue-data-driven.md`
(local planning notes, not committed).

## Decision

**A conversation is a `DialogueDefinition` asset. Its text is on one string table.**
[`DialogueDefinition`](../../Assets/_Project/Scripts/Dialogue/DialogueDefinition.cs) is a
ScriptableObject with an array of
[`DialogueLine`](../../Assets/_Project/Scripts/Dialogue/DialogueLine.cs): a
[`DialogueSpeaker`](../../Assets/_Project/Scripts/Dialogue/DialogueSpeaker.cs) (`Npc`,
`Player`) and a `LocalizedString`. Every `LocalizedString` points into the `Dialogues`
string table (ru, en) by a key such as `Level_01/FinalLevel/00`. The 15 definitions live in
`Assets/_Project/Settings/Dialogues/<Scene>/` (`Prefabs/` for the two prefab defaults), and
the table in `Assets/_Project/Localization/Tables/`. One definition serves every language, so a missing
translation can no longer remove a conversation. The type sits in `DreamVille.Dialogue`,
not in `Configs`: `LocalizedString` needs `Unity.Localization`, and `Configs` references
nothing.

**The service accepts synchronously and completes asynchronously.**
[`IDialogueService.TryStart(definition, ct, out UniTask completion)`](../../Assets/_Project/Scripts/Dialogue/IDialogueService.cs)
returns `false` when a conversation is running, the definition is null or empty, or the
token is already cancelled. On
`true`, `IsActive` is already set and `ActiveChanged(true)` has been raised. `completion`
finishes after the last line is dismissed and `ActiveChanged(false)` was raised. Two
callers need that split:

- `DialogueTrigger` must raise `DialogueStarted` in the very call that was accepted.
  `FinalZone`, `LobbyDirector` and `CameraPriorityHandler` react to that moment.
- A second trigger must be rejected at once, as before.

A lone `UniTask<bool> RunAsync` hides the start moment. It exists only as the extension
[`DialogueServiceExtensions.RunAsync`](../../Assets/_Project/Scripts/Dialogue/DialogueServiceExtensions.cs)
for callers that do not care; only tests use it today. Cancelling the token ends the run
with the same event order and then throws `OperationCanceledException` from `completion`.
A text that cannot be resolved is logged and ends the run, so `IsActive` cannot stick.
[`DialogueTrigger`](../../Assets/_Project/Scripts/Dialogue/DialogueTrigger.cs) passes
`GetCancellationTokenOnDestroy()`, the one-shot pattern of [ADR 0006](0006-unitask-lifetimes.md).
A trigger destroyed mid-conversation ends the run without raising `DialogueEnded`. A scene
unload ends a run the same way, so `DialogueView.Hide` and `PlayerInputSwitch` ignore calls
that arrive after their objects are destroyed (restart or exit to menu during a dialogue
used to log a `MissingReferenceException`). A throwing `DialogueStarted` listener is logged
and does not cost the conversation its `DialogueEnded`.

**Text is resolved once per run.** Before the first line, the service resolves every line
through [`ILocalizedTextProvider`](../../Assets/_Project/Scripts/Dialogue/ILocalizedTextProvider.cs)
and then shows the resolved
[`SpokenLine`](../../Assets/_Project/Scripts/Dialogue/SpokenLine.cs)s. The language can
only be changed in the main menu (`ILocaleSelector` has one consumer, `MainMenuPresenter`),
so a switch during a conversation cannot happen. `ShowNext` is ignored while texts are
still resolving.

**The provider is the seam.**
[`UnityLocalizedTextProvider`](../../Assets/_Project/Scripts/Dialogue/UnityLocalizedTextProvider.cs)
awaits `LocalizedString.GetLocalizedStringAsync()` with UniTask. It never calls
`WaitForCompletion`, which blocks the main thread and is unavailable on WebGL. Both locales
of the `Dialogues` table are preloaded, so resolution normally finishes in the same frame.
[`DialogueServiceTests`](../../Assets/_Project/Tests/EditMode/Dialogue/DialogueServiceTests.cs)
use a hand-written fake provider, so they need no Localization initialisation.

**View and presenter follow ADR 0007 and 0008.**
[`DialogueView`](../../Assets/_Project/Scripts/UI/Dialogues/DialogueView.cs) (the former
`DialogueUI`, moved with `git mv` so its GUID and serialized values stayed) implements
[`IDialogueView`](../../Assets/_Project/Scripts/UI/Dialogues/IDialogueView.cs) and binds its
Next button in `OnEnable`/`OnDisable`.
[`DialoguePresenter`](../../Assets/_Project/Scripts/UI/Dialogues/DialoguePresenter.cs) is a
plain `IInitializable, IDisposable` class in `DreamVille.UI`. It shows and hides the view
with the conversation, maps `DialogueSpeaker.Player` to the right-hand slot, and sends Next
back to the service. The namespace is `DreamVille.UI.Dialogues` (plural) so it does not
shadow `DreamVille.Dialogue`. `DialogueManager` is deleted.
[`UiInstaller`](../../Assets/_Project/Scripts/Bootstrap/UiInstaller.cs) binds `IDialogueView`
with `FromComponentInNewPrefab`, exactly like the HUD, and `DialoguePresenter` next to it.
[`SceneServicesInstaller`](../../Assets/_Project/Scripts/Bootstrap/SceneServicesInstaller.cs)
binds `UnityLocalizedTextProvider` and `DialogueService`. The advance input stays in
`DialogueInputHandler`.

**English falls back to Russian as a safety net.** The English locale carries
`FallbackLocale` metadata pointing at Russian, and both the string and the asset database
have `UseFallback` on. It was added to unblock English players while translations were
missing. All 53 lines now have an English entry, and the fallback stays for future lines.
[`LocalizationSettingsContentTests`](../../Assets/_Project/Tests/EditMode/UI/LocalizationSettingsContentTests.cs)
pins it.

## Consequences

- **The migration is proven by data, not by a promise.** The text was copied from the values
  Unity resolves for each scene instance, never from the prefab assets.
  [`DialogueSnapshot.tsv`](../../Assets/_Project/Tests/EditMode/Dialogue/DialogueSnapshot.tsv)
  pins all 53 lines (scene, trigger path, index, speaker, Russian, English) in 15
  conversations. [`DialogueContentTests`](../../Assets/_Project/Tests/EditMode/Dialogue/DialogueContentTests.cs)
  compares speaker and both texts for every row by exact string equality. It also fails when
  a trigger has no definition, when a snapshot trigger disappears, and when the table has an
  empty or Smart entry, an unreferenced key, or a key used by more than one line. An empty English entry would show nothing instead
  of falling back, hence the empty-entry check. The migration script itself was a one-shot
  Unity MCP session and is not committed.
- **One deliberate content loss.** Level_02's finale keeps its single Russian line. Its three
  English lines were unrelated to it and were dropped on the author's decision; they are kept
  word for word in the body of commit `ba5b900`. The other 50 English lines were translated
  and approved by the author in `c3230f7`.
- **New assembly edges.** `UI -> Dialogue` for `IDialogueService` events and `SpokenLine`.
  `Dialogue` gains `UniTask` and `Unity.ResourceManager`, the latter only to name
  `AsyncOperationHandle<string>`. No cycle: `Dialogue` references no other DreamVille
  assembly than `Core` ([ADR 0002](0002-assembly-layering.md)).
- **A frozen serialized name.** `DialogueTrigger._definition` carries a prefab-instance
  override in 13 scene instances. `[FormerlySerializedAs]` is not guaranteed to remap those
  overrides, so renaming the field would silently reset them. The same goes for the numeric
  values of `DialogueSpeaker`, which are stored in the assets. A trigger without a definition
  now logs an error and does nothing, instead of silently picking a list by locale.
- **Level_01 gained a scene `EventSystem`.** `DialogueManager` used to create one at runtime
  and Level_01 relied on it. [`SceneCompositionTests`](../../Assets/_Project/Tests/EditMode/Composition/SceneCompositionTests.cs)
  now requires exactly one `EventSystem` per gameplay scene and a dialogue prefab on every
  `UiInstaller`. [`DialogueCanvasPrefabTests`](../../Assets/_Project/Tests/EditMode/UI/DialogueCanvasPrefabTests.cs)
  keeps persistent calls to project scripts off the canvas buttons.
- **Dead text remains in one scene.** Lobby_01's second `FinalLevel` instance still carries
  four override strings under `sentence.Array.data[n].Sentence`. They target a
  `DialogueTrigger` that this instance removes, so they apply to nothing and belong to no
  conversation. Cleaning them needs another pass through the editor API.
- English players now get every finale and portal. This is a bug fix and the only
  player-visible change besides the English text itself.
- 549 EditMode tests (547 pass, 2 intentionally ignored), up from 480 before this step.

## Alternatives considered

- **Keep the roadmap name `DialogueAsset`.** The conventions require a role suffix
  (`*Config`, `*Definition`, `*Rules`), and `LevelDefinition` is the precedent. Rejected.
- **Put the definition in `Configs`.** It would give the reference-free Layer 0 assembly a
  package edge to Localization for one type. Rejected.
- **A lone `UniTask<bool> RunAsync` as the only entry point.** The caller cannot observe the
  moment of acceptance without an extra event. Rejected; the extension stays as sugar.
- **A code fallback in `DialogueTrigger` ("if English is empty, use Russian").** It
  re-implements a Localization feature. Rejected in favour of the locale fallback.
- **The presenter in `Dialogue`.** A feature assembly would own presentation and a view
  interface, and the presenter would no longer sit next to its view (ADR 0007). Rejected.
- **A committed editor migration tool.** One-shot code is dead the day after. The snapshot
  and its tests are the permanent part. Rejected.
- **Re-resolve the line on screen when the locale changes.** The shipped UI cannot change
  language during a conversation, and it would add a subscription lifecycle for nothing.
  Rejected; revisit if the pause menu ever gets a language switch.
- **English structure wins for Level_02 (three lines, two new Russian ones).** It would change
  the primary language's content to match English lines that do not translate it. Rejected.

Related: [ADR 0001](0001-zenject-composition-root.md) (installers),
[ADR 0002](0002-assembly-layering.md) (the new edges),
[ADR 0006](0006-unitask-lifetimes.md) (one-shot lifetime),
[ADR 0007](0007-hud-mvp.md) and [ADR 0008](0008-menus-mvp.md) (the pattern this applies).
