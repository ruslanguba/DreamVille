using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

using DreamVille.Configs;
using DreamVille.Core.Players;
using DreamVille.Core.Scenes;

namespace DreamVille.SceneManagement
{
    /// <summary>
    /// Loads the target scene when the player enters this trigger volume. A hub's exit portal can also
    /// count as completing the current level, since the hub has nothing else to finish.
    /// </summary>
    public class SceneTransitionTrigger : MonoBehaviour
    {
        [SerializeField] private SceneDefinition _target;

        [Tooltip("Leaving through this portal completes the current level (for levels with nothing else to finish).")]
        [SerializeField] private bool _completesLevel;

        private ISceneLoader _sceneLoader;
        private ILevelCompletion _levelCompletion;

        [Inject]
        public void Construct(ISceneLoader sceneLoader, [InjectOptional] ILevelCompletion levelCompletion)
        {
            _sceneLoader = sceneLoader;
            _levelCompletion = levelCompletion;
        }

        private void OnTriggerEnter(Collider other)
        {
            // The player has several colliders; the loader owns the "one transition at a time" rule.
            if (_sceneLoader.IsLoading || !PlayerDetection.IsPlayer(other))
                return;

            if (_target == null)
            {
                Debug.LogError($"[{nameof(SceneTransitionTrigger)}] '{name}' has no target scene definition assigned.", this);
                return;
            }

            if (_completesLevel)
                CompleteLevel();

            LoadTargetAsync().Forget();
        }

        private void CompleteLevel()
        {
            if (_levelCompletion == null)
            {
                Debug.LogError($"[{nameof(SceneTransitionTrigger)}] '{name}' completes the level, but the scene has no level to complete.", this);
                return;
            }

            _levelCompletion.CompleteLevel();
        }

        private async UniTaskVoid LoadTargetAsync()
        {
            try
            {
                await _sceneLoader.LoadAsync(_target.SceneName, this.GetCancellationTokenOnDestroy());
            }
            catch (OperationCanceledException)
            {
                // The trigger is destroyed together with the scene it lives in; nothing to report.
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }
    }
}
