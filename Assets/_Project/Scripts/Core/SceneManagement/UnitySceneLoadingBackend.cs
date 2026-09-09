using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DreamVille.Core.SceneManagement
{
    public sealed class UnitySceneLoadingBackend : ISceneLoadingBackend
    {
        public int ActiveSceneBuildIndex => SceneManager.GetActiveScene().buildIndex;

        public UniTask LoadAsync(string sceneName, Action<float> progress)
            => AwaitOperation(SceneManager.LoadSceneAsync(sceneName), progress);

        public UniTask LoadAsync(int buildIndex, Action<float> progress)
            => AwaitOperation(SceneManager.LoadSceneAsync(buildIndex), progress);

        private static UniTask AwaitOperation(AsyncOperation operation, Action<float> progress)
        {
            if (operation == null)
                throw new InvalidOperationException("Unity did not create a scene loading operation.");

            return operation.ToUniTask(progress: Progress.Create<float>(progress));
        }
    }
}
