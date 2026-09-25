using UnityEngine;

using DreamVille.Gameplay.Environment;

namespace DreamVille.Gameplay.Interactables.Effects
{
    /// <summary>Makes a drifting prop (a balloon) float up for a while through its <see cref="PropsMover"/>.</summary>
    public sealed class FloatAwayEffect : MonoBehaviour, IInteractionEffect
    {
        [SerializeField] private PropsMover _mover;

        private void Reset()
        {
            _mover = GetComponentInParent<PropsMover>();
        }

        private void Awake()
        {
            if (_mover == null)
                Debug.LogError($"[{nameof(FloatAwayEffect)}] Props mover is not assigned.", this);
        }

        public void Apply(in InteractionContext context)
        {
            _mover.GetHit();
        }
    }
}
