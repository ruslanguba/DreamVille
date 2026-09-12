using System.Collections.Generic;
using UnityEngine;

namespace DreamVille.Configs
{
    /// <summary>
    /// Every scene the game can load, in play order. The single source of scene names:
    /// no other asset, prefab or script spells one out.
    /// </summary>
    [CreateAssetMenu(fileName = "SceneCatalog", menuName = "DreamVille/Levels/Scene Catalog")]
    public sealed class SceneCatalog : ScriptableObject
    {
        [SerializeField] private SceneDefinition _bootstrapScene;
        [SerializeField] private SceneDefinition _mainMenuScene;
        [SerializeField] private LevelDefinition[] _levels;

        /// <summary>Scene the application starts in.</summary>
        public SceneDefinition BootstrapScene => _bootstrapScene;

        /// <summary>Scene the level-select menu lives in.</summary>
        public SceneDefinition MainMenuScene => _mainMenuScene;

        /// <summary>Playable levels in play order.</summary>
        public IReadOnlyList<LevelDefinition> Levels => _levels;

        /// <summary>
        /// Any scene the game knows about, including the menu and the bootstrap scene.
        /// Null when the name is unknown.
        /// </summary>
        public SceneDefinition FindBySceneName(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
                return null;

            if (Matches(_bootstrapScene, sceneName))
                return _bootstrapScene;

            if (Matches(_mainMenuScene, sceneName))
                return _mainMenuScene;

            return FindLevelBySceneName(sceneName);
        }

        /// <summary>The level with this save-file id, or null when the catalog has none.</summary>
        public LevelDefinition FindLevelById(string levelId)
        {
            if (string.IsNullOrEmpty(levelId) || _levels == null)
                return null;

            foreach (LevelDefinition level in _levels)
            {
                if (level != null && level.LevelId == levelId)
                    return level;
            }

            return null;
        }

        private LevelDefinition FindLevelBySceneName(string sceneName)
        {
            if (_levels == null)
                return null;

            foreach (LevelDefinition level in _levels)
            {
                if (Matches(level, sceneName))
                    return level;
            }

            return null;
        }

        private static bool Matches(SceneDefinition definition, string sceneName)
            => definition != null && definition.SceneName == sceneName;

#if UNITY_INCLUDE_TESTS
        internal static SceneCatalog CreateForTests(
            SceneDefinition bootstrapScene,
            SceneDefinition mainMenuScene,
            params LevelDefinition[] levels)
        {
            var catalog = CreateInstance<SceneCatalog>();
            catalog._bootstrapScene = bootstrapScene;
            catalog._mainMenuScene = mainMenuScene;
            catalog._levels = levels;
            return catalog;
        }
#endif
    }
}
