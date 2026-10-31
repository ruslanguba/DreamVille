using UnityEngine;

namespace DreamVille.Gameplay.Environment
{
    /// <summary>Turns this object every frame so its forward axis points away from the target: the usual billboard setup for a quad that faces a camera.</summary>
    public class LookAtObject : MonoBehaviour
    {
        [SerializeField] private GameObject _object;

        private void Update()
        {
            transform.rotation = Quaternion.LookRotation(transform.position - _object.transform.position);
        }
    }
}
