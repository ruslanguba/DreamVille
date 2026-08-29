using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace DreamVille.Core.SceneManagement
{
    public class LoadScene : MonoBehaviour
    {
        public void LoadLevel(int level)
        {
            SceneManager.LoadScene(level);
        }
    }
}
