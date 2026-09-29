using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

using DreamVille.Configs;
using DreamVille.Core.Async;
using DreamVille.Persistence;
using DreamVille.SceneManagement;

namespace DreamVille.UI.Menus
{
    /// <summary>
    /// Shows saved stars per level and loads a level when it is playable: unlocked by default,
    /// or completed at least once (replay-only select; new levels are reached through portals).
    /// </summary>
    /// <remarks>
    /// Playability is checked again on every click, so a locked level never loads even when its button is
    /// invoked programmatically; the button's <c>interactable</c> flag is only the visible half of the rule.
    /// </remarks>
    public sealed class LevelSelectPresenter : IInitializable, IDisposable
    {
        private readonly ILevelSelectView _view;
        private readonly IProgressRepository _progress;
        private readonly ISceneLoader _sceneLoader;

        private CancellationTokenSource _lifetime = new CancellationTokenSource();

        public LevelSelectPresenter(ILevelSelectView view, IProgressRepository progress, ISceneLoader sceneLoader)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _progress = progress ?? throw new ArgumentNullException(nameof(progress));
            _sceneLoader = sceneLoader ?? throw new ArgumentNullException(nameof(sceneLoader));
        }

        /// <summary>Pushes every level's state once: progress cannot change while the menu scene is loaded.</summary>
        public void Initialize()
        {
            IReadOnlyList<LevelDefinition> levels = _view.Levels;
            for (int i = 0; i < levels.Count; i++)
            {
                LevelDefinition level = levels[i];
                int stars = level != null ? _progress.GetStars(level.LevelId) : 0;
                _view.ShowLevel(i, stars, IsPlayable(level));
            }

            _view.LevelChosen += OnLevelChosen;
        }

        /// <summary>Unsubscribes and cancels a pending scene load; never calls the view.</summary>
        public void Dispose()
        {
            _view.LevelChosen -= OnLevelChosen;

            Cancellation.CancelAndDispose(ref _lifetime);
        }

        private void OnLevelChosen(LevelDefinition level)
        {
            if (!IsPlayable(level))
                return;

            LoadAsync(level.SceneName, _lifetime.Token).Forget();
        }

        private bool IsPlayable(LevelDefinition level)
        {
            return level != null && (level.UnlockedByDefault || _progress.IsCompleted(level.LevelId));
        }

        private async UniTaskVoid LoadAsync(string sceneName, CancellationToken token)
        {
            try
            {
                await _sceneLoader.LoadAsync(sceneName, token);
            }
            catch (OperationCanceledException)
            {
                // The menu is disposed by the very load it requested; nothing to report.
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }
}
