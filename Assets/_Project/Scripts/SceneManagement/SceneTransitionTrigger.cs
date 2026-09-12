using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

using DreamVille.Configs;
using DreamVille.Core.Players;

namespace DreamVille.SceneManagement
{
    /// <summary>Loads the target scene when the player enters this trigger volume.</summary>
    public class SceneTransitionTrigger : MonoBehaviour
    {
        [SerializeField] private SceneDefinition _target;

        private ISceneLoader _sceneLoader;
        private bool _isLoading;

        [Inject]
        public void Construct(ISceneLoader sceneLoader)
        {
            _sceneLoader = sceneLoader;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isLoading || !PlayerDetection.IsPlayer(other))
                return;

            if (_target == null)
            {
                Debug.LogError($"[{nameof(SceneTransitionTrigger)}] '{name}' has no target scene definition assigned.", this);
                return;
            }

            _isLoading = true;
            LoadTargetAsync().Forget();
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
                _isLoading = false;
                Debug.LogException(exception, this);
            }
        }
    }
}
