using UnityEngine;

namespace DreamVille.Gameplay.Interactables.Effects
{
    /// <summary>Opens a closed door and closes an open one through the Animator's <c>open</c>/<c>close</c> triggers.</summary>
    [RequireComponent(typeof(Animator))]
    public sealed class ToggleDoorEffect : MonoBehaviour, IInteractionEffect
    {
        [SerializeField] private bool _isOpen;

        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void Apply(in InteractionContext context)
        {
            _isOpen = !_isOpen;
            _animator.SetTrigger(_isOpen ? AnimParams.Open : AnimParams.Close);
        }
    }
}
