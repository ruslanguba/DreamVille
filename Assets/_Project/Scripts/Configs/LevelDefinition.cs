using UnityEngine;

namespace DreamVille.Configs
{
    /// <summary>A scene that is a playable level: it has progress, stars and an unlock rule.</summary>
    [CreateAssetMenu(fileName = "Level_00", menuName = "DreamVille/Levels/Level Definition")]
    public sealed class LevelDefinition : SceneDefinition
    {
        [SerializeField] private string _levelId;
        [SerializeField] private StarRules _starRules;
        [SerializeField] private bool _unlockedByDefault;

        /// <summary>Save-file identity. Must never change: it is part of every save key.</summary>
        public string LevelId => _levelId;

        /// <summary>Score thresholds the level's stars are awarded at.</summary>
        public StarRules StarRules => _starRules;

        /// <summary>True when the level can be started without completing it first.</summary>
        public bool UnlockedByDefault => _unlockedByDefault;

#if UNITY_INCLUDE_TESTS
        internal static LevelDefinition CreateForTests(
            string sceneName,
            string levelId,
            StarRules starRules = null,
            bool unlockedByDefault = false,
            AudioClip music = null)
        {
            var level = CreateInstance<LevelDefinition>();
            level.SetSceneForTests(sceneName, displayName: null, music);
            level._levelId = levelId;
            level._starRules = starRules;
            level._unlockedByDefault = unlockedByDefault;
            return level;
        }
#endif
    }
}
