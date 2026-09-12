using DreamVille.Persistence;

namespace DreamVille.Progression
{
    /// <summary>Completed-level lookup. The key format matches what the level-select menu reads.</summary>
    public class LevelUnlockService
    {
        private readonly IProgressRepository _progress;

        public LevelUnlockService(IProgressRepository progress)
        {
            _progress = progress;
        }

        public bool IsCompleted(string levelId) => _progress.IsCompleted(levelId);

        public void MarkCompleted(string levelId) => _progress.MarkCompleted(levelId);
    }
}
