using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DreamVille.Core.SceneManagement
{
    /// <summary>
    /// Асинхронная загрузка сцен с прогрессом и отменой.
    /// Обычный C#-класс: живёт в контейнере, MonoBehaviour и корутины не нужны.
    /// </summary>
    public class SceneLoader
    {
        /// <summary>Прогресс текущей загрузки, 0..1.</summary>
        public event Action<float> ProgressChanged;

        public bool IsLoading { get; private set; }

        public async UniTask LoadAsync(string sceneName, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(sceneName))
                throw new ArgumentException("Имя сцены не задано.", nameof(sceneName));

            await RunAsync(SceneManager.LoadSceneAsync(sceneName), token);
        }

        public async UniTask LoadAsync(int buildIndex, CancellationToken token = default)
        {
            await RunAsync(SceneManager.LoadSceneAsync(buildIndex), token);
        }

        public UniTask ReloadActiveAsync(CancellationToken token = default)
            => LoadAsync(SceneManager.GetActiveScene().buildIndex, token);

        private async UniTask RunAsync(AsyncOperation operation, CancellationToken token)
        {
            if (operation == null)
                return;

            IsLoading = true;
            ProgressChanged?.Invoke(0f);

            try
            {
                await operation.ToUniTask(
                    progress: Progress.Create<float>(value => ProgressChanged?.Invoke(value)),
                    cancellationToken: token);

                ProgressChanged?.Invoke(1f);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
