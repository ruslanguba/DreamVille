using System;
using System.Collections.Generic;
using UnityEngine;

using Object = UnityEngine.Object;

namespace DreamVille.Gameplay.Interactables
{
    /// <summary>An ordered, immutable set of effects applied together. One failing effect does not cancel the rest.</summary>
    public sealed class EffectChain
    {
        private readonly IInteractionEffect[] _effects;

        public EffectChain(IReadOnlyList<IInteractionEffect> effects)
        {
            if (effects == null)
                throw new ArgumentNullException(nameof(effects));

            _effects = new IInteractionEffect[effects.Count];
            for (int i = 0; i < effects.Count; i++)
                _effects[i] = effects[i];
        }

        public int Count => _effects.Length;

        /// <summary>Gathers <see cref="IInteractionEffect"/> components of <paramref name="host"/>'s GameObject, in Inspector order.</summary>
        public static EffectChain FromComponents(Component host)
        {
            if (host == null)
                throw new ArgumentNullException(nameof(host));

            return new EffectChain(host.GetComponents<IInteractionEffect>());
        }

        /// <summary>Applies every effect in order; an exception is logged with the effect as context and the next effect still runs.</summary>
        public void Apply(in InteractionContext context)
        {
            foreach (IInteractionEffect effect in _effects)
            {
                try
                {
                    effect.Apply(context);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, effect as Object);
                }
            }
        }
    }
}
