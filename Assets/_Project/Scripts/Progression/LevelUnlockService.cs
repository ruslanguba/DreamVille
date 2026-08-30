using UnityEngine;

namespace DreamVille.Progression
{
    /// <summary>
    /// Отметка о пройденных уровнях. Ключ совпадает с тем, что читает меню выбора уровня.
    /// </summary>
    public class LevelUnlockService
    {
        private const string KeySuffix = "_Completed";

        public bool IsCompleted(string levelId)
            => !string.IsNullOrEmpty(levelId) && PlayerPrefs.GetInt(levelId + KeySuffix, 0) == 1;

        public void MarkCompleted(string levelId)
        {
            if (string.IsNullOrEmpty(levelId))
                return;

            PlayerPrefs.SetInt(levelId + KeySuffix, 1);
            PlayerPrefs.Save();
        }
    }
}
