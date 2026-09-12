using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

using DreamVille.SceneManagement;
using DreamVille.UI;

namespace DreamVille.Bootstrap
{
    /// <summary>
    /// Game entry point: plays the intro and then loads the first scene.
    /// The only place that knows the startup order.
    /// </summary>
    public class GameBootstrap : IInitializable, IDisposable
    {
        private readonly ISceneLoader _sceneLoader;
        private readonly GameStartUI _intro;
        private readonly string _firstScene;

        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

        public GameBootstrap(ISceneLoader sceneLoader, string firstScene, [InjectOptional] GameStartUI intro)
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
                // Leaving the scene during the intro is expected.
            }
        }
    }
}
