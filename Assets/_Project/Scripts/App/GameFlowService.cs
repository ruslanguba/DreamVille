using Cysharp.Threading.Tasks;

using DreamVille.SceneManagement;

namespace DreamVille.App
{
    /// <summary>Scene transitions: restarting the level and leaving for the main menu.</summary>
    public class GameFlowService : IGameFlowService
    {
        private readonly ISceneLoader _sceneLoader;
        private readonly IPauseService _pause;
        private readonly string _mainMenuScene;

        public GameFlowService(ISceneLoader sceneLoader, IPauseService pause, string mainMenuScene)
        {
            _sceneLoader = sceneLoader;
            _pause = pause;
            _mainMenuScene = mainMenuScene;
        }

        public void RestartLevel()
        {
            // Time must be restored before the load starts, otherwise the new scene begins with timeScale = 0.
            _pause.Resume();
            _sceneLoader.ReloadActiveAsync().Forget();
        }

        public void ExitToMenu()
        {
            _pause.Resume();
            _sceneLoader.LoadAsync(_mainMenuScene).Forget();
        }
    }
}
