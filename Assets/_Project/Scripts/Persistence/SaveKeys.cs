namespace DreamVille.Persistence
{
    /// <summary>
    /// Every key the game has ever written. The formats are frozen: changing one
    /// silently wipes the progress of everyone who already played.
    /// </summary>
    public static class SaveKeys
    {
        // The volume keys double as AudioMixer parameter names; keep both verbatim.
        public const string MusicVolume = "MusicVolume";
        public const string SfxVolume = "SFXVolume";

        private const string StarsSuffix = "_Stars";
        private const string CompletedSuffix = "_Completed";
        private const string BestScorePrefix = "BestScore_LVL_";

        public static string Stars(string levelId) => levelId + StarsSuffix;

        public static string Completed(string levelId) => levelId + CompletedSuffix;

        public static string BestScore(string levelId) => BestScorePrefix + levelId;
    }
}
