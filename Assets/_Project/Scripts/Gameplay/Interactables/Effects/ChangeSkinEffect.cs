using UnityEngine;

using DreamVille.Gameplay.Player.Skins;

namespace DreamVille.Gameplay.Interactables.Effects
{
    /// <summary>Dresses the interacting player in the configured skin (a skin pickup).</summary>
    public sealed class ChangeSkinEffect : MonoBehaviour, IInteractionEffect
    {
        [SerializeField, Min(0)] private int _skinIndex;

        public void Apply(in InteractionContext context)
        {
            IPlayerSkins skins = context.Instigator != null
                ? context.Instigator.GetComponentInParent<IPlayerSkins>()
                : null;

            if (skins == null)
            {
                Debug.LogError($"[{nameof(ChangeSkinEffect)}] The instigator has no {nameof(IPlayerSkins)} in its parents.", this);
                return;
            }

            skins.SetSkin(_skinIndex);
        }
    }
}
