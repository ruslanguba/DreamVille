using DreamVille.Core.SceneManagement;

namespace DreamVille.Core
{
    /// <summary>Переходы между сценами: рестарт уровня и выход в меню.</summary>
    public class GameFlowService
    {
        private readonly SceneTransitionManager _transitions;
        private readonly PauseService _pause;

        public GameFlowService(SceneTransitionManager transitions, PauseService pause)
        {
            _transitions = transitions;
            _pause = pause;
        }

        public void RestartLevel()
        {
            // Время возвращаем до загрузки: иначе новая сцена стартует с timeScale = 0.
            _pause.Resume();
            _transitions.RestartScene();
        }

        public void ExitToMenu(string menuSceneName)
        {
            _pause.Resume();
            _transitions.Transition(new SceneTransitionRequest(menuSceneName));
        }
    }
}
