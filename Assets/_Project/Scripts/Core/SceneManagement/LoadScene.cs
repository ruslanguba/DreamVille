using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace DreamVille.Core.SceneManagement
{
    /// <summary>Точка входа для кнопок меню: грузит сцену по индексу в Build Settings.</summary>
    public class LoadScene : MonoBehaviour
    {
        private SceneLoader _sceneLoader;

        [Inject]
        public void Construct(SceneLoader sceneLoader)
        {
            _sceneLoader = sceneLoader;
        }

        public void LoadLevel(int level)
        {
            _sceneLoader.LoadAsync(level).Forget();
        }
    }
}
