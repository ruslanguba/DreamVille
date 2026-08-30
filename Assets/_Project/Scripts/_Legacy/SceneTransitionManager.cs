using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace DreamVille.Legacy
{
    public class SceneTransitionManager : MonoBehaviour
    {
        public void Transition(SceneTransitionRequest request)
        {
            StartCoroutine(LoadSceneRoutine(request));
        }

        public void RestartScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private IEnumerator LoadSceneRoutine(SceneTransitionRequest request)
        {
            // TODO: fade-out
            yield return SceneManager.LoadSceneAsync(request.SceneName);
            // TODO: fade-in
        }
    }
}
