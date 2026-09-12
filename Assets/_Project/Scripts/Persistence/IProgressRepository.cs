namespace DreamVille.Persistence
{
    /// <summary>Per-level player progress. Read by the menu, written by the level.</summary>
    public interface IProgressRepository
    {
        int GetStars(string levelId);

        /// <summary>Stores the star count only when it beats the stored one. True when written.</summary>
        bool SetStarsIfHigher(string levelId, int stars);

        bool IsCompleted(string levelId);

        void MarkCompleted(string levelId);

        float GetBestScore(string levelId);

        /// <summary>Stores the score only when it beats the stored one. True when written.</summary>
        bool TrySetBestScore(string levelId, float score);
    }
}
