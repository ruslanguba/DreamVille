using UnityEngine;

namespace DreamVille.Gameplay.Rendering
{
    /// <summary>
    /// Picks the visuals that can render on this graphics device. Visual Effect Graph simulates on the GPU through
    /// compute shaders, which WebGL 2 does not have: there the effects are switched off (instead of logging an error per
    /// system) and the fallbacks are switched on, so an object whose only visual is an effect stays visible.
    /// Put it on an object that is always active; the fallbacks are saved inactive.
    /// </summary>
    public sealed class VisualEffectSupportSwitch : MonoBehaviour
    {
        [SerializeField, Tooltip("Active only where Visual Effect Graph runs. May include this object itself.")]
        private GameObject[] _visualEffects = new GameObject[0];

        [SerializeField, Tooltip("Active only where Visual Effect Graph does not run. Save them inactive.")]
        private GameObject[] _fallbacks = new GameObject[0];

        /// <summary>Whether this device can run Visual Effect Graph, which needs compute shaders.</summary>
        public static bool IsVisualEffectGraphSupported => SystemInfo.supportsComputeShaders;

        private void Awake()
        {
            Apply(IsVisualEffectGraphSupported);
        }

        /// <summary>Shows the effects and hides the fallbacks when <paramref name="visualEffectGraphSupported"/>, and the reverse otherwise.</summary>
        public void Apply(bool visualEffectGraphSupported)
        {
            // Fallbacks first: an effect list that includes this object deactivates it last.
            SetActive(_fallbacks, !visualEffectGraphSupported);
            SetActive(_visualEffects, visualEffectGraphSupported);
        }

        private void SetActive(GameObject[] targets, bool isActive)
        {
            foreach (GameObject target in targets)
            {
                if (target == null)
                {
                    Debug.LogError($"[{nameof(VisualEffectSupportSwitch)}] '{name}' lists an empty slot.", this);
                    continue;
                }

                target.SetActive(isActive);
            }
        }
    }
}
