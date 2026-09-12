using UnityEngine;

namespace DreamVille.Persistence
{
    /// <summary>Default <see cref="IProgressRepository"/> backed by an <see cref="ISaveStorage"/>.</summary>
    public sealed class ProgressRepository : IProgressRepository
    {
        private readonly ISaveStorage _storage;

        public ProgressRepository(ISaveStorage storage)
        {
            _storage = storage;
        }

        public int GetStars(string levelId)
        {
            if (string.IsNullOrEmpty(levelId))
                return 0;

            return _storage.GetInt(SaveKeys.Stars(levelId), 0);
        }

        public bool SetStarsIfHigher(string levelId, int stars)
        {
            if (string.IsNullOrEmpty(levelId))
            {
                Debug.LogError($"[{nameof(ProgressRepository)}] Cannot save stars for an empty level id.");
                return false;
            }

            string key = SaveKeys.Stars(levelId);
            if (stars <= _storage.GetInt(key, 0))
                return false;

            _storage.SetInt(key, stars);
            _storage.Save();
            return true;
        }

        public bool IsCompleted(string levelId)
        {
            if (string.IsNullOrEmpty(levelId))
                return false;

            return _storage.GetInt(SaveKeys.Completed(levelId), 0) == 1;
        }

        public void MarkCompleted(string levelId)
        {
            if (string.IsNullOrEmpty(levelId))
            {
                Debug.LogError($"[{nameof(ProgressRepository)}] Cannot mark an empty level id as completed.");
                return;
            }

            _storage.SetInt(SaveKeys.Completed(levelId), 1);
            _storage.Save();
        }

        public float GetBestScore(string levelId)
        {
            if (string.IsNullOrEmpty(levelId))
                return 0f;

            return _storage.GetFloat(SaveKeys.BestScore(levelId), 0f);
        }

        public bool TrySetBestScore(string levelId, float score)
        {
            if (string.IsNullOrEmpty(levelId))
            {
                Debug.LogError($"[{nameof(ProgressRepository)}] Cannot save a best score for an empty level id.");
                return false;
            }

            string key = SaveKeys.BestScore(levelId);
            if (score <= _storage.GetFloat(key, 0f))
                return false;

            _storage.SetFloat(key, score);
            _storage.Save();
            return true;
        }
    }
}
