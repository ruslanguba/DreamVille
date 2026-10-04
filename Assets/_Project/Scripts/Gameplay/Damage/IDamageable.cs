using UnityEngine;

namespace DreamVille.Gameplay.Damage
{
    /// <summary>Something a shot or a hazard can hurt.</summary>
    public interface IDamageable
    {
        /// <summary>Applies <paramref name="damage"/>; <paramref name="hitCount"/> is the ricochet count of the shot (1 for a hazard).</summary>
        void TakeDamage(float damage, int hitCount);
    }
}
