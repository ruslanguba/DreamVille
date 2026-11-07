using UnityEngine;

namespace DreamVille.Gameplay.Interactables.Effects
{
    /// <summary>Opens a closed door and closes an open one through the Animator's <c>isOpen</c> bool.</summary>
    [RequireComponent(typeof(Animator))]
    public sealed class ToggleDoorEffect : MonoBehaviour, IInteractionEffect
    {
        // Runtime state, not data: the door starts closed.
        private bool _isDoorOpen;

        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void Apply(in InteractionContext context)
        {
            _isDoorOpen = !_isDoorOpen;
            _animator.SetBool(AnimParams.IsOpen, _isDoorOpen);
        }
    }
}
