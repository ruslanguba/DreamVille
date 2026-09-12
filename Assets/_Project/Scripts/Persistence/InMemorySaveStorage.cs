using System.Collections.Generic;

namespace DreamVille.Persistence
{
    /// <summary>Test double. Also counts <see cref="Save"/> calls so tests can assert flushing.</summary>
    public sealed class InMemorySaveStorage : ISaveStorage
    {
        private readonly Dictionary<string, int> _ints = new Dictionary<string, int>();
        private readonly Dictionary<string, float> _floats = new Dictionary<string, float>();

        /// <summary>Number of times <see cref="Save"/> was called since construction or the last <see cref="Clear"/>.</summary>
        public int SaveCount { get; private set; }

        public bool HasKey(string key) => _ints.ContainsKey(key) || _floats.ContainsKey(key);

        public int GetInt(string key, int defaultValue = 0) => _ints.TryGetValue(key, out int value) ? value : defaultValue;

        public void SetInt(string key, int value) => _ints[key] = value;

        public float GetFloat(string key, float defaultValue = 0f) => _floats.TryGetValue(key, out float value) ? value : defaultValue;

        public void SetFloat(string key, float value) => _floats[key] = value;

        public void DeleteKey(string key)
        {
            _ints.Remove(key);
            _floats.Remove(key);
        }

        public void Save() => SaveCount++;

        /// <summary>Wipes every stored value and resets <see cref="SaveCount"/>.</summary>
        public void Clear()
        {
            _ints.Clear();
            _floats.Clear();
            SaveCount = 0;
        }
    }
}
