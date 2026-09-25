using UnityEngine;

namespace DreamVille.Gameplay.Interactables
{
    /// <summary>Reacts to player shots, every time, by applying the effects on this GameObject.</summary>
    [DisallowMultipleComponent]
    public sealed class Interactable : MonoBehaviour
    {
        private EffectChain _effects;

        // Lazy so that an interaction arriving before Awake (or in an EditMode test, where Awake does not run)
        // still sees the effects.
        private EffectChain Effects => _effects ??= EffectChain.FromComponents(this);

        private void Awake() => _ = Effects;

        public void OnInteract(in InteractionContext context)
        {
            // The player walking into a lamp is not an interaction.
            if (context.Source != InteractionSource.Shot)
                return;

            Effects.Apply(context);
        }
    }
}
