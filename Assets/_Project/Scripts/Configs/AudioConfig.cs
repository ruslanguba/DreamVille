using UnityEngine;
using UnityEngine.Serialization;

namespace DreamVille.Configs
{
    /// <summary>Audio tunables that are not tied to a single scene.</summary>
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "DreamVille/Audio/Audio Config")]
    public sealed class AudioConfig : ScriptableObject
    {
        [FormerlySerializedAs("StartVolume")]
        [Range(0f, 1f)]
        [SerializeField] private float _defaultVolume = 0.3f;

        /// <summary>Volume applied to both channels until the player changes it.</summary>
        public float DefaultVolume => _defaultVolume;

#if UNITY_INCLUDE_TESTS
        internal static AudioConfig CreateForTests(float defaultVolume)
        {
            var config = CreateInstance<AudioConfig>();
            config._defaultVolume = defaultVolume;
            return config;
        }
#endif
    }
}
