using UnityEngine;
using UnityEngine.SceneManagement;

namespace DreamVille.Scoring
{
    /// <summary>
    /// Рекорд очков за уровень. Ключ совпадает с прежним ScoreSaveHandler,
    /// поэтому уже сохранённые результаты игроков не теряются.
    /// </summary>
    public class BestScoreService
    {
        private const string KeyPrefix = "BestScore_LVL_";

        public float GetBest() => PlayerPrefs.GetFloat(GetKey(), 0f);

        /// <summary>Сохраняет результат, если он лучше прежнего. Возвращает true, если рекорд побит.</summary>
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
