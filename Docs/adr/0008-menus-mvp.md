# 0008. Menus as MVP: passive views, presenters, and one Inspector rule

Status: Accepted
Date: 2026-09-30

## Context

The main menu, level select, audio settings and pause menu were wired by scattered scripts and by
persistent calls on Inspector buttons.

- Small MonoBehaviours each did one job: `CloseApp`, `LocaleOnButton`, `PlayMusicButton`,
  `CursorActivatorInMenu`, `DisableText`, `LVLButtonStars`. Several reached services directly.
- Buttons reached services through Inspector UnityEvents. A renamed method broke a button
  silently, with no compile error and no test.
- `InGameMenuController` depended on the concrete MonoBehaviour `GameInput`, so it could not be
  unit-tested with a fake.
- `LocaleOnButton` selected a runtime-created `Locale`, not the asset the project ships.
- The chosen language was forgotten on restart.

[ADR 0007](0007-hud-mvp.md) had already settled the pattern for the HUD. This step applies it to
the menus. Full design, GUID scan, smoke plan and test list: spec
`10-menus-and-pause.md` (local planning notes, not committed).

## Decision

**Menus follow ADR 0007.** Views are passive MonoBehaviours behind small interfaces. A view
subscribes its own `Button.onClick` / `Slider.onValueChanged` in `OnEnable`, unsubscribes in
`OnDisable`, and raises an event. It never calls a service. Presenters are plain classes
(`IInitializable, IDisposable`) that live next to their views in `DreamVille.UI`
(`Scripts/UI/Menus/`), take interfaces, and are tested with hand-written fakes.

| View interface | Presenter | Job |
|---|---|---|
| [`IMainMenuView`](../../Assets/_Project/Scripts/UI/Menus/IMainMenuView.cs) | [`MainMenuPresenter`](../../Assets/_Project/Scripts/UI/Menus/MainMenuPresenter.cs) | New game, quit, language, menu cursor |
| [`ILevelSelectView`](../../Assets/_Project/Scripts/UI/Menus/ILevelSelectView.cs) | [`LevelSelectPresenter`](../../Assets/_Project/Scripts/UI/Menus/LevelSelectPresenter.cs) | Stars and locked state from saved progress; loads the chosen level |
| [`IAudioSettingsView`](../../Assets/_Project/Scripts/UI/Menus/IAudioSettingsView.cs) | [`AudioSettingsPresenter`](../../Assets/_Project/Scripts/UI/Menus/AudioSettingsPresenter.cs) | Volume sliders; one pair serves the main menu and the pause menu |
| [`IPauseMenuView`](../../Assets/_Project/Scripts/UI/Menus/IPauseMenuView.cs) | [`PauseMenuPresenter`](../../Assets/_Project/Scripts/UI/Menus/PauseMenuPresenter.cs) | Turns `IInGameMenu` events into view calls; continue, restart, exit to menu |

**The Inspector rule.** Inspector UnityEvents on buttons may only switch presentation: panel
`SetActive`, animator triggers. Anything that reaches a service is bound in code by a passive
view that raises an event, and a presenter handles it. The panel-toggle buttons stay in the
Inspector on purpose: panel flow is a designer concern and the Inspector shows it well.
[`MenuWiringTests`](../../Assets/_Project/Tests/EditMode/UI/MenuWiringTests.cs) guards the rule.
It fails when a persistent call does not resolve to a method, when a button has no action, when
the level-select view does not list every catalog level in order, or when a pause-menu prefab
reference is unassigned. The rule is also part of the project's coding conventions (section 4 of the local
conventions file, not committed).

**App keeps the rules, UI draws.**
[`InGameMenuController`](../../Assets/_Project/Scripts/App/InGameMenuController.cs) stays in
`DreamVille.App`. It decides when the game pauses and when player control returns after a
dialogue. It raises `MenuOpened`, `ComicsOpened` and `MenuClosed`; `PauseMenuPresenter` draws
them. `App` cannot see `UI` types, so the translation has to sit in `UI`. Moving the controller
into `UI` would take a non-presentation rule with it.

**Four seams keep the rules testable.**

