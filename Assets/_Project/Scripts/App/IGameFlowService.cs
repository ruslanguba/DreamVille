namespace DreamVille.App
{
    /// <summary>Level-level navigation: restart the current level or leave for the main menu.</summary>
    public interface IGameFlowService
    {
        void RestartLevel();

        void ExitToMenu();
    }
}
