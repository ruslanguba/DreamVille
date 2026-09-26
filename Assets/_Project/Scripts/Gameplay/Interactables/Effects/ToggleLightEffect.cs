using UnityEngine;

namespace DreamVille.Gameplay.Interactables.Effects
{
    /// <summary>Switches a <see cref="Light"/> on or off (a lamp).</summary>
    public sealed class ToggleLightEffect : MonoBehaviour, IInteractionEffect
    {
        [SerializeField] private Light _light;

        private void Reset()
        {
            _light = GetComponentInChildren<Light>();
        }

        private void Awake()
        {
            if (_light == null)
                Debug.LogError($"[{nameof(ToggleLightEffect)}] Light is not assigned.", this);
        }

        public void Apply(in InteractionContext context)
        {
            _light.enabled = !_light.enabled;
        }
    }
}
