using UnityEngine;

namespace DreamVille.Presentation.Damage
{
    /// <summary>
    /// Hit-count-to-vignette-intensity curve. Defined for every input, so callers need no guards:
    /// no hits read as no vignette, and a hit count at or past the cap (including a cap of one) as a full vignette.
    /// </summary>
    public static class DamageVignetteIntensity
    {
        /// <summary>0 for no hits; firstHitIntensity for the first hit; then linear to 1 at maxHits.</summary>
        public static float Calculate(int hits, int maxHits, float firstHitIntensity)
        {
            if (hits <= 0)
                return 0f;

            if (hits >= maxHits)
                return 1f;

            if (hits == 1)
                return firstHitIntensity;

            int steps = maxHits - 1;
            int stepIndex = hits - 1;
            float t = Mathf.Clamp01((float)stepIndex / steps);

            return Mathf.Lerp(firstHitIntensity, 1f, t);
        }
    }
}
