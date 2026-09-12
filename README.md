# DreamVille

A cozy third-person shooter built with Unity 6 (URP): bounce pillows off the walls, ricochet shots into runaway enemies, talk to the townsfolk and open the way to the next level.

> **Work in progress.** The project is being reworked into a portfolio piece: clean assembly layering, Zenject composition root, UniTask-based async, data-driven configs and automated tests. This README will grow with each phase.

## Tech stack

| Area | Choice |
|---|---|
| Engine | Unity 6000.0 LTS, Universal Render Pipeline |
| Dependency injection | Zenject / Extenject (ProjectContext + per-scene SceneContext) |
| Async | UniTask |
| Input | Unity Input System |
| Camera | Cinemachine 3 |
| Localization | Unity Localization (English, Russian) |

## Project layout

```
Assets/_Project/
  Scripts/        one assembly definition per module (Core, Input, Gameplay, Dialogue, Progression, Scoring, Audio, UI, Bootstrap, ...)
  Scenes/         GameStart -> MainMenu -> lobbies and levels
  Prefabs/        gameplay, UI and bootstrap prefabs
  Settings/       ScriptableObject configs (bullets, movement, scoring, audio, rendering)
  Characters/     player, skins, enemies, NPCs
  Art/ Audio/ Animations/ Environment/ Localization/
```

## Running

Open the project with Unity 6000.0.62f1 or newer 6000.0.x, open `Assets/_Project/Scenes/GameStart.unity` and press Play.

## Roadmap

Architecture cleanup, test coverage, CI and a playable build are in progress. Detailed architecture notes will live in `Docs/`.