| Seam | Assembly | Default implementation | Why |
|---|---|---|---|
| [`IMenuInput`](../../Assets/_Project/Scripts/Input/IMenuInput.cs) (`PausePressed`, `ComicsPressed`) | `Input` | `GameInput` | `InGameMenuController` no longer depends on a MonoBehaviour |
| [`IApplicationQuitter`](../../Assets/_Project/Scripts/App/IApplicationQuitter.cs) | `App` | `UnityApplicationQuitter` | Quit is testable; leaves Play Mode in the editor |
| [`ILocaleSelector`](../../Assets/_Project/Scripts/UI/Menus/ILocaleSelector.cs) | `UI` | `LocalizationLocaleSelector` | Takes a locale code; selects the shipped asset, not a runtime `Locale` |
| [`ICursorService`](../../Assets/_Project/Scripts/App/ICursorService.cs) | `App` | `CursorService` | `MainMenuPresenter` shows the cursor; `PauseService` toggles it |

`ILocaleSelector` lives in `UI` because the adapter needs `Unity.Localization`, which `UI`
references and `App` does not. One call is not worth a new edge.

**Composition.** [`MainMenuInstaller`](../../Assets/_Project/Scripts/Bootstrap/MainMenuInstaller.cs)
binds the three menu views (`FromComponentInHierarchy`), `LocalizationLocaleSelector` and the
three presenters. [`UiInstaller`](../../Assets/_Project/Scripts/Bootstrap/UiInstaller.cs) adds the
pause menu and its audio settings next to the HUD.
[`ProjectInstaller`](../../Assets/_Project/Scripts/Bootstrap/ProjectInstaller.cs) binds
`CursorService`, `UnityApplicationQuitter` and `FrameRateLimiter` for the whole application.

## Consequences

- No new assembly edges for the menu work itself. Every presenter and every rule in
  `InGameMenuController` is tested with fakes and no scene.
- The old one-job scripts (`CloseApp`, `LocaleOnButton`, `LVLButtonStars` and the rest) are deleted.
  [ADR 0001](0001-zenject-composition-root.md) no longer calls `MainMenuInstaller` empty.
- A button wired to a service in the Inspector is now a test failure, not a silent bug.
- Three behaviours changed on purpose, each on a user decision:
  - **Language is remembered.** Localization's `PlayerPrefLocaleSelector` (key `selected-locale`)
    sits after the command-line selector and before the system-language one.
    [`LocalizationSettingsContentTests`](../../Assets/_Project/Tests/EditMode/UI/LocalizationSettingsContentTests.cs)
    pins the order.
  - **Leaving Lobby 2 through its portal completes the level.** Lobby 2 has no enemies, so it was
    never completed and its level-select button stayed locked.
    `SceneTransitionTrigger._completesLevel` calls the new Core contract
    [`ILevelCompletion`](../../Assets/_Project/Scripts/Core/Scenes/ILevelCompletion.cs), which
    [`LevelDirector`](../../Assets/_Project/Scripts/Progression/LevelDirector.cs) implements, so
    `SceneManagement` still knows nothing about progress. `Progression` gains a `Core` edge,
    already allowed by the conventions. A player who skips the Lobby 2 dialogue still gets both
    pillows from Level_03 on, because `PillowInventory` seeds from completed levels.
    `SceneCompositionTests` requires every gameplay scene to be completable.
  - **Closing the pause menu during a dialogue resumes time.** Before, `timeScale` stayed 0 and
    the dialogue froze. Control stays off until the dialogue ends.
- `IInGameMenu` and `InGameMenuController` keep their names. Renaming them to pause vocabulary
  is churn without a reader gain; revisit in roadmap 6.2.

## Alternatives considered

- **One `MainMenuPresenter` for the whole scene.** It would take seven dependencies and change
  for three unrelated reasons: navigation commands, progress display, audio. Rejected.
- **Make `InGameMenuController` the presenter and move it into `UI`.** Its dialogue-to-player-control
  rule is not presentation and would leave `App` with it. Rejected.
- **A `MenuNavigator` that replaces the Inspector panel-toggle buttons with code.** Large editor
  rewiring for zero behaviour change; panel flow is a designer concern. Rejected.
- **Leave persistent calls to services in the Inspector.** Cheapest, but renames break buttons
  silently. Rejected in favour of the rule above and its test.
- **`ILocaleSelector` in `App`.** Needs a new `App -> Localization` edge for one call. Rejected.
- **Views call services directly (no presenter).** The view would know domain types and could
  not be tested without a scene. Rejected, same reasoning as ADR 0007.

Related: [ADR 0001](0001-zenject-composition-root.md) (installers),
[ADR 0002](0002-assembly-layering.md) (layering),
[ADR 0007](0007-hud-mvp.md) (the pattern this applies).
