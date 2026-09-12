namespace DreamVille.Persistence
{
    /// <summary>Key-value save backend. The only code in the project allowed to touch a storage API.</summary>
    public interface ISaveStorage
    {
        bool HasKey(string key);

        int GetInt(string key, int defaultValue = 0);

        void SetInt(string key, int value);

        float GetFloat(string key, float defaultValue = 0f);

        void SetFloat(string key, float value);

        void DeleteKey(string key);

        /// <summary>Flushes pending writes to disk.</summary>
        void Save();
    }
}
