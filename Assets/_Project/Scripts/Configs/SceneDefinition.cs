using UnityEngine;

namespace DreamVille.Configs
{
    /// <summary>Identity of one scene: its name in Build Settings and what plays there.</summary>
    [CreateAssetMenu(fileName = "Scene_New", menuName = "DreamVille/Levels/Scene Definition")]
    public class SceneDefinition : ScriptableObject
    {
        [SerializeField] private string _sceneName;
        [SerializeField] private AudioClip _music;

        /// <summary>Name the scene is registered under in Build Settings.</summary>
        public string SceneName => _sceneName;

        /// <summary>Background music for this scene. May be null (the bootstrap scene is silent).</summary>
        public AudioClip Music => _music;

#if UNITY_EDITOR
        [Tooltip("Authoring reference: the scene name below is kept in sync with it.")]
        [SerializeField] private UnityEditor.SceneAsset _sceneAsset;

        private void OnValidate()
        {
            if (_sceneAsset != null)
                _sceneName = _sceneAsset.name;
        }
#endif

#if UNITY_INCLUDE_TESTS
        internal static SceneDefinition CreateForTests(string sceneName, AudioClip music = null)
        {
            var definition = CreateInstance<SceneDefinition>();
            definition.SetSceneForTests(sceneName, music);
            return definition;
        }

        /// <summary>Fills the serialized scene identity of a hand-built instance.</summary>
        internal void SetSceneForTests(string sceneName, AudioClip music)
        {
            _sceneName = sceneName;
            _music = music;
        }
#endif
    }
}
