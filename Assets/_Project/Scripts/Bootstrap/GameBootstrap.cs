using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Zenject;

using DreamVille.Configs;
using DreamVille.SceneManagement;
using DreamVille.UI;

namespace DreamVille.Bootstrap
{
    /// <summary>
    /// Game entry point: plays the intro and then loads the menu scene named by the catalog.
    /// The only place that knows the startup order.
    /// </summary>
    public class GameBootstrap : IInitializable, IDisposable
    {
        private readonly ISceneLoader _sceneLoader;
        private readonly GameStartUI _intro;
        private readonly SceneCatalog _catalog;

        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

        public GameBootstrap(ISceneLoader sceneLoader, SceneCatalog catalog, GameStartUI intro)
        {
            _sceneLoader = sceneLoader;
            _catalog = catalog;
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
                await _intro.PlayAsync(token);
                await _sceneLoader.LoadAsync(_catalog.MainMenuScene.SceneName, token);
            }
            catch (OperationCanceledException)
            {
                // Leaving the scene during the intro is expected.
            }
        }
    }
}
