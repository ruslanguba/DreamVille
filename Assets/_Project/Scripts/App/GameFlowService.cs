using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

using DreamVille.Configs;
using DreamVille.SceneManagement;

namespace DreamVille.App
{
    /// <summary>Scene transitions: restarting the level and leaving for the main menu.</summary>
    public class GameFlowService : IGameFlowService
    {
        private readonly ISceneLoader _sceneLoader;
        private readonly IPauseService _pause;
        private readonly SceneCatalog _catalog;

        public GameFlowService(ISceneLoader sceneLoader, IPauseService pause, SceneCatalog catalog)
        {
            _sceneLoader = sceneLoader;
            _pause = pause;
            _catalog = catalog;
        }

        public void RestartLevel()
        {
            // Time must be restored before the load starts, otherwise the new scene begins with timeScale = 0.
            _pause.Resume();
            RunTransitionAsync(_sceneLoader.ReloadActiveAsync()).Forget();
        }

        public void ExitToMenu()
        {
            _pause.Resume();
            RunTransitionAsync(_sceneLoader.LoadAsync(_catalog.MainMenuScene.SceneName)).Forget();
        }

        private static async UniTaskVoid RunTransitionAsync(UniTask transition)
        {
            try
            {
                await transition;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }
}
