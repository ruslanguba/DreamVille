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

        [Tooltip("Unlocked pillows the player may use here. 0 keeps the level pillow-free even after unlocking.")]
        [SerializeField, Min(0)] private int _maxPillows;

        /// <summary>Save-file identity. Must never change: it is part of every save key.</summary>
        public string LevelId => _levelId;

        /// <summary>Score thresholds the level's stars are awarded at.</summary>
        public StarRules StarRules => _starRules;

        /// <summary>True when the level can be started without completing it first.</summary>
        public bool UnlockedByDefault => _unlockedByDefault;

        /// <summary>How many unlocked pillows the player may use in this level (0 keeps the level pillow-free).</summary>
        public int MaxPillows => _maxPillows;

#if UNITY_INCLUDE_TESTS
        internal static LevelDefinition CreateForTests(
            string sceneName,
            string levelId,
            StarRules starRules = null,
            bool unlockedByDefault = false,
            AudioClip music = null,
            int maxPillows = 0)
        {
            var level = CreateInstance<LevelDefinition>();
            level.SetSceneForTests(sceneName, displayName: null, music);
            level._levelId = levelId;
            level._starRules = starRules;
            level._unlockedByDefault = unlockedByDefault;
            level._maxPillows = maxPillows;
            return level;
        }
#endif
    }
}
