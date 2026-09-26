using UnityEngine;

namespace DreamVille.Gameplay.Interactables.Effects
{
    /// <summary>Holds a prop in place (kinematic) until the first interaction hands it over to physics.</summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class ReleaseRigidbodyEffect : MonoBehaviour, IInteractionEffect
    {
        private Rigidbody _rigidbody;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.isKinematic = true;
        }

        public void Apply(in InteractionContext context)
        {
            _rigidbody.isKinematic = false;
        }
    }
}
