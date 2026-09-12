using UnityEngine;
using UnityEngine.SceneManagement;

namespace DreamVille.Scoring
{
    /// <summary>
    /// Best score record for a level. The key format matches the original ScoreSaveHandler,
    /// so previously saved player scores are not lost.
    /// </summary>
    public class BestScoreService : IBestScoreService
    {
        private const string KeyPrefix = "BestScore_LVL_";

        public float GetBest() => PlayerPrefs.GetFloat(GetKey(), 0f);

        /// <summary>Saves the score if it beats the previous best. Returns true when a new record was set.</summary>
        public bool TrySave(float score)
        {
            string key = GetKey();

            if (score <= PlayerPrefs.GetFloat(key, 0f))
                return false;

            PlayerPrefs.SetFloat(key, score);
            PlayerPrefs.Save();
            return true;
        }

        private static string GetKey() => KeyPrefix + SceneManager.GetActiveScene().buildIndex;
    }
}
