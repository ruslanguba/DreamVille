using UnityEngine;

namespace DreamVille.Gameplay.Interactables
{
    /// <summary>Pure math of the pickup animation: grow, then fly to the collector while shrinking to zero.</summary>
    public static class PickupTween
    {
        /// <summary>Scale multiplier reached at the end of the grow phase and held at the start of the flight.</summary>
        public const float GrowFactor = 1.3f;

        /// <summary>elapsed / duration clamped to [0, 1]; a non-positive duration is complete at once.</summary>
        public static float Progress(float elapsed, float duration)
        {
            if (duration <= 0f)
                return 1f;

            return Mathf.Clamp01(elapsed / duration);
        }

        /// <summary>Scale multiplier while growing: 1 → <see cref="GrowFactor"/>.</summary>
        public static float GrowScale(float progress) => Mathf.Lerp(1f, GrowFactor, progress);

        /// <summary>Scale multiplier while flying: <see cref="GrowFactor"/> → 0.</summary>
        public static float FlyScale(float progress) => Mathf.Lerp(GrowFactor, 0f, progress);
    }
}
