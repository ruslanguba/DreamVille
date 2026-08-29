using UnityEngine;

namespace DreamVille.Gameplay.Interactables
{
    public class PortalActivator: MonoBehaviour
    {
        [SerializeField] Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void ActivatePortal()
        {
            _animator.SetTrigger("start");
        }
    }
}
