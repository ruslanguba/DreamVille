namespace DreamVille.Scoring
{
    /// <summary>Persists the best score reached for the active level.</summary>
    public interface IBestScoreService
    {
        float GetBest();

        /// <summary>Saves <paramref name="score"/> if it beats the stored best. Returns true when it did.</summary>
        bool TrySave(float score);
    }
}
