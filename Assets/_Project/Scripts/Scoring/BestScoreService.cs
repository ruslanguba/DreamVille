using UnityEngine.SceneManagement;

using DreamVille.Persistence;

namespace DreamVille.Scoring
{
    /// <summary>
    /// Best score record for a level. The key format matches the original ScoreSaveHandler,
    /// so previously saved player scores are not lost.
    /// </summary>
    public class BestScoreService : IBestScoreService
    {
        private readonly IProgressRepository _progress;

        public BestScoreService(IProgressRepository progress)
        {
            _progress = progress;
        }

        public float GetBest() => _progress.GetBestScore(LevelId);

        /// <summary>Saves the score if it beats the previous best. Returns true when a new record was set.</summary>
        public bool TrySave(float score) => _progress.TrySetBestScore(LevelId, score);

        // Build index until LevelDefinition arrives in batch 2; keeps the key format "BestScore_LVL_{index}".
        private static string LevelId => SceneManager.GetActiveScene().buildIndex.ToString();
    }
}
