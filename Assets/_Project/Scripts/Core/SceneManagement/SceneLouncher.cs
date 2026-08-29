using UnityEngine;

using DreamVille.Gameplay.Player;

namespace DreamVille.Core.SceneManagement
{
    public class SceneLouncher : MonoBehaviour
    {
        [SerializeField] private PlayerContext _character;
        [SerializeField] private Transform _startPosition;
        void Start()
        {
            Instantiate(_character, _startPosition);
        }
    }
}
