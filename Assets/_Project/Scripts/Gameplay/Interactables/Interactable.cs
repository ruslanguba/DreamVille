using UnityEngine;

namespace DreamVille.Gameplay.Interactables
{
    /// <summary>Reacts to player shots, every time, by applying the effects on this GameObject.</summary>
    [DisallowMultipleComponent]
    public sealed class Interactable : MonoBehaviour, IInteractable
    {
        private EffectChain _effects;

        // Lazy so that an interaction arriving before Awake (or in an EditMode test, where Awake does not run)
        // still sees the effects.
        private EffectChain Effects => _effects ??= CreateEffects();

        // Gathers the effects once and reports an empty host at load time instead of on the first shot.
        private void Awake() => _ = Effects;

        public void OnInteract(in InteractionContext context)
        {
            // The player walking into a lamp is not an interaction.
            if (context.Source != InteractionSource.Shot)
                return;

            Effects.Apply(context);
        }

        private EffectChain CreateEffects()
        {
            EffectChain effects = EffectChain.FromComponents(this);
            if (effects.Count == 0)
                Debug.LogError($"[{nameof(Interactable)}] No {nameof(IInteractionEffect)} component on this GameObject; shots do nothing.", this);

            return effects;
        }
    }
}
