using UnityEngine;

namespace DreamVille.Presentation.Damage
{
    /// <summary>
    /// Hit-count-to-vignette-intensity curve, ported from the old PlayerDamageReceiver.CalculateIntensity
    /// and made total: the old code was only ever called after a hit, so "no hits" and a cap of one
    /// had no defined answer. Here they read as no vignette and full vignette respectively.
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
