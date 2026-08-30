using Cysharp.Threading.Tasks;

using DreamVille.Core.SceneManagement;

namespace DreamVille.Core
{
    /// <summary>Переходы между сценами: рестарт уровня и выход в меню.</summary>
    public class GameFlowService
    {
        private readonly SceneLoader _sceneLoader;
        private readonly PauseService _pause;

        public GameFlowService(SceneLoader sceneLoader, PauseService pause)
        {
            _sceneLoader = sceneLoader;
            _pause = pause;
        }

        public void RestartLevel()
        {
            // Время возвращаем до загрузки: иначе новая сцена стартует с timeScale = 0.
            _pause.Resume();
            _sceneLoader.ReloadActiveAsync().Forget();
        }

        public void ExitToMenu(string menuSceneName)
        {
            _pause.Resume();
            _sceneLoader.LoadAsync(menuSceneName).Forget();
        }
    }
}
