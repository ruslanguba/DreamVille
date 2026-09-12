using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

using DreamVille.Configs;
using DreamVille.Persistence;
using DreamVille.SceneManagement;

namespace DreamVille.UI
{
    /// <summary>
    /// One entry of the level-select menu: shows the stars saved for its level and loads
    /// the level's scene. A level is playable once it has been completed at least once.
    /// </summary>
    public sealed class LevelSelectButton : MonoBehaviour
    {
        [SerializeField] private LevelDefinition _level;
        [SerializeField] private Button _button;
        [SerializeField] private Image[] _stars;

        private IProgressRepository _progress;
        private ISceneLoader _sceneLoader;

        [Inject]
        private void Construct(IProgressRepository progress, ISceneLoader sceneLoader)
        {
            _progress = progress;
            _sceneLoader = sceneLoader;
        }

        private void OnEnable()
        {
            Refresh();
            _button.onClick.AddListener(OnClicked);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnClicked);
        }

        private void Refresh()
        {
            int stars = _progress.GetStars(_level.LevelId);

            for (int i = 0; i < _stars.Length; i++)
                _stars[i].gameObject.SetActive(i < stars);

            _button.interactable = _level.UnlockedByDefault || _progress.IsCompleted(_level.LevelId);
        }

        private void OnClicked() => _sceneLoader.LoadAsync(_level.SceneName).Forget();
    }
}
