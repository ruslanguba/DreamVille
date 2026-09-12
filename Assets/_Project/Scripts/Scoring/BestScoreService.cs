using DreamVille.Configs;
using DreamVille.Persistence;

namespace DreamVille.Scoring
{
    /// <summary>Best score record for one level, keyed by the level's save-file id.</summary>
    public class BestScoreService : IBestScoreService
    {
        private readonly string _levelId;
        private readonly IProgressRepository _progress;

        public BestScoreService(LevelDefinition level, IProgressRepository progress)
        {
            _levelId = level.LevelId;
            _progress = progress;
        }

        public float GetBest() => _progress.GetBestScore(_levelId);

        /// <summary>Saves the score if it beats the previous best. Returns true when a new record was set.</summary>
        public bool TrySave(float score) => _progress.TrySetBestScore(_levelId, score);
    }
}
