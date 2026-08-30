using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

using DreamVille.Core.SceneManagement;
using DreamVille.UI;

namespace DreamVille.Core.Bootstrap
{
    /// <summary>
    /// Точка входа игры: проигрывает заставку и уходит в первую сцену.
    /// Единственное место, которое знает порядок запуска.
    /// </summary>
    public class GameBootstrap : IInitializable, IDisposable
    {
        private readonly SceneLoader _sceneLoader;
        private readonly GameStartUI _intro;
        private readonly string _firstScene;

        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

        public GameBootstrap(SceneLoader sceneLoader, string firstScene, [InjectOptional] GameStartUI intro)
        {
            _sceneLoader = sceneLoader;
            _firstScene = firstScene;
            _intro = intro;
        }

        public void Initialize()
        {
            RunAsync(_lifetime.Token).Forget();
        }

        public void Dispose()
        {
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        private async UniTaskVoid RunAsync(CancellationToken token)
        {
            try
            {
                if (_intro != null)
                    await _intro.PlayAsync(token);

                await _sceneLoader.LoadAsync(_firstScene, token);
            }
            catch (OperationCanceledException)
            {
                // Выход из сцены во время заставки — штатная ситуация.
            }
        }
    }
}
